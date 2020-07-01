using LimenawebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.Session
{
    public class Cls_cookies
    {
        public void Check_cookies(string email, string pass, bool? rememberme)
        {
            var Request = HttpContext.Current.Request;
            var Response = HttpContext.Current.Response;
            Sys_Users activeuser = HttpContext.Current.Session["activeUser"] as Sys_Users;
            ///PARA RECORDAR DATOS
            ///

            if (rememberme == true)
            {
                if (Request.Cookies["correo"] != null)
                {
                    if (Request.Cookies["correo"] != null)
                    {
                        var c = new HttpCookie("correo");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }
                    if (Request.Cookies["pass"] != null)
                    {
                        var c = new HttpCookie("pass");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }
                    if (Request.Cookies["rememberme"] != null)
                    {
                        var c = new HttpCookie("rememberme");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }

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
            else
            {
                if (Request.Cookies["correo"] != null)
                {
                    var c = new HttpCookie("correo");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }
                if (Request.Cookies["pass"] != null)
                {
                    var c = new HttpCookie("pass");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }
                if (Request.Cookies["rememberme"] != null)
                {
                    var c = new HttpCookie("rememberme");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }

            }
        }
    }
}