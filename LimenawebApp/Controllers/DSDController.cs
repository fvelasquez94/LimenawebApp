using CrystalDecisions.CrystalReports.Engine;
using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LimenawebApp.Controllers
{
    public class DSDController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        // GET: DSD
        public ActionResult New_Customer()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "DSD";
                ViewData["Page"] = "New Customer";
                ViewBag.menunameid = "dsd_menu";
                ViewBag.submenunameid = "dsdnew_submenu";
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
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult Print_OrderDSD(int? id)
        {
            var header = (from b in dblim.Tb_OrdersDSD where (b.ID_OrderDSD == id) select b).FirstOrDefault();
            var direction = "";
            if (header != null)
            {

                try
                {
                    var directiondb = (from dir in dlipro.OCRD where (dir.CardCode == header.ID_customer) select dir).FirstOrDefault();
                    direction = directiondb.MailAddres + ", " + directiondb.MailZipCod + ", " + directiondb.MailCity + ", " + directiondb.State2;
                }
                catch
                {

                }


            }

            var details = (from a in dblim.Tb_OrdersDetailsDSD where (a.ID_OrderDSD == id) select a).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Order_DSD.rpt"));
            rd.SetDataSource(details);

            rd.SetParameterValue("company", header.Doc_numCompany.ToUpper());
            rd.SetParameterValue("payment_type", header.Payment);
            rd.SetParameterValue("payment_number", header.Doc_numP);
            rd.SetParameterValue("customer", header.CustomerName);
            rd.SetParameterValue("rep", header.User_name);
            rd.SetParameterValue("idorder", header.ID_OrderDSD);
            rd.SetParameterValue("comment", header.Comment);
            rd.SetParameterValue("direction", direction);
            //Firma
            string data = header.Sign;
            if (data != "")
            {
                var base64Data = Regex.Match(data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;

                var binData = Convert.FromBase64String(base64Data);

                using (var streamf = new MemoryStream(binData))
                {

                    Bitmap myImage = new Bitmap(streamf);

                    // Assumes myImage is the PNG you are converting
                    using (var b = new Bitmap(myImage.Width, myImage.Height))
                    {
                        b.SetResolution(myImage.HorizontalResolution, myImage.VerticalResolution);

                        using (var g = Graphics.FromImage(b))
                        {
                            g.Clear(Color.White);
                            g.DrawImageUnscaled(myImage, 0, 0);
                        }

                        // Now save b as a JPEG like you normally would

                        var path = Path.Combine(Server.MapPath("~/Content/appcontent/assets/img"), "signOrders.jpg");
                        b.Save(path, ImageFormat.Jpeg);


                        rd.SetParameterValue("urlimgsign", Path.GetFullPath(path));
                    }



                }
            }
            else
            {
                rd.SetParameterValue("urlimgsign", "");

            }

            var filePathOriginal = Server.MapPath("/Reports/pdf");
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            //PARA VISUALIZAR
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "Order_DSD.pdf; ");
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);      
        }

        public ActionResult Pre_Print_OrderDSD(int? id)
        {
            var header = (from b in dblim.Tb_PreOrdersDSD where (b.ID_OrderDSD == id) select b).FirstOrDefault();
            var direction = "";
            if (header != null)
            {

                    try
                    {
                        var directiondb = (from dir in dlipro.OCRD where (dir.CardCode == header.ID_customer) select dir).FirstOrDefault();
                        direction = directiondb.MailAddres + ", " + directiondb.MailZipCod + ", " + directiondb.MailCity + ", " + directiondb.State2;
                    }
                    catch
                    {

                    }

                
            }

            var details = (from a in dblim.Tb_PreOrdersDetailsDSD where (a.ID_OrderDSD == id) select a).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Order_DSD.rpt"));
            rd.SetDataSource(details);

            rd.SetParameterValue("company", header.Doc_numCompany.ToUpper());
            rd.SetParameterValue("payment_type", header.Payment);
            rd.SetParameterValue("payment_number", header.Doc_numP);
            rd.SetParameterValue("customer", header.CustomerName);
            rd.SetParameterValue("rep", header.User_name);
            rd.SetParameterValue("idorder", header.ID_OrderDSD);
            rd.SetParameterValue("comment", header.Comment);
            rd.SetParameterValue("direction", direction);
            //Firma
            string data = header.Sign;
            if (data != "")
            {
                var base64Data = Regex.Match(data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;

                var binData = Convert.FromBase64String(base64Data);

                using (var streamf = new MemoryStream(binData))
                {

                    Bitmap myImage = new Bitmap(streamf);

                    // Assumes myImage is the PNG you are converting
                    using (var b = new Bitmap(myImage.Width, myImage.Height))
                    {
                        b.SetResolution(myImage.HorizontalResolution, myImage.VerticalResolution);

                        using (var g = Graphics.FromImage(b))
                        {
                            g.Clear(Color.White);
                            g.DrawImageUnscaled(myImage, 0, 0);
                        }

                        // Now save b as a JPEG like you normally would

                        var path = Path.Combine(Server.MapPath("~/Content/appcontent/assets/img"), "signOrders.jpg");
                        b.Save(path, ImageFormat.Jpeg);


                        rd.SetParameterValue("urlimgsign", Path.GetFullPath(path));
                    }



                }
            }
            else
            {
                rd.SetParameterValue("urlimgsign", "");

            }

            var filePathOriginal = Server.MapPath("/Reports/pdf");
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            //PARA VISUALIZAR
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "Order_DSD.pdf; ");
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        public ActionResult PrintResume(string fstartd, string fendd)
        {
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

            List<ResumeSO_DSD> lstOrders = (from o in dblim.Tb_OrdersDSD where (o.Date >= filtrostartdate && o.Date <= filtroenddate) select new ResumeSO_DSD { ID_OrderDSD = o.ID_OrderDSD,
                ID_customer = o.ID_customer, CustomerName = o.CustomerName, ID_payment = o.ID_payment, Payment = o.Payment, Doc_numP = o.Doc_numP, Doc_numCompany = o.Doc_numCompany, docNum_SAP = o.docNum_SAP, Date = o.Date.ToString()
            , ID_User = o.ID_User, User_name = o.User_name, ID_Company = o.ID_Company, Comment = o.Comment, Sign = o.Sign, Total = 0, Direction="" }).OrderBy(b=>b.Date).ToList();

            if (lstOrders != null)
            {
                foreach (var item in lstOrders)
                {
                    var sumtotal = (from se in dblim.Tb_OrdersDetailsDSD where (se.ID_OrderDSD == item.ID_OrderDSD) select se.Total).Sum();
                    item.Total = sumtotal;
                    item.Date = Convert.ToDateTime(item.Date).ToShortDateString();
                    try
                    {
                        var direction = (from dir in dlipro.OCRD where (dir.CardCode == item.ID_customer) select dir).FirstOrDefault();
                        item.Direction = direction.MailAddres + ", " + direction.MailZipCod + ", " + direction.MailCity + ", " + direction.State2;
                    }
                    catch {

                    }
                    item.Date = Convert.ToDateTime(item.Date).ToShortDateString();
                }
            }


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Resume_DSD.rpt"));
            rd.SetDataSource(lstOrders);

            rd.SetParameterValue("start", filtrostartdate.ToShortDateString());
            rd.SetParameterValue("end", filtroenddate.ToShortDateString());
            //rd.SetParameterValue("payment_number", header.Doc_numP);
            //rd.SetParameterValue("customer", header.CustomerName);
            //rd.SetParameterValue("rep", header.User_name);
            //rd.SetParameterValue("idorder", header.ID_OrderDSD);
            //rd.SetParameterValue("comment", header.Comment);
            

            var filePathOriginal = Server.MapPath("/Reports/pdf");
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            //PARA VISUALIZAR
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "ResumeOrders_DSD.pdf; ");
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
        public class MyObj_DSD
        {
            public string id { get; set; }
            public string Product { get; set; }
            public decimal? price { get; set; }
            public decimal? srp { get; set; }
            public string upc { get; set; }
            public string category { get; set; }
            public string subcategory { get; set; }

        }
        public class MyObj_formtemplate

        {
            public string id { get; set; }
            public string text { get; set; }
            public string units { get; set; }
            public string returnReasonID { get; set; }
            public string returnReasonName { get; set; }
            public string Price { get; set; }
            public string UPC { get; set; }
            public string SRP { get; set; }
            public string total { get; set; }

        }

        public class MyObj_SubCat

        {
            public int? id { get; set; }
            public string name { get; set; }
            public string category { get; set; }

        }
        public ActionResult InventoryTR(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "DSD";
                ViewData["Page"] = "New Order";
                ViewBag.menunameid = "dsd_menu";
                ViewBag.submenunameid = "dsdnew_submenu2";
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

                //PARA PRODUCTIVO
                //var lstCustomers = (from b in dlipro.BI_Dim_Customer where (b.SalesRep == "dsd") select new { id = b.id_Customer, text = b.Customer }).ToArray();
                //PARA PRUEBAS LOCALES
                //List<MyObj_lstStoresInputSel> lstCustomers = (from b in dlipro.BI_Dim_Customer where (b.SalesRep == "dsd") select new MyObj_lstStoresInputSel { id = b.id_Customer, text = b.Customer }).ToList();
                //List<MyObj_lstStoresInputSel> lstCustomers = (from b in dlipro.BI_Dim_Customer where (b.SalesRep == "VICTOR CAICEDO") select new MyObj_lstStoresInputSel { id = b.id_Customer, text = b.Customer }).ToList();
                //ViewBag.lstCustomers = JsonConvert.SerializeObject(lstCustomers.ToArray());

                //var lstProducts2 = (from c in dlipro.BI_Dim_Products where (c.DSD == "Y") select c).ToList();
                //var lstProducts = (from c in dlipro.BI_Dim_Products 
                //                   join lp in dlipro.ITM1 on c.id equals lp.ItemCode
                //                   where (c.DSD == "Y" && lp.PriceList==151) //151 es la correcta //148
                //                   select new MyObj_DSD { id=c.id, Product=c.Product, price=lp.Price, srp=c.SRP,upc=c.CodeBars, category=c.category_name, subcategory=c.subcategory_name }).ToList();

                var lstProducts = (from c in dlipro.Dsd_Products
                                       //where (c.DSD == "Y" && c.PriceList == 148) //151 es la correcta //148
                                   select new MyObj_DSD { id = c.id, Product = c.Product, price = c.Price, srp = c.SRP, upc = c.CodeBars, category = c.category_name, subcategory = c.subcategory_name }).ToList();

                ViewBag.lstProducts = lstProducts;

                //var lstReturnReasons = (from d in dlipro.UFD1 where (d.TableID == "RDR1" && d.FieldID == 0) select new { id = d.FldValue, text = d.Descr }).ToArray();
                //ViewBag.lstReturnsR = JsonConvert.SerializeObject(lstReturnReasons);


                //var lstFormasPago = (from e in dlipro.UFD1 where (e.TableID == "ORDR" && e.FieldID == 26) select new { id = e.FldValue, text = e.Descr }).ToArray();
                //ViewBag.lstFormasPago = JsonConvert.SerializeObject(lstFormasPago);


                var lstCategories = (from f in dlipro.BI_Dim_Products where (f.DSD == "Y") select f.category_name).Distinct().ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in dlipro.BI_Dim_Products where (f.DSD == "Y") select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name, category = f.category_name }).Distinct().ToList();
                ViewBag.lstSubCategories = lstSubCategories;

                var lstOrders = (from o in dblim.Tb_InventoryTRDSD where (o.ID_Company == activeuser.ID_Company && o.Date >= filtrostartdate && o.Date <= filtroenddate) select o).ToList();

                if (lstOrders != null)
                {
                    foreach (var item in lstOrders)
                    {
                        var sumtotal = (from se in dblim.Tb_InventoryDetailsTRDSD where (se.ID_InventoryDSD == item.ID_InventoryDSD) select se.Units).Sum();
                        item.docNum_SAP = sumtotal.ToString();
                    }
                }

                //var suggested = (from su in dlipro.View_dsd_suggestedinventory where (su.QryGroup8 == "Y" && su.MaxStock > 0 && su.WhsCode == "01") select su).ToList();//Pruebas
                var suggested = (from su in dlipro.View_dsd_suggestedinventory where (su.QryGroup8 == "Y" && su.MaxStock > 0 && su.WhsName.Contains("DSD")) select su).ToList();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                ViewBag.suggested = javaScriptSerializer.Serialize(suggested.ToArray());
   
                ViewBag.lstOrders = lstOrders;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        [HttpPost]
        public JsonResult addInventoryTR(List<MyObj_formtemplate> objectsProducts)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;

  


            Tb_InventoryTRDSD newOrderDSD = new Tb_InventoryTRDSD();

            newOrderDSD.Date = DateTime.UtcNow;
            newOrderDSD.ID_User = activeuser.ID_User;
            newOrderDSD.User_name = activeuser.Name + " " + activeuser.Lastname;
            newOrderDSD.ID_Company = activeuser.ID_Company;
            newOrderDSD.docNum_SAP = "";

            dblim.Tb_InventoryTRDSD.Add(newOrderDSD);
            dblim.SaveChanges();

            //Guardamos el detalle;

            foreach (var item in objectsProducts)
            {

                Tb_InventoryDetailsTRDSD newDetail = new Tb_InventoryDetailsTRDSD();

                newDetail.ID_InventoryDSD = newOrderDSD.ID_InventoryDSD;
                newDetail.ID_Product = item.id;
                newDetail.Product_Name = item.text;
                newDetail.Units = Convert.ToInt32(item.units);
                newDetail.UPC = item.UPC;

                dblim.Tb_InventoryDetailsTRDSD.Add(newDetail);
                dblim.SaveChanges();


            }



            var result = "Success";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Print_InventoryDSD(int? id)
        {
            var header = (from b in dblim.Tb_InventoryTRDSD where (b.ID_InventoryDSD == id) select b).FirstOrDefault();
  

            var details = (from a in dblim.Tb_InventoryDetailsTRDSD where (a.ID_InventoryDSD == id) select a).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Inventory_DSD.rpt"));
            rd.SetDataSource(details);
         


            var filePathOriginal = Server.MapPath("/Reports/pdf");
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            //PARA VISUALIZAR
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "Inventory_DSD.pdf; ");
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

    }
}