using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers.Purchases
{
    public class PurchasesController : Controller
    {
        private dbLimenaEntities db = new dbLimenaEntities();
        private Cls_session cls_session = new Cls_session();
        private MatrizComprasEntities dbMatriz = new MatrizComprasEntities();

        public ActionResult ProductCatalog()
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Purchases";
                ViewData["Page"] = "Product_catalog";
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

                var data = (from a in dbMatriz.Purchase_catalog select a).ToList();
                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
    }
}