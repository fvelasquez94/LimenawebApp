using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.Authorizations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers.Finance
{
    public class AuthorizationsController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private Cls_session cls_session = new Cls_session();
        private Cls_Authorizations cls_Authorizations = new Cls_Authorizations();
        private Cls_alerts cls_alerts = new Cls_alerts();
        // GET: Authorizations
   
        public ActionResult Index(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Finance";
                ViewData["Page"] = "Authorizations";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                //List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                //ViewBag.notifications = lstAlerts;
                ViewBag.activeuser = activeuser;
                //FIN HEADER
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();

                //Authorizations
                
                var authorizations = cls_Authorizations.GetAuthorizations(0, "", "", filtrostartdate,filtroenddate);

                return View(authorizations.data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult checkAuthorization(string idauth)
        {
            var authorizations = cls_Authorizations.GetAuthorizationbyID(idauth);

            if (authorizations != null)
            {
                if (authorizations.data != null)
                {
                    if (authorizations.data.status == 0)
                    {
                        return View(authorizations.data);
                    }
                    else
                    {
                        return RedirectToAction("AuthAlert", "Authorizations");
                    }
                }
                else {
                    return RedirectToAction("AuthAlert", "Authorizations");
                }

            }
            else
            {

                return RedirectToAction("AuthAlert", "Authorizations");

            }
        }

        public ActionResult ApproveAuthQA(string idauth)
        {
            var authactual = cls_Authorizations.GetAuthorizationbyID(idauth);


                PutAuthorization_api updateauth = new PutAuthorization_api();
      
                updateauth.commentsFinance = "";
                updateauth.idFinanceUser = "0";
                updateauth.status = 1;

                var response = cls_Authorizations.PutAuthorization(updateauth, idauth);

                if (response.IsSuccessful == true)
                {
                    var iduser = (from a in dblim.Sys_Users where (a.IDSAP == authactual.data.idDriver) select a).FirstOrDefault();
                    if (iduser != null)
                    {

                            cls_alerts.New_alert(iduser.ID_User, "Approved Authorization", "for route " + authactual.data.idRoute);
                   

                    }


                return RedirectToAction("AuthMessage", "Authorizations", new { status = 1 });
                }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult DenyAuthQA(string idauth)
        {
            var authactual = cls_Authorizations.GetAuthorizationbyID(idauth);


            PutAuthorization_api updateauth = new PutAuthorization_api();
   
            updateauth.commentsFinance = "";
            updateauth.idFinanceUser = "0";
            updateauth.status = 2;

            var response = cls_Authorizations.PutAuthorization(updateauth, idauth);

            if (response.IsSuccessful == true)
            {
                var iduser = (from a in dblim.Sys_Users where (a.IDSAP == authactual.data.idDriver) select a).FirstOrDefault();
                if (iduser != null)
                {

                    cls_alerts.New_alert(iduser.ID_User, "Denied Authorization", "for route " + authactual.data.idRoute);


                }


                return RedirectToAction("AuthMessage", "Authorizations", new { status = 0 });
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult AuthMessage(int status)
        {
            if (status == 0) { ViewBag.alert = "Authorization denied"; } else { ViewBag.alert = "Authorization approved"; }
            return View();

        }

        public ActionResult AuthAlert()
        {
  
            return View();

        }

        public ActionResult Put_Authorization(string commentsfinance, string id_authorization, int status, string iddriver, string route)
        {
            try
            {
                PutAuthorization_api updateauth = new PutAuthorization_api();
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                updateauth.commentsFinance = commentsfinance;
                updateauth.idFinanceUser = activeuser.IDSAP;
                updateauth.status = status;
              
                var response = cls_Authorizations.PutAuthorization(updateauth, id_authorization);

                if (response.IsSuccessful == true)
                {
                    var iduser = (from a in dblim.Sys_Users where (a.IDSAP == iddriver) select a).FirstOrDefault();
                    if (iduser != null) {
                        if (status == 1)
                        {
                            cls_alerts.New_alert(iduser.ID_User, "Approved Authorization", "for route " + route);
                        }
                        else {
                            cls_alerts.New_alert(iduser.ID_User, "Denied Authorization", "for route " + route);
                        }
                       
                    }
                   
                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = "Error: " + response.StatusDescription;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }
    }
}