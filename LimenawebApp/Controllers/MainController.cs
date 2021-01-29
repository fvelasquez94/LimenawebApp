using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.SalesOrders;
using Newtonsoft.Json;

namespace LimenawebApp.Controllers
{
    public class MainController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();

        private Cls_session cls_session = new Cls_session();
        private Cls_SalesOrders cls_salesorders = new Cls_SalesOrders();
        private Cls_BusinessPartners cls_businesspartners = new Cls_BusinessPartners();
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

        public ActionResult Dashboard()
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Main";
                ViewData["Page"] = "Index";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);
                //List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                //ViewBag.notifications = lstAlerts;
                ViewBag.activeuser = activeuser;
                //FIN HEADER
                //FILTROS VARIABLES
                ViewBag.showfreezermessage = "N";

                if (activeuser.Departments.Contains("Operations") || activeuser.Roles.Contains("Super Admin")) {
                    var freezersactive = (from a in dblim.Tb_logContainers where (a.inRoute == true) select a).Count();
                    if (freezersactive > 0) {
                        ViewBag.showfreezermessage = "Y";
                    }
                    
                }
                List<SalesOrders_api> salesorders = new List<SalesOrders_api>();
                GetBudget_api  budget = new GetBudget_api();
                List<Budget_api> budgetfilter = new List<Budget_api>();
                //finales
                List<Budget_BP> customers = new List<Budget_BP>();
                List<Budget_BP> ordenes = new List<Budget_BP>();
                List<Budget_BP_extra> customers_extra = new List<Budget_BP_extra>();
                string[] clientes;
                if (activeuser.Roles.Contains("Sales Representative"))
                {
                   

                    int idsap = Convert.ToInt32(activeuser.IDSAP);
                    salesorders = cls_salesorders.GetSalesOrders("", 0, 0, sunday, saturday, idsap, false).data; //ordenes de la semana
                    budget = cls_salesorders.GetBudget("", idsap, sunday, saturday, false); //budget general  semanal

                    var weeklySales = salesorders.Sum(c => c.docTotal);
                    var weeklySalesCustomers = salesorders.GroupBy(c => c.cardCode).Count();

                    ViewBag.weeklysales = weeklySales;
                    ViewBag.weeklySalesCustomers = weeklySalesCustomers;
        
                    //
                    var weeklyBudget = budget.data.Sum(c => c.amount);
                    var weeklyCustomers = budget.data.GroupBy(c => c.cardCode).Count();

                    ViewBag.weeklyBudget = weeklyBudget;
                    ViewBag.weeklyCustomers = weeklyCustomers;
                    //

                    if (salesorders != null) {
                        if (salesorders.Count > 0) {
                             ordenes = salesorders.Where(c=>c.docDate==now).GroupBy(c => c.cardCode).Select(c => new Budget_BP//ordenes del dia
                             {
                                cardCode = c.Key,
                                time = c.Max(a => a.docTime.Value),
                                Sales = c.Sum(a => a.docTotal),
                                Budget = 0,
                                cardName = "",
                                Scope = 0,
                                 SlpCode = 0,
                                 SlpName = ""
                             }).ToList();
                             
                        }
                    }
           
                   

                        clientes = budget.data.Where(c=>c.date==now).Select(c => c.cardCode).Distinct().ToArray(); //clientes unicos


                        //budgetfilter = budget.data.Where(c => clientes.Contains(c.cardCode)).ToList(); //budget filtrado de general a especifico
                        var clientesbudget = clientes;//budgetfilter.Select(c => c.cardCode).Distinct().ToArray();

                        //Definimos clientes con budget y clientes sin budget

                        //var clientesfinales = salesorders.data.Where(c => clientesbudget.Contains(c.cardCode)).ToList();
                        var clientesfinalesExtra = ordenes.Where(c => !clientesbudget.Contains(c.cardCode)).ToList();

                        if (budget.data.Where(c => c.date == now).Count() > 0)
                        {
                            foreach (var item in budget.data.Where(c => c.date == now))
                            {
                                Budget_BP newcustomer = new Budget_BP();
                                newcustomer.cardCode = item.cardCode;
                                newcustomer.cardName = cls_businesspartners.GetBusinessPartner(item.cardCode,0,0,false).data.FirstOrDefault().cardName;
                                newcustomer.Budget = item.amount;
                            newcustomer.SlpCode = 0;
                            newcustomer.SlpName = "";
                                if (ordenes.Where(c => c.cardCode == item.cardCode).Count() > 0)
                                {
                                    newcustomer.time = ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.time).FirstOrDefault();
                                    newcustomer.Sales = ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.Sales).FirstOrDefault();
                                    newcustomer.Scope = (ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.Sales).FirstOrDefault() / newcustomer.Budget) * 100;
                                }
                                else {
                                    newcustomer.time = 0;
                                    newcustomer.Sales = 0;
                                    newcustomer.Scope = 0;
                                }

                          
                                customers.Add(newcustomer);
                            }
                        }

                        if (clientesfinalesExtra.Count > 0)
                        {
                


                            foreach (var item in clientesfinalesExtra)
                            {
                                Budget_BP_extra newcustomerextra = new Budget_BP_extra();
                                newcustomerextra.cardCode = item.cardCode;
                                newcustomerextra.cardName = cls_businesspartners.GetBusinessPartner(item.cardCode, 0, 0, false).data.FirstOrDefault().cardName;
                                newcustomerextra.time = item.time;
                                newcustomerextra.Sales = item.Sales;
                            newcustomerextra.SlpCode = 0;
                            newcustomerextra.SlpName = "";
                            var actualbudget = budget.data.Where(c=>c.cardCode==item.cardCode);
                                if (actualbudget != null)
                                {
                                    if (actualbudget.Count() > 0)
                                    {
                                        newcustomerextra.VisitDay = actualbudget.FirstOrDefault().dayName;
                                    }
                                    else { newcustomerextra.VisitDay = "No day assigned"; }
                                    
                                }
                                else {
                                    newcustomerextra.VisitDay = "No day assigned";
                                }
                            
                                customers_extra.Add(newcustomerextra);
                            }
                        }
           
                   
                }
                else {

                }

                ViewBag.customers = customers;
                ViewBag.extracustomers = customers_extra;

                return View();



            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
      

          

        }


        public ActionResult Dashboard_SP()
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Main";
                ViewData["Page"] = "Index";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);
                //List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                //ViewBag.notifications = lstAlerts;
                ViewBag.activeuser = activeuser;
                //FIN HEADER
                //FILTROS VARIABLES
                ViewBag.showfreezermessage = "N";


                List<SalesOrders_api> salesorders = new List<SalesOrders_api>();
                GetBudget_api budget = new GetBudget_api();
                List<Budget_api> budgetfilter = new List<Budget_api>();

                List<RepsSalesReport> repsSurveys = new List<RepsSalesReport>();

                List<int> TagIds = new List<int>();
                var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();

                //finales
                List<Budget_BP> customers = new List<Budget_BP>();
                List<Budget_BP> ordenes = new List<Budget_BP>();
                List<Budget_BP_extra> customers_extra = new List<Budget_BP_extra>();
                string[] clientes;
                string[] usids;

                if (activeuser.Roles.Contains("Sales Supervisor"))
                {

                    TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList(); //sacamos vendedores asignados

                    repsSurveys = users.Where(h => TagIds.Contains(h.ID_User) && h.IDSAP != "").Select(h => new RepsSalesReport { ID_User = h.ID_User, Name = h.Name + " " + h.Lastname, weeklySales = 0, weeklySalesCustomers = 0, weeklyBudget=0, weeklyCustomers=0, idSAP = h.IDSAP, extra = 0, idSAPint=0 }).ToList();
                    //usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                }
                else
                { //Gerencia
                    repsSurveys = users.Where(h => h.IDSAP!="").Select(h => new RepsSalesReport { ID_User = h.ID_User, Name = h.Name + " " + h.Lastname, weeklySales = 0, weeklySalesCustomers = 0, weeklyBudget = 0, weeklyCustomers = 0, idSAP = h.IDSAP, extra = 0, idSAPint=0 }).ToList();
                    //usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                }

                foreach (var itemrep in repsSurveys) {
                    int idsap = Convert.ToInt32(itemrep.idSAP);

                    salesorders = cls_salesorders.GetSalesOrders("", 0, 0, sunday, saturday, idsap, false).data; //ordenes de la semana
                    budget = cls_salesorders.GetBudget("", idsap, sunday, saturday, false); //budget general  semanal
                    itemrep.idSAPint = idsap;
                    //
                    itemrep.weeklyBudget = budget.data.Sum(c => c.amount);
                    itemrep.weeklyCustomers = budget.data.GroupBy(c => c.cardCode).Count();
                    //

                    if (salesorders != null)
                    {
                        if (salesorders.Count > 0)
                        {

                            itemrep.weeklySales = salesorders.Sum(c => c.docTotal);
                            itemrep.weeklySalesCustomers = salesorders.GroupBy(c => c.cardCode).Count();
                         

                            ordenes = salesorders.Where(c => c.docDate == now).GroupBy(c => c.cardCode).Select(c => new Budget_BP//ordenes del dia
                            {
                                cardCode = c.Key,
                                time = c.Max(a => a.docTime.Value),
                                Sales = c.Sum(a => a.docTotal),
                                Budget = 0,
                                cardName = "",
                                Scope = 0,
                                SlpCode = idsap,
                                SlpName = itemrep.Name
                            }).ToList();

                        }
                    }



                    clientes = budget.data.Where(c => c.date == now).Select(c => c.cardCode).Distinct().ToArray(); //clientes unicos


                    //budgetfilter = budget.data.Where(c => clientes.Contains(c.cardCode)).ToList(); //budget filtrado de general a especifico
                    var clientesbudget = clientes;//budgetfilter.Select(c => c.cardCode).Distinct().ToArray();

                    //Definimos clientes con budget y clientes sin budget

                    //var clientesfinales = salesorders.data.Where(c => clientesbudget.Contains(c.cardCode)).ToList();
                    var clientesfinalesExtra = ordenes.Where(c => !clientesbudget.Contains(c.cardCode)).ToList();

                    if (budget.data.Where(c => c.date == now).Count() > 0)
                    {
                        foreach (var item in budget.data.Where(c => c.date == now))
                        {
                            Budget_BP newcustomer = new Budget_BP();
                            newcustomer.cardCode = item.cardCode;
                            newcustomer.cardName = cls_businesspartners.GetBusinessPartner(item.cardCode, 0, 0, false).data.FirstOrDefault().cardName;
                            newcustomer.Budget = item.amount;
                            newcustomer.SlpName = itemrep.Name;
                            newcustomer.SlpCode = idsap;
                            if (ordenes.Where(c => c.cardCode == item.cardCode).Count() > 0)
                            {
                                newcustomer.time = ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.time).FirstOrDefault();
                                newcustomer.Sales = ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.Sales).FirstOrDefault();
                                newcustomer.Scope = (ordenes.Where(c => c.cardCode == item.cardCode).Select(c => c.Sales).FirstOrDefault() / newcustomer.Budget) * 100;
                            }
                            else
                            {
                                newcustomer.time = 0;
                                newcustomer.Sales = 0;
                                newcustomer.Scope = 0;
                            }


                            customers.Add(newcustomer);
                        }
                    }

                    if (clientesfinalesExtra.Count > 0)
                    {



                        foreach (var item in clientesfinalesExtra)
                        {
                            Budget_BP_extra newcustomerextra = new Budget_BP_extra();
                            newcustomerextra.cardCode = item.cardCode;
                            newcustomerextra.cardName = cls_businesspartners.GetBusinessPartner(item.cardCode, 0, 0, false).data.FirstOrDefault().cardName;
                            newcustomerextra.time = item.time;
                            newcustomerextra.Sales = item.Sales;
                            newcustomerextra.SlpName = itemrep.Name;
                            newcustomerextra.SlpCode = idsap;
                            var actualbudget = budget.data.Where(c => c.cardCode == item.cardCode);
                            if (actualbudget != null)
                            {
                                if (actualbudget.Count() > 0)
                                {
                                    newcustomerextra.VisitDay = actualbudget.FirstOrDefault().dayName;
                                }
                                else { newcustomerextra.VisitDay = "No day assigned"; }

                            }
                            else
                            {
                                newcustomerextra.VisitDay = "No day assigned";
                            }

                            customers_extra.Add(newcustomerextra);
                        }
                    }

                }

               



                
             

                ViewBag.customers = customers;
                ViewBag.extracustomers = customers_extra;
                ViewBag.salesrep = repsSurveys;
                return View();



            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }




        }



    }
}