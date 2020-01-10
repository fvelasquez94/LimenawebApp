using ClosedXML.Excel;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LimenawebApp.Controllers
{
    public class OperationsController : Controller
    {
        // GET: Operations
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        //CLASS GENERAL
        private clsGeneral generalClass = new clsGeneral();
        private MatrizComprasEntities dbMatriz = new MatrizComprasEntities();


        public ActionResult Initial_forecast(int iddata)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                var data = (from a in dbMatriz.PurchaseData_product where(a.ID_purchaseData== iddata) select a).ToList();
                //var ids = (from a in data where (a.ID_purchaseData== iddata) select a.ItemCode).ToArray();

                //var forecast = dbMatriz.Database.SqlQuery<Forecast_Cajas>("Select * from Forecast_Cajas").ToList<Forecast_Cajas>();
                //forecast.Where(c => ids.Contains(c.CodProducto)).ToList();
                //Tenemos todos los datos del forecast
                //List<Initial_forecastData> initial_forecast = new List<Initial_forecastData>();
                //foreach (var forec in data)
                //{
                //    Initial_forecastData newforrep = new Initial_forecastData();
                //    newforrep.CodProducto = forec.ItemCode;
                //    newforrep.Producto = data.Where(c => c.ItemCode == forec.ItemCode).Select(c => c.Description).FirstOrDefault();
                //    newforrep.b1 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).FirstOrDefault();
                //    newforrep.b2 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(1).FirstOrDefault();
                //    newforrep.b3 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(2).FirstOrDefault();
                //    newforrep.b4 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(3).FirstOrDefault();
                //    newforrep.b5 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(4).FirstOrDefault();
                //    newforrep.f1 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.Forecast ).FirstOrDefault();
                //    newforrep.f2 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(1).FirstOrDefault();
                //    newforrep.f3 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(2).FirstOrDefault();
                //    newforrep.f4 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(3).FirstOrDefault();
                //    newforrep.f5 = forecast.Where(c => c.CodProducto == forec.ItemCode).Select(c => c.BaseLine ).Skip(4).FirstOrDefault();

                //    initial_forecast.Add(newforrep);
                //}


                ViewBag.iddata = iddata;


                //return View(initial_forecast);
                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public ActionResult Product_stats(string ItemCode, int data)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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
                var product = dbMatriz.PurchaseData_product.Where(c => c.ID_purchaseData == data && c.ItemCode == ItemCode).FirstOrDefault();

                ViewBag.iddata = data;
                ViewBag.idproduct = product.ItemCode;

                var salesHistory = dlipro.Database.SqlQuery<salesHistory_product>("Select * from BI_Sales_History_Matriz_Compras_Items where ItemCode={0} ORDER BY ITEMCODE , OffSetPeriod", ItemCode).ToList<salesHistory_product>();
               
                List<history_statsProduct> listsalesChart = new List<history_statsProduct>();
                //Recorremos para asignar valores
                for (var i = 1; i < 14; i++)
                {
                    history_statsProduct newperiod = new history_statsProduct();

                    var prd = "";
                    if (i >= 10)
                    {
                        prd = i.ToString();
                    }
                    else {
                        prd = ("0" + i);
                    }

                        newperiod.category = "P" + i;
                        newperiod.a2018 = Math.Round(Convert.ToDouble(salesHistory.Where(a => a.period == prd && a.Year_DLI == 2018).Select(a => a.LineTotal).FirstOrDefault()), 2);
                        newperiod.a2019 = Math.Round(Convert.ToDouble(salesHistory.Where(a => a.period == prd && a.Year_DLI == 2019).Select(a => a.LineTotal).FirstOrDefault()), 2);
                    newperiod.a2020 = Math.Round(Convert.ToDouble(salesHistory.Where(a => a.period == prd && a.Year_DLI == 2020).Select(a => a.LineTotal).FirstOrDefault()), 2);

                    listsalesChart.Add(newperiod);
                }

                ViewBag.salesJSON = listsalesChart.ToArray();
                return View(salesHistory);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult Agents_forecast(int iddata)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                var data = (from a in dbMatriz.PurchaseData_product where (a.ID_purchaseData == iddata) select a).ToList();
                var dataprocess = (from a in dblim.Purchase_data where (a.ID_purchaseData == iddata) select a).FirstOrDefault();

                ViewBag.iddata = iddata;
                ViewBag.comment = dataprocess.query1;

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult Final_forecast(int iddata)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                var data = (from a in dbMatriz.PurchaseData_product where (a.ID_purchaseData == iddata) select a).ToList();
                var dataprocess = (from a in dblim.Purchase_data where (a.ID_purchaseData == iddata) select a).FirstOrDefault();

                ViewBag.iddata = iddata;
                ViewBag.comment = dataprocess.query1;

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult Purchase_data(string fstartd, string fendd)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                //SECCION DE FILTROS
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                //filtros de fecha (DIARIO)
                //var sunday = DateTime.Today;
                //var saturday = sunday.AddHours(23);
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                //SALES ORDER INFO

                //var data = (from a in dbMatriz.Transito_Final)
                var data = (from a in dblim.Purchase_data where (a.Date_create >= filtrostartdate && a.Date_create <= filtroenddate) select a).ToList();

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public class Forecast_Cajas
        {
            public int? Anio { get; set; }
            public int? Periodo { get; set; }
            public string CodProducto { get; set; }
            public decimal? BaseLine { get; set; }
            public decimal? Forecast { get; set; }

        }

public class history_statsProduct
{
    public string category { get; set; }
    public double a2018 { get; set; }
    public double a2019 { get; set; }
    public double a2020 { get; set; }


}
public class Initial_forecastData
        {
            public string CodProducto { get; set; }
            public string Producto { get; set; }
            public decimal? b1 { get; set; }
            public decimal? b2 { get; set; }
            public decimal? b3 { get; set; }
            public decimal? b4 { get; set; }
            public decimal? b5 { get; set; }
            public decimal? f1 { get; set; }
            public decimal? f2 { get; set; }
            public decimal? f3 { get; set; }
            public decimal? f4 { get; set; }
            public decimal? f5 { get; set; }

        }

        public class salesHistory_product
        {
            public string id_Period { get; set; }
            public string period { get; set; }
            public Int32 OffSetPeriod { get; set; }
            public Int16 Year_DLI { get; set; }
            public string ItemCode { get; set; }
            public decimal? LineTotal { get; set; }
            public decimal? Profit { get; set; }
            public decimal? Each { get; set; }
        }

        public class salesHistory
        {
            public Int64 ID { get; set; }
            public int id_SalesRep { get; set; }
            public string SalesRep { get; set; }
            public decimal? Utility { get; set; }
            public decimal? Invoices { get; set; }
            public int SKUs { get; set; }
            public int PreviousPeriod { get; set; }
        }
        public class forecastSalesReps
        {
            public int id_SalesRep { get; set; }
            public string SalesRep { get; set; }
            public decimal? p1 { get; set; }
            public decimal? p2 { get; set; }
            public decimal? p3 { get; set; }
            public decimal? p4 { get; set; }
            public decimal? p5 { get; set; }
            public decimal? p6 { get; set; }
            public decimal? AVG { get; set; }
            public decimal? PCT { get; set; }


        }
        public ActionResult edit_process(int data=0, string ItemCode="")
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Process";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
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

    
                var product = dbMatriz.PurchaseData_product.Where(c => c.ID_purchaseData == data && c.ItemCode == ItemCode).FirstOrDefault();

                var salesHistory = dlipro.Database.SqlQuery<salesHistory>("Select * from BI_Sales_History_Matriz_Compras where ItemCode={0}", ItemCode).ToList<salesHistory>();

                var salesreps = salesHistory.Select(c => c.id_SalesRep).Distinct().ToList();
                List<forecastSalesReps> lstforecastRep = new List<forecastSalesReps>();
         
                foreach (var forec in salesreps)
                {
                    forecastSalesReps newforrep = new forecastSalesReps();
                    newforrep.id_SalesRep = forec;
                    newforrep.SalesRep = salesHistory.Where(c => c.id_SalesRep == forec).Select(c => c.SalesRep).FirstOrDefault();
                    newforrep.p1 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -1).Select(c => c.Invoices).FirstOrDefault();
                    newforrep.p2 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -2).Select(c => c.Invoices).FirstOrDefault();
                    newforrep.p3 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -3).Select(c => c.Invoices).FirstOrDefault();
                    newforrep.p4 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -4).Select(c => c.Invoices).FirstOrDefault();
                    newforrep.p5 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -5).Select(c => c.Invoices).FirstOrDefault();
                    newforrep.p6 = salesHistory.Where(c => c.id_SalesRep == forec && c.PreviousPeriod == -6).Select(c => c.Invoices).FirstOrDefault();
                  
                    if (newforrep.p1 == null)
                    {
                        newforrep.p1 = 0;
                    }
                    if (newforrep.p2 == null)
                    {
                        newforrep.p2 = 0;
                    }
                    if (newforrep.p3 == null)
                    {
                        newforrep.p3 = 0;
                    }
                    if (newforrep.p4 == null)
                    {
                        newforrep.p4 = 0;
                    }
                    if (newforrep.p5 == null)
                    {
                        newforrep.p5 = 0;
                    }
                    if (newforrep.p6 == null)
                    {
                        newforrep.p6 = 0;
                    }
                    var sumtot = Convert.ToDecimal(newforrep.p1 + newforrep.p2 + newforrep.p3 + newforrep.p4 + newforrep.p5 + newforrep.p6);
                    newforrep.AVG = sumtot/6;
                    newforrep.PCT = 0;


                    lstforecastRep.Add(newforrep);
                }

                foreach (var itt in lstforecastRep) {
                    var sumav = lstforecastRep.Select(c => c.AVG).Sum();

                    var totperc = ((itt.AVG / sumav) * 100);
                    itt.PCT = totperc;
                }

                ///
                forecastSalesReps newforrep34 = new forecastSalesReps();
                newforrep34.id_SalesRep = 0;
                newforrep34.SalesRep = "";
                newforrep34.p1 = lstforecastRep.Select(c => c.p1).Sum();
                newforrep34.p2 = lstforecastRep.Select(c => c.p2).Sum();
                newforrep34.p3 = lstforecastRep.Select(c => c.p3).Sum();
                newforrep34.p4 = lstforecastRep.Select(c => c.p4).Sum();
                newforrep34.p5 = lstforecastRep.Select(c => c.p5).Sum();
                newforrep34.p6 = lstforecastRep.Select(c => c.p6).Sum();

                newforrep34.AVG = lstforecastRep.Select(c => c.AVG).Sum();
                newforrep34.PCT = lstforecastRep.Select(c => c.PCT).Sum();


                lstforecastRep.Add(newforrep34);
                ///

                ViewBag.salesHistory = lstforecastRep;

                //Cargamos vendedores
                List<forecastSalesReps> lstforecastRepEditable = new List<forecastSalesReps>();
                foreach (var forec in salesreps)
                {
                    forecastSalesReps newforrep2 = new forecastSalesReps();
                    newforrep2.id_SalesRep = forec;
                    newforrep2.SalesRep = salesHistory.Where(c => c.id_SalesRep == forec).Select(c => c.SalesRep).FirstOrDefault();
                    var datosedt = lstforecastRep.Where(a => a.id_SalesRep == forec).FirstOrDefault();

                    newforrep2.p1 = ((product.Forecast1_source2 * datosedt.PCT)/100);
                    newforrep2.p2 = ((product.Forecast2_source2 * datosedt.PCT) / 100);
                    newforrep2.p3 = ((product.Forecast3_source2 * datosedt.PCT) / 100);
                    newforrep2.p4 = ((product.Forecast4_source2 * datosedt.PCT) / 100);
                    newforrep2.p5 = ((product.Forecast5_source2 * datosedt.PCT) / 100);
                    newforrep2.p6 = 0;
                    newforrep2.AVG = 0;
                    newforrep2.PCT = 0;


                    lstforecastRepEditable.Add(newforrep2);
                }
                //
                forecastSalesReps newforrep3 = new forecastSalesReps();
                newforrep3.id_SalesRep = 0;
                newforrep3.SalesRep = "";
                newforrep3.p1 = lstforecastRepEditable.Select(c => c.p1).Sum();
                newforrep3.p2 = lstforecastRepEditable.Select(c => c.p2).Sum();
                newforrep3.p3 = lstforecastRepEditable.Select(c => c.p3).Sum();
                newforrep3.p4 = lstforecastRepEditable.Select(c => c.p4).Sum();
                newforrep3.p5 = lstforecastRepEditable.Select(c => c.p5).Sum();
                newforrep3.p6 = 0;
                newforrep3.AVG = 0;
                newforrep3.PCT = 0;


                lstforecastRepEditable.Add(newforrep3);


                ViewBag.SalesRepsEdit = lstforecastRepEditable;

                var dataprocess = (from a in dblim.Purchase_data where (a.ID_purchaseData == data) select a).FirstOrDefault();

                ViewBag.iddata = data;
                ViewBag.comment = dataprocess.query1;



                return View(product);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public ActionResult ProductCatalog()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "New Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                var data = (from a in dbMatriz.Purchase_catalog select a).ToList();
   
    
                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public class MatrizMadre
        {  
            public int num { get; set; } //GENERADO POR DATATABLE

            public string ProdCodigo { get; set; } //Matriz compra
            public string ProdNombre { get; set; } //fin Matriz compra

            public Nullable<double> FactorCompra { get; set; } //MICRO CATALOGO
            public Nullable<double> FactorUnidadCompra { get; set; } //MICRO CATALOGO
            public Nullable<double> FactorCompra_quiebre { get; set; } //MICRO CATALOGO
            public Nullable<double> Politica_cobertura { get; set; } //FIN MICRO CATALOGO

            public string Marca { get; set; } //Filtros
            public string SubCategory { get; set; }
            public string Category { get; set; }
            public string ProvCodigo { get; set; }
            public string ProvNombre { get; set; }//fin filtros

            public string UnidadMedidaLetras { get; set; } //UoM Group //Matriz compra
            public Nullable<decimal> InventarioEach { get; set; }
            public Nullable<decimal> InventarioCajas { get; set; }
            public Nullable<double> Promedio { get; set; }
            public Nullable<double> Desviacion { get; set; }
            public Nullable<double> Maximo { get; set; }
            public Nullable<double> Minimo { get; set; }
            public Nullable<double> PronosticoPeriodoActual { get; set; }
            public Nullable<double> Promedio_AA { get; set; } //Pronostico periodo actual AA
            public Nullable<double> VentaB1 { get; set; } //Periodo Anterior
            public Nullable<double> Variacion { get; set; }
            public Nullable<decimal> TendenciaPeriodoActual { get; set; }
            public Nullable<double> PronosticoSiguiente1 { get; set; }
            public Nullable<double> PronosticoSiguiente2 { get; set; }
            public Nullable<double> PronosticoSiguiente3 { get; set; }
            public Nullable<double> PronosticoSiguiente4 { get; set; }
            public Nullable<double> CoberturaActual { get; set; }
            public Nullable<double> CoberturaProyectada { get; set; }

            public Nullable<double> OTB { get; set; }
            public Nullable<double> Pedido { get; set; } //Orden a colocar
            public Nullable<DateTime> DeliveryDate { get; set; } 
            public Nullable<DateTime> DocumentDate { get; set; }        
            public Nullable<decimal> U_TI { get; set; } //TIER
            public Nullable<decimal> U_HI { get; set; } //HEIGHT
            public Nullable<decimal> U_PalletCount { get; set; } //CASES PER PALLET
            public Nullable<double> PalletsdeOrden { get; set; } 
            public Nullable<double> CoberturaProyectadaNume { get; set; }
            public Nullable<double> InventarioIngresoPO { get; set; }
            public Nullable<double> CoberturaIngresoPO { get; set; }
            public Nullable<double> Costo { get; set; }
            public Nullable<decimal> DescuentoAp { get; set; } //DESCUENTO /ALLOWANCE (%)
            public Nullable<decimal> DescuentoAn { get; set; } // ... ($)
            public Nullable<double> CostoconDescuento { get; set; }
            public Nullable<double> MontoPO { get; set; }
            public string Comentarios { get; set; }
            //No se muestran pero se utilizane en formulas
            public Nullable<double> CoberturaProyectadaDeno { get; set; }
            public Nullable<double> VentaF1 { get; set; }
            public Nullable<double> Cobertura_OTB { get; set; }
            public int LeadTime { get; set; }
            public Nullable<int> transito { get; set; }

            public Nullable<double>  B1 { get; set; }
            public Nullable<double>  B2 { get; set; }
            public Nullable<double> B3 { get; set; }
            public Nullable<double> B4 { get; set; }
            public Nullable<double> B5 { get; set; }

        }
        public ActionResult edit_purchaseData(int id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "Edit Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                //List<Purchase_data_details> data = (from b in dblim.Purchase_data_details where(b.ID_purchaseData==id) select b).OrderBy(b=>b.num).ToList();
                List<MatrizMadre> data = (from b in dblim.Purchase_data_details
                                          where (b.ID_purchaseData == id)
                                          select new MatrizMadre
                                          {

                                              ProdCodigo = b.ProdCodigo,
                                              ProdNombre = b.ProdNombre,

                                              FactorCompra = b.FactorCompra,
                                              FactorCompra_quiebre = b.FactorCompra_quiebre,
                                              Politica_cobertura = b.Politica_cobertura,
                                              Marca = b.Marca,
                                              SubCategory = b.SubCategory,
                                              Category = b.Category,
                                              ProvCodigo = "0",
                                              ProvNombre = b.ProvNombre,
                                              //nuevo
                                              FactorUnidadCompra = b.FactorUnidadCompra,
                                              B1=b.B1,
                                              B2=b.B2,
                                              B3=b.B3,
                                              B4=b.B4,
                                              B5=b.B5,
               InventarioIngresoPO=b.InventarioIngresoPO,

                                              UnidadMedidaLetras = b.UnidadMedidaLetras,
                                              InventarioCajas = b.InventarioCajas,
                                              InventarioEach = b.InventarioEach,
                                              Promedio = b.Promedio,
                                              Desviacion = b.Desviacion,
                                              Maximo = b.Maximo,
                                              Minimo = b.Minimo,
                                              PronosticoPeriodoActual = b.PronosticoPeriodoActual,
                                              Promedio_AA = b.Promedio_AA,
                                              VentaB1 = b.VentaB1,
                                              Variacion = b.Variacion,
                                              TendenciaPeriodoActual = b.TendenciaPeriodoActual,
                                              PronosticoSiguiente1 = b.PronosticoSiguiente1,
                                              PronosticoSiguiente2 = b.PronosticoSiguiente2,
                                              PronosticoSiguiente3 = b.PronosticoSiguiente3,
                                              PronosticoSiguiente4 = b.PronosticoSiguiente4,
                                              CoberturaActual = b.CoberturaActual,
                                              CoberturaProyectada = b.CoberturaProyectada,
                                              OTB = b.OTB,
                                              Pedido = b.Pedido,
                                              DeliveryDate = b.DeliveryDate,
                                              DocumentDate = b.DocumentDate,
                                              U_TI = b.U_TI,
                                              U_HI = b.U_HI,
                                              U_PalletCount = b.U_PalletCount,
                                              PalletsdeOrden = b.PalletsdeOrden,
                                              CoberturaProyectadaNume = b.CoberturaProyectadaNume,
                                              CoberturaIngresoPO = b.CoberturaIngresoPO,
                                              Costo = b.Costo,
                                              DescuentoAp = b.Descuento_allowancep,
                                              DescuentoAn = b.Descuento_allowanced,
                                              CostoconDescuento = b.CostoconDescuento,
                                              MontoPO = b.MontoPO,
                                              Comentarios = b.Comentarios,
                                              Cobertura_OTB = b.Cobertura_OTB,
                                              CoberturaProyectadaDeno = b.CoberturaProyectadaDeno,
                                              VentaF1 = b.VentaF1,
                                              LeadTime = b.LeadTime,
                                              transito = 0
                                          }).ToList();
                
                foreach (var item in data) {
                    item.transito = (from a in dbMatriz.Transito_Final where (a.ProdCodigo == item.ProdCodigo) select a).Count();
                }

                ViewBag.purchasedataID = id;
                ViewBag.data = data;

                var header = (from c in dblim.Purchase_data where (c.ID_purchaseData == id) select c).FirstOrDefault();
                ViewBag.categories = header.Categories;
                ViewBag.subcategories = header.SubCategories;
                ViewBag.brands = header.Brands;
                ViewBag.vendors = header.Providers;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult new_purchaseData()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "New Purchase Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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
         
                List<view_MatrizMadre> data = (from b in dbMatriz.view_MatrizMadre select b).ToList();
                //List<MatrizMadre> data = (from b in dbMatriz.Matriz_Formato select new MatrizMadre
                //{

                //    ProdCodigo = b.ProdCodigo,
                //    ProdNombre = b.ProdNombre,

                //    FactorCompra = 0,
                //    FactorCompra_quiebre = 0,
                //    Politica_cobertura = 0,
                //    Marca = b.Marca,
                //    SubCategory = b.SubCategory,
                //    Category = b.Category,
                //    ProvCodigo = b.ProvCodigo,
                //    ProvNombre = b.ProvNombre,

                //    UnidadMedidaLetras = b.UnidadMedidaLetras,
                //    InventarioCajas = b.InventarioCajas,
                //    InventarioEach = b.InventarioEach,
                //    Promedio = b.Promedio,
                //    Desviacion = b.Desviacion,
                //    Maximo = b.Maximo,
                //    Minimo = b.Minimo,
                //    PronosticoPeriodoActual = b.PronosticoPeriodoActual,
                //    Promedio_AA = b.Promedio_AA,
                //    VentaB1 = b.VentaB1,
                //    Variacion = 0,
                //    TendenciaPeriodoActual = b.TendenciaPeriodoActual,
                //    PronosticoSiguiente1 = b.PronosticoSiguiente1,
                //    PronosticoSiguiente2 = b.PronosticoSiguiente2,
                //    PronosticoSiguiente3 = b.PronosticoSiguiente3,
                //    PronosticoSiguiente4 = b.PronosticoSiguiente4,
                //    CoberturaActual = b.CoberturaActual,
                //    CoberturaProyectada = b.CoberturaProyectada,
                //    OTB = 0,
                //    Pedido = 0,
                //    DeliveryDate = b.DeliveryDate,
                //    DocumentDate = b.DocumentDate,
                //    U_TI = b.U_TI,
                //    U_HI = b.U_HI,
                //    U_PalletCount = 0,
                //    PalletsdeOrden = 0,
                //    CoberturaProyectadaNume = b.CoberturaProyectadaNume,
                //    CoberturaIngresoPO = 0,
                //    Costo = 0,
                //    DescuentoAp = 0,
                //    DescuentoAn = 0,
                //    CostoconDescuento = 0,
                //    MontoPO = 0,
                //    Comentarios = "",
                //    Cobertura_OTB = 0,
                //    CoberturaProyectadaDeno = b.CoberturaProyectadaDeno,
                //    VentaF1 = b.VentaF1,
                //    LeadTime = b.LeadTime,
                //    transito = 0
                //}).ToList();


                //foreach (var item in data) {
                //    if (item.Promedio_AA == null) { item.Promedio_AA = 0; }
                //    if (item.VentaB1 == null) { item.VentaB1 = 0; }
                //    if (item.TendenciaPeriodoActual == null) { item.TendenciaPeriodoActual = 0; }
                //    if (item.PronosticoSiguiente1 == null) { item.PronosticoSiguiente1 = 0; }
                //    if (item.PronosticoSiguiente2 == null) { item.PronosticoSiguiente2 = 0; }
                //    if (item.PronosticoSiguiente3 == null) { item.PronosticoSiguiente3 = 0; }
                //    if (item.PronosticoSiguiente4 == null) { item.PronosticoSiguiente4 = 0; }
                //    if (item.Desviacion == null) { item.Desviacion = 0; }
                //    if (item.Maximo == null) { item.Maximo = 0; }
                //    if (item.Minimo == null) { item.Minimo = 0; }
                //    if (item.PronosticoPeriodoActual == null) { item.PronosticoPeriodoActual = 0; }
                //    if (item.CoberturaActual == null) { item.CoberturaActual = 0; }
                //    if (item.CoberturaProyectada == null) { item.CoberturaProyectada = 0; }
                //    if (item.CoberturaProyectadaDeno == null) { item.CoberturaProyectadaDeno = 0; }
                //    if (item.CoberturaProyectadaNume == null) { item.CoberturaProyectadaNume = 0; }
                //    if (item.Promedio == null) { item.Promedio = 0; }
                //    if (item.U_TI == null) { item.U_TI = 0; }
                //    if (item.U_HI == null) { item.U_HI = 0; }
                //    if (item.U_PalletCount == null) { item.U_PalletCount = 0; }
                //    if (item.Variacion == null) { item.Variacion = 0; }
                //    if (item.VentaF1 == null) { item.VentaF1 = 0; }


                //    var factorunidad = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.FactorUnidadCompra).FirstOrDefault();
                //    if (factorunidad != null) { item.U_PalletCount = Convert.ToDecimal(factorunidad); }
                    

                //    item.Costo = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Costo).FirstOrDefault();
                //    item.CostoconDescuento = item.Costo;
                //    if (item.Costo == null) { item.Costo = 0; item.CostoconDescuento = 0; }
         


                //    item.FactorCompra = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.FactorCompra).FirstOrDefault();
                //    if (item.FactorCompra == null) { item.FactorCompra = 0; }
                //    item.FactorCompra_quiebre = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.FactorCompra_quiebre).FirstOrDefault();
                //    if (item.FactorCompra_quiebre == null) { item.FactorCompra_quiebre = 0; }

                //    item.Politica_cobertura = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Politica_cobertura).FirstOrDefault();
                //    if (item.Politica_cobertura == null) { item.Politica_cobertura = 0; }
                //    item.transito = (from a in dbMatriz.Transito_Final where (a.ProdCodigo == item.ProdCodigo) select a).Count();

                //    item.Comentarios = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Comentarios).FirstOrDefault();
                //    if (item.Comentarios == null) { item.Comentarios = ""; }
                //    if (item.FactorCompra_quiebre == null) { item.FactorCompra_quiebre = 0; }
                //    if (item.CoberturaProyectada > item.Politica_cobertura)
                //    {
                //        int res2 = 0;
                //        try
                //        {
                //            res2 = Convert.ToInt32((((((item.CoberturaProyectada * 4) * (item.FactorCompra)) - (item.CoberturaProyectadaNume)) / Convert.ToDouble(item.U_PalletCount))) * Convert.ToDouble(item.U_PalletCount));
                //        }
                //        catch
                //        {
                //            res2 = 0;
                //        }
                //        item.OTB = res2;
                //    }
                //    else {
                //        int res2 = 0;
                //        try
                //        {
                //            res2 = Convert.ToInt32((((item.CoberturaProyectada * 4) * (item.FactorCompra_quiebre)) / Convert.ToDouble(item.U_PalletCount)) * Convert.ToDouble(item.U_PalletCount));
                //        }
                //        catch {
                //            res2 = 0;
                //        }
                    
                //        item.OTB = res2;
                //    }
                //    try {
                //        item.Cobertura_OTB = (item.CoberturaProyectadaNume + item.OTB) / item.CoberturaProyectadaDeno;
                //        if (item.CoberturaProyectadaDeno == 0) { item.Cobertura_OTB = 0; }
                //    } catch { item.Cobertura_OTB = 0; }

                //    try { item.CoberturaIngresoPO = (item.CoberturaProyectadaNume + item.Pedido) / item.CoberturaProyectadaDeno;
                //        if (item.CoberturaProyectadaDeno == 0) { item.CoberturaIngresoPO = 0; }
                //    } catch { item.CoberturaIngresoPO = 0; }
                //    try
                //    {
                //        if (item.PronosticoPeriodoActual == 0)
                //        {
                //            item.Variacion = 0;
                //        }
                //        else {
                //            item.Variacion = (item.VentaF1 - item.PronosticoPeriodoActual) / item.PronosticoPeriodoActual;
                //        }
                        
                //    }
                //    catch { item.Variacion = 0; }
                    

                //}
                //var child = (from b in dbMatriz.Transito_Final select b).Take(300).ToList();

                //JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                //var acts = javaScriptSerializer.Serialize(data);

                ViewBag.data = data;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public ActionResult new_Data()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Operations";
                ViewData["Page"] = "New Data";
                ViewBag.menunameid = "oper_menu";
                ViewBag.submenunameid = "operpur_submenu";
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

                List<view_MatrizMadre> data = (from b in dbMatriz.view_MatrizMadre select b).ToList();

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public class MyObj_SubCat

        {
            public int? id { get; set; }
            public string name { get; set; }
            public string category { get; set; }

        }
        //public JsonResult CustomServerSideSearchAction()
        //{
        //    // action inside a standard controller
        //    //int filteredResultsCount;
        //    //int totalResultsCount;
        //    //var res = YourCustomSearchFunc(model, out filteredResultsCount, out totalResultsCount);

        //    //var result = new List<MatrizMadre>(res.Count);
        //    ////foreach (var s in res)
        //    ////{
        //    ////    // simple remapping adding extra info to found dataset
        //    ////    result.Add(new YourCustomSearchClass
        //    ////    {
        //    ////        EmployerId = User.ClaimsUserId(),
        //    ////        Id = s.Id,
        //    ////        Pin = s.Pin,
        //    ////        Firstname = s.Firstname,
        //    ////        Lastname = s.Lastname,
        //    ////        RegistrationStatusId = DoSomethingToGetIt(s.Id),
        //    ////        Address3 = s.Address3,
        //    ////        Address4 = s.Address4
        //    ////    });
        //    ////};

        //    var catalogo = dblim.Purchase_catalog.Select(a => a).ToList();
        //    List<MatrizMadre> result = (from b in dbMatriz.Matriz_Formato
        //                                select new MatrizMadre
        //                                {

        //                                    ProdCodigo = b.ProdCodigo,
        //                                    ProdNombre = b.ProdNombre,

        //                                    FactorCompra = 0,
        //                                    FactorCompra_quiebre = 0,
        //                                    Politica_cobertura = 0,
        //                                    Marca = b.Marca,
        //                                    SubCategory = b.SubCategory,
        //                                    Category = b.Category,
        //                                    ProvCodigo = b.ProvCodigo,
        //                                    ProvNombre = b.ProvNombre,

        //                                    UnidadMedidaLetras = b.UnidadMedidaLetras,
        //                                    InventarioCajas = b.InventarioCajas,
        //                                    InventarioEach = b.InventarioEach,
        //                                    Promedio = b.Promedio,
        //                                    Desviacion = b.Desviacion,
        //                                    Maximo = b.Maximo,
        //                                    Minimo = b.Minimo,
        //                                    PronosticoPeriodoActual = b.PronosticoPeriodoActual,
        //                                    Promedio_AA = b.Promedio_AA,
        //                                    VentaB1 = b.VentaB1,
        //                                    Variacion = 0,
        //                                    TendenciaPeriodoActual = b.TendenciaPeriodoActual,
        //                                    PronosticoSiguiente1 = b.PronosticoSiguiente1,
        //                                    PronosticoSiguiente2 = b.PronosticoSiguiente2,
        //                                    PronosticoSiguiente3 = b.PronosticoSiguiente3,
        //                                    PronosticoSiguiente4 = b.PronosticoSiguiente4,
        //                                    CoberturaActual = b.CoberturaActual,
        //                                    CoberturaProyectada = b.CoberturaProyectada,
        //                                    OTB = 0,
        //                                    Pedido = 0,
        //                                    DeliveryDate = b.DeliveryDate,
        //                                    DocumentDate = b.DocumentDate,
        //                                    U_TI = b.U_TI,
        //                                    U_HI = b.U_HI,
        //                                    U_PalletCount = b.U_PalletCount,
        //                                    PalletsdeOrden = 0,
        //                                    CoberturaProyectadaNume = b.CoberturaProyectadaNume,
        //                                    CoberturaIngresoPO = 0,
        //                                    Costo = 0,
        //                                    DescuentoAp = 0,
        //                                    DescuentoAn = 0,
        //                                    CostoconDescuento = 0,
        //                                    MontoPO = 0,
        //                                    Comentarios = "",
        //                                    Cobertura_OTB = 0,
        //                                    CoberturaProyectadaDeno = b.CoberturaProyectadaDeno,
        //                                    VentaF1 = b.VentaF1,
        //                                    LeadTime = b.LeadTime,
        //                                    transito = 0
        //                                }).Take(10).ToList();

        //    //filteredResultsCount = result.Count();
        //    //totalResultsCount = result.Count();
        //    foreach (var item in result)
        //    {
        //        if (item.Promedio_AA == null) { item.Promedio_AA = 0; }
        //        if (item.VentaB1 == null) { item.VentaB1 = 0; }
        //        if (item.TendenciaPeriodoActual == null) { item.TendenciaPeriodoActual = 0; }
        //        if (item.PronosticoSiguiente1 == null) { item.PronosticoSiguiente1 = 0; }
        //        if (item.PronosticoSiguiente2 == null) { item.PronosticoSiguiente2 = 0; }
        //        if (item.PronosticoSiguiente3 == null) { item.PronosticoSiguiente3 = 0; }
        //        if (item.PronosticoSiguiente4 == null) { item.PronosticoSiguiente4 = 0; }
        //        if (item.Desviacion == null) { item.Desviacion = 0; }
        //        if (item.Maximo == null) { item.Maximo = 0; }
        //        if (item.Minimo == null) { item.Minimo = 0; }
        //        if (item.PronosticoPeriodoActual == null) { item.PronosticoPeriodoActual = 0; }
        //        if (item.CoberturaActual == null) { item.CoberturaActual = 0; }
        //        if (item.CoberturaProyectada == null) { item.CoberturaProyectada = 0; }
        //        if (item.CoberturaProyectadaDeno == null) { item.CoberturaProyectadaDeno = 0; }
        //        if (item.CoberturaProyectadaNume == null) { item.CoberturaProyectadaNume = 0; }
        //        if (item.Promedio == null) { item.Promedio = 0; }
        //        if (item.U_TI == null) { item.U_TI = 0; }
        //        if (item.U_HI == null) { item.U_HI = 0; }
        //        if (item.U_PalletCount == null) { item.U_PalletCount = 0; }
        //        if (item.Variacion == null) { item.Variacion = 0; }
        //        if (item.VentaF1 == null) { item.VentaF1 = 0; }


        //        item.Costo = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Costo).FirstOrDefault();
        //        item.CostoconDescuento = item.Costo;
        //        if (item.Costo == null) { item.Costo = 0; item.CostoconDescuento = 0; }



        //        item.FactorCompra = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.FactorCompra).FirstOrDefault();
        //        if (item.FactorCompra == null) { item.FactorCompra = 0; }
        //        item.FactorCompra_quiebre = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.FactorCompra_quiebre).FirstOrDefault();
        //        if (item.FactorCompra_quiebre == null) { item.FactorCompra_quiebre = 0; }

        //        item.Politica_cobertura = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Politica_cobertura).FirstOrDefault();
        //        if (item.Politica_cobertura == null) { item.Politica_cobertura = 0; }
        //        item.transito = (from a in dbMatriz.Transito_Final where (a.ProdCodigo == item.ProdCodigo) select a).Count();

        //        item.Comentarios = (from a in catalogo where (a.ProductCode == item.ProdCodigo) select a.Comentarios).FirstOrDefault();
        //        if (item.FactorCompra_quiebre == null) { item.FactorCompra_quiebre = 0; }
        //        if (item.CoberturaProyectada > item.Politica_cobertura)
        //        {
        //            int res2 = 0;
        //            try
        //            {
        //                res2 = Convert.ToInt32((((((item.CoberturaProyectada * 4) * (item.FactorCompra)) - (item.CoberturaProyectadaNume)) / Convert.ToDouble(item.U_PalletCount))) * Convert.ToDouble(item.U_PalletCount));
        //            }
        //            catch
        //            {
        //                res2 = 0;
        //            }
        //            item.OTB = res2;
        //        }
        //        else
        //        {
        //            int res2 = 0;
        //            try
        //            {
        //                res2 = Convert.ToInt32((((item.CoberturaProyectada * 4) * (item.FactorCompra_quiebre)) / Convert.ToDouble(item.U_PalletCount)) * Convert.ToDouble(item.U_PalletCount));
        //            }
        //            catch
        //            {
        //                res2 = 0;
        //            }

        //            item.OTB = res2;
        //        }
        //        try
        //        {
        //            item.Cobertura_OTB = (item.CoberturaProyectadaNume + item.OTB) / item.CoberturaProyectadaDeno;
        //            if (item.CoberturaProyectadaDeno == 0) { item.Cobertura_OTB = 0; }
        //        }
        //        catch { item.Cobertura_OTB = 0; }
        //        try
        //        {
        //            item.CoberturaIngresoPO = (item.CoberturaProyectadaNume + item.Pedido) / item.CoberturaProyectadaDeno;
        //            if (item.CoberturaProyectadaDeno == 0) { item.CoberturaIngresoPO = 0; }
        //        }
        //        catch { item.CoberturaIngresoPO = 0; }
        //        try
        //        {
        //            if (item.PronosticoPeriodoActual == 0)
        //            {
        //                item.Variacion = 0;
        //            }
        //            else
        //            {
        //                item.Variacion = (item.VentaF1 - item.PronosticoPeriodoActual) / item.PronosticoPeriodoActual;
        //            }

        //        }
        //        catch { item.Variacion = 0; }


        //    }
        //    MatrizDatatable dataTable = new MatrizDatatable();
        //    dataTable.draw = int.Parse(Request.QueryString["draw"]);


        //    dataTable.data = result.ToArray();
        //    dataTable.recordsTotal = result.Count;
        //    dataTable.recordsFiltered = result.Count();
        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result2 = javaScriptSerializer.Serialize(result.ToArray());
        //    return Json(result2, JsonRequestBehavior.AllowGet);

        //    //JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    //var acts = javaScriptSerializer.Serialize(result);

        //    //return Json(acts, JsonRequestBehavior.AllowGet);
        //    //return Json(new
        //    //{
        //    //    // this is what datatables wants sending back
        //    //    draw = model.num,
        //    //    recordsTotal = totalResultsCount,
        //    //    recordsFiltered = filteredResultsCount,
        //    //    data = result
        //    //});
        //}
        public ActionResult getTransitoFinal(string id) {
            var child = (from b in dbMatriz.Transito_Final where (b.ProdCodigo == id) select b).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(child);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Save_PurchaseData(List<MatrizMadre> objects, string Categories, string Subcategories, string Providers, string Brands)
        {
            string ttresult = "";
            try
            {
                int userid = 0;
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    userid = activeuser.ID_User;
                }

                    Purchase_data newDataM = new Purchase_data();
                newDataM.Date_create = DateTime.UtcNow;
                newDataM.Date_data = DateTime.UtcNow.AddDays(-1);
                newDataM.User_create = userid;
                newDataM.query1 = "";
                newDataM.query2 = "";
                newDataM.Categories = Categories;
                newDataM.SubCategories = Subcategories;
                newDataM.Providers = Providers;
                newDataM.Brands = Brands;
                dblim.Purchase_data.Add(newDataM);
                dblim.SaveChanges();

                

                List<Purchase_data_details> lsttosave = new List<Purchase_data_details>();
                foreach (var items in objects)
                {
                    Purchase_data_details newDet = new Purchase_data_details();
                    newDet.ID_purchaseData = newDataM.ID_purchaseData;
                    newDet.num = items.num;
                    newDet.ProdCodigo = items.ProdCodigo;
                    newDet.ProdNombre = items.ProdNombre;
                    newDet.FactorCompra = items.FactorCompra;
                    //nuevo 09/17/2019
                    newDet.FactorUnidadCompra = items.FactorUnidadCompra;
                    newDet.B1 = items.B1;
                    newDet.B2 = items.B2;
                    newDet.B3 = items.B3;
                    newDet.B4 = items.B4;
                    newDet.B5 = items.B5;

                    newDet.FactorCompra_quiebre = items.FactorCompra_quiebre;
                    newDet.Politica_cobertura = items.Politica_cobertura;
                    newDet.Marca = items.Marca;
                    newDet.Category = items.Category;
                    newDet.SubCategory = items.SubCategory;
                    newDet.ProvNombre = items.ProvNombre;
                    newDet.UnidadMedidaLetras = items.UnidadMedidaLetras;                    
                    if (items.InventarioEach == null) { newDet.InventarioEach = 0; } else { newDet.InventarioEach = items.InventarioEach; }
                    if (items.InventarioCajas == null) { newDet.InventarioCajas = 0; } else { newDet.InventarioCajas = items.InventarioCajas; }
                    newDet.Promedio = items.Promedio;
                    newDet.Desviacion = items.Desviacion;
                    newDet.Maximo = items.Maximo;
                    newDet.Minimo = items.Minimo;
                    newDet.PronosticoPeriodoActual = items.PronosticoPeriodoActual;
                    newDet.Promedio_AA = items.Promedio_AA;
                    newDet.VentaB1 = items.VentaB1;
                    newDet.Variacion = items.Variacion;
                    newDet.TendenciaPeriodoActual = items.TendenciaPeriodoActual;
                    newDet.PronosticoSiguiente1 = items.PronosticoSiguiente1;
                    newDet.PronosticoSiguiente2 = items.PronosticoSiguiente2;
                    newDet.PronosticoSiguiente3 = items.PronosticoSiguiente3;
                    newDet.PronosticoSiguiente4 = items.PronosticoSiguiente4;
                    newDet.CoberturaActual = items.CoberturaActual;
                    newDet.CoberturaProyectada = items.CoberturaProyectada;
                    newDet.OTB = items.OTB;
                        newDet.Cobertura_OTB = items.Cobertura_OTB;
                    if (items.Pedido == null) { newDet.Pedido = 0; } else { newDet.Pedido = items.Pedido; }
                    newDet.DeliveryDate = items.DeliveryDate;
                    newDet.DocumentDate = items.DocumentDate;
                    newDet.U_TI = items.U_TI;
                    newDet.U_HI = items.U_HI;
                    newDet.U_PalletCount = items.U_PalletCount;
                    if (items.PalletsdeOrden == null) { newDet.PalletsdeOrden = 0; } else { newDet.PalletsdeOrden = items.PalletsdeOrden; }
                    newDet.CoberturaProyectadaNume = items.CoberturaProyectadaNume;
                    newDet.CoberturaIngresoPO = items.CoberturaIngresoPO;
                    newDet.InventarioIngresoPO = items.InventarioIngresoPO;
                    newDet.Costo = items.Costo;
                    if (items.DescuentoAn == null) { newDet.Descuento_allowanced = 0; } else { newDet.Descuento_allowanced = items.DescuentoAn; }
                    if (items.DescuentoAp == null) { newDet.Descuento_allowancep = 0; } else { newDet.Descuento_allowancep = items.DescuentoAp; }
                    newDet.CostoconDescuento = items.CostoconDescuento;
                    newDet.MontoPO = items.MontoPO;
 
                    if (items.Comentarios == null) { newDet.Comentarios = ""; } else { newDet.Comentarios = items.Comentarios; }
                    newDet.CoberturaProyectadaDeno = items.CoberturaProyectadaDeno;
                    newDet.VentaF1 = items.VentaF1;
                
                    newDet.LeadTime = items.LeadTime;
                    lsttosave.Add(newDet);
                    //Actualizamos el comentario
                    try
                    {
                        if (items.Comentarios != "") {
                            var producto = (from a in dbMatriz.Purchase_catalog where (a.ProductCode == items.ProdCodigo) select a).FirstOrDefault();

                            if (producto != null)
                            {
                                producto.Comentarios = items.Comentarios;
                                dbMatriz.Entry(producto).State = EntityState.Modified;
                                dbMatriz.SaveChanges();
                            }
                        }

                    }
                    catch { }
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
        public class MyObj_save

        {
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public string Vendor { get; set; }
            public string Brand { get; set; }
            public string Category { get; set; }
            public string Subcategory { get; set; }

        }
        [HttpPost]
        public ActionResult Save_DataProcess(List<MyObj_save> objects, string Categories, string Subcategories, string Providers, string Brands)
        {
            string ttresult = "";
            try
            {
                int userid = 0;
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    userid = activeuser.ID_User;
                }

                Purchase_data newDataM = new Purchase_data();
                newDataM.Date_create = DateTime.UtcNow;
                newDataM.Date_data = DateTime.UtcNow.AddDays(-1);
                newDataM.User_create = userid;
                newDataM.query1 = "";
                newDataM.query2 = "";
                newDataM.Categories = Categories;
                newDataM.SubCategories = Subcategories;
                newDataM.Providers = Providers;
                newDataM.Brands = Brands;
                dblim.Purchase_data.Add(newDataM);
                dblim.SaveChanges();



                List<PurchaseData_product> lsttosave = new List<PurchaseData_product>();
                foreach (var items in objects)
                {
                    PurchaseData_product newDet = new PurchaseData_product();
                    newDet.ItemCode = items.ItemCode;
                    newDet.Description = items.Description;
                    newDet.Vendor = items.Vendor;
                    newDet.Brand = items.Brand;
                    newDet.Category = items.Category;
                    newDet.Subcategory = items.Subcategory;
                    newDet.Last_update = DateTime.UtcNow;
                    newDet.Forecast1_source1 = 0;
                    newDet.Forecast1_source1_period = 0;
                    newDet.Forecast1_source1_year = 0;
                    newDet.Forecast2_source1 = 0;
                    newDet.Forecast2_source1_period = 0;
                    newDet.Forecast2_source1_year = 0;
                    newDet.Forecast3_source1 = 0;
                    newDet.Forecast3_source1_period = 0;
                    newDet.Forecast3_source1_year = 0;
                    newDet.Forecast4_source1 = 0;
                    newDet.Forecast4_source1_period = 0;
                    newDet.Forecast4_source1_year = 0;
                    newDet.Forecast5_source1 = 0;
                    newDet.Forecast5_source1_period = 0;
                    newDet.Forecast5_source1_year = 0;
                    newDet.Forecast1_source2 = 0;
                    newDet.Forecast2_source2 = 0;
                    newDet.Forecast3_source2 = 0;
                    newDet.Forecast4_source2 = 0;
                    newDet.Forecast5_source2 = 0;
                    newDet.Forecast1_source3 = 0;
                    newDet.Forecast2_source3 = 0;
                    newDet.Forecast3_source3 = 0;
                    newDet.Forecast4_source3 = 0;
                    newDet.Forecast5_source3 = 0;
                    newDet.Baseline1 = 0;
                    newDet.Baseline2 = 0;
                    newDet.Baseline3 = 0;
                    newDet.Baseline4 = 0;
                    newDet.Baseline5 = 0;
                    var forecast = dbMatriz.Database.SqlQuery<Forecast_Cajas>("Select * from Forecast_Cajas").ToList<Forecast_Cajas>();

                    newDet.Forecast1_source1 = Convert.ToDecimal(forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Forecast).FirstOrDefault());
                    newDet.Forecast1_source1_period = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Periodo).FirstOrDefault();
                    newDet.Forecast1_source1_year = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Anio).FirstOrDefault();
                    newDet.Baseline1 = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).FirstOrDefault();
                    newDet.Forecast2_source1 = Convert.ToDecimal(forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(1).FirstOrDefault());
                    newDet.Forecast2_source1_period = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Periodo).Skip(1).FirstOrDefault();
                    newDet.Forecast2_source1_year = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Anio).Skip(1).FirstOrDefault();
                    newDet.Baseline2 = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(1).FirstOrDefault();
                    newDet.Forecast3_source1 = Convert.ToDecimal(forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(2).FirstOrDefault());
                    newDet.Forecast3_source1_period = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Periodo).Skip(2).FirstOrDefault();
                    newDet.Forecast3_source1_year = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Anio).Skip(2).FirstOrDefault();
                    newDet.Baseline3 = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(2).FirstOrDefault();
                    newDet.Forecast4_source1 = Convert.ToDecimal(forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(3).FirstOrDefault());
                    newDet.Forecast4_source1_period = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Periodo).Skip(3).FirstOrDefault();
                    newDet.Forecast4_source1_year = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Anio).Skip(3).FirstOrDefault();
                    newDet.Baseline4 = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(3).FirstOrDefault();
                    newDet.Forecast5_source1 = Convert.ToDecimal(forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(4).FirstOrDefault());
                    newDet.Forecast5_source1_period = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Periodo).Skip(4).FirstOrDefault();
                    newDet.Forecast5_source1_year = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.Anio).Skip(4).FirstOrDefault();
                    newDet.Baseline5 = forecast.Where(c => c.CodProducto == items.ItemCode).Select(c => c.BaseLine).Skip(4).FirstOrDefault();

                    newDet.ID_purchaseData = newDataM.ID_purchaseData;
                    newDet.SourceSel = 1;
                    newDet.Comments = "";
                    lsttosave.Add(newDet);
                    
                }

                dbMatriz.BulkInsert(lsttosave);
         


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
        public ActionResult SaveEdit_PurchaseData(int id, List<MatrizMadre> objects)
        {
            string ttresult = "";
            try
            {
                int userid = 0;
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    userid = activeuser.ID_User;
                }

                var lsttodel = (from a in dblim.Purchase_data_details where (a.ID_purchaseData == id) select a).ToList();
                if (lsttodel.Count > 0) {
                    dblim.BulkDelete(lsttodel);
                }
              

                List<Purchase_data_details> lsttosave = new List<Purchase_data_details>();
                foreach (var items in objects)
                {
                    Purchase_data_details newDet = new Purchase_data_details();
                    newDet.ID_purchaseData =id;
                    newDet.num = items.num;
                    newDet.ProdCodigo = items.ProdCodigo;
                    newDet.ProdNombre = items.ProdNombre;
                    newDet.FactorCompra = items.FactorCompra;
                    newDet.FactorCompra_quiebre = items.FactorCompra_quiebre;
                    newDet.Politica_cobertura = items.Politica_cobertura;
                    newDet.Marca = items.Marca;
                    newDet.Category = items.Category;
                    newDet.SubCategory = items.SubCategory;
                    newDet.ProvNombre = items.ProvNombre;
                    newDet.UnidadMedidaLetras = items.UnidadMedidaLetras;
                    if (items.InventarioEach == null) { newDet.InventarioEach = 0; } else { newDet.InventarioEach = items.InventarioEach; }
                    if (items.InventarioCajas == null) { newDet.InventarioCajas = 0; } else { newDet.InventarioCajas = items.InventarioCajas; }
                    newDet.Promedio = items.Promedio;
                    newDet.Desviacion = items.Desviacion;
                    newDet.Maximo = items.Maximo;
                    newDet.Minimo = items.Minimo;
                    newDet.PronosticoPeriodoActual = items.PronosticoPeriodoActual;
                    newDet.Promedio_AA = items.Promedio_AA;
                    newDet.VentaB1 = items.VentaB1;
                    newDet.Variacion = items.Variacion;

                    //nuevo 09/17/2019
                    newDet.FactorUnidadCompra = items.FactorUnidadCompra;
                    newDet.B1 = items.B1;
                    newDet.B2 = items.B2;
                    newDet.B3 = items.B3;
                    newDet.B4 = items.B4;
                    newDet.B5 = items.B5;
                    newDet.InventarioIngresoPO = items.InventarioIngresoPO;

                    newDet.TendenciaPeriodoActual = items.TendenciaPeriodoActual;
                    newDet.PronosticoSiguiente1 = items.PronosticoSiguiente1;
                    newDet.PronosticoSiguiente2 = items.PronosticoSiguiente2;
                    newDet.PronosticoSiguiente3 = items.PronosticoSiguiente3;
                    newDet.PronosticoSiguiente4 = items.PronosticoSiguiente4;
                    newDet.CoberturaActual = items.CoberturaActual;
                    newDet.CoberturaProyectada = items.CoberturaProyectada;
                    newDet.OTB = items.OTB;
                    newDet.Cobertura_OTB = items.Cobertura_OTB;
                    if (items.Pedido == null) { newDet.Pedido = 0; } else { newDet.Pedido = items.Pedido; }
                    newDet.DeliveryDate = items.DeliveryDate;
                    newDet.DocumentDate = items.DocumentDate;
                    newDet.U_TI = items.U_TI;
                    newDet.U_HI = items.U_HI;
                    newDet.U_PalletCount = items.U_PalletCount;
                    if (items.PalletsdeOrden == null) { newDet.PalletsdeOrden = 0; } else { newDet.PalletsdeOrden = items.PalletsdeOrden; }
                    newDet.CoberturaProyectadaNume = items.CoberturaProyectadaNume;
                    newDet.CoberturaIngresoPO = items.CoberturaIngresoPO;
                    newDet.Costo = items.Costo;
                    if (items.DescuentoAn == null) { newDet.Descuento_allowanced = 0; } else { newDet.Descuento_allowanced = items.DescuentoAn; }
                    if (items.DescuentoAp == null) { newDet.Descuento_allowancep = 0; } else { newDet.Descuento_allowancep = items.DescuentoAp; }
                    newDet.CostoconDescuento = items.CostoconDescuento;
                    newDet.MontoPO = items.MontoPO;

                    if (items.Comentarios == null) { newDet.Comentarios = ""; } else { newDet.Comentarios = items.Comentarios; }
                    newDet.CoberturaProyectadaDeno = items.CoberturaProyectadaDeno;
                    newDet.VentaF1 = items.VentaF1;

                    newDet.LeadTime = items.LeadTime;
                    lsttosave.Add(newDet);
                    //Actualizamos el comentario
                    try
                    {
                        if (items.Comentarios != "")
                        {
                            var producto = (from a in dbMatriz.Purchase_catalog where (a.ProductCode == items.ProdCodigo) select a).FirstOrDefault();

                            if (producto != null)
                            {
                                producto.Comentarios = items.Comentarios;
                                dbMatriz.Entry(producto).State = EntityState.Modified;
                                dbMatriz.SaveChanges();
                            }
                        }

                    }
                    catch { }
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

        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public DataSet getDataSetExportToExcel(int id)
        {
            DataSet ds = new DataSet();
            DataTable dtEmp = new DataTable("Matriz");
            var details = (from a in dblim.Purchase_data_details where (a.ID_purchaseData == id) select new {No= a.num, CodigoenSAP = a.ProdCodigo, ItemDescripcion = a.ProdNombre, FactorCompraRegular =a.FactorCompra, FactorUnidadCompra = a.FactorUnidadCompra, FactorCompraQuiebre=a.FactorCompra_quiebre, PoliticadeCobertura = a.Politica_cobertura,
            UoMGroup = a.UnidadMedidaLetras, InventarioEACH = a.InventarioEach, InventarioenCases = a.InventarioCajas,B5=a.B5,B4=a.B4,B3=a.B3,B2=a.B2, B1=a.B1, Avg5Periodos=a.Promedio, Desviacion=a.Desviacion, Maximo =a.Maximo, Minimo =a.Minimo, PronosticoPeriodoActual=a.PronosticoPeriodoActual,Avg5PeriodosPosterioresAA =a.Promedio_AA,
            PeriodoAnterior=a.VentaB1, Variacion=a.Variacion, TendenciaPeriodoActual = a.TendenciaPeriodoActual, PronosticoPeriodoSiguiente1 =a.PronosticoSiguiente1, PronosticoPeriodoSiguiente2=a.PronosticoSiguiente2, PronosticoPeriodoSiguiente3=a.PronosticoSiguiente3, PronosticoPeriodoSiguiente4=a.PronosticoSiguiente4,
            CoberturaActual=a.CoberturaActual, CoberturaProyectada=a.CoberturaProyectada, InventarioProyectado=a.CoberturaProyectadaNume, LEADTIME=a.CoberturaProyectadaDeno, OTB=a.OTB, CoberturadeOTB=a.Cobertura_OTB, OrdenaColocar=a.Pedido, DeliveryDate=a.DeliveryDate, DocumentDate=a.DocumentDate, TIER=a.U_TI, HEIGHT=a.U_HI, CasesPerPallet=a.U_PalletCount, PalletsdeOrden=a.PalletsdeOrden, InventarioalIngresodePO =a.InventarioIngresoPO,
            CoberturaalIngresodePO=a.CoberturaIngresoPO, Costo=a.Costo,DescuentoAllowanceP =a.Descuento_allowancep, DescuentoAllowanceN=a.Descuento_allowanced, CostoconDescuento = a.CostoconDescuento, MontoPO =a.MontoPO, Comentarios=a.Comentarios}).ToList();
            dtEmp = ToDataTable(details);

            var detailsArr = (from c in details select c.CodigoenSAP).ToArray();

            DataTable dtEmpOrder = new DataTable("Transito");
            var transito = (from b in dbMatriz.Transito_Final where ( detailsArr.Contains(b.ProdCodigo)) select new { CodigoenSAP = b.ProdCodigo,NumTransito=b.NumTransito, DocDate =b.DocDate, DiaSemana =b.DiaSemana, DifSemana = b.DifSemanas, NumFactura = b.NumFactura, CantCajas = b.CantCajas,FormulaDia=b.FormulaDia, CoberturaSemanal = b.CoberturaSemanal}).ToList();
            dtEmpOrder = ToDataTable(transito);




            ds.Tables.Add(dtEmp);
            ds.Tables.Add(dtEmpOrder);
            return ds;
        }

        public ActionResult PurchaseData_Export(int id)
        {
            //UTILIZANDO LIBRERIA 
            DataSet ds = getDataSetExportToExcel(id);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(ds);
                wb.Worksheet(1).Name = "Datos de Matriz";
                wb.Worksheet(2).Name = "Transito";
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=PurchaseData.xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            return View();
        }


        public ActionResult Del_PurchaseData(string id)
        {

            try
            {

                var idD = Convert.ToInt32(id);
                var data = dblim.Purchase_data.Where(c => c.ID_purchaseData == idD).Select(c => c).FirstOrDefault();

                if (data != null)
                {

                    var details = (from a in dblim.Purchase_data_details where (a.ID_purchaseData == idD) select a).ToList();
                    dblim.BulkDelete(details);

                    dblim.Purchase_data.Remove(data);
                    dblim.BulkSaveChanges();

                    var rrresult = "SUCCESS";
                    return Json(rrresult, JsonRequestBehavior.AllowGet);
                }
                else
                {
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

        [HttpPost]
        public ActionResult Save_ItemCatalog(string product, double factorn, double factorq, double factoru, double politica, double costo, string comentarios)
        {
            string ttresult = "";
            try
            {
                var producto = (from a in dbMatriz.Purchase_catalog where (a.ProductCode == product) select a).FirstOrDefault();

                if (producto != null)
                {


                    producto.FactorCompra = factorn;
                    producto.FactorCompra_quiebre = factorq;
                    producto.Politica_cobertura = politica;
                    producto.FactorUnidadCompra = factoru;
                    producto.Costo = costo;
                    producto.Comentarios = comentarios;

                    dbMatriz.Entry(producto).State = EntityState.Modified;
                    dbMatriz.SaveChanges();

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else {
                    ttresult = "NO DATA FOUND";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
        [HttpPost]
        public ActionResult Save_agentforecast(string itemcode, string description, string periodo, decimal newval, int iddata)
        {
            string ttresult = "";
            try
            {
                var producto = (from a in dbMatriz.PurchaseData_product where (a.ItemCode == itemcode && a.ID_purchaseData==iddata) select a).FirstOrDefault();

                if (producto != null)
                {
                    //Existe por lo tanto actualizaremos

                    if (periodo == "f1")
                    {
                        producto.Forecast1_source2 = newval;
                    }
                    else if (periodo == "f2") {
                        producto.Forecast2_source2 = newval;
                    }
                    else if (periodo == "f3")
                    {
                        producto.Forecast3_source2 = newval;
                    }
                    else if (periodo == "f4")
                    {
                        producto.Forecast4_source2 = newval;
                    }
                    else if (periodo == "f5")
                    {
                        producto.Forecast5_source2 = newval;
                    }

                    producto.Last_update = DateTime.UtcNow;

                    dbMatriz.Entry(producto).State = EntityState.Modified;
                    dbMatriz.SaveChanges();

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //Crearemos el nuevo registro
                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        [HttpPost]
        public ActionResult Save_dataComment(string comment, int iddata)
        {
            string ttresult = "";
            try
            {
                var data = (from a in dblim.Purchase_data where (a.ID_purchaseData == iddata) select a).FirstOrDefault();

                if (data != null)
                {
                    //Existe por lo tanto actualizaremos

                    data.query1 = comment;
                    dblim.Entry(data).State = EntityState.Modified;
                    dblim.SaveChanges();

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    ttresult = "NO DATA";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
        [HttpPost]
        public ActionResult Save_productComment(string comment, int iddata, string product)
        {
            string ttresult = "";
            try
            {
                var data = (from a in dbMatriz.PurchaseData_product where (a.ID_purchaseData == iddata && a.ItemCode==product) select a).FirstOrDefault();

                if (data != null)
                {
                    //Existe por lo tanto actualizaremos

                    data.Comments = comment;
                    dbMatriz.Entry(data).State = EntityState.Modified;
                    dbMatriz.SaveChanges();

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    ttresult = "NO DATA";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
        [HttpPost]
        public ActionResult Save_finalforecast(string itemcode, string description, int periodo, int anio, decimal newval, int iddata)
        {
            string ttresult = "";
            try
            {
                var producto = (from a in dbMatriz.Purchase_dataHistory where (a.ItemCode == itemcode && a.periodo == periodo && a.year == anio) select a).FirstOrDefault();

                if (producto != null)
                {
                    //Existe por lo tanto actualizaremos
                    producto.newValue = newval;

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //Crearemos el nuevo registro
                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
    }
}