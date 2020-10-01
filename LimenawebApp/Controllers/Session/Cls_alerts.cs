using LimenawebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.Session
{
    public class Cls_alerts
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();

        public void New_alert(int ID_user, string title, string description)
        {
            Tb_Alerts newalert = new Tb_Alerts();
            newalert.Active = true;
            newalert.Date = DateTime.UtcNow;
            newalert.ID_user = ID_user;
            newalert.Title = title;
            newalert.Description= description;

            dblim.Tb_Alerts.Add(newalert);
            dblim.SaveChanges();
        }
    }
}