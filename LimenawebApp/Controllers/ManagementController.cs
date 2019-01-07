using LimenawebApp.Models;
using Newtonsoft.Json;
using Postal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers
{
    public class ManagementController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        public ActionResult Users()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Users";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "users_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER

                List<Sys_Users> lstUsers = (from a in dblim.Sys_Users select a).ToList();
                return View(lstUsers);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        //MERCHANDISING

        public JsonResult CreateUser(string name, string lastname, string email, string password, string telephone, string position, string departments, string roles)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            Sys_Users User = new Sys_Users();
            try
            {
                

                User.Name = name;
                User.Lastname = lastname;
                User.Email = email;
                User.Password = password;
                User.Telephone = telephone;
                User.Position = position;
                User.Departments = departments;
                User.Roles = roles;

                if (User.Position == null)
                {
                    User.Position = "";
                }
                if (User.Departments == null)
                {
                    User.Departments = "";
                }
                if (User.Roles == null)
                {
                    User.Roles = "";
                }

                if (User.Telephone == null)
                {
                    User.Telephone = "";
                }

                User.ID_Company = activeuser.ID_Company;
                User.Registration_Date = DateTime.UtcNow;
                User.Active = true;

                dblim.Sys_Users.Add(User);
                dblim.SaveChanges();

                //Send the email
                dynamic semail = new Email("email_confirmation");
                semail.To = User.Email.ToString();
                semail.From = "donotreply@limenainc.net";
                semail.user = User.Name + " " + User.Lastname;
                semail.email = User.Email;
                semail.password = User.Password;

                semail.Send();

                //FIN email


                return Json(new { Result = "Success" });
            }
            catch (Exception ex){          
                return Json(new { Result = "Something wrong happened, try again. " + ex.Message});
            }

        }
    }
}