using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers
{
    public class ReportsController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();

        public ActionResult Reports_RC(string ID_User)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Daily Routes Summary";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "rep1_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER
                //Evaluamos si es supervisor o usuario normal para mostrar recursos o si es ambos o si es super admin

                foreach (var item in r)
                {
                    if (item == "Sales Representative")
                    {
                        ViewData["showSR_Resources"] = true;
                        List<Tb_Resources> lstresources = (from a in dblim.Tb_Resources where (a.ID_User == activeuser.ID_User) select a).ToList();
                        ViewBag.lstresources = lstresources;
                    }
                    else if (item == "Sales Supervisor" || item == "Super Admin")
                    {
                        var selectedUser = 0;
                        List<Sys_Users> lstUsers = (from a in dblim.Sys_Users where (a.Roles.Contains("Sales Representative") && a.ID_Company == activeuser.ID_Company) select a).ToList();
                        if (ID_User != null || ID_User != "")
                        {
                            var filter_ID = Convert.ToInt32(ID_User);
                            List<Tb_Resources> lstresources = (from a in dblim.Tb_Resources where (a.ID_User == filter_ID) select a).ToList();
                            selectedUser = filter_ID;
                            ViewBag.lstresourcesSS = lstresources;
                        }
                        else {
               
                            List<Tb_Resources> lstresources = (from a in dblim.Tb_Resources where (a.ID_User == 0) select a).ToList();
                            ViewBag.lstresourcesSS = lstresources;
                        }
                        ViewBag.selUser = selectedUser;
                        ViewBag.lstUsers = lstUsers;
                        ViewData["showSS_Resources"] = true;

                    }
                }



                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
    }
}