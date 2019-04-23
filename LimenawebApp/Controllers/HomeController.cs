using LimenawebApp.Models;
using Postal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
        /// 
        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Test_barcode()
        {

            return View();



        }        public ActionResult Test_barcode2()
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

            HttpCookie aCookieCorreo = Request.Cookies["correo"];
            HttpCookie aCookiePassword = Request.Cookies["pass"];
            HttpCookie aCookieRememberme = Request.Cookies["rememberme"];

            try
            {
                var correo = Server.HtmlEncode(aCookieCorreo.Value).ToString();
                var pass = Server.HtmlEncode(aCookiePassword.Value).ToString();
                int remember =Convert.ToInt32(Server.HtmlEncode(aCookieRememberme.Value));

                if (remember == 1) { ViewBag.remember = true; } else { ViewBag.remember = false; }
                ViewBag.correo = correo;
                ViewBag.pass = pass;
              
            }
            catch {
               ViewBag.remember = false; 
  
            }
           


            return View();
        }
        public ActionResult Log_in(string email, string password, string date, bool rememberme)
        {
 

            Session["activeUser"] = (from a in dblim.Sys_Users where (a.Email == email && a.Password == password && a.Active == true) select a).FirstOrDefault();
            if (Session["activeUser"] != null)
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                ///PARA RECORDAR DATOS
                if (rememberme == true)
                {
                    if (Request.Cookies["correo"] != null)
                    {
                        //COMO YA EXISTE NO NECESITAMOS RECREARLA
                    }
                    else
                    {
                        HttpCookie aCookie = new HttpCookie("correo");
                        aCookie.Value = activeuser.Email.ToString();
                        aCookie.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie2 = new HttpCookie("pass");
                        aCookie2.Value = activeuser.Password.ToString();
                        aCookie2.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie3 = new HttpCookie("rememberme");
                        aCookie3.Value = "1";
                        aCookie3.Expires = DateTime.Now.AddMonths(3);


                        Response.Cookies.Add(aCookie);
                        Response.Cookies.Add(aCookie2);
                        Response.Cookies.Add(aCookie3);
                    }
                }

                    ////Save log
                    //Sys_LogCon newLog = new Sys_LogCon();
                    //try
                    //{
                    //    newLog.ID_Company = activeuser.ID_Company;
                    //    newLog.ID_User = activeuser.ID_User;
                    //    newLog.date = Convert.ToDateTime(date);

                    //    newLog.City = "";
                    //    newLog.Country_Code = "";
                    //    newLog.Country_Name = "";
                    //    newLog.Continent_Name = "";
                    //    newLog.Region_Code = "";
                    //    newLog.Region_Name = "";
                    //    newLog.IP = "";
                    //    newLog.TypeH = "";
                    //    newLog.Hostname = "";
                    //    newLog.Lat = "";
                    //    newLog.Long = "";

                    //    dblim.Sys_LogCon.Add(newLog);
                    //    dblim.SaveChanges();
                    //}
                    //catch {

                    //}


                    if (activeuser.Departments.Contains("Sales")) {
                    return RedirectToAction("Dashboard_sales", "Main", null);
                }
                else if (activeuser.Departments.Contains("DSD"))
                {
                    return RedirectToAction("Dashboard_dsd", "Main", null);

                }
                else if (activeuser.Departments.Contains("Operations"))
                {
                    return RedirectToAction("Dashboard_operations", "Main", null);

                }
                else if (activeuser.Departments.Contains("Customer"))
                {
                    return RedirectToAction("Dashboard_customers", "Main", null);

                }

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

        public ActionResult Reset_password(bool token=false, bool email=false)
        {
            if (token == true)
            {
                ViewData["msgnotexist"] = "";
                ViewBag.msg = "Your password has been reset successfully! Your new password has been sent to your email address.";
            }
            else {
                if (email == true)
                {
                    ViewData["msgnotexist"] = "This email does not exist.";
                }
                else {
                    ViewData["msgnotexist"] = "";
                }
                
                ViewBag.msg = "";
            }
            return View();
        }

        public ActionResult Reset_pass(string email)
        {


            Sys_Users User = (from a in dblim.Sys_Users where (a.Email == email) select a).FirstOrDefault();
            if (User != null)
            {
                User.Password = "dli2019";
                dblim.Entry(User).State = EntityState.Modified;
                dblim.SaveChanges();


                //Send the email
                dynamic semail = new Email("reset_password");
                semail.To = User.Email.ToString();
                semail.From = "donotreply@limenainc.net";
                semail.user = User.Name + " " + User.Lastname;
                semail.email = User.Email;
                semail.password = User.Password;

                semail.Send();

                //FIN email
            
                return RedirectToAction("Reset_password", "Home", new { token = true, email=false });

            }
            
            return RedirectToAction("Reset_password", "Home", new { token = false, email=true });
        }
    }
}