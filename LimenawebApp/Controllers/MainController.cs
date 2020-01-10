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
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        // GET: Main Dashboard_sales
        //CLASS GENERAL
        private clsGeneral generalClass = new clsGeneral();

        public class tablahijospadreAct
        {
            public string id { get; set; }
            public string text { get; set; }
            public string parent { get; set; }
        }

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
                var showSurvey = 0;
                int isAdmin = 0;
                if (showSurvey==1) {


                    List<RepsSurveys> repsSurveys = new List<RepsSurveys>(); 
                    var surveys = new List<SurveysTasks>();
                    string[] usids;
                    string[] notinlist;
                    DateTime dtvalue = new DateTime(2019, 1, 1);
                    var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();
                    List<RepsSurveys> supervisors = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Supervisor") && a.ID_User==activeuser.ID_User).Select(c => new RepsSurveys { idSAP = c.IDSAP, ID_User = c.ID_User, idSAPsupervisor = "", Lastname = c.Lastname, Name = c.Name, prop01 = "", prop02 = "" }).ToList();

                    
                    if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                    {
                        isAdmin = 1;

                        List<int> TagIds = new List<int>();

                        if (activeuser.Roles.Contains("Sales Supervisor"))
                        {
                            isAdmin = 0;
                            TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList();

                            repsSurveys = users.Where(d => TagIds.Contains(d.ID_User)).Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP = d.IDSAP, idSAPsupervisor = d.prop01 }).ToList();
                            usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();

                            surveys = (from a in dbcmk.Tasks
                                       where (TagIds.Contains(a.ID_userEnd))
                                       select new SurveysTasks
                                       {
                                           ID_task = a.ID_task,
                                           Customer = a.Customer,
                                           ID_Customer = a.ID_Customer,
                                           ID_taskstatus = a.ID_taskstatus,
                                           ID_taskType = a.ID_taskType,
                                           ID_userEnd = a.ID_userEnd,
                                           ID_userEndSAP = 0,
                                           TaskType = a.TaskType,
                                           Task_description = a.Task_description,
                                           visit_date = a.visit_date
                                       }).ToList();
                            notinlist = surveys.Select(d => d.ID_Customer).ToArray();

                            var customers = (from d in dlipro.BI_Dim_Customer
                                             where (usids.Contains(d.id_SalesRep.ToString()) && !notinlist.Contains(d.id_Customer))
                                             select new SurveysTasks
                                             {
                                                 ID_task = 0,
                                                 Customer = d.Customer,
                                                 ID_Customer = d.id_Customer,
                                                 ID_userEnd = 0,
                                                 ID_userEndSAP = d.id_SalesRep,
                                                 ID_taskstatus = 3,
                                                 ID_taskType = 1,
                                                 Task_description = "",
                                                 TaskType = "",
                                                 visit_date = dtvalue
                                             }).ToList();

                            surveys.AddRange(customers);



                            //ESTADISTICAS POR SUPERVISOR
                            foreach (var sup in supervisors)
                            {
                                decimal totalfinalsup = 0;
                                decimal totalfinishedorcanceledsup = 0;
                                foreach (var user in repsSurveys.Where(c => c.idSAPsupervisor == sup.idSAP))
                                {
                                    decimal totalclientes = surveys.Where(d => d.ID_userEnd == user.ID_User).Count();
                                    decimal totalclientes2 = surveys.Where(d => d.ID_userEndSAP.ToString() == user.idSAP).Count();

                                    totalfinalsup += (totalclientes + totalclientes2);
                                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                                    decimal finishedorCanceled = (from e in surveys where (e.ID_userEnd == user.ID_User && e.ID_taskstatus == 4) select e).Count();

                                    totalfinishedorcanceledsup += finishedorCanceled;
                                }


                                if (totalfinalsup != 0)
                                {
                                    if (totalfinishedorcanceledsup != 0)
                                    {

                                        sup.prop01 = (((Convert.ToDecimal(totalfinishedorcanceledsup) / totalfinalsup) * 100)).ToString();
                                        ViewBag.prop01 = (((Convert.ToDecimal(totalfinishedorcanceledsup) / totalfinalsup) * 100)).ToString();

                                    }

                                    else
                                    {

                                        sup.prop01 = (Convert.ToDecimal(0)).ToString();
                                        ViewBag.prop01 = (Convert.ToDecimal(0)).ToString();
                                    }
                                    sup.prop02 = "(" + totalfinishedorcanceledsup + " / " + totalfinalsup + ")";
                                    ViewBag.prop02 = "(" + totalfinishedorcanceledsup + " / " + totalfinalsup + ")";

                                }
                                else
                                {
                                    sup.prop01 = "0";
                                    sup.prop02 = "(0/ 0)";
                                    ViewBag.prop01 = "0";
                                    ViewBag.prop02 = "(0/ 0)";
                                }

                            }
                        }

                    }
                    else
                    {

                        if (activeuser.Roles.Contains("Sales Representative")) {
                            repsSurveys = dblim.Sys_Users.Where(a => a.ID_User == activeuser.ID_User).Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP = d.IDSAP, idSAPsupervisor = d.prop01 }).ToList();
                            usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                            surveys = (from a in dbcmk.Tasks
                                       where (a.ID_userEnd == activeuser.ID_User)
                                       select new SurveysTasks
                                       {
                                           ID_task = a.ID_task,
                                           Customer = a.Customer,
                                           ID_Customer = a.ID_Customer,
                                           ID_taskstatus = a.ID_taskstatus,
                                           ID_taskType = a.ID_taskType,
                                           ID_userEnd = a.ID_userEnd,
                                           ID_userEndSAP = 0,
                                           TaskType = a.TaskType,
                                           Task_description = a.Task_description,
                                           visit_date = a.visit_date
                                       }).ToList();
                            notinlist = surveys.Select(d => d.ID_Customer).ToArray();


                            int IDusuario = Convert.ToInt32(activeuser.IDSAP);

                            var lstCustomer = (from b in dlipro.BI_Dim_Customer where (b.id_SalesRep == IDusuario) select new tablahijospadreAct { id = b.id_Customer, text = b.Customer, parent = "" }).OrderBy(b => b.text).ToList();

                            foreach (var customer in lstCustomer)
                            {
                                var existe = surveys.Where(a => a.ID_Customer == customer.id).Count();
                                if (existe > 0)
                                {
                                    customer.parent = "existe";
                                }
                            }

                            var customers = (from d in dlipro.BI_Dim_Customer
                                             where (usids.Contains(d.id_SalesRep.ToString()) && !notinlist.Contains(d.id_Customer))
                                             select new SurveysTasks
                                             {
                                                 ID_task = 0,
                                                 Customer = d.Customer,
                                                 ID_Customer = d.id_Customer,
                                                 ID_userEnd = 0,
                                                 ID_userEndSAP = d.id_SalesRep,
                                                 ID_taskstatus = 3,
                                                 ID_taskType = 1,
                                                 Task_description = "",
                                                 TaskType = "",
                                                 visit_date = dtvalue
                                             }).ToList();

                            surveys.AddRange(customers);


                            foreach (var user in repsSurveys)
                            {
                                decimal totalclientes = surveys.Where(d => d.ID_userEnd == user.ID_User).Count();
                                decimal totalclientes2 = surveys.Where(d => d.ID_userEndSAP.ToString() == user.idSAP).Count();

                                totalclientes = (totalclientes + totalclientes2);
                                //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                                decimal finishedorCanceled = (from e in surveys where (e.ID_userEnd == user.ID_User && e.ID_taskstatus == 4) select e).Count();

                                if (totalclientes != 0)
                                {
                                    if (finishedorCanceled != 0)
                                    {

                                        user.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();
                                        ViewBag.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();
                                    }

                                    else
                                    {

                                        user.prop01 = (Convert.ToDecimal(0)).ToString();
                                    }
                                    user.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";
                                    ViewBag.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";
                                }
                                else
                                {
                                    user.prop01 = "0";
                                    user.prop02 = "(0/ 0)";
                                    ViewBag.prop01 = "0";
                                    ViewBag.prop02 = "(0/ 0)";
                                }
                            }
                        }
                        else {
                            isAdmin = 1;
                            ViewBag.prop01 = "0";
                            ViewBag.prop02 = "(0/ 0)";
                        }
                        

                    }

                }


                ViewBag.issuperadmin = isAdmin;


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult Dashboard_ExternalApps(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "External Apps";
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

                ViewBag.userID = activeuser.ID_User;
                ViewBag.userName = activeuser.Name + " " + activeuser.Lastname;

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
                    if (r.Contains("Sales Supervisor"))
                    {
                        var gift_products = (from a in dlipro.BI_Dim_Products
                                             where (a.StockBonif > 0 && a.StockBonif != null && !a.id.Contains("SERVICES")) select a.id).ToArray();


                        bonifications = (from a in internadli.Tb_Bonificaciones where (a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate && a.OrderClosed == true && a.deleted == false && gift_products.Contains(a.CodProducto)) select a);
                    }
                    else {
                        bonifications = (from a in internadli.Tb_Bonificaciones where (a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate && a.OrderClosed == true && a.deleted == false) select a);
                    }
                   
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

        public ActionResult Dashboard_Inventory()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "Inventory";
                ViewBag.menunameid = "home_menuInventory";
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
        public ActionResult Dashboard_OperationsPurchases()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Home";
                ViewData["Page"] = "Inventory";
                ViewBag.menunameid = "home_menuInventory";
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