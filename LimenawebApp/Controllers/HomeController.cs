using LimenawebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers
{
    public class HomeController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Services()
        {
            return View();
        }
        public ActionResult Brands()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult Contactus()
        {
            return View();
        }
        public ActionResult UnderConstruction()
        {
            return View();
        }
        public ActionResult Login(bool access =true)
        {
            if (access == false) {
                ViewBag.warning = "Email or Password wrong." ;
            }
            
            return View();
        }
        public ActionResult Log_in(string email, string password)
        {
            Global_variables.active_user = (from a in dblim.Sys_Users where (a.Email == email && a.Password == password && a.Active == true) select a).FirstOrDefault();
            if (Global_variables.active_user != null)
            {
                Global_variables.active_ID_Company = Global_variables.active_user.ID_Company;
                Global_variables.active_Departments = Global_variables.active_user.Departments;
                Global_variables.active_Roles = Global_variables.active_user.Roles;

                return RedirectToAction("Dashboard_sales", "Main", null);

            }

            return RedirectToAction("Login", "Home", new { access= false });
        }

        public ActionResult Log_out()
        {
            Session.RemoveAll();
            Global_variables.active_user.Name = null;
            Global_variables.active_Departments = null;
            Global_variables.active_Roles = null;
            //if (Request.Cookies["GLOBALUSERID"] != null)
            //{
            //    var c = new HttpCookie("GLOBALUSERID");
            //    c.Expires = DateTime.Now.AddDays(-1);
            //    Response.Cookies.Add(c);
            //}
            return RedirectToAction("Login","Home", new { access=true});
        }
    }
}