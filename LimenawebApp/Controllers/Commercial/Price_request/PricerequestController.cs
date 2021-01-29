using ClosedXML.Excel;
using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static LimenawebApp.Models.Price_request.Mdl_PriceChangeHistory;

namespace LimenawebApp.Controllers.Commercial.Price_request
{
    public class PricerequestController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();
        private Cls_session cls_session = new Cls_session();
        private Cls_Authorizations cls_Authorizations = new Cls_Authorizations();
        // GET: Pricerequest

        public class periods {
            public string periodcode { get; set; } 
            public string periodname { get; set; } 
        }

        public ActionResult Requesthistory(string fstartd, string fendd, string period)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Price Request History";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                //List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                //ViewBag.notifications = lstAlerts;
                ViewBag.activeuser = activeuser;
                //FIN HEADER
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();

                var salesreps = (from a in dblim.Sys_Users where (a.Roles.Contains("Sales Representative")) select new Bolsa_SalesR { ID_user = a.ID_User, ID_userSAP = a.IDSAP, Username = a.Name + " " + a.Lastname, Asignado = a.BolsaValor, Disponible=0, Utilizado=0 }).ToList();
                List<Help_BolsaUtilizada> salesHistory = new List<Help_BolsaUtilizada>();
                if (period == null || period == "")
                {
                    salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select * from Help_BolsaUtilizadaActual").ToList<Help_BolsaUtilizada>();

                }
                else {
                    var periodsel = period.Substring(1);
                    salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select * from Help_BolsaUtilizada where id_Period={0}", periodsel).ToList<Help_BolsaUtilizada>();

                }

                foreach (var item in salesreps) {
                    item.Utilizado = Math.Round(Convert.ToDecimal(salesHistory.Where(c => c.ID_user == item.ID_user).Select(c => c.Utilizado).FirstOrDefault()),2);
                    item.Disponible = Math.Round(Convert.ToDecimal(item.Asignado) - Convert.ToDecimal(item.Utilizado),2);
                }

                ViewBag.salesHistory = salesHistory;

                //periodos activos

                if (period == null || period == "")
                {
                    ViewBag.period = salesHistory.Select(c => c.id_Period).FirstOrDefault() + "| " + salesHistory.Select(c => c.Period_Name).FirstOrDefault();
                    var activeperiodweeks = dlipro.Database.SqlQuery<PeriodoActivoSemana>("select * from HELP_RANGOPERIODOSEMANA where YearDLI in ({0},{1})", "2020","2021").GroupBy(n => new { n.PeriodCode, n.PeriodName}).Select(c => new periods { periodcode = c.Key.PeriodCode, periodname = c.Key.PeriodName }).ToList();
                    ViewBag.activeperiodos = activeperiodweeks;
                }
                else {
                    var periodcode = period;
                    var activeperiodweeks = dlipro.Database.SqlQuery<PeriodoActivoSemana>("select * from HELP_RANGOPERIODOSEMANA where YearDLI in ({0},{1})", "2020", "2021").GroupBy(n => new { n.PeriodCode, n.PeriodName }).Select(c=> new periods { periodcode = c.Key.PeriodCode, periodname = c.Key.PeriodName }).ToList();
                    var activeperiod = activeperiodweeks.Where(a => a.periodcode == periodcode).FirstOrDefault();
                    var selectedperiod = "";
                    if (activeperiod != null) {
                        selectedperiod = activeperiod.periodcode + " | " + activeperiod.periodname;
                    }
                    ViewBag.period = selectedperiod;
                    ViewBag.activeperiodos = activeperiodweeks;
                }

           


                return View(salesreps);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult RequesthistoryNOSR(string fstartd, string fendd, string period)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Price Request History";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                //List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                //ViewBag.notifications = lstAlerts;
                ViewBag.activeuser = activeuser;
                //FIN HEADER
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                ////filtros de fecha (SEMANAL)
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();

                var salesreps = (from a in dblim.Sys_Users where (a.Roles.Contains("Sales Supervisor")) select new Bolsa_SalesR { ID_user = a.ID_User, ID_userSAP = a.IDSAP, Username = a.Name + " " + a.Lastname, Asignado = a.BolsaValor, Disponible = 0, Utilizado = 0 }).ToList();
                List<Help_BolsaUtilizada> salesHistory = new List<Help_BolsaUtilizada>();
                if (period == null || period == "")
                {
                    salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select * from Help_BolsaUtilizadaActual").ToList<Help_BolsaUtilizada>();

                }
                else
                {
                    var periodsel = period.Substring(1);
                    salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select * from Help_BolsaUtilizada where id_Period={0}", periodsel).ToList<Help_BolsaUtilizada>();

                }

                foreach (var item in salesreps)
                {
                    item.Utilizado = Math.Round(Convert.ToDecimal(salesHistory.Where(c => c.ID_user == item.ID_user).Select(c => c.Utilizado).FirstOrDefault()), 2);
                    item.Disponible = Math.Round(Convert.ToDecimal(item.Asignado) - Convert.ToDecimal(item.Utilizado), 2);
                }

                ViewBag.salesHistory = salesHistory;

                //periodos activos

                if (period == null || period == "")
                {
                    ViewBag.period = salesHistory.Select(c => c.id_Period).FirstOrDefault() + "| " + salesHistory.Select(c => c.Period_Name).FirstOrDefault();
                    var activeperiodweeks = dlipro.Database.SqlQuery<PeriodoActivoSemana>("select * from HELP_RANGOPERIODOSEMANA where YearDLI in ({0},{1})", "2020", "2021").GroupBy(n => new { n.PeriodCode, n.PeriodName }).Select(c => new periods { periodcode = c.Key.PeriodCode, periodname = c.Key.PeriodName }).ToList();
                    ViewBag.activeperiodos = activeperiodweeks;
                }
                else
                {
                    var periodcode = period;
                    var activeperiodweeks = dlipro.Database.SqlQuery<PeriodoActivoSemana>("select * from HELP_RANGOPERIODOSEMANA where YearDLI in ({0},{1})", "2020", "2021").GroupBy(n => new { n.PeriodCode, n.PeriodName }).Select(c => new periods { periodcode = c.Key.PeriodCode, periodname = c.Key.PeriodName }).ToList();
                    var activeperiod = activeperiodweeks.Where(a => a.periodcode == periodcode).FirstOrDefault();
                    var selectedperiod = "";
                    if (activeperiod != null)
                    {
                        selectedperiod = activeperiod.periodcode + " | " + activeperiod.periodname;
                    }
                    ViewBag.period = selectedperiod;
                    ViewBag.activeperiodos = activeperiodweeks;
                }




                return View(salesreps);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public class htmlModel
        {
            [AllowHtml]
            public int ID_SalesRepresentative { get; set; }
            public string SalesRepresentative { get; set; }
            public decimal Last_value { get; set; }
            public decimal New_value { get; set; }
            public decimal Value { get; set; }
            public decimal Date { get; set; }
            public string AssignedBy { get; set; }
            public int AssignedBy_ID { get; set; }
            public bool Active { get; set; }
            public string Comments { get; set; }
            public int Type { get; set; }
        }
        public ActionResult Save_bolsa(htmlModel nuevabolsa)
        {
            try
            {

                var salesrep = (from a in dblim.Sys_Users where (a.ID_User == nuevabolsa.ID_SalesRepresentative) select a).FirstOrDefault();

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                Tb_registroBolsa newbolsa = new Tb_registroBolsa();
                newbolsa.ID_SalesRepresentative = nuevabolsa.ID_SalesRepresentative;
                newbolsa.SalesRepresentative = nuevabolsa.SalesRepresentative;
                newbolsa.Last_value = Convert.ToDecimal(salesrep.BolsaValor);
                newbolsa.New_value = nuevabolsa.New_value;
                newbolsa.Value = Convert.ToDecimal(salesrep.BolsaValor) + nuevabolsa.New_value;
                newbolsa.Date = DateTime.UtcNow;
                newbolsa.AssignedBy = activeuser.Name + " " + activeuser.Lastname;
                newbolsa.AssignedBy_ID = activeuser.ID_User;
                newbolsa.Active = true;
                if (nuevabolsa.Comments == null) { nuevabolsa.Comments = ""; }
                newbolsa.Comments = nuevabolsa.Comments;
                newbolsa.Type = nuevabolsa.Type;

          


                internadli.Tb_registroBolsa.Add(newbolsa);
                internadli.SaveChanges();

                salesrep.BolsaValor = newbolsa.Value;
                dblim.Entry(salesrep).State = EntityState.Modified;
                dblim.SaveChanges();

                return Json("Success", JsonRequestBehavior.AllowGet);
                

            }
            catch (Exception ex)
            {
                return Json("Se ha producido el siguiente error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult PriceChangeHistory_Export(string ids, string period)
        {
            try
            {
                //var salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select top 2 * from Help_BolsaUtilizada").ToList<Help_BolsaUtilizada>();

                var periodcode = period.Substring(1);//salesHistory[0].id_Period;

                //UTILIZANDO LIBRERIA 
                DataSet ds = getDataSetExportToExcel(ids, periodcode);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(ds);
                    wb.Worksheet(1).Name = "Resume";
                    wb.Worksheet(2).Name = "Price Change Product History";
                    wb.Worksheet(3).Name = "Price Change History";
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=PriceChangeHistory.xlsx");
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
            catch (Exception ex2) {
                return RedirectToAction("Requesthistory", "Pricerequest");
            }

        }
        public ActionResult PriceChangeHistory_Export2(string ids, string period)
        {
            try
            {
                //var salesHistory = internadli.Database.SqlQuery<Help_BolsaUtilizada>("Select top 2 * from Help_BolsaUtilizada").ToList<Help_BolsaUtilizada>();

                var periodcode = period.Substring(1);//salesHistory[0].id_Period;

                //UTILIZANDO LIBRERIA 
                DataSet ds = getDataSetExportToExcel(ids, periodcode);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(ds);
                    wb.Worksheet(1).Name = "Resume";
                    wb.Worksheet(2).Name = "Price Change Product History";
                    wb.Worksheet(3).Name = "Price Change History";
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=PriceChangeHistory.xlsx");
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
            catch (Exception ex2)
            {
                return RedirectToAction("RequesthistoryNOSR", "Pricerequest");
            }

        }
        public DataSet getDataSetExportToExcel(string ids, string periodcode)
        {
            List<int> TagIds = ids.Split(',').Select(int.Parse).ToList();
            periodcode = "P" + periodcode;
            var activeperiod = dlipro.Database.SqlQuery<PeriodoActivo>("select * from help_rangoperiodos where PeriodCode={0}", periodcode).FirstOrDefault();


            List<PeriodoActivoSemana> activeperiodweeks = dlipro.Database.SqlQuery<PeriodoActivoSemana>("select * from HELP_RANGOPERIODOSEMANA where PeriodCode={0}", periodcode).ToList();

            DataSet ds = new DataSet();

            DataTable dtEmpResume = new DataTable("Resume");
            var detailsresume = internadli.Database.SqlQuery<Help_bolsautilizada>("select * from Help_BolsaUtilizada where ID_user in (" + ids + ") and id_Period={0}", periodcode.Substring(1)).ToList();
            dtEmpResume = ToDataTable(detailsresume);


            DataTable dtEmp = new DataTable("Price Change History");
            var details = (from a in internadli.Tb_registroBolsa where (TagIds.Contains(a.ID_SalesRepresentative) && (a.Date >= activeperiod.BeginDate && a.Date <=activeperiod.EndDate)) select a).ToList();
            dtEmp = ToDataTable(details);


            DataTable dtEmpOrder = new DataTable("Price Change Product History");
            var tb_auth = (from b in internadli.Tb_Autorizaciones where (TagIds.Contains(b.ID_user) && b.Estado == 2 && (b.FechaValidacion >= activeperiod.BeginDate && b.FechaValidacion <= activeperiod.EndDate)) select new Tb_AutorizacionExport {
                ID_detalle = b.ID_detalle, DocNum = b.DocNum, ItemCode = b.ItemCode, Producto = b.Producto, CodUOM = b.CodUOM, UOM = b.UOM, Cantidad = b.Cantidad, PrecioPedido = b.PrecioPedido, PrecioMin = b.PrecioMin,
                NuevoPrecio = b.NuevoPrecio, Resultado = b.NuevoPrecio>0 ? ((b.PrecioPedido - b.NuevoPrecio) * b.Cantidad) : 0, DocNumSAP = b.DocNumSAP, FechaIngreso = b.FechaIngreso, FechaValidacion = b.FechaValidacion, LineNum = b.LineNum,
                UserName = b.UserName, DocPepperi = b.DocPepperi, CodCustomer = b.CodCustomer, Customer = b.Customer, PeriodCode = periodcode, PeriodName = "", WeekDLI = 0
            }).ToList();

            foreach (var item in tb_auth) {
                var ext = (from c in activeperiodweeks where (item.FechaValidacion >= c.WeekBeginDate && item.FechaValidacion <= c.WeekEndDate) select c).FirstOrDefault();
               if (ext != null) { item.PeriodName = ext.PeriodName; item.WeekDLI = ext.WeekDLI; }
            }
            dtEmpOrder = ToDataTable(tb_auth);


            ds.Tables.Add(dtEmpResume);
            ds.Tables.Add(dtEmpOrder);
            ds.Tables.Add(dtEmp);
            return ds;
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
    }
}