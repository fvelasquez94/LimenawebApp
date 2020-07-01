using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.Creditmemos_api;
using LimenawebApp.Models.Returnreasons_api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers.Warehouse
{
    public class WarehouseController : Controller
    {
        private dbLimenaEntities db = new dbLimenaEntities();
        private Cls_session cls_session = new Cls_session();
        private Cls_Creditmemos cls_creditmemos = new Cls_Creditmemos();
        private Cls_Returnreasons cls_returnreasons = new Cls_Returnreasons();
        public ActionResult ReceiveCredits(string fstartd, string fendd)
        {

            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Warehouse";
                ViewData["Page"] = "Receive Credits";
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


                var creditmemos = cls_creditmemos.GetCreditMemos(0, "",filtrostartdate, filtroenddate, true);
                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits")) {
                        roles.Add("FD");
                    }
                  

                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow= returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                List<Returnreason_api> lstreturnsreasons = new List<Returnreason_api>();
                var response = cls_returnreasons.GetReturnreasons();
                if (response.data != null) { lstreturnsreasons = response.data.Where(c => c.visibleFor != null && c.visibleFor.Contains("SDA") && c.active == "Y" && roles.Contains(c.authorizedBy)).ToList(); }

                ViewBag.returnreasonschange = lstreturnsreasons;


                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                string[] rrfilter;
                if (creditmemos.data != null) {

                    var sss= creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
             details.
             OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                    rrfilter = (from creditmemo in creditmemos.data
                                      from reason in creditmemo.details
                                      select reason.returnReasonCode).Distinct().ToArray();

                    returnsToshow = returnsToshow.Where(c => rrfilter.Contains(c.reasonCode)).Select(c => c).ToList();
                }
                //filtramos nuevamente
              

                ViewBag.returnreasons = returnsToshow;

                return View(creditmemos.data);



            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

     
        }


        public ActionResult ReceivedCredits(string fstartd, string fendd)
        {

            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Warehouse";
                ViewData["Page"] = "Received Credits";
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


                var creditmemos = cls_creditmemos.GetCreditMemos(0, "", filtrostartdate, filtroenddate, true);
                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("FD");
                    }


                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow = returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                List<Returnreason_api> lstreturnsreasons = new List<Returnreason_api>();
                var response = cls_returnreasons.GetReturnreasons();
                if (response.data != null) { lstreturnsreasons = response.data.Where(c => c.visibleFor != null && c.visibleFor.Contains("SDA") && c.active == "Y" && roles.Contains(c.authorizedBy)).ToList(); }

                ViewBag.returnreasonschange = lstreturnsreasons;


                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                string[] rrfilter;
                if (creditmemos.data != null)
                {

                    var sss = creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
              details.
              OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                    rrfilter = (from creditmemo in creditmemos.data
                                from reason in creditmemo.details
                                select reason.returnReasonCode).Distinct().ToArray();

                    returnsToshow = returnsToshow.Where(c => rrfilter.Contains(c.reasonCode)).Select(c => c).ToList();
                }
                //filtramos nuevamente


                ViewBag.returnreasons = returnsToshow;

                return View(creditmemos.data);



            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }


        }

        public ActionResult Put_creditMemoDetail(PutDetailsCreditmemos_api Item, int DocentryCredit)
        {
            try
            {
                var response = cls_creditmemos.PutCreditmemo(Item, DocentryCredit);

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


        public ActionResult Get_creditMemoDetail(int DocentryCredit)
        {
            try
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
            
                var creditmemos = cls_creditmemos.GetCreditMemo(DocentryCredit,true);

                var submit = (from creditmemo in creditmemos.data
                              from detail in creditmemo.details
                              where detail.received ==false
                              select detail).Count();


                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("FD");
                    }


                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow = returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                if (creditmemos.data != null)
                {

                    var sss = creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
              details.
              OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                }


                var result = new { details= creditmemos.data[0].details, showsubmit= submit };           
                return Json(result, JsonRequestBehavior.AllowGet);
            

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }


        public ActionResult Transform_creditMemo(int docentryCreditMemo)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                var response = cls_creditmemos.TransformCreditMemo(docentryCreditMemo, activeuser.IDSAP);

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