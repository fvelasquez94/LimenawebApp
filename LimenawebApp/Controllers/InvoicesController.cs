using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LimenawebApp.Controllers
{
    public class InvoicesController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
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
        }

        public ActionResult Planning()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

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
                rutaslst = (from a in dblim.Tb_Planning where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate) select a).ToList();

                List<Routes_calendar> rutas = new List<Routes_calendar>();

                foreach (var item in rutaslst)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_Route + " - " + item.Route_name;
                    rt.url = "";
                    rt.start = item.Departure.ToString("yyyy-MM-dd");
                    //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd");
                    rt.route_leader = item.Routeleader_name;
                    rt.className = ".fc-event";
                    rt.driver = item.Driver_name;
                    rt.truck = item.Truck_name;
                    rt.departure = item.Departure.ToShortTimeString();
                    rutas.Add(rt);
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutas.ToArray());
                ViewBag.calroutes = result;
                ///
 
                //SELECTS
          
                var drivers = dlipro.C_DRIVERS.ToList();
                //Convertimos la lista a array
                ArrayList myArrListDrivers = new ArrayList();
                myArrListDrivers.AddRange((from p in drivers
                                    select new
                                    {
                                        id = p.Code,
                                        text = p.Name.ToUpper()
                                    }).ToList());

                ViewBag.drivers = JsonConvert.SerializeObject(myArrListDrivers);
                //LISTADO DE Routes Leader
                var routeleader = dlipro.C_HELPERS.ToList();
                //Convertimos la lista a array
                ArrayList myArrListrouteleader = new ArrayList();
                myArrListrouteleader.AddRange((from p in routeleader
                                           select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper()
                                           }).ToList());

                ViewBag.routeleaders = JsonConvert.SerializeObject(myArrListrouteleader);
                //LISTADO DE Trucks
                var trucks = dlipro.C_TRUCKS.ToList();
                //Convertimos la lista a array
                ArrayList myArrListtruck = new ArrayList();
                myArrListtruck.AddRange((from p in trucks
                                           select new
                                           {
                                               id = p.Code,
                                               text = p.Name.ToUpper()
                                           }).ToList());

                ViewBag.trucks = JsonConvert.SerializeObject(myArrListtruck);
                //LISTADO DE Rutas
                var mainroutes = dlipro.C_DROUTE.ToList();
                //Convertimos la lista a array
                ArrayList myArrListmainroutes = new ArrayList();
                myArrListmainroutes.AddRange((from p in mainroutes
                                         select new
                                         {
                                             id = p.Code,
                                             text = p.Name
                                         }).ToList());

                ViewBag.mainroutes = JsonConvert.SerializeObject(myArrListmainroutes);
                //FIN HEADER
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult QualityControl_planning()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

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
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                //var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //var saturday = sunday.AddMonths(1).AddDays(-1);
                //FILTROS**************

                filtrostartdate = sunday;
                filtroenddate = saturday;
                //FIN FILTROS*******************


                List<Tb_Planning> lstPlanning = new List<Tb_Planning>();
                lstPlanning = (from a in dblim.Tb_Planning select a).ToList();

                var ArrayPlanning = lstPlanning.Select(a => a.ID_Route).ToArray();

                List<Tb_PlanningSO> SalesOrder = new List<Tb_PlanningSO>();
                SalesOrder = (from b in dblim.Tb_PlanningSO where (ArrayPlanning.Contains(b.ID_Route)) select b).ToList();

                //ESTADISTICA DE RUTAS POR ESTADO DE VISITAS
                decimal totalRutas = lstPlanning.Count();
                foreach (var rutait in lstPlanning)
                {

                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceled = (from e in SalesOrder where ((e.isfinished == true) && e.ID_Route == rutait.ID_Route) select e).Count();
 
                    totalRutas = (from e in SalesOrder where (e.ID_Route == rutait.ID_Route) select e).Count();             

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
                    }
                }


                ViewBag.lstPlanning = lstPlanning;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult QualityControl(int id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

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
                //DateTime filtrostartdate;
                //DateTime filtroenddate;

                //filtros de fecha //SEMANAL
                //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                //var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                //var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                //var saturday = sunday.AddMonths(1).AddDays(-1);
                //FILTROS**************

                //filtrostartdate = sunday;
                //filtroenddate = saturday;
                //FIN FILTROS*******************


                List<Tb_PlanningSO> lstSalesOrders = new List<Tb_PlanningSO>();
                lstSalesOrders = (from a in dblim.Tb_PlanningSO where(a.ID_Route == id) select a).ToList();

                List<Tb_PlanningSO_details> lstDetails = new List<Tb_PlanningSO_details>();
                lstDetails = (from b in dblim.Tb_PlanningSO_details where(b.Quantity>0) select b).ToList();


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
                    }
                }


                ViewBag.lstDetails = lstDetails;
                ViewBag.lstSO = lstSalesOrders;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult GetEvents(DateTime startf, DateTime endf)
        {
            try
            {
                var lstRoutes = dblim.Tb_Planning.Where(dc => dc.Departure >= startf && dc.Departure <= endf).OrderByDescending(dc => dc.ID_Route).ToList();

                List<Routes_calendar> rutaslst = new List<Routes_calendar>();

                foreach (var item in lstRoutes)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_Route + " - " + item.Route_name;
                    rt.url = "";
                    rt.start = item.Departure.ToString("yyyy-MM-dd");
                    //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd");
                    rt.route_leader = item.Routeleader_name;
                    rt.className = ".fc-event";
                    rt.driver = item.Driver_name;
                    rt.truck = item.Truck_name;
                    rt.departure = item.Departure.ToShortTimeString();
                    rutaslst.Add(rt);
                }
                //}
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
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

        public ActionResult GetSalesOrders(string id, DateTime date)
        {

            int idr = Convert.ToInt32(id);
            //var salesOrders = new List<Tb_PlanningSO>();
            DateTime selDate = Convert.ToDateTime(date).AddDays(1);
            selDate = selDate.Date;
            Tb_Planning rt = (from a in dblim.Tb_Planning where (a.ID_Route == idr) select a).FirstOrDefault();

            //salesOrders = (from obj in dbcmk.VisitsM where (obj.ID_route == idr) select obj).ToList();
            var lst = (from obj in dblim.Tb_PlanningSO where (obj.ID_Route == idr) select new { id = obj.ID_salesorder, NumSO=obj.SAP_docnum, CardCode = obj.ID_customer, CustomerName = obj.Customer_name , DeliveryRoute = obj.query2, SalesPerson = obj.Rep_name, SODate = obj.SAP_docdate, OpenAmount = obj.Amount, Weight= obj.Weight, Volume = obj.Volume, Printed= obj.Printed }).ToArray();
            var lstArray = (from obj in dblim.Tb_PlanningSO select  obj.SAP_docnum).ToArray();

            List<int> myCollection = new List<int>();

            foreach (var item in lstArray) {
                myCollection.Add(Convert.ToInt32(item));
            }
            var lstOpenSales = (from obj in dlipro.OpenSalesOrders where(!myCollection.Contains(obj.NumSO)) select new { NumSO=obj.NumSO,CardCode = obj.CardCode, CustomerName = obj.CustomerName , DeliveryRoute = obj.DeliveryRoute, SalesPerson = obj.SalesPerson, SODate = obj.SODate, TotalSO = obj.TotalSO, OpenAmount = obj.OpenAmount, Remarks = obj.Remarks, Printed= obj.Printed }).ToArray();

            var lstRoutes = (from a in lstOpenSales select a.DeliveryRoute).OrderBy(a=>a).Distinct().ToArray();
            var lstReps = (from a in lstOpenSales select a.SalesPerson).OrderBy(a=>a).Distinct().ToArray();
            var lstCustomers = (from a in lstOpenSales select a.CustomerName).OrderBy(a=>a).Distinct().ToArray();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result2 = javaScriptSerializer.Serialize(lst);
            string result3 = javaScriptSerializer.Serialize(lstOpenSales);
            string result4 = javaScriptSerializer.Serialize(lstRoutes);
            string result5 = javaScriptSerializer.Serialize(lstReps);
            string result6 = javaScriptSerializer.Serialize(lstCustomers);
            var result = new { result = result2, result2 = result3 , result3 = result4, result4=result5, result5=result6};
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetSalesOrdersDetails(string id)
        {

            int idr = Convert.ToInt32(id);
            //var salesOrders = new List<Tb_PlanningSO>();

            var rt = (from a in dblim.Tb_PlanningSO_details where (a.ID_salesorder == idr && a.Quantity>0) select new { ID_detail = a.ID_detail, Line_num=a.Line_num, Bin_loc = a.Bin_loc, Quantity = a.Quantity, ID_UOM = a.ID_UOM, UOM = a.UOM, ItemCode = a.ItemCode, ItemName = a.ItemName,
            StockWhs01 = a.StockWhs01, isvalidated = a.isvalidated, ID_storagetype = a.ID_storagetype, Storage_type = a.Storage_type, ID_salesorder = a.ID_salesorder, query1 = a.query1}).ToArray();



            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result2 = javaScriptSerializer.Serialize(rt);
         
            var result = new { result = result2 };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateRoutePlanning(string routeName, string lstMasterCode, string lstMasterName, string lstDriverCode, string lstDriverName, string lstTruckCode, string lstTruckName, string lstRLeaderCode, string lstRLeaderName, DateTime departure)
        {
            try
            {
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

                dblim.Tb_Planning.Add(newPlanning);
                dblim.SaveChanges();
                
                var lstArray = (from obj in dblim.Tb_PlanningSO select obj.SAP_docnum).ToArray();
                //var lstArray = (from obj in dblim.Tb_PlanningSO where (obj.isfinished == false && obj.DocEntry == "") select obj.SAP_docnum).ToArray();

                List<int> myCollection = new List<int>();

                foreach (var item in lstArray)
                {
                    myCollection.Add(Convert.ToInt32(item));
                }
                var salesOrders = (from a in dlipro.OpenSalesOrders where (a.DeliveryRoute == lstMasterName && !myCollection.Contains(a.NumSO)) select a).ToList();

                var avaSO = (from a in salesOrders select a.NumSO).ToArray();

                if(salesOrders.Count > 0)
                {
                    var dtSO_details = (from c in dlipro.OpenSalesOrders_Details where (avaSO.Contains(c.DocNum)) select c).ToList();
                    try {
                        foreach (var saleOrder in salesOrders)
                        {
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
                            newSO.query4 = "";
                            newSO.query5 = "";                  
                            newSO.Weight = "";
                            newSO.Volume = "";
                            newSO.Printed = saleOrder.Printed;
                            newSO.ID_Route = newPlanning.ID_Route;
                            newSO.DocEntry = "";

                            dblim.Tb_PlanningSO.Add(newSO);
                            dblim.SaveChanges();



                            var detailslst = (from a in dtSO_details where (a.DocNum == saleOrder.NumSO) select a).ToList();

                            if (detailslst.Count > 0) {
                                foreach (var dt in detailslst) {
                                    Tb_PlanningSO_details newDtl = new Tb_PlanningSO_details();
                                    newDtl.Line_num = dt.LineNum;
                                    if (dt.U_BinLocation == null) { newDtl.Bin_loc = ""; } else { newDtl.Bin_loc = dt.U_BinLocation; }
                             
                                    newDtl.Quantity = Convert.ToInt32(dt.Quantity);
                                    newDtl.ID_UOM = dt.UomEntry.ToString();
                                    newDtl.UOM = Convert.ToInt32(dt.NumPerMsr).ToString();
                                    newDtl.ItemCode = dt.ItemCode;
                                    newDtl.ItemName = dt.ItemName;
                                    newDtl.StockWhs01 = "";
                                    newDtl.isvalidated = false;
                                    if(dt.U_Storage==null) { newDtl.ID_storagetype = ""; } else { newDtl.ID_storagetype = dt.U_Storage; }
                                    if (dt.U_Storage == null) { newDtl.Storage_type = ""; } else { newDtl.Storage_type = dt.U_Storage; }
                                    newDtl.ID_salesorder = newSO.ID_salesorder;
                                    newDtl.query1 = "";
                                    newDtl.query2 = "";
                                    newDtl.ID_picker = "";
                                    newDtl.Picker_name = "";

                                    dblim.Tb_PlanningSO_details.Add(newDtl);
                                    
                                }
                                
                                dblim.SaveChanges();
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
                    rutaslst.Add(rt);
                

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveSalesOrders(string[] salesOrders, string id_route)
        {
       
            try
            {
                dblim.Configuration.ValidateOnSaveEnabled = false;
                if (salesOrders == null)
                {
                    var id = Convert.ToInt32(id_route);
                    var vp = dblim.Tb_PlanningSO.Where(a => a.ID_Route == id);

                    var todelete = (from f in vp select f.ID_salesorder).ToArray();
                    var vpdet = dblim.Tb_PlanningSO_details.Where(a => todelete.Contains(a.ID_salesorder));
                    dblim.Tb_PlanningSO_details.RemoveRange(vpdet);
                    dblim.SaveChanges();


                    dblim.Tb_PlanningSO.RemoveRange(vp);
                    dblim.SaveChanges();

                    var rrresult = "SUCCESS";
                    return Json(rrresult, JsonRequestBehavior.AllowGet);
                }
                else {
                    if (salesOrders.Length > 0 || salesOrders != null)
                    {
                        var id = Convert.ToInt32(id_route);
                        var soexist = (from f in dblim.Tb_PlanningSO where (f.ID_Route == id) select f.SAP_docnum).ToArray();

                        var fincoll = (from a in soexist where (!salesOrders.Contains(a)) select a).ToArray();

                        
                        Tb_Planning planning = new Tb_Planning();
                        planning = dblim.Tb_Planning.Find(id);
                        if (fincoll.Length > 0) {
                            
                            var todelete = (from f in dblim.Tb_PlanningSO where (f.ID_Route == id && fincoll.Contains(f.SAP_docnum)) select f.ID_salesorder).ToArray();
                            var vpdet = dblim.Tb_PlanningSO_details.Where(a => todelete.Contains(a.ID_salesorder));
                            dblim.Tb_PlanningSO_details.RemoveRange(vpdet);
                            dblim.SaveChanges();


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


                        var dtSO_details = (from c in dlipro.OpenSalesOrders_Details where (myCollection.Contains(c.DocNum)) select c).ToList();
                        foreach (string value in salesOrders)
                        {
                            var idso = Convert.ToInt32(value);
                            var saleOrder = (from a in dtSO where (a.NumSO == idso) select a).First();

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

                            dblim.Tb_PlanningSO.Add(newSO);
                            dblim.SaveChanges();
                            //List<Tb_PlanningSO_details> lstsave = new List<Tb_PlanningSO_details>();
                            var detailslst2 = (from a in dtSO_details where (a.DocNum == saleOrder.NumSO) select a).ToList();

                            if (detailslst2.Count > 0)
                            {
                                foreach (var dt in detailslst2)
                                {
                                    Tb_PlanningSO_details newDtl = new Tb_PlanningSO_details();
                                    newDtl.Line_num = dt.LineNum;
                                    if (dt.U_BinLocation == null) { newDtl.Bin_loc = ""; } else { newDtl.Bin_loc = dt.U_BinLocation; }

                                    newDtl.Quantity = Convert.ToInt32(dt.Quantity);
                                    newDtl.ID_UOM = dt.UomEntry.ToString();
                                    newDtl.UOM = Convert.ToInt32(dt.NumPerMsr).ToString();
                                    newDtl.ItemCode = dt.ItemCode;
                                    newDtl.ItemName = dt.ItemName;
                                    newDtl.StockWhs01 = "";
                                    newDtl.isvalidated = false;
                                    if (dt.U_Storage == null) { newDtl.ID_storagetype = ""; } else { newDtl.ID_storagetype = dt.U_Storage; }
                                    if (dt.U_Storage == null) { newDtl.Storage_type = ""; } else { newDtl.Storage_type = dt.U_Storage; }
                                    newDtl.ID_salesorder = newSO.ID_salesorder;
                                    newDtl.query1 = "";
                                    newDtl.query2 = "";
                                    newDtl.ID_picker = "";
                                    newDtl.Picker_name = "";
                                    dblim.Tb_PlanningSO_details.Add(newDtl);
                                    
                                }
                                dblim.SaveChanges();
                            }

                        }


                        string ttresult = "SUCCESS";
                        return Json(ttresult, JsonRequestBehavior.AllowGet);
                    }
                    else {
                        string result = "ERROR";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }



            }
            catch (Exception ex)
            {
                string result = "ERROR";
                return Json(result, JsonRequestBehavior.AllowGet);
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

        public ActionResult Save_SODetails(string id, List<MyObj> objects)
        {
            var idf = Convert.ToInt32(id);
            if (objects != null) {
                //dblim.Configuration.ValidateOnSaveEnabled = false;
                foreach (var items in objects) {
                    Tb_PlanningSO_details newDet = (from a in dblim.Tb_PlanningSO_details where(a.Line_num == items.lineNumber && a.ItemCode == items.ItemCode) select a).FirstOrDefault();
                    newDet.Quantity = Convert.ToInt32(items.quantity);
                    //newDet.ID_UOM = items.uom;
                    //newDet.UOM = items.uom;
                    if (items.validated == "YES") { newDet.isvalidated = true; } else { newDet.isvalidated = false; }
                    if (items.deleted == "DEL") { newDet.query1 = "DEL"; } else { newDet.query1 = ""; }


                    dblim.Entry(newDet).State = EntityState.Modified;
                    
                }
                dblim.SaveChanges();
            }
            dblim.Dispose();
            string ttresult = "SUCCESS";
            return Json(ttresult, JsonRequestBehavior.AllowGet);
        }

    }
}