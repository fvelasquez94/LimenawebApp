using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static LimenawebApp.Models.Journal.Mdl_Journal;

namespace LimenawebApp.Controllers.Finance
{
    public class JournalController : Controller
    {
        private Cls_session cls_session = new Cls_session();
        private Cls_Journals cls_Journals = new Cls_Journals();
        // GET: Journal
        public ActionResult Balance_control(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Finance";
                ViewData["Page"] = "Balance_control";
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
                var sunday = now;
                var saturday = sunday.AddDays(1).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                GET_Journals_api journals = new GET_Journals_api();


                //Authorizations
                journals = cls_Journals.Getjournals(filtrostartdate, filtroenddate, true);


                return View(journals);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
    }
}