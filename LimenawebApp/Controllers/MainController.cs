using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LimenawebApp.Models;
using Newtonsoft.Json;

namespace LimenawebApp.Controllers
{
    public class MainController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();

        // GET: Main Dashboard_sales
        public ActionResult Dashboard_sales()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "Sales";
                ViewBag.menunameid = "home_menuSales";
                ViewBag.submenunameid = "";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x=>x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult Dashboard_customers(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "Customers";
                ViewBag.menunameid = "home_menuCustomers";
                ViewBag.submenunameid = "";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER
                DateTime filtrostartdate;
                DateTime filtroenddate;
                //filtros de fecha (DIARIO)
                //var sunday = DateTime.Today;
                //var saturday = sunday.AddHours(23);
                ////filtros de fecha (MENSUAL)
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();

                IEnumerable<Tb_Bonificaciones> bonifications;

                if (r.Contains("AdminBonif"))
                {
                    bonifications = (from a in internadli.Tb_Bonificaciones where (a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate && a.OrderClosed==true && a.deleted==false)  select a);
                }
                else {
                    bonifications = (from a in internadli.Tb_Bonificaciones where (a.ID_Vendor == activeuser.IDSAP && a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate && a.OrderClosed == true && a.deleted == false) select a);
                }
       
                ViewBag.userID = activeuser.ID_User;
                ViewBag.userName = activeuser.Name + " " + activeuser.Lastname;

                return View(bonifications);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult Dashboard_operations()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "Operations";
                ViewBag.menunameid = "home_menuOper";
                ViewBag.submenunameid = "";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }


    }
}