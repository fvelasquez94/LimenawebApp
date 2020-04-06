using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
namespace LimenawebApp.Controllers
{
    public class InvoicesController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();

        //CLASS GENERAL
        private clsGeneral generalClass = new clsGeneral();
        // GET: Invoices
        public class Routes_calendar
        {
            public string title { get; set; }
            public string url { get; set; }
            public string start { get; set; }
            public string route_leader { get; set; }
            public string className { get; set; }
            public string driver { get; set; }
            public string truck { get; set; }
            public string departure { get; set; }
            public string amount { get; set; }
            public string orderscount { get; set; }
            public string customerscount { get; set; }
            public string isfinished { get; set; }
            public string extra { get; set; }
            public string totalEach { get; set; }
            public string totalCase { get; set; }
            public string totalPack { get; set; }
            public string totalLbs { get; set; }
            public string AVGEach { get; set; }
            public string Warehouse { get; set; }
            public string driver_WHS { get; set; }
            public string truck_WHS { get; set; }
        }
        public class orderSO
        {
            public int sort { get; set; }
            public int num { get; set; }
            public string SO { get; set; }
            public string comment { get; set; }
        }
        public ActionResult Planning_order(int id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                //Session["opensalesOrders"] = (from obj in dlipro.OpenSalesOrders select new OpenSO{ NumSO = obj.NumSO, CardCode = obj.CardCode, CustomerName = obj.CustomerName, DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed = obj.Printed }).ToList();
                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                var orders = (from a in dblim.Tb_PlanningSO where (a.ID_Route == id && a.Warehouse==company_bodega) select a).OrderBy(a => a.query3).ToList();
                ViewBag.orders = orders;
                //FIN HEADER

                ViewBag.id_route = id;

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        private List<DateTime> GetDateRange(DateTime StartingDate, DateTime EndingDate)
        {
            if (StartingDate > EndingDate)
            {
                return null;
            }
            List<DateTime> rv = new List<DateTime>();
            DateTime tmpDate = StartingDate;
            do
            {
                rv.Add(tmpDate);
                tmpDate = tmpDate.AddDays(1);
            } while (tmpDate <= EndingDate);
            return rv;
        }

        public ActionResult Planning()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;


                var company_bodega = "0";
                if (activeuser.ID_Company == 1) {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                DateTime filtrostartdate;
                DateTime filtroenddate;

                //filtros de fecha //SEMANAL
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                //var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //var saturday = sunday.AddMonths(1).AddDays(-1);
                //FILTROS**************

                filtrostartdate = sunday;
                filtroenddate = saturday;
                //FIN FILTROS*******************

                ///////////////////////////////
                List<Tb_Planning> rutaslst = new List<Tb_Planning>();
                //Nuevo filtro por bodega

                //si es limena podremos ver las rutas de otras pero minimizadas
                if (company_bodega == "01")
                {
                    rutaslst = (from a in dblim.Tb_Planning where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate && a.Warehouse == company_bodega) select a).ToList();
                    //verificamos si hay rutas maestras deKY

                    foreach (DateTime date in GetDateRange(filtrostartdate, filtroenddate))
                    {
                        var date23h = date.AddHours(23);

                        var existeMasterRoute= (from a in dblim.Tb_Planning where ((a.Departure >= date && a.Departure <=date23h) && a.Warehouse != company_bodega) select a).Take(1).ToList();
                        if (existeMasterRoute.Count > 0) {
                            rutaslst.AddRange(existeMasterRoute);
                        }
                    }
                    //
                }
                else {
                    rutaslst = (from a in dblim.Tb_Planning where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate && a.Warehouse == company_bodega) select a).ToList();
                }
                

                var rtids = rutaslst.Select(c => c.ID_Route).ToArray();
                //Cargamos todos los datos maestros a utilizar
                //Nuevo filtro por bodega
                var solist = (from j in dblim.Tb_PlanningSO where (rtids.Contains(j.ID_Route)) select new { IDinterno = j.ID_salesorder, SAPDOC = j.SAP_docnum, IDRoute=j.ID_Route, amount=j.Amount, customerName=j.Customer_name }).ToList();
                var solMaestro = solist.Select(c => c.IDinterno).ToArray();
                //Nuevo filtro por bodega(AQUI NO APLICAR)
                var detallesMaestroSo = (from f in dblim.Tb_PlanningSO_details where (solMaestro.Contains(f.ID_salesorder)) select f).ToList();

                List<Routes_calendar> rutas = new List<Routes_calendar>();

                foreach (var item in rutaslst)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_Route + " - " + item.Route_name;
                    rt.url = "";
                    rt.start = item.Departure.ToString("yyyy-MM-dd");
                    //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd");
                    rt.route_leader = item.Routeleader_name.ToUpper();
                    rt.className = ".fc-event";
                    rt.driver = item.Driver_name.ToUpper();
                    rt.driver_WHS = item.Driver_name_whs.ToUpper();
                    rt.truck = item.Truck_name;
                    rt.truck_WHS = item.Truck_name_whs;
                    rt.departure = item.Departure.ToShortTimeString();
                    rt.Warehouse = item.Warehouse;
                    try {
                        var extra = (from a in dblim.Tb_Planning_extra where (a.ID_Route == item.ID_Route) select a);
                        if (extra.Count() > 0) {
                            rt.extra = extra.Sum(x => x.Value).ToString();
                        }
                        else
                        {
                            rt.extra = "0.00";

                        }

                    }
                    catch {
                        rt.extra = "0.00";
                    }


                    //INFO UOM
                    var listafinalSO = solist.Where(c => c.IDRoute == item.ID_Route).ToList();
                    var sol = listafinalSO.Select(c => c.IDinterno).ToArray();
                    //Verificamos detalles para sacar CASE y EACH totales y luego promediar todal de EACH en base a CASES
                    var detallesSo = (from f in detallesMaestroSo where (sol.Contains(f.ID_salesorder)) select f).ToList();

                    var totalCantEach = 0;
                    var totalCantCases = 0;
                    var totalCantPack = 0;
                    var totalCantLbs = 0;
                    decimal promedioEachxCases = 0;
                    //Para calcular el promedio lo hacemos diviendo
                    try
                    {
                        if (detallesSo.Count() > 0)
                        {
                            totalCantEach = detallesSo.Where(c => c.UomCode.Contains("EACH")).Count();
                            totalCantCases = detallesSo.Where(c => c.UomCode.Contains("CASE")).Count();
                            totalCantPack = detallesSo.Where(c => c.UomCode.Contains("PACK")).Count();
                            totalCantLbs = detallesSo.Where(c => c.UomCode.Contains("LBS")).Count();

                            foreach (var soitem in listafinalSO)
                            {
                                //devolvemos ID externo
                                var docnum = Convert.ToInt32(soitem.SAPDOC);

                                //Devolvemos todos los detalles(itemcode) de esa SO
                                var itemscode = detallesSo.Where(c => c.ID_salesorder == soitem.IDinterno).Select(c => c.ItemCode).ToArray();

                                //Buscamos en la vista creada 9/24/2019
                                var sumatotal = dlipro.PlanningUoMInfo.Where(a => itemscode.Contains(a.ItemCode) && a.DocNum == docnum && a.Quantity >0 && !a.unitMsr.Contains("LBS")).Sum(c => c.TotalCases);

                                if (sumatotal > 0 && sumatotal != null)
                                {
                                    promedioEachxCases += Convert.ToDecimal(sumatotal);
                                }

                            }
                        }
                        else
                        {
                            totalCantEach = 0;
                            totalCantCases = 0;
                            totalCantPack = 0;
                            totalCantLbs = 0;
                            promedioEachxCases = 0;

                        }
                    }
                    catch
                    {
                        totalCantEach = 0;
                        totalCantCases = 0;
                        totalCantPack = 0;
                        totalCantLbs = 0;
                        promedioEachxCases = 0;
                    }
                    ///

                    rt.totalEach = totalCantEach.ToString();
                    rt.totalCase = totalCantCases.ToString();
                    rt.totalPack = totalCantPack.ToString();
                    rt.totalLbs = totalCantLbs.ToString();
                    rt.AVGEach = promedioEachxCases.ToString();




                    if (item.isfinished == true) { rt.isfinished = "Y"; } else { rt.isfinished = "N"; }

                    var sum = (from e in solist where (e.IDRoute == item.ID_Route) select e);
                    if (sum != null)
                    {
                        try
                        {
                            rt.amount = sum.Select(c => c.amount).Sum().ToString();
                        }
                        catch {
                            rt.amount = "0.0";
                        }

                        rt.customerscount = sum.Select(c => c.customerName).Distinct().Count().ToString();
                        rt.orderscount = sum.Count().ToString();
                    }
                    else
                    {
                        rt.amount = "0.0";
                        rt.customerscount = "";
                        rt.orderscount = "";
                    }

                    rutas.Add(rt);
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutas.ToArray());
                ViewBag.calroutes = result;
                ///

                //SELECTS

                var drivers = dlipro.C_DRIVERS.Where(a=>a.U_Whs==company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListDrivers = new ArrayList();
                myArrListDrivers.AddRange((from p in drivers
                                           select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper().Replace("'", ""),
                                           }).ToList());

                ViewBag.drivers = JsonConvert.SerializeObject(myArrListDrivers);



                //DRIVERS OTRAS BODEGAS
                var driversOTROS = dlipro.C_DRIVERS.Where(a => a.U_Whs != company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListDriversOTROS = new ArrayList();
                myArrListDriversOTROS.AddRange((from p in driversOTROS
                                                select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper().Replace("'", ""),
                                           }).ToList());

                ViewBag.driversOtros = JsonConvert.SerializeObject(myArrListDriversOTROS);


                //LISTADO DE Routes Leader
                var routeleader = dlipro.C_HELPERS.Where(a => a.U_Whs == company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListrouteleader = new ArrayList();
                myArrListrouteleader.AddRange((from p in routeleader
                                               select new
                                               {
                                                   id = p.Code,
                                                   text = p.Name.ToUpper().Replace("'", ""),
                                               }).ToList());

                ViewBag.routeleaders = JsonConvert.SerializeObject(myArrListrouteleader);
                //LISTADO DE Trucks
                var trucks = dlipro.C_TRUCKS.Where(a => a.U_Whs == company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListtruck = new ArrayList();
                myArrListtruck.AddRange((from p in trucks
                                         select new
                                         {
                                             id = p.Code,
                                             text = p.Name.ToUpper().Replace("'", ""),
                                         }).ToList());

                ViewBag.trucks = JsonConvert.SerializeObject(myArrListtruck);

                var trucksOTRASBODEGAS = dlipro.C_TRUCKS.Where(a => a.U_Whs != company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListtruckOTRAS = new ArrayList();
                myArrListtruckOTRAS.AddRange((from p in trucksOTRASBODEGAS
                                         select new
                                         {
                                             id = p.Code,
                                             text = p.Name.ToUpper().Replace("'", ""),
                                         }).ToList());

                ViewBag.trucksOtros = JsonConvert.SerializeObject(myArrListtruckOTRAS);

                //LISTADO DE Rutas
                var mainroutes = dlipro.C_DROUTE.Where(a => a.U_Whs == company_bodega).ToList();
                //Convertimos la lista a array
                ArrayList myArrListmainroutes = new ArrayList();
                myArrListmainroutes.AddRange((from p in mainroutes
                                              select new
                                              {
                                                  id = p.Code,
                                                  text = p.Name
                                              }).ToList());

                ViewBag.mainroutes = JsonConvert.SerializeObject(myArrListmainroutes);

                //Session["opensalesOrders"] = (from obj in dlipro.OpenSalesOrders select new OpenSO{ NumSO = obj.NumSO, CardCode = obj.CardCode, CustomerName = obj.CustomerName, DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed = obj.Printed }).ToList();
                ViewBag.warehousesel = company_bodega;

                //FIN HEADER
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public class OpenSO
        {
            public int NumSO { get; set; }
            public string CardCode { get; set; }
            public string CustomerName { get; set; }
            public string DeliveryRoute { get; set; }
            public string SalesPerson { get; set; }
            public DateTime? SODate { get; set; }
            public decimal? TotalSO { get; set; }
            public decimal? OpenAmount { get; set; }
            public string Remarks { get; set; }
            public string Printed { get; set; }

        }
        public class MyObj_formtemplate {
            public string NumSO { get; set; }
        }

        public ActionResult Print_OpenSalesOrders(List<MyObj_formtemplate> objects)
        {
            try {

                List<string> list = new List<string>();
                foreach (var item in objects)
                {
                    var idact = item.NumSO.Substring(4);
                    list.Add(idact);
                }
                List<Tb_planning_print> lsttosave = new List<Tb_planning_print>();
                foreach (var save in list) {
                    Tb_planning_print print = new Tb_planning_print();

                    print.IsRoute = false;
                    print.Printed = false;
                    print.Doc_key = save;
                    print.Module = "OpenSalesOrders";
                    lsttosave.Add(print);                  

                }
                dblim.BulkInsert(lsttosave);

                return Json("success", JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult OpenSalesOrders(string fstartd, string fendd)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operation";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER
                DateTime filtrostartdate;
                DateTime filtroenddate;

                //filtros de fecha //SEMANAL
                //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                //var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                //var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //var saturday = sunday.AddMonths(1).AddDays(-1);
                //filtros de fecha (DIARIO)
                //var sunday = DateTime.Today;
                //var saturday = sunday.AddDays(1);
                //filtros de fecha (ANUAL)
                int year = DateTime.Now.Year;
                var sunday = new DateTime(year, 1, 1);
                var saturday = new DateTime(year, 12, 31);
                //FILTROS**************

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }
                //if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = DateTime.ParseExact(fstartd, "MM/dd/yyyy", CultureInfo.InvariantCulture); }
                //if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = DateTime.ParseExact(fendd, "MM/dd/yyyy", CultureInfo.InvariantCulture).AddHours(23).AddMinutes(59); }
                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                //FIN FILTROS*******************


                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                IEnumerable<OpenSalesOrders> SalesOrder;
                //SalesOrder = (from b in dlipro.OpenSalesOrders select b).Take(10);
                SalesOrder = (from b in dlipro.OpenSalesOrders where (b.SODate >= filtrostartdate && b.SODate <= filtroenddate) select b);

                return View(SalesOrder);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }



        public ActionResult QualityControl_planning(string fstartd, string fendd)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operation";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER
                DateTime filtrostartdate;
                DateTime filtroenddate;

                //filtros de fecha //SEMANAL
                //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                //var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                //var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //var saturday = sunday.AddMonths(1).AddDays(-1);
                //filtros de fecha (DIARIO)
                var sunday = DateTime.Today;
                var saturday = sunday.AddDays(1).AddHours(20);
                //FILTROS**************

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                //FIN FILTROS*******************

                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                List<QualityControl_SO> lstPlanning = new List<QualityControl_SO>();


                if (company_bodega == "01")
                {
                    lstPlanning = (from a in dblim.Tb_Planning
                                   where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate)
                                   select new QualityControl_SO
                                   {
                                       ID_Route = a.ID_Route,
                                       Route_name = a.Route_name,
                                       ID_driver = a.ID_driver,
                                       Driver_name = a.Driver_name,
                                       ID_routeleader = a.ID_routeleader,
                                       Routeleader_name = a.Routeleader_name,
                                       ID_truck = a.ID_truck,
                                       Truck_name = a.Truck_name,
                                       Departure = a.Departure,
                                       isfinished = a.isfinished,
                                       ID_SAPRoute = a.ID_SAPRoute,
                                       query1 = a.query1,
                                       query2 = a.query2,
                                       query3 = a.query3,
                                       query4 = "",
                                       query5 = "",
                                       query6 = 0,
                                       Date = a.Date,
                                       Invoiced = a.Invoiced,
                                       DateCheckIn = a.DateCheckIn,
                                       DateCheckOut = a.DateCheckOut,
                                       ID_userValidate = a.ID_userValidate,
                                       Warehouse = a.Warehouse,
                                       transferred = 0
                                   }).ToList();
                }
                else {
                    lstPlanning = (from a in dblim.Tb_Planning
                                   where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate && a.Warehouse == company_bodega)
                                   select new QualityControl_SO
                                   {
                                       ID_Route = a.ID_Route,
                                       Route_name = a.Route_name,
                                       ID_driver = a.ID_driver,
                                       Driver_name = a.Driver_name,
                                       ID_routeleader = a.ID_routeleader,
                                       Routeleader_name = a.Routeleader_name,
                                       ID_truck = a.ID_truck,
                                       Truck_name = a.Truck_name,
                                       Departure = a.Departure,
                                       isfinished = a.isfinished,
                                       ID_SAPRoute = a.ID_SAPRoute,
                                       query1 = a.query1,
                                       query2 = a.query2,
                                       query3 = a.query3,
                                       query4 = "",
                                       query5 = "",
                                       query6 = 0,
                                       Date = a.Date,
                                       Invoiced = a.Invoiced,
                                       DateCheckIn = a.DateCheckIn,
                                       DateCheckOut = a.DateCheckOut,
                                       ID_userValidate = a.ID_userValidate,
                                       Warehouse = a.Warehouse,        
                                       transferred = 0
                                   }).ToList();
                }




                var ArrayPlanning = lstPlanning.Select(a => a.ID_Route).ToArray();

                List<Tb_PlanningSO> SalesOrder = new List<Tb_PlanningSO>();


                    SalesOrder = (from b in dblim.Tb_PlanningSO where (ArrayPlanning.Contains(b.ID_Route)) select b).ToList();




                //SalesOrder = (from b in dblim.Tb_PlanningSO where (ArrayPlanning.Contains(b.ID_Route)) select b).ToList();

                //ESTADISTICA DE RUTAS POR ESTADO DE VISITAS
                decimal totalRutas = lstPlanning.Count();
                decimal totalRutas_bodega = lstPlanning.Count();
                foreach (var rutait in lstPlanning)
                {
                    var arrso = SalesOrder.Where(c=> c.ID_Route == rutait.ID_Route).Select(c => c.ID_salesorder).ToArray();
                    var count = SalesOrder.Where(c=> c.ID_Route == rutait.ID_Route).Select(c => c).Count();
                    var allvalidated = (from f in SalesOrder where (f.isfinished == true && f.ID_Route == rutait.ID_Route) select f).Count();
                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceled = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && e.isvalidated==true && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.QC_count == e.QC_totalCount) select e).Count();
                    //DATOS EN OTRA BODEGA
                    decimal finishedorCanceled_bodega = 0;

                    var existendeOtraBodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.QC_count != e.QC_totalCount && e.Warehouse != company_bodega) select e).Count();
                    ViewBag.existendeOtraBodega = existendeOtraBodega;

                    var existemibodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && !e.type.Contains("I") && e.Warehouse == company_bodega) select e).Count();


                    if (company_bodega == "01")
                    {
                        finishedorCanceled_bodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && e.isvalidated == true && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.Warehouse == company_bodega && e.QC_totalCount != e.QC_count) select e).Count();
                    }
                    else {
                        finishedorCanceled_bodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && e.isvalidated == true && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.Warehouse != company_bodega && e.QC_totalCount != e.QC_count) select e).Count();
                    }

                    int trans = 0;
                    //sabes si ya estan transferidas
                    if (company_bodega == "01")
                    {
                        trans = SalesOrder.Where(a => a.Warehouse != company_bodega && a.Transferred ==1 && a.ID_Route == rutait.ID_Route).Count();

                        if (trans > 0) {
                            rutait.transferred = 1;
                        }
                        trans = 0;
                        trans = SalesOrder.Where(a => a.Warehouse != company_bodega && a.Transferred == 2 && a.ID_Route == rutait.ID_Route).Count();

                        if (trans > 0)
                        {
                            rutait.transferred = 2;
                        }
                    }

                    totalRutas = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.QC_count == e.QC_totalCount) select e).Count();
                    if (company_bodega == "01")
                    {
                        totalRutas_bodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.Warehouse == company_bodega && e.QC_totalCount != e.QC_count) select e).Count();
                    }
                    else {
                        totalRutas_bodega = (from e in dblim.Tb_PlanningSO_details where (arrso.Contains(e.ID_salesorder) && !e.query1.Contains("DEL") && e.Quantity > 0 && !e.type.Contains("I") && e.Warehouse != company_bodega && e.QC_totalCount != e.QC_count) select e).Count();
                    }
                        

                    if (totalRutas != 0)
                    {
                       if (finishedorCanceled != 0)
                        {

                            rutait.query1 = (((Convert.ToDecimal(finishedorCanceled) / totalRutas) * 100)).ToString();
                       
                        }

                        else
                        {
                           
                            rutait.query1 = (Convert.ToDecimal(0)).ToString();
                        }
                        rutait.query2 = "(" + finishedorCanceled + " / " + totalRutas + ")";

                    }
                    else
                    {
                        rutait.query1 = "0";
                        rutait.query2 = "(0 / 0)";
                    }


                    if (totalRutas_bodega != 0)
                    {
                        if (finishedorCanceled_bodega != 0)
                        {

                            rutait.query4 = (((Convert.ToDecimal(finishedorCanceled_bodega) / totalRutas_bodega) * 100)).ToString();

                        }

                        else
                        {

                            rutait.query4 = (Convert.ToDecimal(0)).ToString();
                        }
                        rutait.query5 = "(" + finishedorCanceled_bodega + " / " + totalRutas_bodega + ")";
                        if (company_bodega == "01")
                        {
                            rutait.query6 = existemibodega;
                        }
                        else {
                            rutait.query6 = totalRutas_bodega;
                        }
                     
                    }
                    else
                    {
                        rutait.query4 = "0";
                        rutait.query5 = "(0 / 0)";
                        if (company_bodega == "01")
                        {
                            rutait.query6 = existemibodega;
                        }
                        else
                        {
                            rutait.query6 = 0;
                        }
                    }


                    if (count == allvalidated && totalRutas_bodega==0) {
                        rutait.query3 = 1;
                    }
                        
                   
                }
                //Verificamos si rutas estan finalizadas en bodega distinta(eliminado)
                //Se verificara si todas las rutas cumplen el porcentaje de finalizacion
                var mostrarbilloflading = 0;
                var rutas="";
                if (company_bodega == "01") {
                    var lstotrabod = lstPlanning.Where(a => a.Warehouse != company_bodega && a.transferred==0).ToList();
                    rutas = String.Join(",", lstotrabod.Select(o => o.ID_Route.ToString()).ToArray());

                    foreach (var subruta in lstotrabod) {
                        mostrarbilloflading = 0;
                        if (Convert.ToDecimal(subruta.query4) >= 100)
                        {
                            mostrarbilloflading = 1;
                        }
        
                    }
                }
                ViewBag.rutasids = rutas;
                ViewBag.mostrarBOF = mostrarbilloflading;
                //ViewBag.lstPlanning = lstPlanning;
                ViewBag.company = company_bodega;
                return View(lstPlanning);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public class ConsolidatedChild
        {
            public int ID_salesOrder { get; set; }
            public string ID_storage { get; set; }
            public string Storage { get; set; }
            public string data1 { get; set; }
            public string data2 { get; set; }
            public string Picker { get; set; }
            public int delete { get; set; }

        }

        public ActionResult Print_route(int id)
        {
            try
            {
                Tb_planning_print print = new Tb_planning_print();

                print.IsRoute = true;
                print.Printed = false;
                print.Doc_key = id.ToString();
                print.Module = "QualityControl";
                dblim.Tb_planning_print.Add(print);
                dblim.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

            //return RedirectToAction("QualityControl_planning","Invoices");


        }

        public ActionResult Print_so(int id, string docentry )
        {
            try {
                Tb_planning_print print = new Tb_planning_print();

                print.IsRoute = false;
                print.Printed = false;
                print.Doc_key = docentry;
                print.Module = "QualityControl";

                dblim.Tb_planning_print.Add(print);
                dblim.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }



        }


        public ActionResult QualityControl(int id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operation";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER

                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                List<Tb_PlanningSO> lstSalesOrders = new List<Tb_PlanningSO>();

                if (company_bodega == "01")
                {
                    lstSalesOrders = (from a in dblim.Tb_PlanningSO where (a.ID_Route == id) select a).OrderBy(a => a.Customer_name).ThenBy(a => a.ID_customer).ToList();
                }
                else {
                    lstSalesOrders = (from a in dblim.Tb_PlanningSO where (a.ID_Route == id && a.Warehouse == company_bodega) select a).OrderBy(a => a.Customer_name).ThenBy(a => a.ID_customer).ToList();
                }

                




                var Arry = lstSalesOrders.Where(a => a.Warehouse == company_bodega).Select(a => a.ID_salesorder).ToArray();
               

                List<Tb_PlanningSO_details> lstDetails = new List<Tb_PlanningSO_details>();

                if (company_bodega == "01")
                {
                    var Arry_Bodega = lstSalesOrders.Where(a => a.Warehouse != company_bodega).Select(a => a.ID_salesorder).ToArray();

                    lstDetails = (from b in dblim.Tb_PlanningSO_details where (Arry.Contains(b.ID_salesorder) && b.Quantity > 0 && !b.type.Contains("I")) select b).ToList();

                    var detallesdifbod = (from b in dblim.Tb_PlanningSO_details where (Arry_Bodega.Contains(b.ID_salesorder) && b.Quantity > 0 && !b.type.Contains("I") && b.Warehouse == company_bodega && b.QC_totalCount != b.QC_count) select b).ToList();

                    if (detallesdifbod.Count > 0) {
                        lstDetails.AddRange(detallesdifbod);
                    }
                }
                else
                {
                    lstDetails = (from b in dblim.Tb_PlanningSO_details where (Arry.Contains(b.ID_salesorder) && b.Quantity > 0 && !b.type.Contains("I") && b.Warehouse == company_bodega) select b).ToList();


                }




                //ESTADISTICA DE SALES ORDERS POR ESTADO DE DETALLES
                decimal totalRutas = lstDetails.Count();
                foreach (var rutait in lstSalesOrders)
                {

                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceled = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesorder && !e.query1.Contains("DEL")) select e).Count();

                    totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesorder && !e.query1.Contains("DEL")) select e).Count();

                    if (totalRutas != 0)
                    {
                        if (finishedorCanceled != 0)
                        {

                            rutait.query1 = (((Convert.ToDecimal(finishedorCanceled) / totalRutas) * 100)).ToString();

                        }

                        else
                        {

                            rutait.query1 = (Convert.ToDecimal(0)).ToString();
                        }
                        rutait.query2 = "(" + finishedorCanceled + " / " + totalRutas + ")";

                    }
                    else
                    {
                        rutait.query1 = "0";
                        rutait.query2 = "(0/ 0)";
                    }
                }
                var consolidatedChildren = (from c in lstDetails group c by new { c.ID_salesorder, c.ID_storagetype, c.Storage_type } into gcs

                                            select new ConsolidatedChild()
                                            {
                                                ID_salesOrder = gcs.Key.ID_salesorder,
                                                ID_storage = gcs.Key.ID_storagetype,
                                                Storage = gcs.Key.Storage_type,
                                                data1 = "",
                                                data2 = "",
                                                Picker = "",
                                                delete = 0
                                           }).ToList();



                var pickers = dlipro.C_PICKERS.Where(a => a.U_Whs == company_bodega && a.U_Active=="Y").ToList();
                //Convertimos la lista a array
                ArrayList myArrListPickers = new ArrayList();
                myArrListPickers.AddRange((from p in pickers
                                           select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper().Replace("'", ""),
                                           }).ToList());

                ViewBag.pickers = JsonConvert.SerializeObject(myArrListPickers);

                //ESTADISTICA DE DETALLE STORAGE TYPE EN SALES ORDERS
                decimal totalDet = consolidatedChildren.Count();
                var flag = 0;
                int salesant = 0;
                List<ConsolidatedChild> arrytodel = new List<ConsolidatedChild>();
                foreach (var rutait in consolidatedChildren)
                {
                    if (rutait.ID_salesOrder == 302) {
                        var h = 0;
                    }
                    if (salesant != rutait.ID_salesOrder)

                    {
                        flag = 0;
                    }
                    //NUEVO PARA UNIR COOLER Y FREEZER
                    if (rutait.ID_storage == "COOLER" || rutait.ID_storage == "FREEZER")
                    {
                        if (salesant == rutait.ID_salesOrder && flag == 1)

                        {
                            flag = 1;
                        }
                        else {
                            flag = 0;
                        }
                            if (flag == 0)
                            {
                                //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                                decimal finishedorCanceled2 = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).Count();

                                totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).Count();

                                if (totalRutas != 0)
                                {
                                    if (finishedorCanceled2 != 0)
                                    {

                                        rutait.data1 = (((Convert.ToDecimal(finishedorCanceled2) / totalRutas) * 100)).ToString();

                                    }

                                    else
                                    {

                                        rutait.data1 = (Convert.ToDecimal(0)).ToString();
                                    }
                                    rutait.data2 = "(" + finishedorCanceled2 + " / " + totalRutas + ")";

                                    var dtl = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).FirstOrDefault();
                                    if (dtl != null)
                                    {
                                        rutait.Picker = dtl.Picker_name;
                                    }
                                }
                                else
                                {
                                    rutait.data1 = "0";
                                }

                                flag = 1;
                            arrytodel.Add(rutait);
                        }
                            else
                            {
                                flag = 0;
                            }
                         
                        
                    }
                    else {
                        //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                        decimal finishedorCanceled2 = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).Count();

                        totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).Count();

                        if (totalRutas != 0)
                        {
                            if (finishedorCanceled2 != 0)
                            {

                                rutait.data1 = (((Convert.ToDecimal(finishedorCanceled2) / totalRutas) * 100)).ToString();

                            }

                            else
                            {

                                rutait.data1 = (Convert.ToDecimal(0)).ToString();
                            }
                            rutait.data2 = "(" + finishedorCanceled2 + " / " + totalRutas + ")";

                            var dtl = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).FirstOrDefault();
                            if (dtl != null)
                            {
                                rutait.Picker = dtl.Picker_name;
                            }
                        }
                        else
                        {
                            rutait.data1 = "0";
                        }
                        arrytodel.Add(rutait);

                    }

                    salesant = rutait.ID_salesOrder;
                }

                ViewBag.lstSO = lstSalesOrders;

                


                ViewBag.lstSO_children = arrytodel;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }



        public ActionResult QualityControl_storagetype(int id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operation";
                ViewData["Page"] = "Invoices";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "drop3";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER

                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }

                List<Tb_PlanningSO> lstSalesOrders = new List<Tb_PlanningSO>();


                    lstSalesOrders = (from a in dblim.Tb_PlanningSO where (a.ID_Route == id && a.QC_count >1) select a).OrderBy(a => a.Customer_name).ThenBy(a => a.ID_customer).ToList();
                






                var Arry = lstSalesOrders.Select(a => a.ID_salesorder).ToArray();


                List<Tb_PlanningSO_details> lstDetails = new List<Tb_PlanningSO_details>();


                    lstDetails = (from b in dblim.Tb_PlanningSO_details where (Arry.Contains(b.ID_salesorder) && b.Quantity > 0 && !b.type.Contains("I") && b.Warehouse != company_bodega) select b).ToList();
                




                //ESTADISTICA DE SALES ORDERS POR ESTADO DE DETALLES
                decimal totalRutas = lstDetails.Count();
                foreach (var rutait in lstSalesOrders)
                {

                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceled = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesorder && !e.query1.Contains("DEL")) select e).Count();

                    totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesorder && !e.query1.Contains("DEL")) select e).Count();

                    if (totalRutas != 0)
                    {
                        if (finishedorCanceled != 0)
                        {

                            rutait.query1 = (((Convert.ToDecimal(finishedorCanceled) / totalRutas) * 100)).ToString();

                        }

                        else
                        {

                            rutait.query1 = (Convert.ToDecimal(0)).ToString();
                        }
                        rutait.query2 = "(" + finishedorCanceled + " / " + totalRutas + ")";

                    }
                    else
                    {
                        rutait.query1 = "0";
                        rutait.query2 = "(0/ 0)";
                    }
                }
                var consolidatedChildren = (from c in lstDetails
                                            group c by new { c.ID_salesorder, c.ID_storagetype, c.Storage_type } into gcs

                                            select new ConsolidatedChild()
                                            {
                                                ID_salesOrder = gcs.Key.ID_salesorder,
                                                ID_storage = gcs.Key.ID_storagetype,
                                                Storage = gcs.Key.Storage_type,
                                                data1 = "",
                                                data2 = "",
                                                Picker = "",
                                                delete=0
                                            }).ToList();



                var pickers = dlipro.C_PICKERS.Where(a => a.U_Whs == company_bodega && a.U_Active == "Y").ToList();
                //Convertimos la lista a array
                ArrayList myArrListPickers = new ArrayList();
                myArrListPickers.AddRange((from p in pickers
                                           select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper().Replace("'", ""),
                                           }).ToList());

                ViewBag.pickers = JsonConvert.SerializeObject(myArrListPickers);

                //ESTADISTICA DE DETALLE STORAGE TYPE EN SALES ORDERS
                decimal totalDet = consolidatedChildren.Count();
                var flag = 0;
                int salesant = 0;
                List<ConsolidatedChild> arrytodel = new List<ConsolidatedChild>();
                foreach (var rutait in consolidatedChildren)
                {
                    if (rutait.ID_salesOrder == 302)
                    {
                        var h = 0;
                    }
                    if (salesant != rutait.ID_salesOrder)

                    {
                        flag = 0;
                    }
                    //NUEVO PARA UNIR COOLER Y FREEZER
                    if (rutait.ID_storage == "COOLER" || rutait.ID_storage == "FREEZER")
                    {
                        if (salesant == rutait.ID_salesOrder && flag == 1)

                        {
                            flag = 1;
                        }
                        else
                        {
                            flag = 0;
                        }
                        if (flag == 0)
                        {
                            //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                            decimal finishedorCanceled2 = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).Count();

                            totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).Count();
                            var eliminados = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER") && e.Transferred==2) select e).Count();

                            if (eliminados > 0) {
                                rutait.delete = 1;
                            }

                            if (totalRutas != 0)
                            {
                                if (finishedorCanceled2 != 0)
                                {

                                    rutait.data1 = (((Convert.ToDecimal(finishedorCanceled2) / totalRutas) * 100)).ToString();

                                }

                                else
                                {

                                    rutait.data1 = (Convert.ToDecimal(0)).ToString();
                                }
                                rutait.data2 = "(" + finishedorCanceled2 + " / " + totalRutas + ")";

                                var dtl = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && (e.ID_storagetype == "COOLER" || e.ID_storagetype == "FREEZER")) select e).FirstOrDefault();
                                if (dtl != null)
                                {
                                    rutait.Picker = dtl.Picker_nameWHS;
                                }
                            }
                            else
                            {
                                rutait.data1 = "0";
                            }

                            flag = 1;
                            arrytodel.Add(rutait);
                        }
                        else
                        {
                            flag = 0;
                        }


                    }
                    else
                    {
                        //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                        decimal finishedorCanceled2 = (from e in lstDetails where ((e.isvalidated == true) && e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).Count();

                        totalRutas = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).Count();
                        var eliminados = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage && e.Transferred == 2) select e).Count();

                        if (eliminados > 0)
                        {
                            rutait.delete = 1;
                        }
                        if (totalRutas != 0)
                        {
                            if (finishedorCanceled2 != 0)
                            {

                                rutait.data1 = (((Convert.ToDecimal(finishedorCanceled2) / totalRutas) * 100)).ToString();

                            }

                            else
                            {

                                rutait.data1 = (Convert.ToDecimal(0)).ToString();
                            }
                            rutait.data2 = "(" + finishedorCanceled2 + " / " + totalRutas + ")";

                            var dtl = (from e in lstDetails where (e.ID_salesorder == rutait.ID_salesOrder && !e.query1.Contains("DEL") && e.ID_storagetype == rutait.ID_storage) select e).FirstOrDefault();
                            if (dtl != null)
                            {
                                rutait.Picker = dtl.Picker_nameWHS;
                            }
                        }
                        else
                        {
                            rutait.data1 = "0";
                        }
                        arrytodel.Add(rutait);

                    }

                    salesant = rutait.ID_salesOrder;
                }

                ViewBag.lstSO = lstSalesOrders;




                ViewBag.lstSO_children = arrytodel;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public class RoutesInfo {
            public int ID_Route { get; set; }
            public string Route_name { get; set; }
            public string ID_driver { get; set; }
            public string Driver_name { get; set; }
            public string ID_routeleader { get; set; }
            public string Routeleader_name { get; set; }
            public string ID_truck { get; set; }
            public string Truck_name { get; set; }
            public System.DateTime Departure { get; set; }
            public bool isfinished { get; set; }
            public string ID_SAPRoute { get; set; }
            public string query1 { get; set; }
            public string query2 { get; set; }
            public decimal query3 { get; set; }
            public System.DateTime Date { get; set; }
            public System.DateTime DatetoInvoice { get; set; }
            public bool Invoiced { get; set; }
        
        }

        public ActionResult GetRouteData(string id)
        {
            try
            {
                int idr = Convert.ToInt32(id);
                var lstRoutes = (from a in dblim.Tb_Planning
                                 where (a.ID_Route == idr)
                                 select new RoutesInfo
                                 {
                                     ID_Route = a.ID_Route,
                                     Route_name = a.Route_name,
                                     ID_driver = a.ID_driver,
                                     Driver_name = a.Driver_name,
                                     ID_routeleader = a.ID_routeleader,
                                     Routeleader_name = a.Routeleader_name,
                                     ID_truck = a.ID_truck,
                                     Truck_name = a.Truck_name,
                                     Departure = a.Departure,
                                     isfinished = a.isfinished,
                                     ID_SAPRoute = a.ID_SAPRoute,
                                     query1 = a.query1,
                                     query2 = a.query2,
                                     query3 = a.query3,
                                     Date = a.Date,
                                     Invoiced = a.Invoiced,
                                     DatetoInvoice = a.DatetoInvoice
                               
                                 });


          

                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    string result = javaScriptSerializer.Serialize(lstRoutes.ToList());
                    return Json(result, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetExtraData(string id)
        {
            try
            {
                int idr = Convert.ToInt32(id);
                var lstExtra = (from a in dblim.Tb_Planning_extra
                                where (a.ID_Route == idr)
                                select a);




                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(lstExtra.ToList());
                return Json(result, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetEvents(DateTime startf, DateTime endf)
        {
            try
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }
                List<Tb_Planning> lstRoutes = new List<Tb_Planning>();
                if (company_bodega == "01")
                {
                    lstRoutes = dblim.Tb_Planning.Where(dc => dc.Departure >= startf && dc.Departure <= endf).OrderByDescending(dc => dc.ID_Route).ToList();
                    foreach (DateTime date in GetDateRange(startf, endf))
                    {
                        var existeMasterRoute = (from a in dblim.Tb_Planning where (a.Departure == date && a.Warehouse != company_bodega) select a).Take(1).ToList();
                        if (existeMasterRoute.Count > 0)
                        {
                            lstRoutes.AddRange(existeMasterRoute);
                        }
                    }
                }
                else {
                    lstRoutes = dblim.Tb_Planning.Where(dc => dc.Departure >= startf && dc.Departure <= endf && dc.Warehouse == company_bodega).OrderByDescending(dc => dc.ID_Route).ToList();
                }
                

                List<Routes_calendar> rutaslst = new List<Routes_calendar>();



                var rtids = lstRoutes.Select(c => c.ID_Route).ToArray();
                //Cargamos todos los datos maestros a utilizar
                var solist = (from j in dblim.Tb_PlanningSO where (rtids.Contains(j.ID_Route)) select new { IDinterno = j.ID_salesorder, SAPDOC = j.SAP_docnum, IDRoute = j.ID_Route, amount = j.Amount, customerName = j.Customer_name }).ToList();
                var solMaestro = solist.Select(c => c.IDinterno).ToArray();
                var detallesMaestroSo = (from f in dblim.Tb_PlanningSO_details where (solMaestro.Contains(f.ID_salesorder)) select f).ToList();

                List<Routes_calendar> rutas = new List<Routes_calendar>();




                    foreach (var item in lstRoutes)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_Route + " - " + item.Route_name;
                    rt.url = "";
                    rt.start = item.Departure.ToString("yyyy-MM-dd");
                    //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd");
                    rt.route_leader = item.Routeleader_name.ToUpper();
                    rt.className = ".fc-event";
                    rt.driver = item.Driver_name.ToUpper();
                    rt.truck = item.Truck_name;
                    rt.departure = item.Departure.ToShortTimeString();
                    rt.Warehouse = item.Warehouse;
                    if (item.isfinished == true) { rt.isfinished = "Y"; } else { rt.isfinished = "N"; }



                    try
                    {
                        var extra = (from a in dblim.Tb_Planning_extra where (a.ID_Route == item.ID_Route) select a);
                        if (extra.Count() > 0)
                        {
                            rt.extra = extra.Sum(x => x.Value).ToString();
                        }
                        else
                        {
                            rt.extra = "0.00";

                        }

                    }
                    catch
                    {
                        rt.extra = "0.00";
                    }


                    //INFO UOM
                    var listafinalSO = solist.Where(c => c.IDRoute == item.ID_Route).ToList();
                    var sol = listafinalSO.Select(c => c.IDinterno).ToArray();
                    //Verificamos detalles para sacar CASE y EACH totales y luego promediar todal de EACH en base a CASES
                    var detallesSo = (from f in detallesMaestroSo where (sol.Contains(f.ID_salesorder)) select f).ToList();

                    var totalCantEach = 0;
                    var totalCantCases = 0;
                    var totalCantPack = 0;
                    var totalCantLbs = 0;
                    decimal promedioEachxCases = 0;
                    //Para calcular el promedio lo hacemos diviendo
                    try
                    {
                        if (detallesSo.Count() > 0)
                        {
                            totalCantEach = detallesSo.Where(c => c.UomCode.Contains("EACH")).Count();
                            totalCantCases = detallesSo.Where(c => c.UomCode.Contains("CASE")).Count();
                            totalCantPack = detallesSo.Where(c => c.UomCode.Contains("PACK")).Count();
                            totalCantLbs = detallesSo.Where(c => c.UomCode.Contains("LBS")).Count();

                            foreach (var soitem in listafinalSO)
                            {
                                //devolvemos ID externo
                                var docnum = Convert.ToInt32(soitem.SAPDOC);

                                //Devolvemos todos los detalles(itemcode) de esa SO
                                var itemscode = detallesSo.Where(c => c.ID_salesorder == soitem.IDinterno).Select(c => c.ItemCode).ToArray();

                                //Buscamos en la vista creada 9/24/2019
                                var sumatotal = dlipro.PlanningUoMInfo.Where(a => itemscode.Contains(a.ItemCode) && a.DocNum == docnum && a.Quantity > 0 && !a.unitMsr.Contains("LBS")).Sum(c => c.TotalCases);

                                if (sumatotal > 0 && sumatotal != null) {
                                    promedioEachxCases += Convert.ToDecimal(sumatotal);
                                }
                                

                            }
                        }
                        else
                        {
                            totalCantEach = 0;
                            totalCantCases = 0;
                            totalCantPack = 0;
                            totalCantLbs = 0;
                            promedioEachxCases = 0;

                        }
                    }
                    catch
                    {
                        totalCantEach = 0;
                        totalCantCases = 0;
                        totalCantPack = 0;
                        totalCantLbs = 0;
                        promedioEachxCases = 0;
                    }
                    ///

                    rt.totalEach = totalCantEach.ToString();
                    rt.totalCase = totalCantCases.ToString();
                    rt.totalPack = totalCantPack.ToString();
                    rt.totalLbs = totalCantLbs.ToString();
                    rt.AVGEach = Math.Round(promedioEachxCases, 2, MidpointRounding.ToEven).ToString();


                    var sum = (from e in solist where (e.IDRoute == item.ID_Route) select e);
                    if (sum != null && sum.Count() >0)
                    {
                        rt.amount = sum.Select(c => c.amount).Sum().ToString();
                        rt.customerscount = sum.Select(c => c.customerName).Distinct().Count().ToString();
                        rt.orderscount = sum.Count().ToString();
                    }
                    else {
                        rt.amount = "0.0";
                        rt.customerscount = "";
                        rt.orderscount = "";
                    }
                    

                    rutaslst.Add(rt);
                }
                //}
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult closeSO(string id_sales)
        {
            try
            {
                var ID = Convert.ToInt32(id_sales);

                Tb_PlanningSO selSO = dblim.Tb_PlanningSO.Find(ID);

                selSO.isfinished = true;
                dblim.Entry(selSO).State = EntityState.Modified;
                dblim.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult closeRoute(string id_route)
        {
            try
            {
                var ID = Convert.ToInt32(id_route);

                Tb_Planning route = dblim.Tb_Planning.Find(ID);

                route.isfinished = true;
                dblim.Entry(route).State = EntityState.Modified;
                dblim.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult closeAllSalesOrders(string id_route)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                var ID = Convert.ToInt32(id_route);

                var route = dblim.Tb_PlanningSO.Where(a=> a.ID_Route==ID).Select(a=>a).ToList();
               
                foreach (var item in route) {
                    item.isfinished = true;
                    if (activeuser != null) {
                       
                        item.ID_userValidate = activeuser.ID_User;
                    }
                    item.DateCheckOut = DateTime.UtcNow;
                    dblim.Entry(item).State = EntityState.Modified;

                }
               
                dblim.BulkSaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult transferItems(string id_route)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }


                List<int> TagIds = id_route.Split(',').Select(int.Parse).ToList();

                var soedit = dblim.Tb_PlanningSO.Where(a => TagIds.Contains(a.ID_Route) && a.Transferred==0 && a.QC_count>1).Select(a => a).ToList();
                var so_list = soedit.Select(a => a.ID_salesorder).ToArray();

                foreach (var so in soedit)
                {
                     so.Transferred= 1;
                    dblim.Entry(so).State = EntityState.Modified;

                }

                var item_list = dblim.Tb_PlanningSO_details.Where(b => so_list.Contains(b.ID_salesorder) && b.Warehouse == company_bodega && !b.query1.Contains("DEL")).ToList();

                foreach (var item in item_list)
                {
                    item.isvalidated = false;
                    item.Transferred = 1;
                    item.QC_totalCount = item.QC_totalCount + 1;
                    dblim.Entry(item).State = EntityState.Modified;

                }

                dblim.BulkSaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetSalesOrders(string id, DateTime date)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var company_bodega = "0";
            if (activeuser.ID_Company == 1)
            {
                company_bodega = "01";
            }
            else if (activeuser.ID_Company == 2)
            {
                company_bodega = "02";
            }


            int idr = Convert.ToInt32(id);
            //var salesOrders = new List<Tb_PlanningSO>();
            DateTime selDate = Convert.ToDateTime(date).AddDays(1);
            DateTime filterdate = DateTime.Today.AddDays(-31);

            var tbplanningSO = (from obj in dblim.Tb_PlanningSO where(obj.SAP_docdate > filterdate && obj.Warehouse==company_bodega) select obj).ToList();


            var rt = (from a in dblim.Tb_Planning where (a.ID_Route == idr && a.Warehouse==company_bodega) select new { id=a.ID_Route, isfinished=a.isfinished,
                transf = (from c in dblim.Tb_PlanningSO where (c.ID_Route == idr && c.Transferred > 0) select c).Count()
            }).ToList();
            //Solo IDSO interno
            var solist = (from j in dblim.Tb_PlanningSO where (j.ID_Route == idr && j.Warehouse==company_bodega) select j.ID_salesorder).ToList();

       

            //Verificamos detalles para sacar CASE y EACH totales y luego promediar todal de EACH en base a CASES
            var detallesSo = (from f in dblim.Tb_PlanningSO_details where (solist.Contains(f.ID_salesorder) && f.Warehouse==company_bodega) select f).ToList();

            var totalCantEach = 0;
            var totalCantCases = 0;
            var totalCantPack = 0;
            var totalCantLbs = 0;
            decimal promedioEachxCases = 0;
            //Para calcular el promedio lo hacemos diviendo
            try
            {
                if (detallesSo.Count() > 0)
                {
                    totalCantEach = detallesSo.Where(c=>c.UomCode.Contains("EACH")).Count();
                    totalCantCases = detallesSo.Where(c=>c.UomCode.Contains("CASE")).Count();
                    totalCantPack = detallesSo.Where(c=>c.UomCode.Contains("PACK")).Count();
                    totalCantLbs = detallesSo.Where(c=>c.UomCode.Contains("LBS")).Count();

                    foreach (var item in solist) {
                        //devolvemos ID externo
                        var docnum = Convert.ToInt32(dblim.Tb_PlanningSO.Where(j => j.ID_Route == idr && j.ID_salesorder==item && j.Warehouse==company_bodega).Select(c => c.SAP_docnum).FirstOrDefault());

                        //Devolvemos todos los detalles(itemcode) de esa SO
                        var itemscode = detallesSo.Where(c=>c.ID_salesorder==item).Select(c => c.ItemCode).ToArray();

                        //Buscamos en la vista creada 9/24/2019
                        var sumatotal = dlipro.PlanningUoMInfo.Where(a => itemscode.Contains(a.ItemCode) && a.DocNum == docnum && a.Quantity > 0 && !a.unitMsr.Contains("LBS")).Sum(c => c.TotalCases);

                        promedioEachxCases += Math.Round(Convert.ToDecimal(sumatotal),2, MidpointRounding.ToEven);                      

                    }
                }
                else
                {
                    totalCantEach = 0;
                    totalCantCases = 0;
                    totalCantPack = 0;
                    totalCantLbs = 0;
                    promedioEachxCases = 0;

                }
            }
            catch
            {
                totalCantEach = 0;
                totalCantCases = 0;
                totalCantPack = 0;
                totalCantLbs = 0;
                promedioEachxCases = 0;
            }



            //salesOrders = (from obj in dbcmk.VisitsM where (obj.ID_route == idr) select obj).ToList();
            var lst = (from obj in dblim.Tb_PlanningSO where (obj.ID_Route == idr && obj.Warehouse==company_bodega) select new { id = obj.ID_salesorder, NumSO=obj.SAP_docnum, CardCode = obj.ID_customer, CustomerName = obj.Customer_name , DeliveryRoute = obj.query2, SalesPerson = obj.Rep_name, SODate = obj.SAP_docdate, OpenAmount = obj.Amount, Weight= obj.Weight, Volume = obj.Volume, Printed= obj.Printed, Remarks = obj.Remarks, Order=obj.query3 }).OrderBy(c=>c.Order).ToArray();
            var lstArray = (from obj in lst select  obj.NumSO).ToArray();
            var totalextra = "";
            try
            {
                var extra = (from a in dblim.Tb_Planning_extra where (a.ID_Route == idr) select a);
                if (extra.Count() > 0)
                {
                    totalextra = extra.Sum(x => x.Value).ToString();
                }
                else
                {
                    totalextra = "0.00";

                }

            }
            catch
            {
                totalextra = "0.00";
            }


            if (rt[0].isfinished == true || rt[0].transf>0)
            {
                var lstOpenSales = (from obj in dlipro.OpenSalesOrders where (obj.NumSO==11938023) select new OpenSO { NumSO = obj.NumSO, CardCode = obj.CardCode, CustomerName = obj.CustomerName, DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed = obj.Printed }).ToArray();
                //estaesbuenasinfiltro//var lstOpenSales = (from obj in dlipro.OpenSalesOrders select new OpenSO { NumSO=obj.NumSO,CardCode = obj.CardCode, CustomerName = obj.CustomerName , DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed= obj.Printed }).ToArray();
                //var lstOpenSales = (List<OpenSO>)Session["opensalesOrders"];

                var lstRoutes = (from a in lstOpenSales select a.DeliveryRoute).OrderBy(a => a).Distinct().ToArray();
                var lstReps = (from a in lstOpenSales select a.SalesPerson).OrderBy(a => a).Distinct().ToArray();
                var lstCustomers = (from a in lstOpenSales select new { CustomerName = a.CardCode + " - " + a.CustomerName }).OrderBy(a => a.CustomerName).Distinct().ToArray();
                //var lstCustomers = (from a in lstOpenSales select new { CardCode=a.CardCode, CustomerName = a.CustomerName  }).OrderBy(a=>a.CardCode).Distinct().ToArray();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result2 = javaScriptSerializer.Serialize(lst);
                string result3 = javaScriptSerializer.Serialize(lstOpenSales);
                string result4 = javaScriptSerializer.Serialize(lstRoutes);
                string result5 = javaScriptSerializer.Serialize(lstReps);
                string result6 = javaScriptSerializer.Serialize(lstCustomers);
                string result7 = javaScriptSerializer.Serialize(rt);
                decimal result8 = Convert.ToDecimal(totalextra);



                //RESULT 8-12 para UOM INFO

                var result = new { result = result2, result2 = result3, result3 = result4, result4 = result5, result5 = result6, result6 = result7, result7=result8, result8= totalCantEach.ToString(), result9= totalCantCases,
                result10=totalCantPack, result11=totalCantLbs, result12=promedioEachxCases};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else {

                var lstOpenSales = (from obj in dlipro.OpenSalesOrders where(obj.SODate > filterdate && obj.WareHouse== company_bodega) select new OpenSO { NumSO = obj.NumSO, CardCode = obj.CardCode, CustomerName = obj.CustomerName, DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount =obj.OpenAmount, Remarks = obj.Remarks, Printed = obj.Printed }).ToArray();
                List<int> myCollection = new List<int>();

                foreach (var saleOrder in lstOpenSales)
                {


                    decimal openO = Convert.ToDecimal(saleOrder.OpenAmount);


                    var sapdoc = saleOrder.NumSO.ToString();
                    var existe = (from a in tbplanningSO where (a.Amount == openO && a.SAP_docnum == sapdoc && a.Warehouse == company_bodega) select a).FirstOrDefault();
                    if (existe != null)
                    {
                        myCollection.Add(Convert.ToInt32(sapdoc));
                    }


                }

                lstOpenSales = (from a in lstOpenSales where (!myCollection.Contains(a.NumSO)) select a).ToArray();

                //estaesbuenasinfiltro//var lstOpenSales = (from obj in dlipro.OpenSalesOrders select new OpenSO { NumSO=obj.NumSO,CardCode = obj.CardCode, CustomerName = obj.CustomerName , DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed= obj.Printed }).ToArray();
                //var lstOpenSales = (List<OpenSO>)Session["opensalesOrders"];

                var lstRoutes = (from a in lstOpenSales select a.DeliveryRoute).OrderBy(a => a).Distinct().ToArray();
                var lstReps = (from a in lstOpenSales select a.SalesPerson).OrderBy(a => a).Distinct().ToArray();
                var lstCustomers = (from a in lstOpenSales select new { CustomerName = a.CardCode + " - " + a.CustomerName }).OrderBy(a => a.CustomerName).Distinct().ToArray();
                //var lstCustomers = (from a in lstOpenSales select new { CardCode=a.CardCode, CustomerName = a.CustomerName  }).OrderBy(a=>a.CardCode).Distinct().ToArray();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result2 = javaScriptSerializer.Serialize(lst);
                string result3 = javaScriptSerializer.Serialize(lstOpenSales);
                string result4 = javaScriptSerializer.Serialize(lstRoutes);
                string result5 = javaScriptSerializer.Serialize(lstReps);
                string result6 = javaScriptSerializer.Serialize(lstCustomers);
                string result7 = javaScriptSerializer.Serialize(rt);
                decimal result8 = Convert.ToDecimal(totalextra);
                var result = new { result = result2, result2 = result3, result3 = result4, result4 = result5, result5 = result6, result6 = result7, result7=result8,result8 = totalCantEach.ToString(),
                    result9 = totalCantCases,
                    result10 = totalCantPack,
                    result11 = totalCantLbs,
                    result12 = promedioEachxCases
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }



        }
        public class UomLst
        {
            public string UomCode { get; set; }
            public int UomEntry { get; set; }
        }

            public class detailsSO
        {
            public int ID_detail { get; set; }
            public int Line_num { get; set; }
            public string Bin_loc { get; set; }
            public decimal Quantity { get; set; }
            public string UomCode { get; set; }
            public string UomEntry { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string StockWhs01 { get; set; }
            public bool isvalidated { get; set; }
            public string ID_storagetype { get; set; }
            public string Storage_type { get; set; }
            public int ID_salesorder { get; set; }
            public string query1 { get; set; }
            public List<UomLst> lstuom { get; set; }

    }
        public ActionResult GetSalesOrdersDetails(string id, string id_storage)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var company_bodega = "0";
            if (activeuser.ID_Company == 1)
            {
                company_bodega = "01";
            }
            else if (activeuser.ID_Company == 2)
            {
                company_bodega = "02";
            }
            int idr = Convert.ToInt32(id);
            List<detailsSO> rt = new List<detailsSO>();
            
            if (id_storage == "COOLER" || id_storage == "FREEZER")
            {



                rt = (from a in dblim.Tb_PlanningSO_details
                      where (a.ID_salesorder == idr && a.Quantity > 0 && (a.ID_storagetype == "COOLER" || a.ID_storagetype == "FREEZER") && a.Warehouse == company_bodega && !a.type.Contains("I"))
                      select new detailsSO
                      {
                          ID_detail = a.ID_detail,
                          Line_num = a.Line_num,
                          Bin_loc = a.Bin_loc,
                          Quantity = a.Quantity,
                          UomCode = a.UomCode,
                          UomEntry = a.UomEntry,
                          ItemCode = a.ItemCode,
                          ItemName = a.ItemName,
                          StockWhs01 = a.StockWhs01,
                          isvalidated = a.isvalidated,
                          ID_storagetype = a.ID_storagetype,
                          Storage_type = a.Storage_type,
                          ID_salesorder = a.ID_salesorder,
                          query1 = a.query1
                      }).OrderBy(a=>a.ID_detail).ToList();
            }
            else {
                rt = (from a in dblim.Tb_PlanningSO_details
                      where (a.ID_salesorder == idr && a.Quantity > 0 && a.ID_storagetype == id_storage && !a.type.Contains("I") && a.Warehouse == company_bodega)
                      select new detailsSO
                      {
                          ID_detail = a.ID_detail,
                          Line_num = a.Line_num,
                          Bin_loc = a.Bin_loc,
                          Quantity = a.Quantity,
                          UomCode = a.UomCode,
                          UomEntry = a.UomEntry,
                          ItemCode = a.ItemCode,
                          ItemName = a.ItemName,
                          StockWhs01 = a.StockWhs01,
                          isvalidated = a.isvalidated,
                          ID_storagetype = a.ID_storagetype,
                          Storage_type = a.Storage_type,
                          ID_salesorder = a.ID_salesorder,
                          query1 = a.query1
                      }).OrderBy(a => a.ID_detail).ToList();
            }


            foreach (var item in rt) {
                var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == item.ItemCode) select new UomLst { UomCode = a.UomCode, UomEntry = a.UomEntry }).ToList();
                item.lstuom = lstava;
            }


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result2 = javaScriptSerializer.Serialize(rt);
         
            var result = new { result = result2 };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Routes_Report()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Routes Report";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operrep_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;
                //FIN HEADER
                //Evaluamos si es supervisor o usuario normal para mostrar recursos o si es ambos o si es super admin





                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        [HttpPost]
        public JsonResult EditRoute(string id, string routeName, string lstDriverCode, string lstDriverName, string lstTruckCode, string lstTruckName, string lstRLeaderCode, string lstRLeaderName, DateTime departure, DateTime preparationdate)
        {
            try
            {
                int rdf = Convert.ToInt32(id);

                Tb_Planning tbpl = dblim.Tb_Planning.Find(rdf);
                tbpl.Route_name = routeName;
                tbpl.ID_driver = lstDriverCode;
                tbpl.Driver_name = lstDriverName;
                tbpl.ID_truck = lstTruckCode;
                tbpl.Truck_name = lstTruckName;
                tbpl.ID_routeleader = lstRLeaderCode;
                tbpl.Routeleader_name = lstRLeaderName;
                tbpl.Departure = departure;
                tbpl.DatetoInvoice = preparationdate;
                dblim.Entry(tbpl).State = EntityState.Modified;
                dblim.SaveChanges();


                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult AssignRoute(string id, string lstDriverCode, string lstDriverName, string lstTruckCode, string lstTruckName)
        {
            try
            {
                int rdf = Convert.ToInt32(id);

                Tb_Planning tbpl = dblim.Tb_Planning.Find(rdf);


                var route = dblim.Tb_Planning.Where(a => a.ID_Route == tbpl.ID_Route).FirstOrDefault();
                var fecharuta = route.Departure;

                var fecharuta23 = fecharuta.AddHours(23);

                var rutasenfecha = dblim.Tb_Planning.Where(a => (a.Departure>=fecharuta && a.Departure<=fecharuta23) && a.Warehouse=="02").ToList();

                foreach (var item in rutasenfecha) {
                    item.ID_driver_whs = lstDriverCode;
                    item.Driver_name_whs = lstDriverName;
                    item.ID_truck_whs = lstTruckCode;
                    item.Truck_name_whs = lstTruckName;
                }

                dblim.BulkUpdate(rutasenfecha);


                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }


        private dbLimenaEntities AddToContext(dbLimenaEntities context, Tb_PlanningSO_details entity, int count, int commitCount, bool recreateContext)
        {
            context.Set<Tb_PlanningSO_details>().Add(entity);
            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new dbLimenaEntities();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }
            return context;
        }


        public ActionResult CreateRoutePlanning(string routeName, string lstMasterCode, string lstMasterName, string lstDriverCode, string lstDriverName, string lstTruckCode, string lstTruckName, string lstRLeaderCode, string lstRLeaderName, DateTime departure, DateTime preparationdate)
        {

            try
            {
                if (generalClass.checkSession())
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                    var company_bodega = "0";
                    if (activeuser.ID_Company == 1)
                    {
                        company_bodega = "01";
                    }
                    else if (activeuser.ID_Company == 2)
                    {
                        company_bodega = "02";
                    }


                    dblim.Configuration.AutoDetectChangesEnabled = false;
                    dblim.Configuration.ValidateOnSaveEnabled = false;
                    Tb_Planning newPlanning = new Tb_Planning();

                    newPlanning.Route_name = routeName;
                    newPlanning.ID_SAPRoute = lstMasterCode;
                    newPlanning.ID_driver = lstDriverCode;
                    newPlanning.Driver_name = lstDriverName;
                    newPlanning.ID_truck = lstTruckCode;
                    newPlanning.Truck_name = lstTruckName;
                    newPlanning.ID_routeleader = lstRLeaderCode;
                    newPlanning.Routeleader_name = lstRLeaderName;
                    newPlanning.Departure = departure;
                    newPlanning.isfinished = false;
                    newPlanning.query1 = "";
                    newPlanning.query2 = "";
                    newPlanning.query3 = 0;
                    newPlanning.Invoiced = false;
                    newPlanning.Date = departure.Date;
                    newPlanning.DateCheckIn = DateTime.UtcNow;
                    newPlanning.DateCheckOut = DateTime.UtcNow;
                    newPlanning.ID_userValidate = 0;
                    //Nueva propiedad para bodega
                    newPlanning.Warehouse = company_bodega;
                    newPlanning.ID_driver_whs = "";
                    newPlanning.Driver_name_whs = "";
                    newPlanning.ID_truck_whs = "";
                    newPlanning.Truck_name_whs = "";
                    newPlanning.DatetoInvoice = preparationdate;

                    dblim.Tb_Planning.Add(newPlanning);
                    dblim.SaveChanges();

                    DateTime filterdate = DateTime.Today.AddDays(-31);

                    var lstArray = (from obj in dblim.Tb_PlanningSO where (obj.SAP_docdate > filterdate) select obj.SAP_docnum).ToArray();
                    //var lstArray = (from obj in dblim.Tb_PlanningSO where (obj.isfinished == false && obj.DocEntry == "") select obj.SAP_docnum).ToArray();

                    List<int> myCollection = new List<int>();

                    foreach (var item in lstArray)
                    {
                        myCollection.Add(Convert.ToInt32(item));
                    }
                    var salesOrders = (from a in dlipro.OpenSalesOrders where (a.DeliveryRoute == lstMasterName && !myCollection.Contains(a.NumSO) && a.SODate > filterdate) select a).ToList();

                    var avaSO = (from a in salesOrders select a.NumSO).ToArray();

                    if (salesOrders.Count > 0)
                    {
                        //ORDER BY AREA, SUBAREA, UoMFilter, PrintOrder, U_BinLocation, T1.ItemCode 
                        var dtSO_details = (from c in dlipro.OpenSalesOrders_Details where (avaSO.Contains(c.DocNum) && !c.TreeType.Contains("I")) select c).OrderBy(c => c.AREA).ThenBy(c => c.SUBAREA).ThenBy(c => c.UoMFilter).ThenBy(c => c.PrintOrder).ThenBy(c => c.U_BinLocation).ThenBy(c => c.ItemCode).ToList();
                        try {
                            var count = 0;
                            foreach (var saleOrder in salesOrders)
                            {
                                if (saleOrder.OpenAmount == saleOrder.TotalSO)
                                {
                                    decimal openO = Convert.ToDecimal(saleOrder.OpenAmount);
                                    var sapdoc = saleOrder.NumSO.ToString();
                                    var existe = (from a in dblim.Tb_PlanningSO where (a.Amount == openO && a.SAP_docnum == sapdoc) select a).FirstOrDefault();
                                    if (existe == null) {
                                        Tb_PlanningSO newSO = new Tb_PlanningSO();
                                        newSO.SAP_docnum = saleOrder.NumSO.ToString();
                                        newSO.SAP_docdate = Convert.ToDateTime(saleOrder.SODate);
                                        newSO.ID_customer = saleOrder.CardCode;
                                        newSO.Customer_name = saleOrder.CustomerName.ToUpper();
                                        newSO.ID_rep = saleOrder.IDSalesPerson.ToString();
                                        newSO.Rep_name = saleOrder.SalesPerson.ToUpper();
                                        newSO.ID_SAPRoute = newPlanning.ID_SAPRoute;
                                        newSO.Amount = Convert.ToDecimal(saleOrder.OpenAmount);
                                        newSO.isfinished = false;
                                        newSO.query1 = "";
                                        newSO.query2 = saleOrder.DeliveryRoute;
                                        newSO.query3 = 0;
                                        //newSO.query3 = count;
                                        newSO.query4 = "";
                                        newSO.query5 = "";
                                        newSO.Weight = "";
                                        newSO.Volume = "";
                                        newSO.Printed = saleOrder.Printed;
                                        newSO.ID_Route = newPlanning.ID_Route;
                                        newSO.DocEntry = "";
                                        newSO.DateCheckIn = DateTime.UtcNow;
                                        newSO.DateCheckOut = DateTime.UtcNow;
                                        newSO.ID_userValidate = 0;
                                        newSO.Warehouse = saleOrder.WareHouse;
                                        newSO.Transferred = 0;
                                        newSO.QC_count = Convert.ToInt32(saleOrder.NoWhs);
                                        newSO.MensajeError = "";
                                        newSO.Error = 0;
                                        if (saleOrder.Remarks == null) { newSO.Remarks = ""; } else { newSO.Remarks = saleOrder.Remarks; }

                                        dblim.Tb_PlanningSO.Add(newSO);
                                        dblim.SaveChanges();

                                        count++;

                                        var detailslst = (from a in dtSO_details where (a.DocNum == saleOrder.NumSO) select a).ToList();

                                        if (detailslst.Count > 0)
                                        {

                                            List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                                            foreach (var dt in detailslst)
                                            {
                                                Tb_PlanningSO_details newDtl = new Tb_PlanningSO_details();
                                                newDtl.Line_num = dt.LineNum;
                                                if (dt.U_BinLocation == null) { newDtl.Bin_loc = ""; } else { newDtl.Bin_loc = dt.U_BinLocation; }

                                                newDtl.Quantity = Convert.ToDecimal(dt.Quantity);
                                                if (dt.UomEntry == null) { newDtl.UomEntry = ""; } else { newDtl.UomEntry = dt.UomEntry.ToString(); }
                                                if (dt.UomCode == null) { newDtl.UomCode = ""; } else { newDtl.UomCode = dt.UomCode; }

                                                newDtl.NumPerMsr = Convert.ToDecimal(dt.NumPerMsr);
                                                newDtl.ItemCode = dt.ItemCode;
                                                newDtl.AREA = dt.AREA.ToString();
                                                newDtl.SUBAREA = dt.SUBAREA.ToString();
                                                newDtl.UomFilter = dt.UoMFilter.ToString();
                                                newDtl.PrintOrder = dt.PrintOrder.ToString();
                                                newDtl.ItemName = dt.ItemName;
                                                newDtl.StockWhs01 = "";
                                                newDtl.isvalidated = false;
                                                if (dt.U_Storage == null) { newDtl.ID_storagetype = ""; } else { newDtl.ID_storagetype = dt.U_Storage; }
                                                if (dt.U_Storage == null) { newDtl.Storage_type = ""; } else { newDtl.Storage_type = dt.U_Storage; }
                                                newDtl.ID_salesorder = newSO.ID_salesorder;
                                                newDtl.query1 = "";
                                                newDtl.query2 = dt.CambioPrecio.ToString();
                                                newDtl.ID_picker = "";
                                                newDtl.Picker_name = "";
                                                newDtl.ID_pickerWHS = "";
                                                newDtl.Picker_nameWHS = "";
                                                newDtl.DateCheckIn = DateTime.UtcNow;
                                                newDtl.DateCheckOut = DateTime.UtcNow;
                                                newDtl.ID_userValidate = 0;
                                                newDtl.ID_ValidationDetails = 0;
                                                newDtl.ValidationDetails = "";
                                                newDtl.type = dt.TreeType;
                                                newDtl.parent = "";
                                                newDtl.childrendefqty = 0;
                                                newDtl.Transferred = 0;
                                                newDtl.Warehouse = dt.Whs;
                                                newDtl.QC_count = Convert.ToInt32(dt.NoWhs);
                                                if (newSO.Warehouse == dt.Whs)
                                                {
                                                    newDtl.QC_totalCount = Convert.ToInt32(dt.NoWhs);
                                                }
                                                else {
                                                    newDtl.QC_totalCount = 1;
                                                }
                                               
                                                lsttosave.Add(newDtl);


                                                //Si tiene hijos o es propiedad S, agregamos
                                                if (dt.TreeType == "S") {
                                                    int countLineNum = dt.LineNum + 1;
                                                    var kit_childs = (from d in dlipro.ITT1 where (d.Father == dt.ItemCode) select d).OrderBy(d => d.ChildNum).ToList();

                                                    if (kit_childs.Count > 0) {
                                                        foreach (var hijo in kit_childs) {
                                                            var childinfo = (from ad in dlipro.BI_Dim_Products where (ad.id == hijo.Code) select ad).FirstOrDefault();
                                                            var itemnamechild = "";
                                                            if (childinfo != null) {
                                                                itemnamechild = childinfo.item_name;
                                                            }
                                                            Tb_PlanningSO_details newDtlHijo = new Tb_PlanningSO_details();
                                                            newDtlHijo.Line_num = countLineNum;
                                                            if (dt.U_BinLocation == null) { newDtlHijo.Bin_loc = ""; } else { newDtlHijo.Bin_loc = dt.U_BinLocation; }

                                                            newDtlHijo.Quantity = Convert.ToInt32(hijo.Quantity);

                                                            if (dt.UomEntry == null) { newDtlHijo.UomEntry = ""; } else { newDtlHijo.UomEntry = dt.UomEntry.ToString(); }
                                                            if (dt.UomCode == null) { newDtlHijo.UomCode = ""; } else { newDtlHijo.UomCode = dt.UomCode; }

                                                            newDtlHijo.NumPerMsr = 0;
                                                            newDtlHijo.ItemCode = hijo.Code;
                                                            newDtlHijo.AREA = dt.AREA.ToString();
                                                            newDtlHijo.SUBAREA = dt.SUBAREA.ToString();
                                                            newDtlHijo.UomFilter = dt.UoMFilter.ToString();
                                                            newDtlHijo.PrintOrder = dt.PrintOrder.ToString();
                                                            newDtlHijo.ItemName = itemnamechild;
                                                            newDtlHijo.StockWhs01 = "";
                                                            newDtlHijo.isvalidated = false;
                                                            if (dt.U_Storage == null) { newDtlHijo.ID_storagetype = ""; } else { newDtlHijo.ID_storagetype = dt.U_Storage; }
                                                            if (dt.U_Storage == null) { newDtlHijo.Storage_type = ""; } else { newDtlHijo.Storage_type = dt.U_Storage; }
                                                            newDtlHijo.ID_salesorder = newSO.ID_salesorder;
                                                            newDtlHijo.query1 = "";
                                                            newDtlHijo.query2 = "";
                                                            newDtlHijo.ID_picker = "";
                                                            newDtlHijo.Picker_name = "";
                                                            newDtlHijo.ID_pickerWHS = "";
                                                            newDtlHijo.Picker_nameWHS = "";
                                                            newDtlHijo.DateCheckIn = DateTime.UtcNow;
                                                            newDtlHijo.DateCheckOut = DateTime.UtcNow;
                                                            newDtlHijo.ID_userValidate = 0;
                                                            newDtlHijo.ID_ValidationDetails = 0;
                                                            newDtlHijo.ValidationDetails = "";
                                                            newDtlHijo.type = "I";
                                                            newDtlHijo.parent = dt.ItemCode;
                                                            newDtlHijo.childrendefqty = Convert.ToInt32(hijo.Quantity);
                                                            newDtlHijo.Transferred = 0;
                                                            newDtlHijo.Warehouse = dt.Whs;
                                                            newDtlHijo.QC_count = Convert.ToInt32(dt.NoWhs);
                                                            if (newSO.Warehouse == dt.Whs)
                                                            {
                                                                newDtlHijo.QC_totalCount = Convert.ToInt32(dt.NoWhs);
                                                            }
                                                            else
                                                            {
                                                                newDtlHijo.QC_totalCount = 1;
                                                            }
                                                            lsttosave.Add(newDtlHijo);
                                                            countLineNum++;
                                                        }


                                                    }
                                                }


                                            }

                                            dblim.BulkInsert(lsttosave);
                                        }
                                    }


                                }


                            }
                        } catch { }

                    }



                    List<Routes_calendar> rutaslst = new List<Routes_calendar>();


                    Routes_calendar rt = new Routes_calendar();

                    rt.title = newPlanning.ID_Route + " - " + newPlanning.Route_name;
                    rt.url = "";
                    rt.start = newPlanning.Departure.ToString("yyyy-MM-dd");
                    //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd");
                    rt.route_leader = newPlanning.Routeleader_name;
                    rt.className = ".fc-event";
                    rt.driver = newPlanning.Driver_name;
                    rt.truck = newPlanning.Truck_name;
                    rt.departure = newPlanning.Departure.ToShortTimeString();
                    if (newPlanning.isfinished == true) { rt.isfinished = "Y"; } else { rt.isfinished = "N"; }
                    var sum = (from e in dblim.Tb_PlanningSO where (e.ID_Route == newPlanning.ID_Route && e.Warehouse==company_bodega) select e);
                    if (sum != null && sum.Count() > 0)
                    {
                        rt.amount = sum.Select(c => c.Amount).Sum().ToString();
                        rt.customerscount = sum.Select(c => c.Customer_name).Distinct().Count().ToString();
                        rt.orderscount = sum.Count().ToString();
                    }
                    else
                    {
                        rt.amount = "0.0";
                        rt.customerscount = "";
                        rt.orderscount = "";
                    }


                    rutaslst.Add(rt);


                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    string result = javaScriptSerializer.Serialize(rutaslst);
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult deleteRoute(string id_route)
        {

            try
            {

                    var id = Convert.ToInt32(id_route);
                var route = dblim.Tb_Planning.Where(c => c.ID_Route == id).Select(c => c).FirstOrDefault();

                if (route != null)
                {
                    var vp = dblim.Tb_PlanningSO.Where(a => a.ID_Route == id);

                    var todelete = (from f in vp select f.ID_salesorder).ToArray();
                    var vpdet = dblim.Tb_PlanningSO_details.Where(a => todelete.Contains(a.ID_salesorder));
                    //dblim.Tb_PlanningSO_details.RemoveRange(vpdet);
                    dblim.BulkDelete(vpdet);


                    dblim.Tb_PlanningSO.RemoveRange(vp);
                    dblim.SaveChanges();


                    dblim.Tb_Planning.Remove(route);
                   

                    var vp_extra = dblim.Tb_Planning_extra.Where(a => a.ID_Route == id);
                    dblim.Tb_Planning_extra.RemoveRange(vp_extra);

                    dblim.SaveChanges();

                    var rrresult = "SUCCESS";
                    return Json(rrresult, JsonRequestBehavior.AllowGet);
                }
                else {
                    string result = "NO DATA FOUND";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                


            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SaveSalesOrders(string[] salesOrders, string id_route)
        {

            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                var company_bodega = "0";
                if (activeuser.ID_Company == 1)
                {
                    company_bodega = "01";
                }
                else if (activeuser.ID_Company == 2)
                {
                    company_bodega = "02";
                }
                if (salesOrders == null)
                {
                    var id = Convert.ToInt32(id_route);
                    var vp = dblim.Tb_PlanningSO.Where(a => a.ID_Route == id);

                    var todelete = (from f in vp select f.ID_salesorder).ToArray();
                    var vpdet = dblim.Tb_PlanningSO_details.Where(a => todelete.Contains(a.ID_salesorder));
                    //dblim.Tb_PlanningSO_details.RemoveRange(vpdet);
                    dblim.BulkDelete(vpdet);




                    dblim.Tb_PlanningSO.RemoveRange(vp);
                    dblim.SaveChanges();

                    var rrresult = "SUCCESS";
                    return Json(rrresult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (salesOrders.Length > 0 || salesOrders != null)
                    {
                        var id = Convert.ToInt32(id_route);
                        var soexist = (from f in dblim.Tb_PlanningSO where (f.ID_Route == id ) select f.SAP_docnum).ToArray();

                        var fincoll = (from a in soexist where (!salesOrders.Contains(a)) select a).ToArray();


                        Tb_Planning planning = new Tb_Planning();
                        planning = dblim.Tb_Planning.Find(id);
                        if (fincoll.Length > 0)
                        {

                            var todelete = (from f in dblim.Tb_PlanningSO where (f.ID_Route == id && fincoll.Contains(f.SAP_docnum)) select f.ID_salesorder).ToArray();
                            var vpdet = dblim.Tb_PlanningSO_details.Where(a => todelete.Contains(a.ID_salesorder));
                            //dblim.Tb_PlanningSO_details.RemoveRange(vpdet);
                            dblim.BulkDelete(vpdet);


                            var vp = dblim.Tb_PlanningSO.Where(a => a.ID_Route == id && fincoll.Contains(a.SAP_docnum));
                            dblim.Tb_PlanningSO.RemoveRange(vp);
                            dblim.SaveChanges();
                        }

                        salesOrders = (from a in salesOrders where (!soexist.Contains(a)) select a).ToArray();


                        List<int> myCollection = new List<int>();

                        foreach (var item in salesOrders)
                        {
                            myCollection.Add(Convert.ToInt32(item));
                        }




                        var dtSO = (from c in dlipro.OpenSalesOrders where (myCollection.Contains(c.NumSO)) select c).ToList();


                        var dtSO_details = (from c in dlipro.OpenSalesOrders_Details where (myCollection.Contains(c.DocNum) && !c.TreeType.Contains("I")) select c).OrderBy(c => c.AREA).ThenBy(c => c.SUBAREA).ThenBy(c => c.UoMFilter).ThenBy(c => c.PrintOrder).ThenBy(c => c.U_BinLocation).ThenBy(c => c.ItemCode).ToList();
                        foreach (string value in salesOrders)
                        {
                            var idso = Convert.ToInt32(value);
                            var saleOrder = (from a in dtSO where (a.NumSO == idso) select a).First();

                            decimal openO = Convert.ToDecimal(saleOrder.OpenAmount);
                            var sapdoc = saleOrder.NumSO.ToString();
                            var existe = (from a in dblim.Tb_PlanningSO where (a.Amount == openO && a.SAP_docnum == sapdoc) select a).FirstOrDefault();
                            if (existe == null)
                            {



                                Tb_PlanningSO newSO = new Tb_PlanningSO();
                                newSO.SAP_docnum = saleOrder.NumSO.ToString();
                                newSO.SAP_docdate = Convert.ToDateTime(saleOrder.SODate);
                                newSO.ID_customer = saleOrder.CardCode;
                                newSO.Customer_name = saleOrder.CustomerName.ToUpper();
                                newSO.ID_rep = saleOrder.IDSalesPerson.ToString();
                                newSO.Rep_name = saleOrder.SalesPerson.ToUpper();
                                newSO.ID_SAPRoute = planning.ID_SAPRoute;
                                newSO.Amount = Convert.ToDecimal(saleOrder.OpenAmount);
                                newSO.isfinished = false;
                                newSO.query1 = "";
                                newSO.query2 = saleOrder.DeliveryRoute;
                                newSO.query3 = 0;
                                newSO.query4 = "";
                                newSO.query5 = "";
                                newSO.Weight = "";
                                newSO.Volume = "";
                                newSO.Printed = saleOrder.Printed;
                                newSO.ID_Route = planning.ID_Route;
                                newSO.DocEntry = "";
                                newSO.DateCheckIn = DateTime.UtcNow;
                                newSO.DateCheckOut = DateTime.UtcNow;
                                newSO.ID_userValidate = 0;
                                newSO.Transferred = 0;
                                newSO.Warehouse = saleOrder.WareHouse;
                                newSO.MensajeError = "";
                                newSO.Error = 0;
                                newSO.QC_count = Convert.ToInt32(saleOrder.NoWhs);

                                if (saleOrder.Remarks == null) { newSO.Remarks = ""; } else { newSO.Remarks = saleOrder.Remarks; }
                                dblim.Tb_PlanningSO.Add(newSO);
                                dblim.SaveChanges();
                                //List<Tb_PlanningSO_details> lstsave = new List<Tb_PlanningSO_details>();
                                var detailslst2 = (from a in dtSO_details where (a.DocNum == saleOrder.NumSO) select a).ToList();

                                if (detailslst2.Count > 0)
                                {
                                    List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                                    foreach (var dt in detailslst2)
                                    {
                                        Tb_PlanningSO_details newDtl = new Tb_PlanningSO_details();

                                        newDtl.Line_num = dt.LineNum;
                                        if (dt.U_BinLocation == null) { newDtl.Bin_loc = ""; } else { newDtl.Bin_loc = dt.U_BinLocation; }

                                        newDtl.Quantity = Convert.ToDecimal(dt.Quantity);
                                        if (dt.UomEntry == null) { newDtl.UomEntry = ""; } else { newDtl.UomEntry = dt.UomEntry.ToString(); }
                                        if (dt.UomCode == null) { newDtl.UomCode = ""; } else { newDtl.UomCode = dt.UomCode; }

                                        newDtl.NumPerMsr = Convert.ToDecimal(dt.NumPerMsr);
                                        newDtl.ItemCode = dt.ItemCode;
                                        newDtl.AREA = dt.AREA.ToString();
                                        newDtl.SUBAREA = dt.SUBAREA.ToString();
                                        newDtl.UomFilter = dt.UoMFilter.ToString();
                                        newDtl.PrintOrder = dt.PrintOrder.ToString();
                                        newDtl.ItemCode = dt.ItemCode;
                                        newDtl.ItemName = dt.ItemName;
                                        newDtl.StockWhs01 = "";
                                        newDtl.isvalidated = false;
                                        if (dt.U_Storage == null) { newDtl.ID_storagetype = ""; } else { newDtl.ID_storagetype = dt.U_Storage; }
                                        if (dt.U_Storage == null) { newDtl.Storage_type = ""; } else { newDtl.Storage_type = dt.U_Storage; }
                                        newDtl.ID_salesorder = newSO.ID_salesorder;
                                        newDtl.query1 = "";
                                        newDtl.query2 = dt.CambioPrecio.ToString();
                                        newDtl.ID_picker = "";
                                        newDtl.Picker_name = "";
                                        newDtl.ID_pickerWHS = "";
                                        newDtl.Picker_nameWHS = "";
                                        newDtl.DateCheckIn = DateTime.UtcNow;
                                        newDtl.DateCheckOut = DateTime.UtcNow;
                                        newDtl.ID_userValidate = 0;
                                        newDtl.ID_ValidationDetails = 0;
                                        newDtl.ValidationDetails = "";
                                        newDtl.type = dt.TreeType;
                                        newDtl.parent = "";
                                        newDtl.childrendefqty = 0;
                                        newDtl.Warehouse = dt.Whs;
                                        newDtl.Transferred = 0;

                                        //Evaluamos si se haran 2 conteos o 1
                                        //02=Louisville, KY ----- WHS=01-Nashville, TN
                                        //Se haran 2 conteos si la orden es solicitada en KY pero el detalle sale de TN


                                        newDtl.QC_count = Convert.ToInt32(dt.NoWhs);
                                        if (newSO.Warehouse == dt.Whs)
                                        {
                                            newDtl.QC_totalCount = Convert.ToInt32(dt.NoWhs);
                                        }
                                        else
                                        {
                                            newDtl.QC_totalCount = 1;
                                        }


                                        lsttosave.Add(newDtl);
                                        //dblim.Tb_PlanningSO_details.Add(newDtl);

                                        //Si tiene hijos o es propiedad S, agregamos
                                        if (dt.TreeType == "S")
                                        {
                                            int countLineNum = dt.LineNum + 1;
                                            var kit_childs = (from d in dlipro.ITT1 where (d.Father == dt.ItemCode) select d).OrderBy(d => d.ChildNum).ToList();

                                            if (kit_childs.Count > 0)
                                            {
                                                foreach (var hijo in kit_childs)
                                                {
                                                    var childinfo = (from ad in dlipro.BI_Dim_Products where (ad.id == hijo.Code) select ad).FirstOrDefault();
                                                    var itemnamechild = "";
                                                    if (childinfo != null)
                                                    {
                                                        itemnamechild = childinfo.item_name;
                                                    }
                                                    Tb_PlanningSO_details newDtlHijo = new Tb_PlanningSO_details();
                                                    newDtlHijo.Line_num = countLineNum;
                                                    if (dt.U_BinLocation == null) { newDtlHijo.Bin_loc = ""; } else { newDtlHijo.Bin_loc = dt.U_BinLocation; }

                                                    newDtlHijo.Quantity = Convert.ToDecimal(hijo.Quantity);

                                                    if (dt.UomEntry == null) { newDtlHijo.UomEntry = ""; } else { newDtlHijo.UomEntry = dt.UomEntry.ToString(); }
                                                    if (dt.UomCode == null) { newDtlHijo.UomCode = ""; } else { newDtlHijo.UomCode = dt.UomCode; }

                                                    newDtlHijo.NumPerMsr = 0;
                                                    newDtlHijo.ItemCode = hijo.Code;
                                                    newDtlHijo.AREA = dt.AREA.ToString();
                                                    newDtlHijo.SUBAREA = dt.SUBAREA.ToString();
                                                    newDtlHijo.UomFilter = dt.UoMFilter.ToString();
                                                    newDtlHijo.PrintOrder = dt.PrintOrder.ToString();
                                                    newDtlHijo.ItemName = itemnamechild;
                                                    newDtlHijo.StockWhs01 = "";
                                                    newDtlHijo.isvalidated = false;
                                                    if (dt.U_Storage == null) { newDtlHijo.ID_storagetype = ""; } else { newDtlHijo.ID_storagetype = dt.U_Storage; }
                                                    if (dt.U_Storage == null) { newDtlHijo.Storage_type = ""; } else { newDtlHijo.Storage_type = dt.U_Storage; }
                                                    newDtlHijo.ID_salesorder = newSO.ID_salesorder;
                                                    newDtlHijo.query1 = "";
                                                    newDtlHijo.query2 = "";
                                                    newDtlHijo.ID_picker = "";
                                                    newDtlHijo.Picker_name = "";
                                                    newDtlHijo.ID_pickerWHS = "";
                                                    newDtlHijo.Picker_nameWHS = "";
                                                    newDtlHijo.DateCheckIn = DateTime.UtcNow;
                                                    newDtlHijo.DateCheckOut = DateTime.UtcNow;
                                                    newDtlHijo.ID_userValidate = 0;
                                                    newDtlHijo.ID_ValidationDetails = 0;
                                                    newDtlHijo.ValidationDetails = "";
                                                    newDtlHijo.type = "I";
                                                    newDtlHijo.parent = dt.ItemCode;
                                                    newDtlHijo.childrendefqty = Convert.ToInt32(hijo.Quantity);
                                                    newDtlHijo.Warehouse = dt.Whs;
                                                    newDtlHijo.QC_count = Convert.ToInt32(dt.NoWhs);
                                                    if (newSO.Warehouse == dt.Whs)
                                                    {
                                                        newDtlHijo.QC_totalCount = Convert.ToInt32(dt.NoWhs); ;
                                                    }
                                                    else
                                                    {
                                                        newDtlHijo.QC_totalCount = 1;
                                                    }
                                                    newDtlHijo.Transferred = 0;
                                                    lsttosave.Add(newDtlHijo);
                                                    countLineNum++;
                                                }


                                            }
                                        }

                                    }


                                    dblim.BulkInsert(lsttosave);
                                }
                            }

                        }




                        string ttresult = "SUCCESS";
                        return Json(ttresult, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string result = "NO DATA";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }



            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SortSalesOrders(string[] salesOrders, string id_route)
        {
       
            try
            {


                if (salesOrders != null) {
                    if (salesOrders.Length > 0)
                    {
                        var id = Convert.ToInt32(id_route);
                        var sodb = (from f in dblim.Tb_PlanningSO where (f.ID_Route == id ) select f);

                        var total = sodb.Count();
                        var count = 0;
                        if (total > 0)
                        {
                            foreach (var item in salesOrders) //Ordenamos en base al orden que trae la cadena
                            {
                                //var idsoin = Convert.ToInt32(item);
                                var saUp = (from a in sodb where (a.SAP_docnum == item) select a).FirstOrDefault();
                                saUp.query3 = count;
                                dblim.Entry(saUp).State = EntityState.Modified;
                                count++;


                            }
                            dblim.BulkSaveChanges();
                        }


                        string ttresult = "SUCCESS";
                        return Json(ttresult, JsonRequestBehavior.AllowGet);


                    }
                }

                string result = "NO DATA";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Save_order(List<orderSO> objects)
        {

            try
            {
                if (objects != null)
                {
                    if (objects.Count > 0)
                    {
             
                        var count = 0;
           
                            foreach (var item in objects) //Ordenamos en base al orden que trae la cadena
                            {
                            if (item.comment == null) { item.comment = ""; }
                          
                                //var idsoin = Convert.ToInt32(item);
                                var saUp = (from a in dblim.Tb_PlanningSO where (a.ID_salesorder == item.num) select a).FirstOrDefault();
                                saUp.query3 = item.sort;
                            saUp.query4 = item.comment;
                                dblim.Entry(saUp).State = EntityState.Modified;
                                count++;


                            }
                            dblim.BulkSaveChanges();
                        


                        string ttresult = "SUCCESS";
                        return Json(ttresult, JsonRequestBehavior.AllowGet);


                    }
                }

                string result = "NO DATA";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public class MyObj_extra
        {
            public string Description { get; set; }
            public decimal Value { get; set; }

        }

        [HttpPost]
        public ActionResult SaveExtras(string id, List<MyObj_extra> objects)
        {
            string ttresult = "";
            try
            {
               
                    var idf = Convert.ToInt32(id);

                    var listtodelete = (from a in dblim.Tb_Planning_extra where (a.ID_Route == idf) select a);
                    dblim.BulkDelete(listtodelete);

                    List<Tb_Planning_extra> lsttosave = new List<Tb_Planning_extra>();

                    foreach (var items in objects)
                    {
                        Tb_Planning_extra newDet = new Tb_Planning_extra();

                        newDet.Description = items.Description;
                        newDet.Value = items.Value;
                        newDet.ID_Route = idf;
                        newDet.query1 = "";
                        lsttosave.Add(newDet);

                    }

                    dblim.BulkInsert(lsttosave);

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
  


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
        public class MyObj
        {
            public int lineNumber { get; set; }
            public string validated { get; set; }
            public string quantity { get; set; }
            public string uom { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string deleted { get; set; }
        }
        [HttpPost]
        public ActionResult Save_SODetails(string id, List<MyObj> objects, string idPicker, string pickername)
        {
            string ttresult = "";
            try
            {
                var idf = Convert.ToInt32(id);

                    List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                    List<Tb_PlanningSO_details> lsttodelete = new List<Tb_PlanningSO_details>();

                    var allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I")) select a);
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                foreach (var items in objects)
                    {
                        Tb_PlanningSO_details newDet = (from a in allDet where (a.Line_num == items.lineNumber && a.ItemCode == items.ItemCode) select a).FirstOrDefault();

                    if (newDet.query2.Contains("3"))
                    {//Es una bonificacion
                        //var productonormal = dblim.Tb_PlanningSO_details.Where(a => a.ID_salesorder==idf && a.ItemCode == newDet.ItemCode && !a.query2.Contains("3")).FirstOrDefault();
                        //if (productonormal != null) {
                            //newDet.isvalidated = newDet.isvalidated;
                            //newDet.query1 = productonormal.query1;
                            newDet.Quantity = Convert.ToDecimal(items.quantity);

                            if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                            if (items.deleted == "DEL") {
                            newDet.query1 = "DEL";
                            //Se agrego linea de comando para eliminar bonificaciones u otro tipo de producto

                                //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1}", idf, items.ItemCode);
                           



                        } else { newDet.query1 = ""; }



                            var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                            if (lstava != null)
                            {
                                newDet.UomCode = lstava.UomCode;
                                newDet.UomEntry = lstava.UomEntry.ToString();
                                newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                            }

                            newDet.ID_picker = idPicker;
                            newDet.Picker_name = pickername;


                            if (activeuser != null)
                            {
                                newDet.ID_userValidate = activeuser.ID_User;
                            }
                            newDet.DateCheckOut = DateTime.UtcNow;

                            lsttosave.Add(newDet);
                        //}
                 

                    }
                    else {
                        //newDet.ID_UOM = items.uom;
                        //newDet.UOM = items.uom;
                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL") {
                            newDet.query1 = "DEL";
                            //Se agrego linea de comano para eliminar bonificaciones u otro tipo de producto
                            dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                        }
                        else { newDet.query1 = ""; }

                        //Evaluamos si la cantidad es menor a lo que colocaron, si es asi mandar propiedad DEL
                        if (!newDet.query2.Contains("0"))
                        {
                            if (newDet.UomCode.Contains("LBS"))
                            {

                            }
                            else {
                                if (Convert.ToDecimal(items.quantity) < newDet.Quantity)
                                {
                                    newDet.query1 = "DEL";
                                    //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                                    if (newDet.type == "S")
                                    {
                                        dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and parent={1}", idf, items.ItemCode);
                                    }

                                }
                            }

                        }

                      
                            if (items.quantity == "")
                            {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                            else if (items.quantity == "0")
                            {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                            else {

                                newDet.Quantity = Convert.ToDecimal(items.quantity);

                            }
                       

                        
                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }
                      
                        newDet.ID_picker = idPicker;
                        newDet.Picker_name = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        //Evaluamos si es kit para actualizar sus hijos
                        if (newDet.type == "S")
                        {
                            //llamamos los hijos
                            var hijos = (from b in dblim.Tb_PlanningSO_details where (b.parent == newDet.ItemCode && b.type == "I") select b).ToList();
                            if (hijos.Count > 0)
                            {
                                foreach (var item in hijos)
                                {
                                    item.query1 = newDet.query1;
                                    item.isvalidated = newDet.isvalidated;
                                    item.Quantity = Convert.ToDecimal(newDet.Quantity * item.childrendefqty);
                                    item.ID_picker = newDet.ID_picker;
                                    item.Picker_name = newDet.Picker_name;
                                    item.ID_userValidate = newDet.ID_userValidate;
                                    item.DateCheckOut = newDet.DateCheckOut;
                                    item.QC_totalCount = newDet.QC_totalCount;
                                    item.Transferred = newDet.Transferred;
                                }

                                dblim.BulkUpdate(hijos);

                            }
                        }

                        lsttosave.Add(newDet);
                    }
                    //Evaluamos si el producto se eliminio para actualizar todo despues
                    if (items.deleted == "DEL") {
                        lsttodelete.Add(newDet);
                    }



                }

                    dblim.BulkUpdate(lsttosave);

                //Eliminamos por producto
                try
                {
                    foreach (var itemdel in lsttodelete)
                    {
                        dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, itemdel.ItemCode);
                    }
                }
                catch {

                }


                ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                
            }
            catch(Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }

            
            
     
        }

        //public int lineNumber { get; set; }
        //public string validated { get; set; }
        //public string quantity { get; set; }
        //public string uom { get; set; }
        //public string ItemCode { get; set; }
        //public string ItemName { get; set; }
        //public string deleted { get; set; }
        [HttpPost]
        public ActionResult Save_SODetailsByStorage(string id, string storage, string idPicker, string pickername)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var company_bodega = "0";
            if (activeuser.ID_Company == 1)
            {
                company_bodega = "01";
            }
            else if (activeuser.ID_Company == 2)
            {
                company_bodega = "02";
            }

            string ttresult = "";
            try
            {

                var idf = Convert.ToInt32(id);
                List<MyObj> objects = new List<MyObj>();

                if (storage == "COOLER")
                {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER") && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "YES",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = a.query1
                               }).ToList();
                } else if (storage == "FREEZER") {
                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER") && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "YES",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = a.query1
                               }).ToList();
                }
                else {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && a.ID_storagetype == storage && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "YES",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = a.query1
                               }).ToList();
                }




                List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                List<Tb_PlanningSO_details> lsttodelete = new List<Tb_PlanningSO_details>();

                IQueryable<Tb_PlanningSO_details> allDet;


                if (storage == "COOLER")
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER")) select a);
                } else if (storage == "FREEZER") {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER")) select a);
                }
                else {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && a.ID_storagetype == storage) select a);
                }
                    
   
                foreach (var items in objects)
                {
                    Tb_PlanningSO_details newDet = (from a in allDet where (a.Line_num == items.lineNumber && a.ItemCode == items.ItemCode) select a).FirstOrDefault();

                    if (newDet.query2.Contains("3"))
                    {//Es una bonificacion
                     //var productonormal = dblim.Tb_PlanningSO_details.Where(a => a.ID_salesorder==idf && a.ItemCode == newDet.ItemCode && !a.query2.Contains("3")).FirstOrDefault();
                     //if (productonormal != null) {
                     //newDet.isvalidated = newDet.isvalidated;
                     //newDet.query1 = productonormal.query1;
                        newDet.Quantity = Convert.ToDecimal(items.quantity);

                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comando para eliminar bonificaciones u otro tipo de producto

                            //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1}", idf, items.ItemCode);




                        }
                        else { newDet.query1 = ""; }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_pickerWHS = idPicker;
                        newDet.Picker_nameWHS = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        lsttosave.Add(newDet);
                        //}


                    }
                    else
                    {
                        //newDet.ID_UOM = items.uom;
                        //newDet.UOM = items.uom;
                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comano para eliminar bonificaciones u otro tipo de producto
                            dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                        }
                        else { newDet.query1 = ""; }

                        //Evaluamos si la cantidad es menor a lo que colocaron, si es asi mandar propiedad DEL
                        if (!newDet.query2.Contains("0"))
                        {
                            if (newDet.UomCode.Contains("LBS"))
                            {

                            }
                            else {
                            if (Convert.ToDecimal(items.quantity) < newDet.Quantity)
                            {
                                newDet.query1 = "DEL";
                                //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                                if (newDet.type == "S")
                                {
                                    dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and parent={1}", idf, items.ItemCode);
                                }

                            }
                            }

                        }


                        if (items.quantity == "")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else if (items.quantity == "0")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else
                        {

                            newDet.Quantity = Convert.ToDecimal(items.quantity);

                        }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_pickerWHS = idPicker;
                        newDet.Picker_nameWHS = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        //Evaluamos si es kit para actualizar sus hijos
                        if (newDet.type == "S")
                        {
                            //llamamos los hijos
                            var hijos = (from b in dblim.Tb_PlanningSO_details where (b.parent == newDet.ItemCode && b.type == "I") select b).ToList();
                            if (hijos.Count > 0)
                            {
                                foreach (var item in hijos)
                                {
                                    item.query1 = newDet.query1;
                                    item.isvalidated = newDet.isvalidated;
                                    item.Quantity = Convert.ToDecimal(newDet.Quantity * item.childrendefqty);
                                    item.ID_picker = newDet.ID_picker;
                                    item.Picker_name = newDet.Picker_name;
                                    item.ID_userValidate = newDet.ID_userValidate;
                                    item.DateCheckOut = newDet.DateCheckOut;
                                    item.QC_totalCount = newDet.QC_totalCount;
                                    item.Transferred = newDet.Transferred;
                                }

                                dblim.BulkUpdate(hijos);

                            }
                        }

                        lsttosave.Add(newDet);
                    }
                    //Evaluamos si el producto se eliminio para actualizar todo despues
                    if (items.deleted == "DEL")
                    {
                        lsttodelete.Add(newDet);
                    }



                }

                dblim.BulkUpdate(lsttosave);

                //Eliminamos por producto
                try
                {
                    foreach (var itemdel in lsttodelete)
                    {
                        dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, itemdel.ItemCode);
                    }
                }
                catch
                {

                }


                ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        [HttpPost]
        public ActionResult Delete_SODetailsByStorage(string id, string storage, string idPicker, string pickername)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var company_bodega = "0";
            if (activeuser.ID_Company == 1)
            {
                company_bodega = "01";
            }
            else if (activeuser.ID_Company == 2)
            {
                company_bodega = "02";
            }

            string ttresult = "";
            try
            {

                var idf = Convert.ToInt32(id);
                List<MyObj> objects = new List<MyObj>();

                if (storage == "COOLER")
                {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER") && a.Transferred==2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = "DEL"
                               }).ToList();
                }
                else if (storage == "FREEZER")
                {
                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER") && a.Transferred == 2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = "DEL"
                               }).ToList();
                }
                else
                {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && a.ID_storagetype == storage && a.Transferred == 2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = "DEL"
                               }).ToList();
                }




                List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                List<Tb_PlanningSO_details> lsttodelete = new List<Tb_PlanningSO_details>();

                IQueryable<Tb_PlanningSO_details> allDet;


                if (storage == "COOLER")
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER") && a.Transferred == 2) select a);
                }
                else if (storage == "FREEZER")
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER") && a.Transferred == 2) select a);
                }
                else
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && a.ID_storagetype == storage && a.Transferred == 2) select a);
                }


                foreach (var items in objects)
                {
                    Tb_PlanningSO_details newDet = (from a in allDet where (a.Line_num == items.lineNumber && a.ItemCode == items.ItemCode) select a).FirstOrDefault();

                    if (newDet.query2.Contains("3"))
                    {//Es una bonificacion
                     //var productonormal = dblim.Tb_PlanningSO_details.Where(a => a.ID_salesorder==idf && a.ItemCode == newDet.ItemCode && !a.query2.Contains("3")).FirstOrDefault();
                     //if (productonormal != null) {
                     //newDet.isvalidated = newDet.isvalidated;
                     //newDet.query1 = productonormal.query1;
                        newDet.Quantity = Convert.ToDecimal(items.quantity);

                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comando para eliminar bonificaciones u otro tipo de producto

                            //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1}", idf, items.ItemCode);




                        }
                        else { newDet.query1 = ""; }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_picker = idPicker;
                        newDet.Picker_name = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        lsttosave.Add(newDet);
                        //}


                    }
                    else
                    {
                        //newDet.ID_UOM = items.uom;
                        //newDet.UOM = items.uom;
                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comano para eliminar bonificaciones u otro tipo de producto
                            dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                        }
                        else { newDet.query1 = ""; }

                        //Evaluamos si la cantidad es menor a lo que colocaron, si es asi mandar propiedad DEL
                        if (!newDet.query2.Contains("0"))
                        {
                            if (newDet.UomCode.Contains("LBS"))
                            {

                            }
                            else {
                            if (Convert.ToDecimal(items.quantity) < newDet.Quantity)
                            {
                                newDet.query1 = "DEL";
                                //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                                if (newDet.type == "S")
                                {
                                    dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and parent={1}", idf, items.ItemCode);
                                }

                            }
                            }

                        }


                        if (items.quantity == "")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else if (items.quantity == "0")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else
                        {

                            newDet.Quantity = Convert.ToDecimal(items.quantity);

                        }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_picker = idPicker;
                        newDet.Picker_name = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        //Evaluamos si es kit para actualizar sus hijos
                        if (newDet.type == "S")
                        {
                            //llamamos los hijos
                            var hijos = (from b in dblim.Tb_PlanningSO_details where (b.parent == newDet.ItemCode && b.type == "I") select b).ToList();
                            if (hijos.Count > 0)
                            {
                                foreach (var item in hijos)
                                {
                                    item.query1 = newDet.query1;
                                    item.isvalidated = newDet.isvalidated;
                                    item.Quantity = Convert.ToDecimal(newDet.Quantity * item.childrendefqty);
                                    item.ID_picker = newDet.ID_picker;
                                    item.Picker_name = newDet.Picker_name;
                                    item.ID_userValidate = newDet.ID_userValidate;
                                    item.DateCheckOut = newDet.DateCheckOut;
                                    item.QC_totalCount = newDet.QC_totalCount;
                                    item.Transferred = newDet.Transferred;
                                }

                                dblim.BulkUpdate(hijos);

                            }
                        }

                        lsttosave.Add(newDet);
                    }
                    //Evaluamos si el producto se eliminio para actualizar todo despues
                    if (items.deleted == "DEL")
                    {
                        lsttodelete.Add(newDet);
                    }



                }

                dblim.BulkUpdate(lsttosave);

                //Eliminamos por producto
                try
                {
                    foreach (var itemdel in lsttodelete)
                    {
                        dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, itemdel.ItemCode);
                    }
                }
                catch
                {

                }


                ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }


        [HttpPost]
        public ActionResult Restore_SODetailsByStorage(string id, string storage, string idPicker, string pickername)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var company_bodega = "0";
            if (activeuser.ID_Company == 1)
            {
                company_bodega = "01";
            }
            else if (activeuser.ID_Company == 2)
            {
                company_bodega = "02";
            }

            string ttresult = "";
            try
            {

                var idf = Convert.ToInt32(id);
                List<MyObj> objects = new List<MyObj>();

                if (storage == "COOLER")
                {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER") && a.Transferred == 2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = ""
                               }).ToList();
                }
                else if (storage == "FREEZER")
                {
                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER") && a.Transferred == 2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = ""
                               }).ToList();
                }
                else
                {

                    objects = (from a in dblim.Tb_PlanningSO_details
                               where (a.ID_salesorder == idf && a.Warehouse != company_bodega && a.ID_storagetype == storage && a.Transferred == 2 && !a.type.Contains("I"))
                               select new MyObj
                               {
                                   ItemCode = a.ItemCode,
                                   ItemName = a.ItemName,
                                   lineNumber = a.Line_num,
                                   validated = "NO",
                                   quantity = a.Quantity.ToString(),
                                   uom = a.UomCode,
                                   deleted = ""
                               }).ToList();
                }




                List<Tb_PlanningSO_details> lsttosave = new List<Tb_PlanningSO_details>();
                List<Tb_PlanningSO_details> lsttodelete = new List<Tb_PlanningSO_details>();

                IQueryable<Tb_PlanningSO_details> allDet;


                if (storage == "COOLER")
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "FREEZER") && a.Transferred == 2) select a);
                }
                else if (storage == "FREEZER")
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && (a.ID_storagetype == storage || a.ID_storagetype == "COOLER") && a.Transferred == 2) select a);
                }
                else
                {
                    allDet = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idf && !a.type.Contains("I") && a.Warehouse != company_bodega && a.ID_storagetype == storage && a.Transferred == 2) select a);
                }


                foreach (var items in objects)
                {
                    Tb_PlanningSO_details newDet = (from a in allDet where (a.Line_num == items.lineNumber && a.ItemCode == items.ItemCode) select a).FirstOrDefault();

                    if (newDet.query2.Contains("3"))
                    {//Es una bonificacion
                     //var productonormal = dblim.Tb_PlanningSO_details.Where(a => a.ID_salesorder==idf && a.ItemCode == newDet.ItemCode && !a.query2.Contains("3")).FirstOrDefault();
                     //if (productonormal != null) {
                     //newDet.isvalidated = newDet.isvalidated;
                     //newDet.query1 = productonormal.query1;
                        newDet.Quantity = Convert.ToDecimal(items.quantity);

                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comando para eliminar bonificaciones u otro tipo de producto

                            //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1}", idf, items.ItemCode);




                        }
                        else { newDet.query1 = ""; }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_picker = idPicker;
                        newDet.Picker_name = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        lsttosave.Add(newDet);
                        //}


                    }
                    else
                    {
                        //newDet.ID_UOM = items.uom;
                        //newDet.UOM = items.uom;
                        if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                        if (items.deleted == "DEL")
                        {
                            newDet.query1 = "DEL";
                            newDet.isvalidated = false;
                            //Se agrego linea de comano para eliminar bonificaciones u otro tipo de producto
                            dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                        }
                        else { newDet.query1 = ""; }

                        //Evaluamos si la cantidad es menor a lo que colocaron, si es asi mandar propiedad DEL
                        if (!newDet.query2.Contains("0"))
                        {
                            if (newDet.UomCode.Contains("LBS"))
                            {
                                //No se evalua ya que la cantidad puede tender a fallar por los decimales
                            }
                            else {
                            if (Convert.ToDecimal(items.quantity) < newDet.Quantity)
                            {
                                newDet.query1 = "DEL";
                                //dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, items.ItemCode);

                                if (newDet.type == "S")
                                {
                                    dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and parent={1}", idf, items.ItemCode);
                                }

                            }
                            }

                        }


                        if (items.quantity == "")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else if (items.quantity == "0")
                        {
                            newDet.Quantity = 0;
                            newDet.query1 = "DEL";
                        }
                        else
                        {

                            newDet.Quantity = Convert.ToDecimal(items.quantity);

                        }



                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == items.ItemCode && a.UomCode == items.uom) select a).FirstOrDefault();
                        if (lstava != null)
                        {
                            newDet.UomCode = lstava.UomCode;
                            newDet.UomEntry = lstava.UomEntry.ToString();
                            newDet.NumPerMsr = Convert.ToDecimal(lstava.Units);
                        }

                        newDet.ID_picker = idPicker;
                        newDet.Picker_name = pickername;


                        if (activeuser != null)
                        {
                            newDet.ID_userValidate = activeuser.ID_User;
                        }
                        newDet.DateCheckOut = DateTime.UtcNow;

                        //Evaluamos si es kit para actualizar sus hijos
                        if (newDet.type == "S")
                        {
                            //llamamos los hijos
                            var hijos = (from b in dblim.Tb_PlanningSO_details where (b.parent == newDet.ItemCode && b.type == "I") select b).ToList();
                            if (hijos.Count > 0)
                            {
                                foreach (var item in hijos)
                                {
                                    item.query1 = newDet.query1;
                                    item.isvalidated = newDet.isvalidated;
                                    item.Quantity = Convert.ToDecimal(newDet.Quantity * item.childrendefqty);
                                    item.ID_picker = newDet.ID_picker;
                                    item.Picker_name = newDet.Picker_name;
                                    item.ID_userValidate = newDet.ID_userValidate;
                                    item.DateCheckOut = newDet.DateCheckOut;
                                    item.QC_totalCount = newDet.QC_totalCount;
                                    item.Transferred = newDet.Transferred;
                                }

                                dblim.BulkUpdate(hijos);

                            }
                        }

                        lsttosave.Add(newDet);
                    }
                    //Evaluamos si el producto se eliminio para actualizar todo despues
                    if (items.deleted == "DEL")
                    {
                        lsttodelete.Add(newDet);
                    }



                }

                dblim.BulkUpdate(lsttosave);

                //Eliminamos por producto
                try
                {
                    foreach (var itemdel in lsttodelete)
                    {
                        dblim.Database.ExecuteSqlCommand("update Tb_PlanningSO_details set query1='DEL' where ID_salesorder={0} and ItemCode={1} and Quantity > 0", idf, itemdel.ItemCode);
                    }
                }
                catch
                {

                }


                ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }


        public ActionResult Print_Roadmap(int? id)
        {
            var header = (from b in dblim.Tb_Planning where (b.ID_Route == id) select b).FirstOrDefault();

            //var details = dblim.Tb_PlanningSO.Where(g => g.ID_Route==id).GroupBy(x => x.Customer_name).Select(g => g.FirstOrDefault()).OrderBy(c=>c.query3);
            var details = dblim.Tb_PlanningSO.Where(g => g.ID_Route==id).OrderBy(c=>c.query3).ToList();
            details = details.GroupBy(x => x.Customer_name).Select(g => g.FirstOrDefault()).OrderBy(c => c.query3).ToList();

            foreach (var item in details) { //Agregamos el payment method
                var customer = (from a in dlipro.BI_Dim_Customer where (a.id_Customer == item.ID_customer) select a).FirstOrDefault();
                if (customer != null) {
                    item.query5 = customer.PymntGroup;
                }
            }


            //INFO UOM
            var listafinalSO = details.ToList();
            var sol = listafinalSO.Select(c => c.ID_salesorder).ToArray();
            //Verificamos detalles para sacar CASE y EACH totales y luego promediar todal de EACH en base a CASES
            var detallesSo = (from f in dblim.Tb_PlanningSO_details where (sol.Contains(f.ID_salesorder)) select f).ToList();

            var totalCantEach = 0;
            var totalCantCases = 0;
            var totalCantPack = 0;
            var totalCantLbs = 0;
  
            //Para calcular el promedio lo hacemos diviendo
            try
            {
                if (detallesSo.Count() > 0)
                {
                    totalCantEach = detallesSo.Where(c => c.UomCode.Contains("EACH")).Count();
                    totalCantCases = detallesSo.Where(c => c.UomCode.Contains("CASE")).Count();
                    totalCantPack = detallesSo.Where(c => c.UomCode.Contains("PACK")).Count();
                    totalCantLbs = detallesSo.Where(c => c.UomCode.Contains("LBS")).Count();

                }
                else
                {
                    totalCantEach = 0;
                    totalCantCases = 0;
                    totalCantPack = 0;
                    totalCantLbs = 0;


                }
            }
            catch
            {
                totalCantEach = 0;
                totalCantCases = 0;
                totalCantPack = 0;
                totalCantLbs = 0;

            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "rptRoadmap.rpt"));
            rd.SetDataSource(details);

            rd.SetParameterValue("Route", header.Route_name.ToUpper());
            rd.SetParameterValue("Driver", header.Driver_name);
            rd.SetParameterValue("Route_leader", header.Routeleader_name);
            rd.SetParameterValue("Truck", header.Truck_name);
            rd.SetParameterValue("Date", header.Date.ToLongDateString().ToUpper());


            rd.SetParameterValue("totalEach", totalCantEach.ToString());
            rd.SetParameterValue("totalCase", totalCantCases.ToString());
            rd.SetParameterValue("totalPack", totalCantPack.ToString());
            rd.SetParameterValue("totalLBS", totalCantLbs.ToString());


            //rd.SetParameterValue("idorder", header.ID_OrderDSD);
            //rd.SetParameterValue("comment", header.Comment);


            var filePathOriginal = Server.MapPath("/Reports/pdf");
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            //PARA VISUALIZAR
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "RoadMap.pdf; ");
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }


        public ActionResult Route_WHSInvoices(string activityname)
        {
            // retrieve byte array here
            List<byte[]> myStream =  TempData["Output"] as List<byte[]>;
            //var filePathOriginal = Server.MapPath("/Reports/pdf");
            //var path2 = Path.Combine(filePathOriginal, "Route_WHSInvoices.pdf");

            var report = concatAndAddContent(myStream);
            if (report != null)
            {

                //Con eso se descarga
                //Response.AddHeader("Content-Disposition", "attachment; filename=" + "Route_WHSInvoices.pdf");
                return File(report, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            else
            {
                return new EmptyResult();
            }
        }

        public ActionResult DailyPaymentCrossDock(string activityname)
        {
            // retrieve byte array here
            List<byte[]> myStream =  TempData["Output"] as List<byte[]>;
            //var filePathOriginal = Server.MapPath("/Reports/pdf");
            //var path2 = Path.Combine(filePathOriginal, "Route_WHSInvoices.pdf");

            var report = concatAndAddContent(myStream);
            if (report != null)
            {

                //Con eso se descarga
                //Response.AddHeader("Content-Disposition", "attachment; filename=" + "Route_WHSInvoices.pdf");
                return File(report, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            else
            {
                return new EmptyResult();
            }
        }

        public ActionResult Print_routeWHS(int id_route)
        {
            try
            {
                var so = (from a in dblim.Tb_PlanningSO where (a.ID_Route == id_route && a.DocEntry !="--") select a).ToList();

                //var quemadosSO = new List<string>();
                //quemadosSO.Add("78492");
                //quemadosSO.Add("78497");
                if (so.Count > 0)
                {
                    //Aca iria ciclo foreach

                    MemoryStream finalStream = new MemoryStream();
                    //PdfCopyFields copy = new PdfCopyFields(finalStream);
                    List<byte[]> listafinal = new List<byte[]>();
                    foreach (var item in so)
                    {
                        //Buscamos datos en la vista maestra de facturas
                        var sqlQueryText = dlipro.sp_genericInvoice(item.DocEntry.ToString()).ToList();
                        foreach (var iteminterno in sqlQueryText)
                        {
                            iteminterno.UomCode = "";
                        }

                        ReportDocument rd = new ReportDocument();

                        rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Invoice Storage Type (RR).rpt"));

                        rd.SetDataSource(sqlQueryText);
                        //rd.DataSourceConnections.Clear();
                  
                        //ConnectionInfo connectInfo = new ConnectionInfo()
                        //{
                        //    //ServerName = ".",
                        //    //DatabaseName = "DLI_PRO",
                        //    //UserID = "sa",
                        //    //Password = "sa123"         
                        //    ServerName = "192.168.1.14",
                        //    DatabaseName = "PEPPERI_TEST",
                        //    UserID = "sa",
                        //    Password = "DiLimen@2018"
                        //};
                        //rd.SetDatabaseLogon("sa", "DiLimen@2018", "192.168.1.14", "PEPPERI_TEST");
                        //foreach (Table tbl in rd.Database.Tables)
                        //{
                        //    tbl.LogOnInfo.ConnectionInfo = connectInfo;
                        //    tbl.ApplyLogOnInfo(tbl.LogOnInfo);
                        //}

                        //var filePathOriginal = Server.MapPath("/Reports/pdf");

                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.AppendHeader("Content-Disposition", "inline; filename=Route_WHSInvoices.pdf;");
                        byte[] getBytes = null;
                        Stream ms = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        ms.Seek(0, SeekOrigin.Begin);
                        getBytes = ReadFully(ms);
                        listafinal.Add(getBytes);
                        //para sacar la copia
                        listafinal.Add(getBytes);
                        ms.Dispose();
                        //var path2 = Path.Combine(filePathOriginal, id_route + "Route_WHSInvoices.pdf");


                        //rd.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path2);

                        //var ms1 = new MemoryStream(getBytes);
                        //ms1.Position = 0;
                        //copy.AddDocument(new PdfReader(ms1));
                        //ms1.Dispose();

                        //PARA VISUALIZAR
                        //Response.AppendHeader("Content-Disposition", "inline; filename=Route_WHSInvoices.pdf;");
                        //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        //stream.Seek(0, SeekOrigin.Begin);

                        //stream.Position = 0;
                        //copy.AddDocument(new PdfReader(stream));
                        //stream.Dispose();
                    }

     


                    //Nueva descarga
                    TempData["Output"] = listafinal;

                    //return Json(urlcontent);
                    return Json("Success", JsonRequestBehavior.AllowGet);

                }
                else
                {

                    return Json("CountError", JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex) {
                //return Json(urlcontent);
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }



        public ActionResult Print_WHSDailyPayment(int id_route)
        {
            try
            {
                var so = (from a in dblim.Tb_Planning where (a.ID_Route == id_route) select a).FirstOrDefault();

                
                //var quemadosSO = new List<string>();
                //quemadosSO.Add("78492");
                //quemadosSO.Add("78497");
                if (so !=null)
                {
                    //Aca iria ciclo foreach

                    MemoryStream finalStream = new MemoryStream();
                    //PdfCopyFields copy = new PdfCopyFields(finalStream);
                    List<byte[]> listafinal = new List<byte[]>();

                    //Buscamos datos en la vista maestra de facturas
                    var dateshor = so.Departure.ToShortDateString();
                        var sqlQueryText = dlipro.sp_genericDailyPaymentsCrossDock(so.ID_truck, dateshor).ToList();

                    foreach (var item in sqlQueryText) {
                        if (item.RouteLeader == null) { item.RouteLeader = ""; }
                        if (item.Driver == null) { item.Driver = ""; }
                    }
                        ReportDocument rd = new ReportDocument();

                        rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Daily Payments Add Copies - Whs 01 & 02.rpt"));

                    rd.SetDataSource(sqlQueryText);
                    
                    rd.SetParameterValue("Date", so.Departure.ToShortDateString());
                    rd.SetParameterValue("Truck", so.ID_truck);


                    Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.AppendHeader("Content-Disposition", "inline; filename=DailyPaymentCrossDock.pdf;");
                        byte[] getBytes = null;
                        Stream ms = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        ms.Seek(0, SeekOrigin.Begin);
                        getBytes = ReadFully(ms);
                        listafinal.Add(getBytes);
                        //para sacar la copia
                        //listafinal.Add(getBytes);
                        ms.Dispose();





                    //Nueva descarga
                    TempData["Output"] = listafinal;

                    //return Json(urlcontent);
                    return Json("Success", JsonRequestBehavior.AllowGet);

                }
                else
                {

                    return Json("CountError", JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                //return Json(urlcontent);
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }



        public ActionResult Print_DPCD(int id_route)
        {
            var so = (from a in dblim.Tb_Planning where (a.ID_Route == id_route) select a).FirstOrDefault();

            if (so !=null)
            {
                var dateshor = so.Departure.ToShortDateString();
                var sqlQueryText = dlipro.sp_genericDailyPaymentsCrossDock(so.ID_truck, dateshor).ToList();



                if (sqlQueryText.Count > 0)
                {


                    foreach (var item in sqlQueryText)
                    {
                        if (item.RouteLeader == null) { item.RouteLeader = ""; }
                        if (item.Driver == null) { item.Driver = ""; }
                    }



                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Daily Payments Add Copies - Whs 01 & 02.rpt"));


                    
                    rd.SetDataSource(sqlQueryText);
                    rd.Database.Tables[0].SetDataSource(sqlQueryText);

                    var filePathOriginal = Server.MapPath("/Reports/pdfReports");

                    Response.Buffer = false;

                    Response.ClearContent();

                    Response.ClearHeaders();


                    //PARA VISUALIZAR
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + "DailyPaymentCrossDock.pdf; ");

                   
                    rd.SetParameterValue("Date", so.Departure);
                    rd.SetParameterValue("Truck", so.ID_truck);
                    rd.SetParameterValue("DRoute", so.Route_name);
                    rd.SetParameterValue("IDrouteazure", so.ID_Route);
                    rd.SetParameterValue("DateString", so.Departure.ToShortDateString());

                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

                    stream.Seek(0, SeekOrigin.Begin);



                    return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);

                }
                else
                {
                    TempData["advertencia"] = "No details found.";
                    return RedirectToAction("QualityControl_planning", "Invoices", null);
                }
            }
            else
            {
                TempData["advertencia"] = "No data found.";
                return RedirectToAction("QualityControl_planning", "Invoices", null);
            }
        }



        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        

private static Byte[] ConvertList(List<Byte[]> list)
    {
        List<Byte> tmpList = new List<byte>();
        foreach (Byte[] byteArray in list)
            foreach (Byte singleByte in byteArray)
                tmpList.Add(singleByte);
        return tmpList.ToArray();
    }

        public static byte[] concatAndAddContent(List<byte[]> pdf)
        {
            byte[] all;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document();

                PdfWriter writer = PdfWriter.GetInstance(doc, ms);

                doc.SetPageSize(PageSize.LETTER);
                doc.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page;

                PdfReader reader;
                foreach (byte[] p in pdf)
                {
                    reader = new PdfReader(p);
                    int pages = reader.NumberOfPages;

                    // loop over document pages
                    for (int i = 1; i <= pages; i++)
                    {
                        doc.SetPageSize(PageSize.LETTER);
                        doc.NewPage();
                        page = writer.GetImportedPage(reader, i);
                        cb.AddTemplate(page, 0, 0);
                    }
                }

                doc.Close();
                all = ms.GetBuffer();
                ms.Flush();
                ms.Dispose();
            }

            return all;
        }

    }
}