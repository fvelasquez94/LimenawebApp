using LimenawebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.Session
{
    public class Cls_session
    {
        private dbLimenaEntities db = new dbLimenaEntities();

        public bool checkSession()
        {
            var flag = false;
            Sys_Users activeuser = HttpContext.Current.Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                flag = true;
            }
            else
            {
                if (HttpContext.Current.Request.Cookies["correo"] != null)
                {
                    //COMO YA EXISTE NO NECESITAMOS RECREARLA Y SOLO VOLVEMOS A INICIAR SESION
                    flag = true;
                    var email = HttpContext.Current.Request.Cookies["correo"].Value;
                    var password = HttpContext.Current.Request.Cookies["pass"].Value;
                    HttpContext.Current.Session["activeUser"] = (from a in db.Sys_Users where (a.Email == email && a.Password == password && a.Active == true) select a).FirstOrDefault();
                    Sys_Users activeuserAgain = HttpContext.Current.Session["activeUser"] as Sys_Users;
                    if (activeuserAgain != null)
                    {
                        flag = true;
                    }
                    else { flag = false; }


                }
                else
                {
                    flag = false;
                }
            }
            return flag;
        }
    }
}