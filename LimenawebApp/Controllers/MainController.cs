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

        // GET: Main
        public ActionResult Dashboard_sales()
        {
            if (Global_variables.active_user.Name != null)
            {
                
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Departments";
                ViewData["Page"] = "Sales";
                ViewBag.menunameid = "dep_menu";
                ViewBag.submenunameid = "sales_submenu";
                List<string> d = new List<string>(Global_variables.active_user.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(Global_variables.active_user.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //FIN HEADER
                //Evaluamos si es supervisor o usuario normal para mostrar recursos o si es ambos o si es super admin

                foreach (var item in r) {
                    if (item == "Sales Representative") {
                        ViewData["showSR_Resources"] = true;
                       List<Tb_Resources> lstresources = (from a in dblim.Tb_Resources where (a.ID_User == Global_variables.active_user.ID_User) select a).ToList();
                        ViewBag.lstresources = lstresources;
                    }
                    else if (item == "Sales Supervisor")
                    {
                        ViewData["showSS_Resources"] = true;

                    }
                }



                return View();

            }
            else {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult Users()
        {
            if (Global_variables.active_user.Name != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Users";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "users_submenu";
                List<string> s = new List<string>(Global_variables.active_user.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);

                //FIN HEADER

                List<Sys_Users> lstUsers = (from a in dblim.Sys_Users select a).ToList();
                return View(lstUsers);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
    }
}