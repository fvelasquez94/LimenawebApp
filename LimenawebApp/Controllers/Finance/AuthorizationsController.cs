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

        private Cls_session cls_session = new Cls_session();
        private Cls_Authorizations cls_Authorizations = new Cls_Authorizations();
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

        public ActionResult Put_Authorization(string commentsfinance, string id_authorization, int status)
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
                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = "Error";
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