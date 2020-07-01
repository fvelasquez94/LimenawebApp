using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using LimenawebApp.Models;
using Newtonsoft.Json;
using Postal;
using System;

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;


namespace LimenawebApp.Controllers
{
    public class CommercialController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();

        //TIENDAS Y RUTAS (Se utiliza en Routes)
        public class tablahijospadreAct
        {
            public string id { get; set; }
            public string text { get; set; }
            public string parent { get; set; }
        }

        public class MyObj_tablapadreAct
        {
            public string id { get; set; }
            public string text { get; set; }
            public List<MyObj_tablapadreAct> children { get; set; }
        }

        public static List<MyObj_tablapadreAct> ObtenerCategoriarJerarquiaByIDAct(List<MyObj_tablapadreAct> Categoriaspadre, List<tablahijospadreAct> categoriashijas)
        {


            List<MyObj_tablapadreAct> query = (from item in Categoriaspadre

                                               select new MyObj_tablapadreAct
                                               {
                                                   id = "", //SI QUEREMOS AGRUPAR POR ID SE LO PONEMOS, SINO SE LO QUITAMOS PARA QUE NOS CARGUE LAS TIENDAS DESPLEGADAS
                                                   text = item.text.Replace("'", ""),
                                                   children = ObtenerHijosByIDAct(item.id, categoriashijas)
                                               }).ToList();

            return query;





        }

        private static List<MyObj_tablapadreAct> ObtenerHijosByIDAct(string Categoria, List<tablahijospadreAct> categoriashijas)
        {



            List<MyObj_tablapadreAct> query = (from item in categoriashijas

                                               where item.parent == Categoria
                                               select new MyObj_tablapadreAct
                                               {
                                                   id = item.id,
                                                   text = item.text.Replace("'", ""),
                                                   children = null
                                               }).ToList();

            return query;

        }

        public class Routes_calendar
        {
            public string title { get; set; }
            public string url { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string className { get; set; }
        }


        public static DataTable ImportExceltoDatatable(string filePath, string sheetName)
        {
            // Open the Excel file using ClosedXML.
            // Keep in mind the Excel file cannot be open when trying to read it
            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(1);

                //Create a new DataTable.
                DataTable dt = new DataTable();

                //Loop through the Worksheet rows.
                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;

                        foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            i++;
                        }
                    }
                }

                return dt;
            }
        }
        public static string ConvertDataTableToHTMLTable(DataTable dt)
        {
            string ret = "";
            ret = "<table id=" + (char)34 + "tblExcel" + (char)34 + ">";
            ret += "<tr>";
            foreach (DataColumn col in dt.Columns)
            {
                ret += "<td class=" + (char)34 + "tdColumnHeader" + (char)34 + ">" + col.ColumnName + "</td>";
            }
            ret += "</tr>";
            foreach (DataRow row in dt.Rows)
            {
                ret += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ret += "<td class=" + (char)34 + "tdCellData" + (char)34 + ">" + row[i].ToString() + "</td>";
                }
                ret += "</tr>";
            }
            ret += "</table>";
            return ret;
        }


        public ActionResult Credits_request()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Credits Request";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss5_submenu";
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

                //Lista de productos con filtro de Pepperi, el dia 22/08/2019 se solicito eliminar el GOYA/PRE-SALE (114)
                //Producto Don Beto BUL300326 (Siempre mostrar)
                //Filtro original
               //var products = (from a in dlipro.BI_Dim_Products where (a.Pepperi == "YES" && a.id_brand != 114 && a.Credits =="NO" || a.id== "BUL300326") select a);
               
                //Cambios requeridos por Aldo Quijano 05/07/2020
                var products = (from a in dlipro.BI_Dim_Products where (a.Active =="Y" && a.Blocked=="N" && a.InventoryItem=="Y" && !a.id.Contains("FIX") && a.id_brand != 114 || a.id == "BUL300326") select a);


                var lstCategories = (from f in products select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in products select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name.Replace("'", ""), category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in products select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name.Replace("'", ""), category = f.subcategory_name.Replace("'", "") }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;

                List<string> uids = new List<string>() { "99","103","105","107","110","111", "114" };


                var lstReturnReasons = (from dr in dlipro.UFD1 where (dr.TableID == "RDR1" && dr.FieldID == 0 && uids.Contains(dr.FldValue)) select new { id = dr.FldValue, text = dr.Descr }).ToArray();
                ViewBag.lstReturnsR = JsonConvert.SerializeObject(lstReturnReasons);

                var lstproducts = products.Select(c=>new { id=c.id, text=c.id + " | " + c.item_name }).ToArray();
                ViewBag.lstproducts = JsonConvert.SerializeObject(lstproducts);

                //LISTADO DE CLIENTES


                //SABER SI ES ADMIN
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor") || activeuser.Roles.Contains("Credits Authorizer"))
                {
                    isAdmin = 1;
                    var customers = (from b in dlipro.BI_Dim_Customer where (b.Customer.StartsWith("C") && b.Customer != null && b.Customer != "") select new { id = b.id_Customer, text = b.Customer.Replace(",", "") }).OrderBy(a => a.text).ToArray();
                    ViewBag.customers = JsonConvert.SerializeObject(customers);
                }
                else
                {
                    isAdmin = 0;
                    int? sapdli = Convert.ToInt32(activeuser.IDSAP);
                    var customers = (from b in dlipro.BI_Dim_Customer where (b.Customer.StartsWith("C") && b.Customer != null && b.Customer != "" && b.id_SalesRep== sapdli) select new { id = b.id_Customer, text = b.Customer.Replace(",", "") }).OrderBy(a => a.text).ToArray();
                    ViewBag.customers = JsonConvert.SerializeObject(customers);
                }





                return View(products);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public class ContactsCustomers
        {
            public string CardCode { get; set; }
            public string CardName { get; set; }
            public int Total { get; set; }
            public int Status { get; set; }
            public string IsBuyer { get; set; }
            public string StatusCustomer { get; set; }
        }

        public class ContactsCustomersByRep
        {
            public string CardCode { get; set; }
            public string CardName { get; set; }
            public int Total { get; set; }
            public int Status { get; set; }
            public string IsBuyer { get; set; }
            public string slpCode { get; set; }
            public string classification { get; set; }
            public string statuscustomer { get; set; }
        }


        public class ContactsCustomersByRep_cat
        {
            public int iduser { get; set; }
            public string prop01 { get; set; }
            public string prop02 { get; set; }
            public string slpCode { get; set; }
            public string classification { get; set; }
        }


        public ActionResult Customer_contactdata()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Customer Contact Data";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu2";
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

                var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();
                List<RepsSurveys> repsSurveys = new List<RepsSurveys>();
                List<RepsSurveys> supervisors = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Supervisor")).Select(c => new RepsSurveys { idSAP = c.IDSAP, ID_User = c.ID_User, idSAPsupervisor = "", Lastname = c.Lastname, Name = c.Name, prop01 = "", prop02 = "" }).ToList();
       
                string[] usids;
                string[] notinlist;
                List<ContactsCustomers> lstcustomer = new List<ContactsCustomers>();
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor") || activeuser.Roles.Contains("Contacts"))
                {
                    isAdmin = 1;

                    List<int> TagIds = new List<int>();

                    if (activeuser.Roles.Contains("Sales Supervisor"))
                    {
                        TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList();

                        repsSurveys = users.Where(h => TagIds.Contains(h.ID_User)).Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();


                        lstcustomer = (from a in dlipro.BI_Dim_Customer
                                       where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C"))
                                       select new ContactsCustomers
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           StatusCustomer=a.StatusCustomer
                                       }).ToList();

                    }
                    else
                    {
                        //repsSurveys = users.Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                        //usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();


                        lstcustomer = (from a in dlipro.BI_Dim_Customer
                                       where (a.id_Customer.StartsWith("C"))
                                       select new ContactsCustomers
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           StatusCustomer = a.StatusCustomer
                                       }).ToList();
                    }
                }
                else
                {
                    int? idre = Convert.ToInt32(activeuser.IDSAP);
                    //repsSurveys = users.Where(a => a.ID_User == activeuser.ID_User).Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                    //usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                    lstcustomer = (from a in dlipro.BI_Dim_Customer
                                   where (a.id_SalesRep== idre && a.id_Customer.StartsWith("C"))
                                   select new ContactsCustomers
                                   {
                                       CardCode = a.id_Customer,
                                       CardName = a.Customer,
                                       Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                       Status = 1,
                                       IsBuyer = a.IsBuyer,
                                       StatusCustomer = a.StatusCustomer
                                   }).ToList();
                }

                return View(lstcustomer);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }


        public ActionResult Contacts_details(string id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Customer Contact Data";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu2";
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

                 var contacts = (from a in dlipro.BI_Contact_Person
                                   where (a.CardCode == id)
                                   select a).ToList();

                var contactsLocal = (from a in internadli.Tb_customerscontacts
                                   where (a.CardCode == id)
                                   select a).ToList();

                ViewBag.idcustomer = id;
                ViewBag.contactslocal = contactsLocal;
                return View(contacts);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult Customersbcl(string id, string profile, string salesname, string module)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Pepperi Online Customers List";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewBag.module = module;
                var user = dblim.Sys_Users.Where(a => a.IDSAP == id && a.Roles.Contains("Sales Representative")).FirstOrDefault();
                //FIN HEADER
                List<ContactsCustomersByRep> lstcustomer = new List<ContactsCustomersByRep>();
                if (profile == "NO CLASSIFICATION")
                {
                    lstcustomer = (from a in dlipro.BI_Dim_Customer
                                       where (a.id_SalesRep.ToString() == id && a.id_Customer.StartsWith("C") && a.Classification !="DIAMOND" && a.Classification != "GOLD" && a.Classification != "SILVER")
                                       select new ContactsCustomersByRep
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer && b.Email !="" && b.Email !=null) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           slpCode = a.id_SalesRep.ToString(),
                                           classification = a.Classification,
                                           statuscustomer=a.StatusCustomer

                                       }).ToList();
                }
                else {
                    lstcustomer = (from a in dlipro.BI_Dim_Customer
                                   where (a.id_SalesRep.ToString() == id && a.id_Customer.StartsWith("C") && a.Classification == profile)
                                   select new ContactsCustomersByRep
                                   {
                                       CardCode = a.id_Customer,
                                       CardName = a.Customer,
                                       Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer && b.Email != "" && b.Email != null) select b).Count(),
                                       Status = 1,
                                       IsBuyer = a.IsBuyer,
                                       slpCode = a.id_SalesRep.ToString(),
                                       classification = a.Classification,
                                       statuscustomer = a.StatusCustomer

                                   }).ToList();
                }


                ViewBag.profile = profile;
                ViewBag.salesname = user.Name.ToUpper() + " " + user.Lastname.ToUpper();
                return View(lstcustomer);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }


        public ActionResult Prices_request()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Prices Request";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss1_submenu";
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

                //Lista de productos con filtro de Pepperi, el dia 22/08/2019 se solicito eliminar el GOYA/PRE-SALE (114)
                var products = (from a in dlipro.BI_Dim_Products
                                where (a.Pepperi == "YES" && a.id_brand != 114)
                                select new BI_Dim_ProductsUOM
                                {
                                    id = a.id,
                                    Product = a.Product,
                                    Brand_Name = a.Brand_Name,
                                    id_subcategory = a.id_subcategory
,
                                    subcategory_name = a.subcategory_name,
                                    id_category = a.id_category,
                                    category_name = a.category_name,
                                    Vendor = a.Vendor,
                                    LeadTime = a.LeadTime,
                                    BuyUnitMsr = a.BuyUnitMsr,
                                    NumInBuy = a.NumInBuy,
                                    Active = a.Active,
                                    Blocked = a.Blocked,
                                    Pepperi = a.Pepperi,
                                    CatCommission = a.CatCommission,
                                    CommPerc = a.CommPerc,
                                    CodeBars = a.CodeBars,
                                    DSD = a.DSD,
                                    SRP = a.SRP,
                                    id_Vendor = a.id_Vendor,
                                    id_brand = a.id_brand,
                                    unitCost = a.unitCost,
                                    MinPorcent = a.MinPorcent,
                                    MinPrice = a.MinPrice,
                                    item_name = a.item_name,
                                    Vendor_Name = a.Vendor,
                                    Bonificables = a.Bonificables,
                                    MinPercIndiv = a.MinPercIndiv,
                                    CantBonif = a.CantBonif,
                                    FactorBonif = a.FactorBonif,
                                    Credits = a.Credits,
                                    lstuom = (from fd in dlipro.OpenSalesOrders_DetailsUOM where (fd.ItemCode == a.id) select new UomLstBIDIM { Units = fd.Units, UomCode = fd.UomCode }).ToList()

                                });


                //foreach (var item in products)
                //{
                //    var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == item.id) select new UomLstBIDIM { UomCode = a.UomCode, UomEntry = a.UomEntry }).ToList();
                //    item.lstuom = lstava;
                //}


                var usuarioBolsa = (from g in dblim.Sys_Users where (g.ID_User == activeuser.ID_User) select g.BolsaValor).FirstOrDefault();

                if (usuarioBolsa != 0 && usuarioBolsa != null)
                {
                    ViewBag.bolsa = usuarioBolsa;
                }
                else
                {
                    ViewBag.bolsa = 0;
                }

                var lstCategories = (from f in products select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in products select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name.Replace("'", ""), category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in products select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name.Replace("'", ""), category = f.subcategory_name.Replace("'", "") }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;

                if (r.Contains("Sales Supervisor"))
                {
                    List<int?> TagIds = activeuser.prop01.Split(',').Select(int.Parse).Cast<int?>().ToList();

                    int slpcode = Convert.ToInt32(activeuser.IDSAP);
                    var pedidos = (from g in dlipro.view_ordenesAbiertasDet
                                   join j in dlipro.OCRD on g.CardCode equals j.CardCode
                                   where (TagIds.Contains(g.SlpCode))
                                   select new lstPedidos { docnum = g.DocNum, date = g.DocDate, cardcode = g.CardCode, cardname = j.CardName, status = 0 }).Distinct().OrderBy(c => c.cardname).ToArray();

                    foreach (var item in pedidos)
                    {
                        item.cardname = item.cardname.Replace("'", "").Replace("\"", "").Replace(@"""", @"\""");
                    }

                    var docnumArray = pedidos.Select(c => c.docnum.ToString()).ToArray();
                    var docnumArrayInt = pedidos.Select(c => c.docnum).ToArray();

                    var existenBonif = (from s in internadli.Tb_Bonificaciones
                                        where (docnumArray.Contains(s.CodPedido) && s.OrderClosed == true)
                                        select s.CodPedido
                                   ).Distinct().Cast<int>().ToArray();

                    var existenCambiosPrecio = (from w in internadli.Tb_Autorizaciones
                                                where (docnumArrayInt.Contains(w.DocNum) && w.OrderClosed == true)
                                                select w.DocNum
                                   ).Distinct().Cast<int>().ToArray();

                    var pedidos2 = pedidos.Where(c => existenBonif.Contains(c.docnum)).ToArray();


                    foreach (var item in pedidos2)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 2;
                        }
                    }
                    pedidos2 = pedidos.Where(c => existenCambiosPrecio.Contains(c.docnum)).ToArray();
                    foreach (var item in pedidos2)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 2;
                        }
                    }
                    var existenBonifOpen = (from s in internadli.Tb_Bonificaciones
                                            where (docnumArray.Contains(s.CodPedido) && s.OrderClosed == false)
                                            select s.CodPedido
).Distinct().Cast<int>().ToArray();

                    var existenCambiosPrecioOpen = (from w in internadli.Tb_Autorizaciones
                                                    where (docnumArrayInt.Contains(w.DocNum) && w.OrderClosed == false)
                                                    select w.DocNum
                                   ).Distinct().Cast<int>().ToArray();

                    var pedidos3 = pedidos.Where(c => existenBonifOpen.Contains(c.docnum)).ToArray();

                    foreach (var item in pedidos3)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 1;
                        }
                    }
                    pedidos3 = pedidos.Where(c => existenCambiosPrecioOpen.Contains(c.docnum)).ToArray();
                    foreach (var item in pedidos3)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 1;
                        }
                    }
                    ArrayList myArrList = new ArrayList();
                    myArrList.AddRange((from p in pedidos
                                        select new
                                        {
                                            id = p.docnum,
                                            text = p.cardcode + " - " + p.cardname + " | #" + p.docnum,
                                            status = p.status
                                        }).ToList());
                    ViewBag.pedidos = JsonConvert.SerializeObject(myArrList);
                }
                else
                { //VENDEDORES
                    int slpcode = Convert.ToInt32(activeuser.IDSAP);
                    var pedidos = (from g in dlipro.view_ordenesAbiertasDet
                                   join j in dlipro.OCRD on g.CardCode equals j.CardCode
                                   where (g.SlpCode == slpcode)
                                   select new lstPedidos { docnum = g.DocNum, date = g.DocDate, cardcode = g.CardCode, cardname = j.CardName, status = 0 }).Distinct().OrderBy(c => c.cardname).ToArray();

                    foreach (var item in pedidos)
                    {
                        item.cardname = item.cardname.Replace("'", "").Replace("\"", "").Replace(@"""", @"\""");
                    }

                    var docnumArray = pedidos.Select(c => c.docnum.ToString()).ToArray();
                    var docnumArrayInt = pedidos.Select(c => c.docnum).ToArray();

                    var existenBonif = (from s in internadli.Tb_Bonificaciones
                                        where (docnumArray.Contains(s.CodPedido) && s.OrderClosed == true)
                                        select s.CodPedido
                                   ).Distinct().Cast<int>().ToArray();

                    var existenCambiosPrecio = (from w in internadli.Tb_Autorizaciones
                                                where (docnumArrayInt.Contains(w.DocNum) && w.OrderClosed == true)
                                                select w.DocNum
                                   ).Distinct().Cast<int>().ToArray();

                    var pedidos2 = pedidos.Where(c => existenBonif.Contains(c.docnum)).ToArray();


                    foreach (var item in pedidos2)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 2;
                        }
                    }
                    pedidos2 = pedidos.Where(c => existenCambiosPrecio.Contains(c.docnum)).ToArray();

                    foreach (var item in pedidos2)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 2;
                        }
                    }

                    var existenBonifOpen = (from s in internadli.Tb_Bonificaciones
                                            where (docnumArray.Contains(s.CodPedido) && s.OrderClosed == false)
                                            select s.CodPedido
               ).Distinct().Cast<int>().ToArray();

                    var existenCambiosPrecioOpen = (from w in internadli.Tb_Autorizaciones
                                                    where (docnumArrayInt.Contains(w.DocNum) && w.OrderClosed == false)
                                                    select w.DocNum
                                   ).Distinct().Cast<int>().ToArray();

                    var pedidos3 = pedidos.Where(c => existenBonifOpen.Contains(c.docnum)).ToArray();


                    foreach (var item in pedidos3)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 1;
                        }
                    }
                    pedidos3 = pedidos.Where(c => existenCambiosPrecioOpen.Contains(c.docnum)).ToArray();

                    foreach (var item in pedidos3)
                    {
                        var existe = pedidos.Where(e => e.docnum == item.docnum).FirstOrDefault();

                        if (existe == null)
                        {

                        }
                        else
                        {
                            existe.status = 1;
                        }
                    }

                    ArrayList myArrList = new ArrayList();
                    myArrList.AddRange((from p in pedidos
                                        select new
                                        {
                                            id = p.docnum,
                                            text = p.cardcode + " - " + p.cardname + " | #" + p.docnum,
                                            status = p.status
                                        }).ToList());
                    ViewBag.pedidos = JsonConvert.SerializeObject(myArrList);
                    //ViewBag.pedidos = pedidos;
                }


                return View(products);
                //return RedirectToAction("Dashboard_sales", "Main", null);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult Prices_requestHistory(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Prices Request History";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss3_submenu";
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
                //filtros de fecha //MENSUAL
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);
                //FILTROS**************
                DateTime filtrostartdate;
                DateTime filtroenddate;

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }
                //if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = DateTime.ParseExact(fstartd, "MM/dd/yyyy", CultureInfo.InvariantCulture); }
                //if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = DateTime.ParseExact(fendd, "MM/dd/yyyy", CultureInfo.InvariantCulture).AddHours(23).AddMinutes(59); }
                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();


                if (r.Contains("Sales Supervisor"))
                {
                    List<int?> TagIds = activeuser.prop02.Split(',').Select(int.Parse).Cast<int?>().ToList();
                    List<string> TagIdsString = activeuser.prop02.Split(',').ToList();

                    var products = (from a in internadli.Tb_Autorizaciones where ((a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate) && TagIds.Contains(a.ID_user)) select a);
                    var boni = (from b in internadli.Tb_Bonificaciones where ((b.FechaIngreso >= filtrostartdate && b.FechaIngreso <= filtroenddate) && TagIdsString.Contains(b.CodVendedor)) select b);

                    ViewBag.bonif = boni;
                    return View(products);
                } else if (r.Contains("Super Admin")) {

                    var products = (from a in internadli.Tb_Autorizaciones where ((a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate)) select a);
                    var boni = (from b in internadli.Tb_Bonificaciones where ((b.FechaIngreso >= filtrostartdate && b.FechaIngreso <= filtroenddate)) select b);

                    ViewBag.bonif = boni;
                    return View(products);
                }
                else {
                    string slpcode = activeuser.ID_User.ToString();

                    var products = (from a in internadli.Tb_Autorizaciones where ((a.FechaIngreso >= filtrostartdate && a.FechaIngreso <= filtroenddate) && a.ID_user == activeuser.ID_User) select a);
                    var boni = (from b in internadli.Tb_Bonificaciones where ((b.FechaIngreso >= filtrostartdate && b.FechaIngreso <= filtroenddate) && b.CodVendedor == slpcode) select b);

                    ViewBag.bonif = boni;
                    return View(products);
                }


           
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public class lstPedidos
        {
            public int docnum { get; set; }
            public DateTime? date { get; set; }
            public string cardcode { get; set; }
            public string cardname { get; set; }
            public int status { get; set; }
        
        }

        public ActionResult PedidoExiste(int docnum)
        {
            try
            {
                var so_details = (from a in dlipro.view_ordenesAbiertasDet where (a.DocNum == docnum) select a).Count();
                if (so_details > 0)
                {
                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else if (so_details == 0)
                {
                    var result = "NO EXIST";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex){
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
            var result3 = "NO EXIST";
            return Json(result3, JsonRequestBehavior.AllowGet);

        }

        public class MyObjObj
        {
            public Boolean? deleted { get; set; }
            public int lineNumber { get; set; }
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public string uom { get; set; }
            public int Qty { get; set; }
            public decimal Price { get; set; }
            public decimal MinPrice { get; set; }
            public decimal NewPrice { get; set; }
        }
        [HttpPost]
        public ActionResult Save_SODetails(List<MyObjObj> objects, int pedido, decimal bolsa, int timesClosed)
        {
            string ttresult = "";
            try
            {


                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    var pedidostring = pedido.ToString();
                    //Primero borramos todo rastro
                    internadli.Database.ExecuteSqlCommand("delete from Tb_Autorizaciones where DocNum={0}", pedido);
                    //Reiniciamos el estado de los productos eliminados para comprobar nuevamente
                    internadli.Database.ExecuteSqlCommand("update Tb_Bonificaciones set deleted=0 where CodPedido={0}", pedidostring);
                    //internadli.SaveChanges();
                    List<Tb_Autorizaciones> lsttosave = new List<Tb_Autorizaciones>();

                    var so_details = (from a in dlipro.view_ordenesAbiertasDet where (a.DocNum == pedido) select a).FirstOrDefault();
                    var cliente = (from a in dlipro.OCRD where (a.CardCode == so_details.CardCode) select a.CardName).FirstOrDefault();
                    var nombrec = "";
                    if (cliente == null)
                    {
                        nombrec = "XXXXXX";

                    }
                    else {
                        nombrec = cliente;
                    }

                    var usuario = (from a in dblim.Sys_Users where (a.ID_User == activeuser.ID_User) select a).FirstOrDefault();

                    if (objects != null && objects.Count >0) {
                
                        foreach (var items in objects)
                        {
                            Tb_Autorizaciones newDet = new Tb_Autorizaciones();
                            newDet.DocNum = pedido;
                            newDet.ItemCode = items.ItemCode;
                            newDet.Producto = items.Description;
                            newDet.CodUOM = items.uom;
                            newDet.UOM = "";
                            newDet.Cantidad = items.Qty;
                            newDet.PrecioPedido = items.Price;
                            newDet.PrecioMin = items.MinPrice;
                          
                            newDet.Estado = 1;
                            newDet.FechaIngreso = DateTime.UtcNow;
                            newDet.FechaValidacion = DateTime.UtcNow;
                            newDet.LineNum = items.lineNumber;
                            newDet.DocNumSAP = "";
                            newDet.OrderClosed = false;
                            newDet.ID_user = usuario.ID_User;
                            newDet.UserName = usuario.Name + " " + usuario.Lastname;
                            newDet.CodCustomer = so_details.CardCode;
                            newDet.Customer = nombrec;
                            newDet.DocPepperi = so_details.NoPepperi;
                            //ver cuantas veces se cerro la orden
                            newDet.closedOrderTimes = timesClosed;
                            newDet.DeletedSAP = 0;
                            newDet.Error = 0;
                            newDet.MensajeError = "";
                            if (items.deleted == true)
                            {
                                newDet.Deleted = true;
                                newDet.NuevoPrecio = 0;
                                
                                //Buscamos si existen bonificaciones
                                var existeProducto = (from a in internadli.Tb_Bonificaciones where (a.CodProducto == items.ItemCode && a.CodPedido == pedidostring) select a).ToList();

                                if (existeProducto.Count > 0) {
                                    foreach (var item in existeProducto) {
                                        Tb_Bonificaciones bonificationUpdate = item;
                                        bonificationUpdate.deleted = true;
                                        internadli.Entry(bonificationUpdate).State = EntityState.Modified;
                                    }
                                    internadli.SaveChanges();
                                }

                                //
                            }
                            else {
                                newDet.Deleted = false;
                                newDet.NuevoPrecio = items.NewPrice;
                            }

                            lsttosave.Add(newDet);

                        }

                        try
                        {

                            internadli.BulkInsert(lsttosave);


                        }
                        catch (Exception ex)
                        {
                            ttresult = "ERROR SAVING DATA: " + ex.Message;
                            return Json(ttresult, JsonRequestBehavior.AllowGet);
                        }


                    }
                    try
                    {
                        //Guardamos bolsa
                        usuario.BolsaValor = bolsa;
                        dblim.Entry(usuario).State = EntityState.Modified;
                        dblim.SaveChanges();
                    }
                    catch {

                    }







                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                ttresult = "EXPIRED SESSION, PLEASE REFRESH THE PAGE";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        [HttpPost]
        public ActionResult Save_SODetailsAfterOrderClosed(List<MyObjObj> objects, int pedido, decimal bolsa,int timesClosed)
        {
            string ttresult = "";
            try
            {


                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    var pedidostring = pedido.ToString();

                    //internadli.SaveChanges();
                    List<Tb_Autorizaciones> lsttosave = new List<Tb_Autorizaciones>();

                    var so_details = (from a in dlipro.view_ordenesAbiertasDet where (a.DocNum == pedido) select a).FirstOrDefault();
                    var cliente = (from a in dlipro.OCRD where (a.CardCode == so_details.CardCode) select a.CardName).FirstOrDefault();
                    var nombrec = "";
                    if (cliente == null)
                    {
                        nombrec = "XXXXXX";

                    }
                    else
                    {
                        nombrec = cliente;
                    }

                    var usuario = (from a in dblim.Sys_Users where (a.ID_User == activeuser.ID_User) select a).FirstOrDefault();

                    if (objects != null && objects.Count > 0)
                    {

                        foreach (var items in objects)
                        {
                            Tb_Autorizaciones newDet = new Tb_Autorizaciones();
                            newDet.DocNum = pedido;
                            newDet.ItemCode = items.ItemCode;
                            newDet.Producto = items.Description;
                            newDet.CodUOM = items.uom;
                            newDet.UOM = "";
                            newDet.Cantidad = items.Qty;
                            newDet.PrecioPedido = items.Price;
                            newDet.PrecioMin = items.MinPrice;
                             //ver cuantas veces se cerro la orden
                            newDet.closedOrderTimes = timesClosed;
                            newDet.DeletedSAP = 0;
                            newDet.Estado = 1;
                            newDet.FechaIngreso = DateTime.UtcNow;
                            newDet.FechaValidacion = DateTime.UtcNow;
                            newDet.LineNum = items.lineNumber;
                            newDet.DocNumSAP = "";
                            newDet.OrderClosed = true;
                            newDet.ID_user = usuario.ID_User;
                            newDet.UserName = usuario.Name + " " + usuario.Lastname;
                            newDet.CodCustomer = so_details.CardCode;
                            newDet.Customer = nombrec;
                            newDet.DocPepperi = so_details.NoPepperi;


                            if (items.deleted == true)
                            {
                                newDet.Deleted = true;
                                newDet.NuevoPrecio = 0;

                                //Se suprime esta funcion ya que NO HAY FORMA DE EVALUAR EL NUMERO DE LINEA DE LA BONIFICACION

                                //Buscamos si existen bonificaciones
                                var existeProducto = (from a in internadli.Tb_Bonificaciones where (a.CodProducto == items.ItemCode && a.CodPedido == pedidostring) select a).ToList();

                                if (existeProducto.Count > 0)
                                {
                                    foreach (var item in existeProducto)
                                    {
                                        Tb_Bonificaciones bonificationUpdate = item;
                                        bonificationUpdate.deleted = true;
                                        //bonificationUpdate.DocNum = "";
                                        internadli.Entry(bonificationUpdate).State = EntityState.Modified;
                                    }
                                    internadli.SaveChanges();
                                }

                                //
                            }
                            else
                            {
                                newDet.Deleted = false;
                                newDet.NuevoPrecio = items.NewPrice;
                            }

                            lsttosave.Add(newDet);

                        }

                        try
                        {

                            internadli.BulkInsert(lsttosave);


                        }
                        catch (Exception ex)
                        {
                            ttresult = "ERROR SAVING DATA: " + ex.Message;
                            return Json(ttresult, JsonRequestBehavior.AllowGet);
                        }


                    }
                    try
                    {
                        //Guardamos bolsa
                        usuario.BolsaValor = bolsa;
                        dblim.Entry(usuario).State = EntityState.Modified;
                        dblim.SaveChanges();
                    }
                    catch { }





                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                ttresult = "EXPIRED SESSION, PLEASE REFRESH THE PAGE";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }


        [HttpPost]
        public ActionResult ApproveBonification(int bonification, string producto, int unidades, decimal precio, DateTime date, string userID, string userName, int tipobonificacion, string uom)
        {
            string ttresult = "";
            try
            {
                var bon = (from a in internadli.Tb_Bonificaciones where (a.ID_bonificacion == bonification) select a).FirstOrDefault();
                var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == producto && a.UomCode == uom) select a.UomEntry).FirstOrDefault();


                bon.Estado = 2;
                bon.FechaValidacion = DateTime.UtcNow;
                bon.CantidadFinal = Convert.ToDecimal(unidades);
                bon.CurrencyFinal = precio;
                bon.FechaValidacion = date;
                bon.CodAutorizo = Convert.ToInt32(userID);
                bon.Autorizo = userName;
                bon.TipoIngreso = tipobonificacion;
                bon.CodUOM = uom;
                bon.UOM = lstava.ToString();

                internadli.Entry(bon).State = EntityState.Modified;
                    internadli.SaveChanges();


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
        public ActionResult DenyBonification(int bonification)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            string ttresult = "";
            try
            {
                var bon = (from a in internadli.Tb_Bonificaciones where (a.ID_bonificacion == bonification) select a).FirstOrDefault();

                bon.Estado = 4;
                bon.FechaValidacion = DateTime.UtcNow;
                bon.CodAutorizo = activeuser.ID_User;
                bon.Autorizo = activeuser.Name + " " + activeuser.Lastname;
                internadli.Entry(bon).State = EntityState.Modified;
                internadli.SaveChanges();


                ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }
        public ActionResult Save_bonification(string pedido, string producto, int? unidades,decimal precio, DateTime date, string userID, string userName, int tipobonificacion, string lineNum, string uom, int timesClosed)
        {

            try
            {
                if (unidades == null) { unidades = 0; }
                if (uom == null) { uom = ""; }
                int pedidodoc = Convert.ToInt32(pedido);
                var so_details = (from a in dlipro.view_ordenesAbiertasDet where (a.DocNum == pedidodoc && a.Quantity > 0) select a).ToArray();

                var codCliente = so_details.Select(a => a.CardCode).FirstOrDefault();
                var pepperi = so_details.Select(a => a.NoPepperi).FirstOrDefault();
                var cliente = (from a in dlipro.OCRD where (a.CardCode == codCliente) select a.CardName).FirstOrDefault();
                var productinfo = (from a in dlipro.BI_Dim_Products where (a.id == producto) select a).FirstOrDefault();
                var routedf = so_details.Select(a => a.U_DeliveryR).FirstOrDefault();
                var productinfoPedido = (from c in so_details where (c.ItemCode == producto) select c).FirstOrDefault();

                var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == producto && a.UomCode==uom) select  a.UomEntry).FirstOrDefault();
              
    
                Tb_Bonificaciones newBonificacion = new Tb_Bonificaciones();
                newBonificacion.CodPedido = pedido;
                newBonificacion.CodCliente = codCliente;
                newBonificacion.Cliente = cliente;
                newBonificacion.CodProducto = producto;
                newBonificacion.Producto = productinfo.Product;
                newBonificacion.ID_brand = productinfo.id_brand.ToString();
                newBonificacion.Brand = productinfo.Brand_Name;
                newBonificacion.CodUOM = uom;
                newBonificacion.UOM = lstava.ToString();
                newBonificacion.CodVendedor = userID;
                newBonificacion.Vendedor =userName;
                newBonificacion.Cantidad = Convert.ToDecimal(unidades);
                newBonificacion.Estado = 1;
                newBonificacion.DocNum = "";
                newBonificacion.FechaIngreso = date;
                newBonificacion.FechaValidacion = date;
                newBonificacion.ID_Vendor = productinfo.id_Vendor;
                newBonificacion.Vendor = productinfo.Vendor;
                //--Verificar cuantas veces se cerro la orden
                newBonificacion.closedOrderTimes = timesClosed;
                //
                newBonificacion.CodUOMAnt = uom;
                newBonificacion.UOMAnt = lstava.ToString();
                newBonificacion.CantidadFinal = Convert.ToDecimal(unidades);
                newBonificacion.deleted = false;
                newBonificacion.TipoIngreso = tipobonificacion;
                newBonificacion.LineNum = lineNum;
                newBonificacion.Currency = precio;
                newBonificacion.CurrencyFinal = precio;
                newBonificacion.Porcentaje = 0;
                newBonificacion.PorcentajeFinal = 0;
                newBonificacion.CodAutorizo = 0;
                newBonificacion.Autorizo = "";
                newBonificacion.OrderClosed = false;

                if (productinfoPedido != null)
                {
                    newBonificacion.CantidadPedido = Convert.ToInt32(productinfoPedido.Quantity);
                }
                else {
                    newBonificacion.CantidadPedido = 0;
                }

              
                newBonificacion.DocPepperi =pepperi;
                newBonificacion.RutaDef = routedf;
                newBonificacion.Error = 0;
                newBonificacion.MensajeError = "";
                newBonificacion.DeletedSAP = 0;

                internadli.Tb_Bonificaciones.Add(newBonificacion);
                internadli.SaveChanges();


                    //var userVendor = (from a in dblim.Sys_Users where (a.IDSAP == productinfo.id_Vendor) select a).FirstOrDefault();

                    //if (userVendor != null) {
                    //    //Send the email
                    //    dynamic semail = new Email("email_confirmation_bonification");
                    //    semail.To = userVendor.Email.ToString();
                    //    semail.From = "donotreply@limenainc.net";
                    //    semail.Send();
                    //}

                string ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CambiarPorcentajeMicro(int id, double factorn)
        {
            string ttresult = "";
            try
            {
                var producto = (from a in internadli.Tb_MaestroPorcentajes where (a.ID_detail == id) select a).FirstOrDefault();

                if (producto != null)
                {


                    producto.min_porc = factorn;
                    producto.updated = true;

                    internadli.Entry(producto).State = EntityState.Modified;
                    internadli.SaveChanges();

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {
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

        public class MyObj_UOM
        {
            public string UOM { get; set; }
            public int UOMEntry { get; set; }
       
        }


        public ActionResult GetUOM(string product)
        {
            try
            {
                if (product != "")
                {

                        var lstava = (from a in dlipro.OpenSalesOrders_DetailsUOM where (a.ItemCode == product) select new MyObj_UOM { UOM = a.UomCode, UOMEntry = a.UomEntry }).ToList();


                        //}
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstava);
                        return Json(result, JsonRequestBehavior.AllowGet);


                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult CambiarPorcentajePorUsuario(int id, double factorn)
        {
            string ttresult = "";
            try
            {
                var usuario = (from a in dblim.Sys_Users where (a.ID_User == id) select a).FirstOrDefault();

                if (usuario != null)
                {


                    usuario.PorcentajeBolsa = Convert.ToDecimal(factorn);              

                    dblim.Entry(usuario).State = EntityState.Modified;
                    dblim.SaveChanges();


                    internadli.Database.ExecuteSqlCommand("update Tb_ConfigBolsa set active=0 where ID_userSAP={0}", usuario.IDSAP);

                    List<Tb_ConfigBolsa> lstAdd = new List<Tb_ConfigBolsa>();

                    Tb_ConfigBolsa nuevoPorcentaje = new Tb_ConfigBolsa();
                    //Guardamos nueva conf para usuarios

                    Tb_ConfigBolsa nuevoPorcentajeUsuario = new Tb_ConfigBolsa();
                    nuevoPorcentajeUsuario.Description = "NUEVO VALOR PORCENTAJE BOLSA USUARIO";
                    nuevoPorcentajeUsuario.Value = Convert.ToDecimal(factorn);
                    nuevoPorcentajeUsuario.Date = DateTime.UtcNow;
                    nuevoPorcentajeUsuario.active = true;
                    nuevoPorcentajeUsuario.prop01 = "";
                    nuevoPorcentajeUsuario.ID_userSAP = Convert.ToInt32(usuario.IDSAP);
                    lstAdd.Add(nuevoPorcentajeUsuario);


                    internadli.BulkInsert(lstAdd);


                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                else
                {
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
        public ActionResult CambiarPorcentajeBolsa(decimal valor)
        {
            string ttresult = "";
            try
            {
                DateTime hora = DateTime.Now;
               
                //Primero actualizamos todos los estados a false
                internadli.Database.ExecuteSqlCommand("update Tb_ConfigBolsa set active=0");
                //internadli.SaveChanges();
                //Solicitamos la lista de usuarios
                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Sales Representative")).ToList();
                List<Tb_ConfigBolsa> lstAdd = new List<Tb_ConfigBolsa>();

                Tb_ConfigBolsa nuevoPorcentaje = new Tb_ConfigBolsa();
                nuevoPorcentaje.Description = "NUEVO PORCENTAJE BOLSA";
                nuevoPorcentaje.Value = valor;
                nuevoPorcentaje.Date =hora;
                nuevoPorcentaje.active = true;
                nuevoPorcentaje.prop01 = "";
                nuevoPorcentaje.ID_userSAP = 0;
                lstAdd.Add(nuevoPorcentaje);

            
                ////Guardamos nueva conf para usuarios
                //foreach (var item in usuarios) {
                //    Tb_ConfigBolsa nuevoPorcentajeUsuario = new Tb_ConfigBolsa();
                //    nuevoPorcentajeUsuario.Description = "NUEVO VALOR BOLSA USUARIO";
                //    nuevoPorcentajeUsuario.Value = valor;
                //    nuevoPorcentajeUsuario.Date = hora;
                //    nuevoPorcentajeUsuario.active = true;
                //    nuevoPorcentajeUsuario.prop01 = "";
                //    nuevoPorcentajeUsuario.ID_userSAP = Convert.ToInt32(item.IDSAP);
                //    lstAdd.Add(nuevoPorcentajeUsuario);
                //}

                internadli.BulkInsert(lstAdd);

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        public ActionResult OpenClosedOrder(string docnum)
        {

            var result = "";
            var docint = Convert.ToInt32(docnum);
            try
            {
                var bonif = (from a in internadli.Tb_Bonificaciones where (a.CodPedido == docnum) select a);
                var not = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docint) select b).Count();

                var pedido = (from a in dlipro.OpenSalesOrders where (a.NumSO == docint) select a).FirstOrDefault();


                var bonifcount = bonif.Count();

                int flag = 0;
                if (bonifcount > 0)
                {
                    flag = 1;
                    internadli.Database.ExecuteSqlCommand("update Tb_Bonificaciones set OrderClosed=0 where CodPedido={0}", docnum);

                }
                if (not > 0)
                {
                    flag = 1;
                    internadli.Database.ExecuteSqlCommand("update Tb_Autorizaciones set OrderClosed=0 where DocNum={0}", docint);
                }

                if (flag == 1)
                {

                }


                result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult Prices_requestCerrarOrden(string docnum, int timesClosed)
        {
            
            var result = "";
            var docint = Convert.ToInt32(docnum);
            try {
                var bonif = (from a in internadli.Tb_Bonificaciones where (a.CodPedido == docnum) select a);
                var not = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docint) select b).Count();

                var pedido = (from a in dlipro.OpenSalesOrders where (a.NumSO == docint) select a).FirstOrDefault();


                var bonifcount = bonif.Count();

                int flag = 0;
                if (bonifcount > 0) { flag = 1;
                    internadli.Database.ExecuteSqlCommand("update Tb_Bonificaciones set OrderClosed=1, closedOrderTimes={0} where CodPedido={1}", (timesClosed+1), docnum);

                    //Verificamos si hay bonificaciones eliminadas

                    internadli.Database.ExecuteSqlCommand("delete from Tb_Bonificaciones where deleted=1 and CodPedido={0}", docnum);


                    //PROCESO PARA ENVIAR TABLA DE BONIF A MARCA
                    //var arrayProdBon = bonif.Select(a => a.ID_Vendor).Distinct().ToArray(); //cargamos los vendors unicos

                    //foreach (var marca in arrayProdBon) { 
                    //var productsmarca = bonif.Where(a => a.ID_Vendor == marca);//cargamos los productos bonificados por marca

                    // var usersVendor = (from a in dblim.Sys_Users where (a.IDSAP == marca) select a).ToList(); //cargamos usuarios de vendors

                    //if (usersVendor.Count>0)
                    //{
                    //var tabla = "";
                    //foreach (var producto in productsmarca)//generamos la tabla a mostrar
                    //{
                    //    var tr = "";
                    //    tr = "<tr>";
                    //    tr += "<td>" + producto.Producto + "</td>";
                    //    tr += "<td>" + producto.CodUOM + "</td>";
                    //    if (producto.TipoIngreso == 1)
                    //    {
                    //        tr += "<td>" + "QUANTITY" + "</td>";
                    //    }
                    //    else if(producto.TipoIngreso==2){
                    //        tr += "<td>" + "CURRENCY" + "</td>";
                    //    }
                    //    tr += "<td>" + producto.CantidadFinal + "</td>";
                    //    tr += "<td>$ " + producto.CurrencyFinal + "</td>";
                    //    tr += "</tr>";
                    //    tabla += tr;
                    //}

                    //try {
                    //    foreach (var user in usersVendor)
                    //    { //enviamos mensaje a cada uno de los vendors
                    //      //Send the email

                    //        List<string> r = new List<string>(user.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                    //        if (r.Contains("Bonifications Emails"))
                    //        {
                    //            dynamic semail = new Email("email_confirmation_bonification");
                    //            semail.To = user.Email.ToString();
                    //            semail.From = "donotreply@limenainc.net";
                    //            semail.Date = DateTime.Now.ToShortDateString();
                    //            semail.SLP = pedido.SalesPerson.ToUpper();
                    //            semail.Customer = pedido.CustomerName.ToUpper();
                    //            semail.tabla = tabla;

                    //            semail.Send();

                    //        }


                    //    }
                    //}
                    //catch{

                    //}


                    //}

                    //}

                }
                if (not > 0) { flag = 1;
                    internadli.Database.ExecuteSqlCommand("update Tb_Autorizaciones set OrderClosed=1, closedOrderTimes={0} where DocNum={1}", (timesClosed + 1), docint);
                }

                if (flag == 1) {
                    
                }

                    
            result = "SUCCESS";
            return Json(result, JsonRequestBehavior.AllowGet);


        }
            catch (Exception ex)
            {
                result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }


        }

public ActionResult Prices_requestpdo(int docnum)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Prices Request";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss1_submenu";
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
                var so_details = (from a in dlipro.view_ordenesAbiertasDet where(a.DocNum==docnum && a.Quantity >0 && !a.TreeType.Contains("I")) select a).ToArray();
                var autolst = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docnum) select b).ToArray();

                var products = so_details.Select(a => a.ItemCode).ToArray();

                var infoproducts = (from c in dlipro.BI_Dim_Products where (products.Contains(c.id)) select c).ToArray();

                var finaldetails = (from a in so_details
                                   join b in autolst on a.ItemCode  equals b.ItemCode into data
                                   join c in infoproducts on a.ItemCode equals c.id
                                   from final in data.DefaultIfEmpty()             
                                   select new Pedidos_precios { LineNum = a.LineNum, Quantity = a.Quantity, CardCode = a.CardCode, UomCode = a.UomCode, UomEntry = a.UomEntry, ItemCode = a.ItemCode, ItemName = a.ItemName, DocNum = a.DocNum, DocDate = a.DocDate, Price = a.Price,
                                       NewPrice = final == null ? 0 : final.NuevoPrecio, Estado = final == null ? 0 : final.Estado, MinPrice=(a.NumPerMsr * c.MinPrice), Total=a.Price*a.Quantity, Brand_Name=c.Brand_Name, id_brand=c.id_brand,
                                       category_name=c.category_name, id_subcategory=c.id_subcategory, subcategory_name=c.subcategory_name, Bonificable=c.Bonificables, CantBonif=c.CantBonif, FactorBonif=c.FactorBonif, deleted= final ==null ? false : final.Deleted
                                   });

                //var arryprod = finaldetails.Select(a => a.ItemCode).ToArray();
                var codCliente = "";
                var cliente = "";
                double montoDeuda = 0.0;
                List<view_cuentascobrarCliente> deuda = new List<view_cuentascobrarCliente>();
                List<HistorialVentas> historial = new List<HistorialVentas>();
                List<Tb_Bonificaciones> bonificaciones = new List<Tb_Bonificaciones>();
                if (so_details != null) {
                    codCliente = so_details.Select(a => a.CardCode).FirstOrDefault();
                    cliente = (from a in dlipro.OCRD where (a.CardCode == codCliente) select a.CardName).FirstOrDefault();

                    deuda = (from a in dlipro.view_cuentascobrarCliente where (a.CardCode == codCliente) select a).ToList();

                    if (deuda.Count() > 0) {
                        montoDeuda = Convert.ToDouble(deuda.Sum(a => a.Saldo));
                    }

                    historial = dlipro.Database.SqlQuery<HistorialVentas>("Select * from view_historialVentas where CardCode=@id", new SqlParameter("@id", codCliente)).OrderBy(a=>a.PreviousPeriod).ToList();
                    var codped = docnum.ToString();
                    bonificaciones = internadli.Tb_Bonificaciones.Where(a => a.CodPedido == codped).ToList();

                }
                ViewBag.cliente = codCliente + "-" + cliente;
                ViewBag.deuda = deuda;
                ViewBag.deudaMonto = montoDeuda;
                ViewBag.historialVentas = historial;
                ViewBag.bonificaciones = bonificaciones;
                //ArrayList myArrList = new ArrayList();
                //myArrList.AddRange((from p in so_details
                //                    select new
                //                    {
                //                        id = p.ItemCode,
                //                        text = p.ItemCode + " | " + p.ItemName.Replace("'", "")
                //                    }).ToList());


                ////LISTADO DE REPRESENTANTES

                //ViewBag.lstproducts = JsonConvert.SerializeObject(myArrList);
                ViewBag.docnum = docnum;



                //SABER SI ES ADMIN
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    isAdmin = 1;
                   
                }
                else
                {
                    isAdmin = 0;
                }
                ViewBag.isAdmin = isAdmin;

                

                    var detailsBonifications = finaldetails.Where(c => c.Bonificable == "Y").ToList();
                //Incluimos los nuevos para bonificaciones  StockBonif>0
                var ordenDetails = finaldetails.FirstOrDefault();
                var gift_products = (from a in dlipro.BI_Dim_Products
                                     where (a.StockBonif > 0 && a.StockBonif != null && !a.id.Contains("SERVICES"))
                                     select new Pedidos_precios
                                     {
                                         LineNum = 10000,
                                         Quantity = a.StockBonif,
                                         CardCode = ordenDetails.CardCode,
                                         UomCode = "",
                                         UomEntry = 0,
                                         ItemCode = a.id,
                                         ItemName = "GIFT - " + a.Product,
                                         DocNum = docnum,
                                         DocDate = ordenDetails.DocDate,
                                         Price = 0,
                                         NewPrice = 0,
                                         Estado = 0,
                                         MinPrice = 0,
                                         Total = 0,
                                         Brand_Name = a.Brand_Name,
                                         id_brand = a.id_brand,
                                         category_name = a.category_name,
                                         id_subcategory = a.id_subcategory,
                                         subcategory_name = a.subcategory_name,
                                         Bonificable = a.Bonificables,
                                         CantBonif = a.StockBonif,
                                         FactorBonif = 1,
                                         deleted = false
                                     }).ToList();
                //
                detailsBonifications.AddRange(gift_products);

                var lstCategories = (from f in detailsBonifications select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in detailsBonifications select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name, category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in detailsBonifications select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name, category = f.subcategory_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;
                int slpcode = Convert.ToInt32(activeuser.IDSAP);
                //var pedidos = (from g in dlipro.view_ordenesAbiertasDet
                               //join j in dlipro.OCRD on g.CardCode equals j.CardCode
                               //where (g.SlpCode == slpcode)
                               //select new lstPedidos { docnum = g.DocNum, date = g.DocDate, cardcode = g.CardCode, cardname = j.CardName }).Distinct().ToArray();



                var usuarioBolsa = (from g in dblim.Sys_Users where (g.ID_User == activeuser.ID_User) select g.BolsaValor).FirstOrDefault();

                if (usuarioBolsa != 0 && usuarioBolsa != null)
                {
                    ViewBag.bolsa = usuarioBolsa;
                }
                else
                {
                    ViewBag.bolsa = 0;
                }
                ViewBag.userID = activeuser.ID_User;
                ViewBag.userName = activeuser.Name + " " + activeuser.Lastname;
                var arraysexcept = bonificaciones.Select(a => a.CodProducto).ToArray();
                ViewBag.products = detailsBonifications.Where(c => !arraysexcept.Contains(c.ItemCode));


                int contadorOrderClosed = 0;

                //Verificamos que boton mostrar
                var bonif = bonificaciones.Count();
    
                var not = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docnum) select b).Count();

                int flag = 0;
                if (bonif > 0)
                {
                    flag = 1;
                   
                }
                if (not > 0)
                {
                    flag = 1;
                }

                if (flag == 1)
                {
                    //EXISTEN BONIFICACIONES Y/O AUTORIZACIONES
                    ViewBag.mostrarOrdenEditar = 1;

                    var bonif2 = bonificaciones.Where(c => c.OrderClosed == true).Count();
                    var bonifGeneral = bonificaciones.Count();

                    if (bonifGeneral > 0) {
                        var bonCont = bonificaciones.Select(c => c.closedOrderTimes).FirstOrDefault();
                        contadorOrderClosed = bonCont;
                    }
                
                    
                    var not2 = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docnum && b.OrderClosed == true) select b).Count();
                    var notGeneral = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docnum) select b).Count();
                    if (notGeneral > 0) {
                        var notCont = (from b in internadli.Tb_Autorizaciones where (b.DocNum == docnum) select b.closedOrderTimes).FirstOrDefault();
                        contadorOrderClosed = notCont;
                    }

                    //NO MOSTRAMOS BOTONES PARA EDITAR NI PARA SALIR CON CERRAR ORDEN 
                   
                    if (bonif2 > 0) { ViewBag.mostrarOrdenEditar = 0; } //Como hay datos pero estan en true, no moestramos
                    if (not2 > 0) { ViewBag.mostrarOrdenEditar = 0; }

                    

      
                }
                else {
                    ViewBag.mostrarOrdenEditar = 2;
                }

                ViewBag.contadorClosed = contadorOrderClosed;


                return View(finaldetails);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult Prices_requestMicro()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Prices Request";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss1_submenu";
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
                var catalogo = (from a in internadli.Tb_MaestroPorcentajes select a);
                //Siempre tomara el registro con la propiedad activo=true para sacar el valor del porcentaje para los usuarios, luego de eso al cambiar
                //esta actualizara la misma tabla con la diferencia que agregara un registro nuevo tanto para conf de porcentaje de bolsa, como para conf de porcentaje por usuario
                var confbolsa = (from b in internadli.Tb_ConfigBolsa where (b.active == true) select b).FirstOrDefault();
                if (confbolsa != null) {
                    ViewBag.confbolsa = confbolsa.Value;
                    ViewBag.confbolsadate = confbolsa.Date;
                }


                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Sales Representative"));
                ViewBag.usuarios = usuarios;

                return View(catalogo);
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult Klass_report(int id) {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Klass Report";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "";
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

                var dt = new DataTable();
                dt = ImportExceltoDatatable(Server.MapPath("~/Content/documents/SPSKlass_ACEPTADA.xlsx"), "PropuestaAceptada");

                ViewBag.dt_test = ConvertDataTableToHTMLTable(dt);

                return View();
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult GetVisits(string id)
        {

            int idr = Convert.ToInt32(id);
            var visitas = new List<VisitsM>();
            var porcentaje = "";

            RoutesM rt = (from a in dbcmk.RoutesM where (a.ID_route == idr) select a).FirstOrDefault();

            visitas = (from obj in dbcmk.VisitsM where (obj.ID_route == idr) select obj).ToList();
            var lst = (from obj in dbcmk.VisitsM where (obj.ID_route == idr) select new { id = obj.ID_visit, store = obj.ID_store + " - " + obj.store, idstore = obj.ID_store, address = (obj.address + ", " + obj.city + ", " + obj.zipcode), visitstate = obj.ID_visitstate }).ToArray();


            //ESTADISTICA DE RUTAS POR ESTADO DE VISITAS
            decimal totalRutas = visitas.Count();


            //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
            decimal finishedorCanceled = (from e in visitas where (e.ID_visitstate == 4) select e).Count();
            decimal inprogressv = (from e in visitas where (e.ID_visitstate == 2) select e).Count();
            totalRutas = visitas.Count();

            ViewBag.finished = finishedorCanceled;

            if (totalRutas != 0)
            {
                if (inprogressv != 0 && finishedorCanceled != 0)
                {
                    decimal n = (finishedorCanceled / totalRutas) * 100;
                    decimal m = (inprogressv / totalRutas) * 50;
                    porcentaje = (n + m).ToString();

                }
                else if (inprogressv == 0 && finishedorCanceled != 0)
                {

                    porcentaje = (((Convert.ToDecimal(finishedorCanceled) / totalRutas) * 100)).ToString();
                }
                else if (inprogressv != 0 && finishedorCanceled == 0)
                {
                    porcentaje = (((Convert.ToDecimal(inprogressv) / totalRutas) * 50)).ToString();
                }
                else
                {
                    porcentaje = (Convert.ToDecimal(0)).ToString();
                }


            }
            else
            {
                porcentaje = "0";
            }

    

            var idsV = (from a in visitas select a.ID_visit).ToArray();
            var reps = (from e in dbcmk.VisitsM_representatives  where (idsV.Contains(e.ID_visit)) select e.ID_usuario).Distinct().ToArray();

            var lstUsers = (from a in dblim.Sys_Users where (reps.Contains(a.ID_User)) select new { Name = a.Name, LastName = a.Lastname }).ToList();

            
            


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result2 = javaScriptSerializer.Serialize(lst);
            string result3 = javaScriptSerializer.Serialize(lstUsers);
           
            var result = new { result = result2, result2=result3, porcentaje = porcentaje, sel = rt.query3 };
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public class storesbyRep
        {
            public string id { get; set; }
            public string text { get; set; }
        }

        public ActionResult getStoresByRep(int rep)
        {
            try
            {
                if (rep != 0)
                {

                    try
                    {
                        ArrayList myArrList2 = new ArrayList();
                        var replst = (from a in dblim.Sys_Users where (a.ID_User == rep) select a).FirstOrDefault();
                        if (replst != null) {
                            int idre = Convert.ToInt32(replst.prop02);
                            myArrList2.AddRange(dlipro.BI_Dim_Customer
                                    .Where(i => i.id_SalesRep == idre)
                                    .Select(i => new storesbyRep { id = i.id_Customer, text = i.Customer })
                                    .Distinct()
                                    .OrderBy(i => i.text)
                                    .ToList());
                        }
                        


                        string result = JsonConvert.SerializeObject(myArrList2);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {

                        return Json("error", JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        public ActionResult Routes_details(int? id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Marketing Activities (Route Details)";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "routes_submenu";
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

                //SELECCIONAMOS RUTAS
                var rutas = new List<VisitsM>();

  
                    rutas = dbcmk.VisitsM.Where(c => c.ID_route == id).ToList();
                


                //ESTADISTICA DE RUTAS POR ESTADO
                int totalRutas = rutas.Count();

                int onhold = (from e in rutas where (e.ID_visitstate == 3) select e).Count();
                int inprogress = (from e in rutas where (e.ID_visitstate == 2) select e).Count();
                int canceled = (from e in rutas where (e.ID_visitstate == 1) select e).Count();
                int finished = (from e in rutas where (e.ID_visitstate == 4) select e).Count();


                ViewBag.onhold = onhold;
                ViewBag.inprogress = inprogress;
                ViewBag.canceled = canceled;
                ViewBag.finished = finished;

                if (totalRutas != 0)
                {
                    ViewBag.onholdP = ((Convert.ToDecimal(onhold) / totalRutas) * 100);
                    ViewBag.inprogressP = ((Convert.ToDecimal(inprogress) / totalRutas) * 100);
                    ViewBag.canceledP = ((Convert.ToDecimal(canceled) / totalRutas) * 100);
                    ViewBag.finishedP = ((Convert.ToDecimal(finished) / totalRutas) * 100);
                }
                else
                {

                    ViewBag.onholdP = 0;
                    ViewBag.inprogressP = 0;
                    ViewBag.canceledP = 0;
                    ViewBag.finishedP = 0;
                }
                //Agregamos los representantes
                foreach (var itemVisita in rutas)
                {
                    var nombreRep = "";
                    var reps = (from e in dbcmk.VisitsM_representatives where (e.ID_visit == itemVisita.ID_visit) select e).ToList();



                    foreach (var itemrep in reps)
                    {
                        if (itemrep.ID_usuario == 0)//Es usuario demo
                        {
                            if (reps.Count() == 1)
                            {
                                nombreRep = itemrep.query1;
                            }
                            else if (reps.Count() > 1)
                            {
                                nombreRep += itemrep.query1 + ", ";
                            }
                        }
                        else
                        {
                            var usuario = (from u in dblim.Sys_Users where (u.ID_User == itemrep.ID_usuario) select u).FirstOrDefault();
                            if (reps.Count() == 1)
                            {
                                nombreRep = usuario.Name + " " + usuario.Lastname;
                            }
                            else if (reps.Count() > 1)
                            {
                                nombreRep += usuario.Name + " " + usuario.Lastname + ", ";
                            }
                        }

                    }
                    //utiliamos esta variable para el nombre del representante
                    itemVisita.city = nombreRep;
                }



                //MAPA DE RUTAS
                var demos_map = rutas;




                // Convertimos la lista a array
                ArrayList myArrList = new ArrayList();
                myArrList.AddRange((from p in demos_map
                                    select new
                                    {
                                        id = p.ID_visit,
                                        representatives = p.city,
                                        store = p.store,
                                        address = p.address,
                                        GeoLong = p.geoLong,
                                        GeoLat = p.geoLat,
                                        demo_state = p.ID_visitstate,
                                        customer = p.customer,
                                        date = p.visit_date.ToLongDateString(),
                                        comment = p.comments
                                    }).ToList());

                ViewBag.routes_map = JsonConvert.SerializeObject(myArrList);

                //Para ruta animada
                ArrayList myArrList2 = new ArrayList();
                myArrList2.AddRange((from p in demos_map
                                     where (p.ID_visitstate == 4 || p.ID_visitstate == 2)
                                     select new
                                     {
                                         GeoLong = p.geoLong,
                                         GeoLat = p.geoLat,
                                         demo_state = p.ID_visitstate,
                                         check_ind = p.check_in,
                                     }).OrderByDescending(c => c.demo_state).ThenBy(c => c.check_ind).ToList());

                ViewBag.routes_animated = JsonConvert.SerializeObject(myArrList2);
                //LISTADO DE TIENDAS


                try
                {
                    var visitas = (from a in rutas select a.ID_visit).ToArray();
                    var visitrep = (from a in dbcmk.VisitsM_representatives where (visitas.Contains(a.ID_visit)) select a).Take(1).FirstOrDefault();
                    var IDRep = visitrep.ID_usuario;

               
                    var replst = (from a in dblim.Sys_Users where (a.ID_User == IDRep) select a).FirstOrDefault();
                    if (replst != null)
                    {
                        int idre = Convert.ToInt32(replst.prop02);
                        var stores = (dlipro.BI_Dim_Customer
                                .Where(i => i.id_SalesRep == idre)
                                .Select(i => new storesbyRep { id = i.id_Customer, text = i.Customer })
                                .Distinct()
                                .OrderBy(i => i.text)
                                .ToList());

                        //var stores = (from b in dlipro.OCRD where ((b.Series == 61 || b.Series == 1) && b.CardCode.StartsWith("C") && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();
                        ViewBag.stores = stores;
                    }


                  

                }
                catch
                {

                }




                var ruta = (from rt in dbcmk.RoutesM where (rt.ID_route == id) select rt).FirstOrDefault();

                ViewBag.routename = ruta.query2;

                ViewBag.visitas = rutas;

                ViewBag.id_route = id;


                return View();


            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult Routes(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Marketing Activities (Routes)";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "routes_submenu";
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

                //VISITAS

                var visitas = new List<VisitsM>();
                var rutas = new List<RoutesM>();

                DateTime filtrostartdate;
                DateTime filtroenddate;

                //filtros de fecha //SEMANAL
                //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                //var saturday = sunday.AddDays(6).AddHours(23);
                //filtros de fecha //MENSUAL
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);
                //FILTROS**************

                if (fstartd == null || fstartd == "")
                {
                    filtrostartdate = sunday;
                }
                else
                {
                    filtrostartdate = Convert.ToDateTime(fstartd);
                }

                if (fendd == null || fendd == "")
                {
                    filtroenddate = saturday;
                }
                else
                {
                    filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59);
                }

                //SABER SI ES ADMIN
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    isAdmin = 1;
                    rutas = dbcmk.RoutesM.Where(dc => dc.date >= filtrostartdate && dc.end_date <= filtroenddate && dc.ID_empresa == 11).OrderByDescending(dc => dc.date).ToList();
                }
                else
                {
                    isAdmin = 0;                  
                    rutas = dbcmk.RoutesM.Where(dc => dc.date >= filtrostartdate && dc.end_date <= filtroenddate && dc.ID_empresa == 11).OrderByDescending(dc => dc.date).ToList();

                    var arryruts = rutas.Select(a => a.ID_route).ToArray();

                    var lstvisit = (from b in dbcmk.VisitsM where (arryruts.Contains(b.ID_route)) select b).ToList();

                    var arrvisit = lstvisit.Select(a => a.ID_visit).Distinct().ToArray();
                    var exist = (from a in dbcmk.VisitsM_representatives where (arrvisit.Contains(a.ID_visit) && a.ID_usuario == activeuser.ID_User) select a.ID_visit).ToArray();

                    arrvisit = (from b in arrvisit where (exist.Contains(b)) select b).ToArray();

                    lstvisit = (from a in lstvisit where (arrvisit.Contains(a.ID_visit)) select a).ToList();
                    arrvisit = lstvisit.Select(a => a.ID_route).Distinct().ToArray();

                    arryruts = (from c in arryruts where (arrvisit.Contains(c)) select c).ToArray();

                    rutas = (from c in rutas where (arryruts.Contains(c.ID_route)) select c).ToList();
                }

                ViewBag.isadmin = isAdmin;
                //FIN FILTROS*******************

                //visitas = dbcmk.VisitsM.Where(dc => dc.visit_date >= filtrostartdate && dc.end_date <= filtroenddate ).ToList();
     


                //Agregamos los representantes y tambien el estado de cada visita por REP filtro



                ////ESTADISTICA DE RUTAS POR ESTADO DE VISITAS
                //decimal totalRutas = visitas.Count();
                //foreach (var rutait in rutas)
                //{

                //    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                //    decimal finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4) && e.ID_route == rutait.ID_route) select e).Count();
                //    decimal inprogressv = (from e in visitas where (e.ID_visitstate == 2 && e.ID_route == rutait.ID_route) select e).Count();
                //    totalRutas = (from e in visitas where (e.ID_route == rutait.ID_route) select e).Count();

                //    ViewBag.finished = finishedorCanceled;

                //    if (totalRutas != 0)
                //    {
                //        if (inprogressv != 0 && finishedorCanceled != 0)
                //        {
                //            decimal n = (finishedorCanceled / totalRutas) * 100;
                //            decimal m = (inprogressv / totalRutas) * 50;
                //            rutait.query3 = (n + m).ToString();

                //        }
                //        else if (inprogressv == 0 && finishedorCanceled != 0)
                //        {

                //            rutait.query3 = (((Convert.ToDecimal(finishedorCanceled) / totalRutas) * 100)).ToString();
                //        }
                //        else if (inprogressv != 0 && finishedorCanceled == 0)
                //        {
                //            rutait.query3 = (((Convert.ToDecimal(inprogressv) / totalRutas) * 50)).ToString();
                //        }
                //        else
                //        {
                //            rutait.query3 = (Convert.ToDecimal(0)).ToString();
                //        }


                //    }
                //    else
                //    {
                //        rutait.query3 = "0";
                //    }
                //}

                ////MAPA DE RUTAS
                //var demos_map = (from a in rutas select a).ToList();


                //Convertimos la lista a array
                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Sales Representative"));
                //Convertimos la lista a array
                ArrayList myArrList = new ArrayList();
                myArrList.AddRange((from p in usuarios
                                    select new
                                    {
                                        id = p.ID_User,
                                        text = p.Name + " " + p.Lastname
                                    }).ToList());


                //LISTADO DE REPRESENTANTES

                ViewBag.usuarios = JsonConvert.SerializeObject(myArrList);
                ////LISTADO DE RUTAS
                //var rutass = dlipro.C_ROUTES.OrderBy(c => c.Code);
                //ViewBag.rutass = rutass.ToList();
                //LISTADO DE TIENDAS

                //List<MyObj_tablapadre> listapadres = (from p in CMKdbcmk.C_ROUTES
                //                                      select
                //             new MyObj_tablapadre
                //             {
                //                 id = p.Code,
                //                 text = p.Name
                //             }
                //                                      ).ToList();

                //List<tablahijospadre> listahijas = (from p in CMKdbcmk.C_ROUTE
                //                                    join store in CMKdbcmk.OCRD on p.U_CardCode equals store.CardCode
                //                                    select new tablahijospadre
                //                                    {
                //                                        id = p.U_CardCode,
                //                                        text = store.CardName.Replace("\"", "\\\""),
                //                                        parent = p.U_Route
                //                                    }).ToList();


                //List<MyObj_tablapadre> categoriasList = ObtenerCategoriarJerarquiaByName(listapadres, listahijas);
                //var stod = (from b in dlipro.OCRD
                //            where ((b.Series == 61 || b.Series==1) && b.CardCode.StartsWith("C") && b.CardName != null && b.CardName != "" && b.validFor == "Y") select b);
                //ArrayList myArrList2 = new ArrayList();
                //myArrList2.AddRange((from b in stod
                             
                //              select new 
                //              {
                //                  id = b.CardCode,
                //                  text = b.CardCode + " - " + b.CardName.Replace("'", "").Replace("\"", "").Replace(@"""", @"\""")
                //              }).ToList());
                //ViewBag.stores = JsonConvert.SerializeObject(myArrList2);
                //FIN LISTADO DE TIENDAS

                //LISTADO DE ACTIVIDADES

                List<MyObj_tablapadreAct> listapadresActivities = (from a in dbcmk.ActivitiesM_types
                                                                   select
                                                                      new MyObj_tablapadreAct
                                                                      {
                                                                          id = a.ID_activity.ToString(),
                                                                          text = a.description
                                                                      }
                                      ).ToList();

                List<tablahijospadreAct> listahijasActivities = (from p in dbcmk.FormsM
                                                                 select new tablahijospadreAct
                                                                 {
                                                                     id = p.ID_form.ToString(),
                                                                     text = p.name,
                                                                     parent = p.ID_activity.ToString()
                                                                 }).ToList();


                List<MyObj_tablapadreAct> categoriasListActivities = ObtenerCategoriarJerarquiaByIDAct(listapadresActivities, listahijasActivities);


                ViewBag.activitieslist = JsonConvert.SerializeObject(categoriasListActivities);

                //LISTADO DE CLIENTES
                var customers = (from b in dlipro.OCRD where (b.Series == 61 && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();
                ViewBag.customers = customers.ToList();

                //Filtros Viewbag
                //Filtros viewbag

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString(); ;
                //*****************

                List<Routes_calendar> rutaslst = new List<Routes_calendar>();

                foreach (var item in rutas)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_route + " - " + item.query2;
                    rt.url = "";
                    rt.start = item.date.ToString("yyyy-MM-dd");
                    rt.end = item.end_date.AddDays(1).ToString("yyyy-MM-dd");
                    //rt.color = "#.fc-event";//"#2081d6";
                    rt.className = ".fc-event";
                    rutaslst.Add(rt);
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst.ToArray());
                ViewBag.calroutes = result;

                return View(rutas.ToList());


            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public class MyObj_lstUsers
        {
            public string id { get; set; }
            public string text { get; set; }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVisitRoute(string id_store, string ID_route, DateTime date)
        {
            int IDR = Convert.ToInt32(ID_route);
            var route = dbcmk.RoutesM.Find(IDR);


            try
            {
                if (route != null)
                {
                    var storeSAP = (from a in dlipro.OCRD where (a.CardCode == id_store) select a).FirstOrDefault();

                    VisitsM visita = new VisitsM();
                    visita.ID_customer = "";
                    visita.customer = "";
                    visita.ID_store = id_store;
                    visita.store = storeSAP.CardName;
                    visita.address = storeSAP.MailAddres;
                    visita.city = storeSAP.MailCity;
                    if (storeSAP.MailZipCod == null)
                    {
                        visita.zipcode = "";
                    }
                    else { visita.zipcode = storeSAP.MailZipCod; }

                    if (storeSAP.State2 == null)
                    {
                        visita.state = "";
                    }
                    else { visita.state = storeSAP.State2; }
                    visita.visit_date = date;
                    visita.ID_visitstate = 3; //On Hold
                    visita.comments = "";
                    visita.check_in = date;
                    visita.check_out = date;
                    visita.end_date = date;
                    visita.extra_hours = 0;
                    visita.ID_route = IDR;
                    visita.ID_empresa = 11;
                    //GEOLOCALIZACION
                    try
                    {
                        string address = storeSAP.CardName.ToString() + ", " + storeSAP.MailAddres.ToString() + ", " + storeSAP.MailCity.ToString() + ", " + storeSAP.MailZipCod.ToString();
                        string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key=AIzaSyC3zDvE8enJJUHLSmhFAdWhPRy_tNSdQ6g&address={0}&sensor=false", Uri.EscapeDataString(address));

                        WebRequest request = WebRequest.Create(requestUri);
                        WebResponse response = request.GetResponse();
                        XDocument xdoc = XDocument.Load(response.GetResponseStream());

                        XElement result = xdoc.Element("GeocodeResponse").Element("result");
                        XElement locationElement = result.Element("geometry").Element("location");
                        XElement lat = locationElement.Element("lat");
                        XElement lng = locationElement.Element("lng");
                        //NO SE PORQUE LO TIRA AL REVEZ
                        visita.geoLat = lng.Value;
                        visita.geoLong = lat.Value;
                        //FIN

                    }
                    catch
                    {
                        visita.geoLong = "";
                        visita.geoLat = "";
                    }

                    dbcmk.VisitsM.Add(visita);
                    dbcmk.SaveChanges();


                    try
                    {
                        var visitas = (from a in dbcmk.VisitsM where (a.ID_route == IDR) select a.ID_visit).ToArray();
                        var visitrep = (from a in dbcmk.VisitsM_representatives where (visitas.Contains(a.ID_visit)) select a).Take(1).FirstOrDefault();
                        var IDRep = visitrep.ID_usuario;

                        VisitsM_representatives repvisita = new VisitsM_representatives();

                        repvisita.ID_visit = visita.ID_visit;
                        repvisita.ID_usuario = IDRep;
                        repvisita.query1 = "3";
                        repvisita.ID_empresa = visita.ID_empresa;
                        dbcmk.VisitsM_representatives.Add(repvisita);
                        dbcmk.SaveChanges();


                    }
                    catch
                    {
             
                    }



                    return RedirectToAction("Routes_details", "Commercial", new { id = IDR });
                }
                else
                {
              
                    return RedirectToAction("Routes_details", "Commercial", new { id = IDR });
                }
            }
            catch
            {
                TempData["advertencia"] = "Something wrong happened, try again.";
                return RedirectToAction("Routes_details", "Commercial", new { id = IDR });
            }


        }

        public ActionResult DelBonification(int id)
        {
            int docnum = 0;
            try
            {
                Tb_Bonificaciones bon = internadli.Tb_Bonificaciones.Find(id);
                if (bon != null)
                {
                    docnum = Convert.ToInt32(bon.CodPedido);
            
                    internadli.Tb_Bonificaciones.Remove(bon);
                    internadli.SaveChanges();
                }

                return RedirectToAction("Prices_requestpdo", "Commercial", new { docnum = docnum });
            }
            catch
            {

                return RedirectToAction("Prices_request", "Commercial", null);
            }



        }
        [ValidateAntiForgeryToken]
        public ActionResult DeleteVisit(string ID_visitD)
        {
            int routeid = 0;
            try
            {

                int id = Convert.ToInt32(ID_visitD);


                VisitsM visita = dbcmk.VisitsM.Find(id);
                if (visita != null)
                {
                    routeid = visita.ID_route;
                    dbcmk.VisitsM.Remove(visita);
                    dbcmk.SaveChanges();
                }


                //Eliminamos las actividades
                var actividades = (from e in dbcmk.ActivitiesM where (e.ID_visit == id) select e).ToList();
                var actividadesArray = (from f in dbcmk.ActivitiesM where (f.ID_visit == id) select f.ID_activity).ToArray();


                foreach (var act in actividades)
                {
                    ActivitiesM actividad = dbcmk.ActivitiesM.Find(act.ID_activity);
                    if (actividad != null)
                    {
                        dbcmk.ActivitiesM.Remove(actividad);
                        dbcmk.SaveChanges();
                    }
                }
                //Eliminamos los detalles de formulario

                var detalles = (from h in dbcmk.FormsM_details where (actividadesArray.Contains(h.ID_visit)) select h).ToList();
                //var detallesArray = (from k in db.FormsM_details where (actividadesArray.Contains(k.ID_visit)) select k.ID_details).ToArray();
                foreach (var det in detalles)
                {
                    FormsM_details detalle = dbcmk.FormsM_details.Find(det.ID_details);
                    if (detalle != null)
                    {
                        dbcmk.FormsM_details.Remove(detalle);
                        dbcmk.SaveChanges();
                    }
                }



                return RedirectToAction("Routes_details", "Commercial", new { id = routeid });
            }
            catch
            {

                return RedirectToAction("Routes_details", "Commercial", new { id = routeid });
            }



        }

        public ActionResult Marketing_activities(string user, int visit)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    ViewBag.isAdmin = 1;
                }
                else
                {
                    ViewBag.isAdmin = 0;
                }

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Marketing activities";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "routes_submenu";
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
                //VARIABLES GLOBALES

                int idv = visit;
                var visitsM = (from a in dbcmk.VisitsM where (a.ID_visit == idv) select a).FirstOrDefault();

                ViewBag.id_route = visitsM.ID_route;
                ViewBag.id_visit = visit;
                ViewBag.id_user = activeuser.ID_User;
                //
                //SECCION DE FILTROS
                MyObj_lstUsers[] ArrayUsers = new MyObj_lstUsers[0];

                //VisitsM visit = new VisitsM();
                if (activeuser.Departments == "Sales" && activeuser.Roles == "Sales Representative")
                {
                    MyObj_lstUsers userSel = new MyObj_lstUsers();

                    userSel.id = activeuser.ID_User.ToString();
                    userSel.text = activeuser.Name + " " + activeuser.Lastname;
                    List<MyObj_lstUsers> lst = new List<MyObj_lstUsers>();
                    lst.Add(userSel);

                    ArrayUsers = lst.ToArray();
                    //Estado de visita
                    var rep = (from a in dbcmk.VisitsM_representatives
                               where (a.ID_visit == idv && a.ID_usuario == activeuser.ID_User)
                               select a).FirstOrDefault();

                    if (rep != null)
                    {
                        ViewBag.estadovisita = Convert.ToInt32(rep.query1);//Utilizaremos este campo para filtrar el estado por usuario
                    }
                    else {
                        ViewBag.estadovisita = 3;//Utilizaremos este campo para filtrar el estado por usuario
                    }
                 

                }
                else
                {

                    ArrayUsers = (from b in dblim.Sys_Users where (b.Active == true && b.Departments == "Sales" && b.Roles == "Sales Representative" && b.ID_Company == activeuser.ID_Company) select new MyObj_lstUsers { id = b.ID_User.ToString(), text = b.Name.ToUpper() + " " + b.Lastname.ToUpper() }).ToArray();
                    ViewBag.estadovisita = visitsM.ID_visitstate;
                }

                ViewBag.lstUsers = JsonConvert.SerializeObject(ArrayUsers);
                //if ((user != null || user !="") && (store !=null || store !="")) {
                //    activities = (from a in dbcmk.ActivitiesM where(a.visit_date >= filtrostartdate && a.end_date <= filtroenddate))
                //}

                //CREACION DE ACTIVIDADES//
                var activeForms = (from at in dbcmk.FormsM where (at.ID_empresa == 11) select at).ToList();
                ViewBag.activeForms = activeForms;

                var lstCustomer = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select new { id = b.CardCode, text = b.CardName }).OrderBy(b => b.text).ToArray();
                ViewBag.lstCustomer = JsonConvert.SerializeObject(lstCustomer);
                ///END
                ///
                var lstbrands = dlipro.BI_Dim_Products
                        .Where(i=> i.id_brand !=null)
                        .Select(i => new { id = i.id_brand.ToString(), text = i.Brand_Name})
                        .Distinct()
                        .OrderBy(i => i.text)
                        .ToArray();

                ViewBag.lstBrands = JsonConvert.SerializeObject(lstbrands);

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public class brands
        {
            public string FirmCode { get; set; }
            public string FirmName { get; set; }
            public string Customer { get; set; }
            public Boolean isselected { get; set; }
        }
        //OLD
        //public ActionResult Marketing_activities(string user,string visit, string fstartd, string fendd, string store)
        //{
        //    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
        //    if (activeuser != null)
        //    {
        //        if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor")) {
        //            ViewBag.isAdmin = 1;
        //        }
        //        else{
        //            ViewBag.isAdmin = 0;
        //        }

        //        //HEADER
        //        //PAGINAS ACTIVAS
        //        ViewData["Menu"] = "Commercial";
        //        ViewData["Page"] = "Marketing activities";
        //        ViewBag.menunameid = "marketing_menu";
        //        ViewBag.submenunameid = "markact_submenu";
        //        List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
        //        ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
        //        List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
        //        ViewBag.lstRoles = JsonConvert.SerializeObject(r);
        //        ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
        //        //NOTIFICATIONS
        //        DateTime now = DateTime.Today;
        //        List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
        //        ViewBag.lstAlerts = lstAlerts;
        //        //FIN HEADER
        //        //VARIABLES GLOBALES
        //        //List<ActivitiesM> activities = new List<ActivitiesM>();
        //        MyObj_lstUsers[] ArrayUsers = new MyObj_lstUsers[0];

        //        //VisitsM visit = new VisitsM();
        //        if (activeuser.Departments == "Sales" && activeuser.Roles == "Sales Representative")
        //        {
        //            MyObj_lstUsers userSel = new MyObj_lstUsers();

        //            userSel.id = activeuser.ID_User.ToString();
        //            userSel.text = activeuser.Name + " " + activeuser.Lastname;
        //            List<MyObj_lstUsers> lst = new List<MyObj_lstUsers>();
        //            lst.Add(userSel);

        //            ArrayUsers = lst.ToArray();

        //        }
        //        else
        //        {

        //            ArrayUsers = (from b in dblim.Sys_Users where (b.Active == true && b.Departments == "Sales" && b.Roles == "Sales Representative" && b.ID_Company == activeuser.ID_Company) select new MyObj_lstUsers { id = b.ID_User.ToString(), text = b.Name.ToUpper() + " " + b.Lastname.ToUpper() }).ToArray();
        //        }

        //        ViewBag.lstUsers = JsonConvert.SerializeObject(ArrayUsers);

        //        //
        //        //SECCION DE FILTROS
        //        //FILTROS VARIABLES
        //        DateTime filtrostartdate;
        //        DateTime filtroenddate;
        //        //filtros de fecha (DIARIO)
        //        //var sunday = DateTime.Today;
        //        //var saturday = sunday.AddHours(23);
        //        ////filtros de fecha (SEMANAL)
        //        var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        //        var saturday = sunday.AddDays(6).AddHours(23);

        //        if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
        //        if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

        //        ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
        //        ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
        //        ////
        //        //if ((user != null || user !="") && (store !=null || store !="")) {
        //        //    activities = (from a in dbcmk.ActivitiesM where(a.visit_date >= filtrostartdate && a.end_date <= filtroenddate))
        //        //}

        //        //CREACION DE ACTIVIDADES//
        //        var activeForms = (from at in dbcmk.FormsM where (at.ID_empresa == 11) select at).ToList();
        //        ViewBag.activeForms = activeForms;

        //        var lstCustomer = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select new { id = b.CardCode, text = b.CardName }).OrderBy(b => b.text).ToArray();
        //        ViewBag.lstCustomer = JsonConvert.SerializeObject(lstCustomer);
        //        ///END

        //        return View();

        //    }
        //    else
        //    {

        //        return RedirectToAction("Login", "Home", new { access = false });

        //    }

        //}
        public class MyObj_lstStoresInputSel
        {
            public string id { get; set; }
            public string text { get; set; }
        }
        public class tablahijospadreInputSel
        {
            public string id { get; set; }
            public string text { get; set; }
            public string parent { get; set; }
        }

        public class MyObj_tablapadreInputSel
        {
            public string id { get; set; }
            public string text { get; set; }
            public List<MyObj_tablapadreInputSel> children { get; set; }
        }
        public ActionResult GetStores(string UserId, string start, string end)
        {
            try
            {
                if (UserId != null || UserId != "")
                {
                    int ID = Convert.ToInt32(UserId);
                    DateTime dts = Convert.ToDateTime(start);
                    DateTime dte = Convert.ToDateTime(end).AddHours(23);


                    var visitrep = (from gg in dbcmk.VisitsM_representatives where (gg.ID_usuario == ID) select gg.ID_visit).ToArray();

                    var visitas = (from r in dbcmk.VisitsM where (visitrep.Contains(r.ID_visit) && r.visit_date >= dts && r.end_date <= dte) select r).ToArray();

                    var arrayVisiID = (from arr in visitas select arr.ID_route).ToArray();
                    var rutas = (from rut in dbcmk.RoutesM where (arrayVisiID.Contains(rut.ID_route)) select rut).ToList();




                    List<MyObj_tablapadreInputSel> listapadres = (from p in rutas
                                                                  select
                                         new MyObj_tablapadreInputSel
                                         {
                                             id = p.ID_route.ToString(),
                                             text = p.query2
                                         }
                                                          ).ToList();

                    List<tablahijospadreInputSel> listahijas = (from p in rutas
                                                                join store in visitas on p.ID_route equals store.ID_route
                                                                select new tablahijospadreInputSel
                                                                {
                                                                    id = store.ID_visit.ToString(),
                                                                    text = store.store.Replace("\"", "\\\""),
                                                                    parent = p.ID_route.ToString()
                                                                }).ToList();


                    List<MyObj_tablapadreInputSel> categoriasList = ObtenerCategoriarJerarquiaByName(listapadres, listahijas);


                    //}
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    string result = javaScriptSerializer.Serialize(categoriasList);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }
        //CABE MENCIONAR QUE CON ESTOS DOS METODOS SE RELACIONA EL PADRE POR LA CARACTERISTICA TEXT LO CUAL VIENE SIENDO EQUIVALENTE
        //AL NOMBRE O DESCRIPCION DEL ITEM. ESTO SE HIZO POR LA RELACION EN LA BASE DE DATOS QUE ES PARA RUTAS, PERO EN TEORIA TENDRIA QUE SER POR ID Y NO POR NAME
        public static List<MyObj_tablapadreInputSel> ObtenerCategoriarJerarquiaByName(List<MyObj_tablapadreInputSel> Categoriaspadre, List<tablahijospadreInputSel> categoriashijas)
        {
            List<MyObj_tablapadreInputSel> query = (from item in Categoriaspadre

                                                    select new MyObj_tablapadreInputSel
                                                    {
                                                        id = item.id, //SI QUEREMOS AGRUPAR POR ID SE LO PONEMOS, SINO SE LO QUITAMOS PARA QUE NOS CARGUE LAS TIENDAS DESPLEGADAS
                                                        text = item.text.Replace("'", ""),
                                                        children = ObtenerHijos(item.id, categoriashijas)
                                                    }).ToList();

            return query;
        }
        private static List<MyObj_tablapadreInputSel> ObtenerHijos(string Categoria, List<tablahijospadreInputSel> categoriashijas)
        {
            List<MyObj_tablapadreInputSel> query = (from item in categoriashijas

                                                    where item.parent == Categoria
                                                    select new MyObj_tablapadreInputSel
                                                    {
                                                        id = item.id,
                                                        text = item.text.Replace("'", ""),
                                                        children = null
                                                    }).ToList();

            return query;
        }


        public ActionResult GetVisitData(string UserId, string VisitId, string isadmin)
        {
            try
            {
                if ((UserId != null || UserId != "") && (VisitId != null || VisitId != ""))
                {
                    int IDU = Convert.ToInt32(UserId);
                    int IDV = Convert.ToInt32(VisitId);

                    List<ActivitiesM> lstActivities = new List<ActivitiesM>();

                    if (isadmin == "1")
                    {
                        lstActivities = (from a in dbcmk.ActivitiesM where (a.ID_visit == IDV) select a).ToList();
                    }
                    else {
                        lstActivities = (from a in dbcmk.ActivitiesM where (a.ID_visit == IDV && a.ID_usuarioEnd == IDU) select a).ToList();
                    }

                    if (lstActivities.Count() > 0)
                    {
                        foreach (var item in lstActivities)
                        {
                            var brand = (from a in dbcmk.FormsM_details where (a.ID_visit == item.ID_activity && a.ID_formresourcetype == 13) select a).FirstOrDefault();
                            if (brand != null)
                            {
                                item.Customer = brand.fdescription;

                            }

                            var usuario = (from a in dblim.Sys_Users where (a.ID_User == item.ID_usuarioEnd) select a).FirstOrDefault();
                            if (usuario != null)
                            {
                                item.comments = usuario.Name + " " + usuario.Lastname;
                            }
                            else {
                                item.comments = "";
                            }
          
                        }
                    }

                    //List<VisitsM> selVisit = (from b in dbcmk.VisitsM where (b.ID_visit == IDV) select b).ToList();
                    var areas = dbcmk.VisitsM.Where(x => x.ID_visit == IDV).Select(a => new { ID_Visit = a.ID_visit,storeDate=a.visit_date,comment=a.comments, storeName = a.store, ID_Store = a.ID_store, storeAddress = (a.address + ", " + a.state + ", " + a.city + ", " + a.zipcode), ID_visitstate = a.ID_visitstate, lng = a.geoLong, lat = a.geoLat });


                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    var acts = javaScriptSerializer.Serialize(lstActivities);

                    var result = new { resultAct = acts, resultV = areas };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        //CHECK IN
        public ActionResult Check_in(string ID_visit, string check_in, string lat, string lng)
        {
            try
            {
                int IDvisita = Convert.ToInt32(ID_visit);

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    VisitsM visita = dbcmk.VisitsM.Find(Convert.ToInt32(ID_visit));
                    if (visita != null)
                    {
                        //Cambiamos estado de visita global
                        visita.ID_visitstate = 2;
                        visita.check_in = Convert.ToDateTime(check_in);
                        dbcmk.Entry(visita).State = EntityState.Modified;
                        //dbcmk.SaveChanges();

                        //Cambios estado de visita por rep
                        VisitsM_representatives rep = (from a in dbcmk.VisitsM_representatives
                                                       where (a.ID_visit == IDvisita && a.ID_usuario == activeuser.ID_User)
                                                       select a).FirstOrDefault();
                        rep.query1 = "2";
                        dbcmk.Entry(rep).State = EntityState.Modified;
                        //dbcmk.SaveChanges();


                        if (lat != null || lat != "")
                        {
                            //Guardamos el log de la actividad
                            ActivitiesM_log nuevoLog = new ActivitiesM_log();
                            nuevoLog.latitude = lat;
                            nuevoLog.longitude = lng;
                            nuevoLog.ID_usuario = activeuser.ID_User;
                            nuevoLog.ID_activity = 0;
                            nuevoLog.fecha_conexion = Convert.ToDateTime(check_in);
                            nuevoLog.query1 = ID_visit;
                            nuevoLog.query2 = "";
                            nuevoLog.action = "CHECK IN  - " + visita.store;
                            nuevoLog.ip = "";
                            nuevoLog.hostname = "";
                            nuevoLog.typeh = "";
                            nuevoLog.continent_name = "";
                            nuevoLog.country_code = "";
                            nuevoLog.country_name = "";
                            nuevoLog.region_code = "";
                            nuevoLog.region_name = "";
                            nuevoLog.city = "";

                            dbcmk.ActivitiesM_log.Add(nuevoLog);
                           
                        }
                        dbcmk.SaveChanges();
                        return Json(new { Result = "Success" });
                    }
                }

                return Json(new { Result = "Fail" });
            }
            catch
            {
                return Json(new { Result = "Error" });
            }



        }
        //CHECK IN
        public ActionResult saveComment(string ID_visit, string comments)
        {
            try
            {
                int IDvisita = Convert.ToInt32(ID_visit);

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    VisitsM visita = dbcmk.VisitsM.Find(Convert.ToInt32(ID_visit));
                    if (visita != null)
                    {
                        //Cambiamos estado de visita global
                        visita.comments = comments;
                        dbcmk.Entry(visita).State = EntityState.Modified;


                        dbcmk.SaveChanges();
                        return Json(new { Result = "Success" });
                    }
                }

                return Json(new { Result = "Fail" });
            }
            catch
            {
                return Json(new { Result = "Error" });
            }



        }

        public ActionResult updatecustomerstatus(string id, string cardcode)
        {
            try
            {
                int idstatus = Convert.ToInt32(id);

                //Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //if (activeuser != null)
                //{
                try
                {
                    var del = (from a in internadli.Tb_customerstatus where (a.CardCode == cardcode && a.DocEntry == "0") select a).ToList();
                    if (del.Count > 0)
                    {
                        internadli.Tb_customerstatus.BulkDelete(del);
                    }
                }
                catch {

                }

              
                    Tb_customerstatus newtrans = new Tb_customerstatus();

                        //Cambiamos estado de visita global
                        newtrans.Active_customer = idstatus;
                        newtrans.CardCode = cardcode;
                        newtrans.Error = 0;
                        newtrans.ErrorMsg = "";
                        newtrans.DocEntry = "0";
                internadli.Tb_customerstatus.Add(newtrans);
                internadli.SaveChanges();


                return Json(new { Result = "SUCCESS" });
      
            }
            catch
            {
                return Json(new { Result = "Error" });
            }



        }


        //CHECK OUT
        public ActionResult Check_out(string ID_visit, string check_in, string lat, string lng)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null)
                {
                    int idid = Convert.ToInt32(ID_visit);
                   //CON ESTO EVALUAMOS LA VISITA COMPLETA
                    bool flagok = true;
                    var actvities = (from ac in dbcmk.ActivitiesM where (ac.ID_visit == idid) select ac).ToList();

                    foreach (var item in actvities)
                    {
                        if (item.isfinished == false) { flagok = false; }

                    }


                    if (flagok != false)
                    {

                        VisitsM visita = dbcmk.VisitsM.Find(Convert.ToInt32(ID_visit));
                        if (visita != null)
                        {
                            visita.ID_visitstate = 4; //FINALIZADO
                            visita.check_out = Convert.ToDateTime(check_in);
                            dbcmk.Entry(visita).State = EntityState.Modified;
                            dbcmk.SaveChanges();


                            if (lat != null || lat != "")
                            {
                                ////Guardamos el log de la actividad
                                ActivitiesM_log nuevoLog = new ActivitiesM_log();
                                nuevoLog.latitude = lat;
                                nuevoLog.longitude = lng;
                                nuevoLog.ID_usuario = activeuser.ID_User;
                                nuevoLog.ID_activity = 0;
                                nuevoLog.fecha_conexion = Convert.ToDateTime(check_in);
                                nuevoLog.query1 = ID_visit;
                                nuevoLog.query2 = "";
                                nuevoLog.action = "CHECK OUT  - " + visita.store;
                                nuevoLog.ip = "";
                                nuevoLog.hostname = "";
                                nuevoLog.typeh = "";
                                nuevoLog.continent_name = "";
                                nuevoLog.country_code = "";
                                nuevoLog.country_name = "";
                                nuevoLog.region_code = "";
                                nuevoLog.region_name = "";
                                nuevoLog.city = "";

                                dbcmk.ActivitiesM_log.Add(nuevoLog);
                                dbcmk.SaveChanges();
                            }


                        }
                    }


                    bool flagokrep = true;
         
                    //Cambios estado de visita por rep
                    VisitsM_representatives rep = (from a in dbcmk.VisitsM_representatives
                                                   where (a.ID_visit == idid && a.ID_usuario == activeuser.ID_User)
                                                   select a
                         ).FirstOrDefault();

                    var actvitiesrep = (from ac in dbcmk.ActivitiesM where (ac.ID_visit == idid && ac.ID_usuarioEnd == activeuser.ID_User) select ac).ToList();

                    foreach (var item in actvitiesrep)
                    {
                        if (item.isfinished == false) { flagokrep = false; }

                    }

                    if (flagokrep != false)
                    {
                        rep.query1 = "4";
                        dbcmk.Entry(rep).State = EntityState.Modified;
                        dbcmk.SaveChanges();

                        return Json(new { Result = "Success" });
                    }
                    else
                    {

                        return Json(new { Result = "There are some incomplete activities. Please check and try again" });
                    }
                }
                return Json(new { Result = "There are some incomplete activities. Please check and try again" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Error: " + ex.Message });
            }



        }

        public ActionResult CreateActivity(string ID_form, string ID_customer, string Customer, string ID_visita, string ID_rep, string ID_cat, string cat, string ID_subcat, string subcat, string ID_brand,string brand)
        {
            try
            {
                int IDForm = Convert.ToInt32(ID_form);
                int IDRep = 0;

                int esDemoUser = 0;//Variable para identificar si el usuario seleccionado es demo o es un reps

                try
                {
                    IDRep = Convert.ToInt32(ID_rep);
                    int vi = Convert.ToInt32(ID_visita);
                    var visit = (from a in dbcmk.VisitsM_representatives where (a.ID_visit == vi) select a).Take(1).FirstOrDefault();
                    IDRep = visit.ID_usuario;
                    esDemoUser = 0;
                }
                catch
                {
                    esDemoUser = 1;
                }

                int idb = Convert.ToInt32(ID_brand);

                var custom = (from a in dlipro.BI_Dim_Products where (a.id_brand == idb) select a).FirstOrDefault();


                //CREAMOS LA ESTRUCTURA DE LA ACTIVIDAD
                ActivitiesM nuevaActividad = new ActivitiesM();

                nuevaActividad.ID_form = Convert.ToInt32(ID_form);
                nuevaActividad.ID_visit = Convert.ToInt32(ID_visita);
                nuevaActividad.ID_customer = custom.id_Vendor;
                nuevaActividad.Customer = custom.Vendor;
                nuevaActividad.comments = "";
                nuevaActividad.check_in = DateTime.Today.Date;
                nuevaActividad.check_out = DateTime.Today.Date;
                nuevaActividad.query1 = "";
                nuevaActividad.ID_empresa = 11;//Codigo de limena en Comercia DB
                nuevaActividad.isfinished = false;
                nuevaActividad.description = "";
                nuevaActividad.ID_activitytype = 0;

                nuevaActividad.date = DateTime.Today.Date;
                var form = (from c in dbcmk.FormsM where (c.ID_form == IDForm) select c).FirstOrDefault();
                if (form != null)
                {
                    nuevaActividad.description = form.name;
                    nuevaActividad.ID_activitytype = form.ID_activity;
                    nuevaActividad.query1 = "";
                    //if (form.ID_activity == 4) //PARA DEMOS
                    //{
                    //    nuevaActividad.date = time;
                    //}
                }


                int ID_usuario = Convert.ToInt32(Session["IDusuario"]);
                nuevaActividad.ID_usuarioCreate = ID_usuario;

                //OJO ESTA PARTE SE AGREGO PARA COMERCIA ES PROPIA DE LA EMPRESA
                if (nuevaActividad.ID_activitytype != 4)
                {
                    nuevaActividad.ID_usuarioEnd = IDRep;  //Usuario que sera asignado
                    nuevaActividad.ID_usuarioEndString = "";
                }
                else
                {
                    //Guardamos el usuario de SAP en la variable tipo String
                    if (esDemoUser == 1)
                    {
                        nuevaActividad.ID_usuarioEnd = 0;
                    }
                    else
                    {
                        nuevaActividad.ID_usuarioEnd = IDRep;

                    }
                    nuevaActividad.ID_usuarioEndString = ID_rep;
                }

                //SOLO PARA COMERCIA
                if (nuevaActividad.ID_activitytype == 4)
                {

                    nuevaActividad.query1 = "start";
                }



                //guardamos
                dbcmk.ActivitiesM.Add(nuevaActividad);
                dbcmk.SaveChanges();

                //LUEGO ASIGNAMOS LA PLANTILLA DE FORMULARIO A LA ACTIVIDAD
                //Guardamos el detalle
                var detalles_acopiar = (from a in dbcmk.FormsM_details where (a.ID_formM == IDForm && a.original == true) select a).ToList();



                foreach (var item in detalles_acopiar)
                {
                    FormsM_details nuevodetalle = new FormsM_details();
                    nuevodetalle = item;
                    nuevodetalle.original = false;
                    nuevodetalle.ID_visit = nuevaActividad.ID_activity; //TOMAREMOS ID VISIT COMO ID ACTIVITY PORQUE ES POR REPRESENTANTE Y NO POR VISITA

                    if (nuevodetalle.ID_formresourcetype == 13) {
                        if (ID_brand != null || ID_brand !="0") {
                            nuevodetalle.fdescription = brand;
                            nuevodetalle.fvalueText = ID_brand;
                        }

        
                    }
                    if (nuevodetalle.ID_formresourcetype == 30)
                    {
                        if ( ID_cat!= null || ID_cat !="0"){
                            nuevodetalle.fdescription = cat;
                            nuevodetalle.fvalueText = ID_cat;
                        }
                 
                    }
                    if (nuevodetalle.ID_formresourcetype == 31)
                    {
                        if (ID_subcat != null || ID_subcat !="0") {
                            nuevodetalle.fdescription = subcat;
                            nuevodetalle.fvalueText = ID_subcat;
                        }
                      
                    }

                    dbcmk.FormsM_details.Add(nuevodetalle);
                    
                }
                dbcmk.BulkSaveChanges();

                //Por ultimo asignamos el usuario a la visita
                //Pero verificamos si ya existe

                var existeenvisita = (from v in dbcmk.VisitsM_representatives where (v.ID_visit == nuevaActividad.ID_visit && v.ID_usuario == IDRep) select v).Count();


                if (existeenvisita > 0)

                {
                }
                else
                {

                    if (esDemoUser == 1)

                    {
                        VisitsM_representatives repvisita = new VisitsM_representatives();

                        repvisita.ID_visit = nuevaActividad.ID_visit;
                        repvisita.ID_usuario = 0;
                        repvisita.query1 = nuevaActividad.ID_usuarioEndString;
                        repvisita.ID_empresa = nuevaActividad.ID_empresa;
                        dbcmk.VisitsM_representatives.Add(repvisita);
                        dbcmk.SaveChanges();
                    }
                    else
                    {
                        VisitsM_representatives repvisita = new VisitsM_representatives();

                        repvisita.ID_visit = nuevaActividad.ID_visit;
                        repvisita.ID_usuario = IDRep;
                        repvisita.query1 = "3";
                        repvisita.ID_empresa = nuevaActividad.ID_empresa;
                        dbcmk.VisitsM_representatives.Add(repvisita);
                        dbcmk.SaveChanges();

                    }

                }

                ////enviamos correo SI ES UN USUARIO DEMO
                //if (esDemoUser == 1)
                //{
                //    //Obtenemos el nombre de las marcas o brands por cada articulo
                //    var listadeItems = (from d in dbcmk.FormsM_details where (d.ID_visit == nuevaActividad.ID_activity && d.ID_formresourcetype == 3) select d).ToList();


                //    foreach (var itemd in listadeItems)
                //    {
                //        itemd.fdescription = (from k in CMKdbcmk.OITM join j in CMKdbcmk.OMRC on k.FirmCode equals j.FirmCode where (k.ItemCode == itemd.fsource) select j.FirmName).FirstOrDefault();
                //    }

                //    var brands = listadeItems.GroupBy(test => test.fdescription).Select(grp => grp.First()).ToList();

                //    var brandstoshow = "";
                //    int count = 0;
                //    foreach (var items in brands)
                //    {
                //        if (count == 0)
                //        {
                //            try
                //            {
                //                brandstoshow = items.fdescription.ToString();
                //            }
                //            catch
                //            {
                //                brandstoshow = "no data from db";
                //            }

                //        }
                //        else
                //        {
                //            try
                //            {
                //                brandstoshow += ", " + items.fdescription.ToString();
                //            }
                //            catch
                //            {
                //                brandstoshow = "no data from db";
                //            }
                //        }
                //        count += 1;
                //    }
                //    //*******************************
                //    try
                //    {
                //        var usuario = (from a in CMKdbcmk.OCRD where (a.CardCode == nuevaActividad.ID_usuarioEndString) select a).FirstOrDefault();

                //        var store = (from s in dbcmk.VisitsM where (s.ID_visit == nuevaActividad.ID_visit) select s).FirstOrDefault();

                //        //Enviamos correo para notificar
                //        dynamic email = new Email("newdemo_alert");
                //        email.To = usuario.E_Mail.ToString();
                //        email.From = "customercare@comerciamarketing.com";
                //        email.Vendor = brandstoshow;
                //        email.Date = Convert.ToDateTime(nuevaActividad.date).ToLongDateString();
                //        email.Time = Convert.ToDateTime(nuevaActividad.date).ToLongTimeString();
                //        email.Place = store.store + ", " + store.address;
                //        //email.link = "https://comerciamarketing.com/Home/Internal" + demos.ID_demo + Server.HtmlDecode("&") + "id_form=" + demos.ID_form;
                //        email.link = "https://comerciamarketing.com/Home/Internal";
                //        email.accesscode = "ACCMK00" + nuevaActividad.ID_activity.ToString();
                //        email.enddate = Convert.ToDateTime(nuevaActividad.date).AddDays(1).ToLongDateString();
                //        email.Send();

                //        //FIN email
                //    }
                //    catch
                //    {

                //    }
                //}

                //************
                //var result = "Activity created successfully.";
                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = "Error: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }




        }
        public ActionResult DeleteActivity(int ID_activity)
        {
            try
            {

                ActivitiesM activity = dbcmk.ActivitiesM.Find(ID_activity);
                dbcmk.ActivitiesM.Remove(activity);
                dbcmk.SaveChanges();

                //Eliminamos el detalle que genero la actividad en FormsM_details
                var lista_eliminar = (from c in dbcmk.FormsM_details where (c.ID_visit == ID_activity && c.original == false) select c).ToList();

                dbcmk.BulkDelete(lista_eliminar);

                //foreach (var item in lista_eliminar)
                //{
                //    FormsM_details detailstodelete = dbcmk.FormsM_details.Find(item.ID_details);
                //    dbcmk.FormsM_details.Remove(detailstodelete);
                //    dbcmk.SaveChanges();

                //}
                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = "Error: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }
        //FORMULARIOS Y DETALLES DE FORMULARIOS (Se utiliza en Activities)
        public class tablahijospadre
        {
            public int ID_details { get; set; }
            public int id_resource { get; set; }
            public string fsource { get; set; }
            public string fdescription { get; set; }
            public int fvalue { get; set; }
            public decimal fvalueDecimal { get; set; }
            public string fvalueText { get; set; }
            public int ID_formM { get; set; }
            public int ID_visit { get; set; }
            public bool original { get; set; }
            public int obj_order { get; set; }
            public int obj_group { get; set; }
            public int idkey { get; set; }
            public int parent { get; set; }
            public string query1 { get; set; }
            public string query2 { get; set; }
            public int ID_empresa { get; set; }

        }
        public class MyObj_tablapadre
        {
            public int ID_details { get; set; }
            public int id_resource { get; set; }
            public string fsource { get; set; }
            public string fdescription { get; set; }
            public int fvalue { get; set; }
            public decimal fvalueDecimal { get; set; }
            public string fvalueText { get; set; }
            public int ID_formM { get; set; }
            public int ID_visit { get; set; }
            public bool original { get; set; }
            public int obj_order { get; set; }
            public int obj_group { get; set; }
            public int idkey { get; set; }
            public int parent { get; set; }
            public string query1 { get; set; }
            public string query2 { get; set; }
            public int ID_empresa { get; set; }
            public List<MyObj_tablapadre> children { get; set; }
        }

        //CLASE PARA ALMACENAR OBJETOS
        public class MyObj
        {
            public string id_resource { get; set; }
            public string fsource { get; set; }
            public string fdescription { get; set; }
            public string fvalue { get; set; }
            public int idkey { get; set; }
            public int parent { get; set; }
            public IList<MyObj> children { get; set; }
        }

        public static List<MyObj_tablapadre> ObtenerCategoriarJerarquiaByID(List<MyObj_tablapadre> Categoriaspadre, List<tablahijospadre> Categoriashijas)
        {


            List<MyObj_tablapadre> query = (from item in Categoriaspadre

                                            select new MyObj_tablapadre
                                            {
                                                ID_details = item.ID_details,
                                                id_resource = item.id_resource,
                                                fsource = item.fsource,
                                                fdescription = item.fdescription,
                                                fvalue = item.fvalue,
                                                fvalueDecimal = item.fvalueDecimal,
                                                fvalueText = item.fvalueText,
                                                ID_formM = item.ID_formM,
                                                ID_visit = item.ID_visit,
                                                original = item.original,
                                                obj_order = item.obj_order,
                                                obj_group = item.obj_group,
                                                idkey = item.idkey,
                                                parent = item.parent,
                                                query1 = item.query1,
                                                query2 = item.query2,
                                                ID_empresa = item.ID_empresa,
                                                children = ObtenerHijosByID(item.idkey, Categoriashijas)

                                            }).ToList();

            return query;





        }

        private static List<MyObj_tablapadre> ObtenerHijosByID(int ID_parent, List<tablahijospadre> categoriashijas)
        {



            List<MyObj_tablapadre> query = (from item in categoriashijas

                                            where item.parent == ID_parent
                                            select new MyObj_tablapadre
                                            {
                                                ID_details = item.ID_details,
                                                id_resource = item.id_resource,
                                                fsource = item.fsource,
                                                fdescription = item.fdescription,
                                                fvalue = item.fvalue,
                                                fvalueDecimal = item.fvalueDecimal,
                                                fvalueText = item.fvalueText,
                                                ID_formM = item.ID_formM,
                                                ID_visit = item.ID_visit,
                                                original = item.original,
                                                obj_order = item.obj_order,
                                                obj_group = item.obj_group,
                                                idkey = item.idkey,
                                                parent = item.parent,
                                                query1 = item.query1,
                                                query2 = item.query2,
                                                ID_empresa = item.ID_empresa,
                                                children = ObtenerHijosByID(item.idkey, categoriashijas)
                                            }).ToList();

            return query;

        }
        public ActionResult Activityon_preview(int? id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                var activity = (from v in dbcmk.ActivitiesM where (v.ID_activity == id) select v).FirstOrDefault();

                FormsM formsM = dbcmk.FormsM.Find(activity.ID_form);

                //LISTADO DE CLIENTES
                //VERIFICAMOS SI SELECCIONARON CLIENTE PREDEFINIDO

                if (activity.Customer != "")
                {
                    var customers = (from b in dlipro.OCRD where (b.CardCode == activity.ID_customer) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                }
                else
                {
                    //if ((datosUsuario.ID_tipomembresia == 8 && datosUsuario.ID_rol == 8) || datosUsuario.ID_tipomembresia == 1)//Administrador
                    //{
                    //    var customers = (from b in COM_MKdbcmk.OCRD where (b.Series == 61 && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();
                    //    ViewBag.customers = customers.ToList();
                    //}
                    //else
                    //{

                    var customers = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                    //}

                }

                var FormsMDet = (from a in dbcmk.FormsM_details where (a.ID_visit == activity.ID_activity && a.original == false) select a).ToList();

                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in FormsMDet
                                                                where (item.parent == 0)
                                                                select
                                                                   new MyObj_tablapadre
                                                                   {
                                                                       ID_details = item.ID_details,
                                                                       id_resource = item.ID_formresourcetype,
                                                                       fsource = item.fsource,
                                                                       fdescription = item.fdescription,
                                                                       fvalue = item.fvalue,
                                                                       fvalueDecimal = item.fvalueDecimal,
                                                                       fvalueText = item.fvalueText,
                                                                       ID_formM = item.ID_formM,
                                                                       ID_visit = item.ID_visit,
                                                                       original = item.original,
                                                                       obj_order = item.obj_order,
                                                                       obj_group = item.obj_group,
                                                                       idkey = item.idkey,
                                                                       parent = item.parent,
                                                                       query1 = item.query1,
                                                                       query2 = item.query2,
                                                                       ID_empresa = item.ID_empresa
                                                                   }
                                      ).OrderBy(a => a.obj_order).ToList();

                List<tablahijospadre> listahijasActivities = (from item in FormsMDet
                                                              select new tablahijospadre
                                                              {
                                                                  ID_details = item.ID_details,
                                                                  id_resource = item.ID_formresourcetype,
                                                                  fsource = item.fsource,
                                                                  fdescription = item.fdescription,
                                                                  fvalue = item.fvalue,
                                                                  fvalueDecimal = item.fvalueDecimal,
                                                                  fvalueText = item.fvalueText,
                                                                  ID_formM = item.ID_formM,
                                                                  ID_visit = item.ID_visit,
                                                                  original = item.original,
                                                                  obj_order = item.obj_order,
                                                                  obj_group = item.obj_group,
                                                                  idkey = item.idkey,
                                                                  parent = item.parent,
                                                                  query1 = item.query1,
                                                                  query2 = item.query2,
                                                                  ID_empresa = item.ID_empresa

                                                              }).OrderBy(a => a.obj_order).ToList();


                List<MyObj_tablapadre> categoriasListActivities = ObtenerCategoriarJerarquiaByID(listapadresActivities, listahijasActivities);

                ///
                var showbuttondynamic = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_activity && item.ID_formresourcetype == 11)
                                         select item).Count();

                if (showbuttondynamic > 0)
                {
                    ViewBag.dinamicos = 1;
                    var existproducts = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_activity && item.ID_formresourcetype == 3)
                                         select item).Count();
                    if (existproducts > 0)
                    {
                        ViewBag.mostrarboton = 0; //Lo ocultamos
                    }
                    else
                    {
                        ViewBag.mostrarboton = 1; //Lo mostramos
                    }

                }
                else
                {
                    ViewBag.mostrarboton = 0;
                    ViewBag.dinamicos = 0;
                }
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.idvisitareal = activity.ID_visit;
                ViewBag.idvisita = activity.ID_activity;
                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = FormsMDet;

                Session["detailsForm"] = FormsMDet;

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });
            }


        }
        public ActionResult Activityon(int? id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                var activity = (from v in dbcmk.ActivitiesM where (v.ID_activity == id) select v).FirstOrDefault();

                FormsM formsM = dbcmk.FormsM.Find(activity.ID_form);

                //LISTADO DE CLIENTES
                //VERIFICAMOS SI SELECCIONARON CLIENTE PREDEFINIDO

                if (activity.Customer != "")
                {
                    var customers = (from b in dlipro.OCRD where (b.CardCode == activity.ID_customer) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                }
                else
                {
                    //if ((datosUsuario.ID_tipomembresia == 8 && datosUsuario.ID_rol == 8) || datosUsuario.ID_tipomembresia == 1)//Administrador
                    //{
                    //    var customers = (from b in COM_MKdbcmk.OCRD where (b.Series == 61 && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();
                    //    ViewBag.customers = customers.ToList();
                    //}
                    //else
                    //{

                    var customers = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                    //}

                }

                var FormsMDet = (from a in dbcmk.FormsM_details where (a.ID_visit == activity.ID_activity && a.original == false) select a).ToList();

                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in FormsMDet
                                                                where (item.parent == 0)
                                                                select
                                                                   new MyObj_tablapadre
                                                                   {
                                                                       ID_details = item.ID_details,
                                                                       id_resource = item.ID_formresourcetype,
                                                                       fsource = item.fsource,
                                                                       fdescription = item.fdescription,
                                                                       fvalue = item.fvalue,
                                                                       fvalueDecimal = item.fvalueDecimal,
                                                                       fvalueText = item.fvalueText,
                                                                       ID_formM = item.ID_formM,
                                                                       ID_visit = item.ID_visit,
                                                                       original = item.original,
                                                                       obj_order = item.obj_order,
                                                                       obj_group = item.obj_group,
                                                                       idkey = item.idkey,
                                                                       parent = item.parent,
                                                                       query1 = item.query1,
                                                                       query2 = item.query2,
                                                                       ID_empresa = item.ID_empresa
                                                                   }
                                      ).OrderBy(a => a.obj_order).ToList();

                List<tablahijospadre> listahijasActivities = (from item in FormsMDet
                                                              select new tablahijospadre
                                                              {
                                                                  ID_details = item.ID_details,
                                                                  id_resource = item.ID_formresourcetype,
                                                                  fsource = item.fsource,
                                                                  fdescription = item.fdescription,
                                                                  fvalue = item.fvalue,
                                                                  fvalueDecimal = item.fvalueDecimal,
                                                                  fvalueText = item.fvalueText,
                                                                  ID_formM = item.ID_formM,
                                                                  ID_visit = item.ID_visit,
                                                                  original = item.original,
                                                                  obj_order = item.obj_order,
                                                                  obj_group = item.obj_group,
                                                                  idkey = item.idkey,
                                                                  parent = item.parent,
                                                                  query1 = item.query1,
                                                                  query2 = item.query2,
                                                                  ID_empresa = item.ID_empresa

                                                              }).OrderBy(a => a.obj_order).ToList();


                List<MyObj_tablapadre> categoriasListActivities = ObtenerCategoriarJerarquiaByID(listapadresActivities, listahijasActivities);

                ///
                var showbuttondynamic = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_activity && item.ID_formresourcetype == 11)
                                         select item).Count();

                if (showbuttondynamic > 0)
                {
                    ViewBag.dinamicos = 1;
                    var existproducts = (from item in FormsMDet
                                             where (item.ID_visit == activity.ID_activity && item.ID_formresourcetype == 3)
                                             select item).Count();
                    if (existproducts > 0)
                    {
                        ViewBag.mostrarboton = 0; //Lo ocultamos
                    }
                    else {
                        ViewBag.mostrarboton = 1; //Lo mostramos
                    }
               
                }
                else
                {
                    ViewBag.mostrarboton = 0;
                    ViewBag.dinamicos = 0;
                }
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.idvisitareal = activity.ID_visit;
                ViewBag.idvisita = activity.ID_activity;
                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = FormsMDet;

                Session["detailsForm"] = FormsMDet;

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });
            }


        }

        public ActionResult SurveyForm(int? id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                var activity = (from v in dbcmk.Tasks where (v.ID_task == id) select v).FirstOrDefault();

                FormsM formsM = dbcmk.FormsM.Find(activity.ID_formM);

                //LISTADO DE CLIENTES
                //VERIFICAMOS SI SELECCIONARON CLIENTE PREDEFINIDO

                if (activity.Customer != "")
                {
                    var customers = (from b in dlipro.OCRD where (b.CardCode == activity.ID_Customer) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                }
                else
                {
                    var customers = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
            

                }

                var FormsMDet = (from a in dbcmk.FormsM_detailsTasks where (a.ID_visit == activity.ID_task && a.original == false) select a).ToList();

                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in FormsMDet
                                                                where (item.parent == 0)
                                                                select
                                                                   new MyObj_tablapadre
                                                                   {
                                                                       ID_details = item.ID_details,
                                                                       id_resource = item.ID_formresourcetype,
                                                                       fsource = item.fsource,
                                                                       fdescription = item.fdescription,
                                                                       fvalue = item.fvalue,
                                                                       fvalueDecimal = item.fvalueDecimal,
                                                                       fvalueText = item.fvalueText,
                                                                       ID_formM = item.ID_formM,
                                                                       ID_visit = item.ID_visit,
                                                                       original = item.original,
                                                                       obj_order = item.obj_order,
                                                                       obj_group = item.obj_group,
                                                                       idkey = item.idkey,
                                                                       parent = item.parent,
                                                                       query1 = item.query1,
                                                                       query2 = item.query2,
                                                                       ID_empresa = item.ID_empresa
                                                                   }
                                      ).OrderBy(a => a.obj_order).ToList();

                List<tablahijospadre> listahijasActivities = (from item in FormsMDet
                                                              select new tablahijospadre
                                                              {
                                                                  ID_details = item.ID_details,
                                                                  id_resource = item.ID_formresourcetype,
                                                                  fsource = item.fsource,
                                                                  fdescription = item.fdescription,
                                                                  fvalue = item.fvalue,
                                                                  fvalueDecimal = item.fvalueDecimal,
                                                                  fvalueText = item.fvalueText,
                                                                  ID_formM = item.ID_formM,
                                                                  ID_visit = item.ID_visit,
                                                                  original = item.original,
                                                                  obj_order = item.obj_order,
                                                                  obj_group = item.obj_group,
                                                                  idkey = item.idkey,
                                                                  parent = item.parent,
                                                                  query1 = item.query1,
                                                                  query2 = item.query2,
                                                                  ID_empresa = item.ID_empresa

                                                              }).OrderBy(a => a.obj_order).ToList();


                List<MyObj_tablapadre> categoriasListActivities = ObtenerCategoriarJerarquiaByID(listapadresActivities, listahijasActivities);

                ///
                var showbuttondynamic = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_task && item.ID_formresourcetype == 11)
                                         select item).Count();

                if (showbuttondynamic > 0)
                {
                    ViewBag.dinamicos = 1;
                    var existproducts = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_task && item.ID_formresourcetype == 3)
                                         select item).Count();
                    if (existproducts > 0)
                    {
                        ViewBag.mostrarboton = 0; //Lo ocultamos
                    }
                    else
                    {
                        ViewBag.mostrarboton = 1; //Lo mostramos
                    }

                }
                else
                {
                    ViewBag.mostrarboton = 0;
                    ViewBag.dinamicos = 0;
                }
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.task = activity.ID_task;
                ViewBag.idvisita = activity.ID_task;
                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = FormsMDet;

                Session["detailsForm"] = FormsMDet;

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });
            }


        }


        public ActionResult SurveyFormC(int? id)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                var activity = (from v in dbcmk.Tasks where (v.ID_task == id) select v).FirstOrDefault();

                FormsM formsM = dbcmk.FormsM.Find(activity.ID_formM);

                //LISTADO DE CLIENTES
                //VERIFICAMOS SI SELECCIONARON CLIENTE PREDEFINIDO

                if (activity.Customer != "")
                {
                    var customers = (from b in dlipro.OCRD where (b.CardCode == activity.ID_Customer) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();
                }
                else
                {
                    var customers = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select b).OrderBy(b => b.CardName).ToList();
                    ViewBag.customers = customers.ToList();


                }

                var FormsMDet = (from a in dbcmk.FormsM_detailsTasks where (a.ID_visit == activity.ID_task && a.original == false) select a).ToList();

                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in FormsMDet
                                                                where (item.parent == 0)
                                                                select
                                                                   new MyObj_tablapadre
                                                                   {
                                                                       ID_details = item.ID_details,
                                                                       id_resource = item.ID_formresourcetype,
                                                                       fsource = item.fsource,
                                                                       fdescription = item.fdescription,
                                                                       fvalue = item.fvalue,
                                                                       fvalueDecimal = item.fvalueDecimal,
                                                                       fvalueText = item.fvalueText,
                                                                       ID_formM = item.ID_formM,
                                                                       ID_visit = item.ID_visit,
                                                                       original = item.original,
                                                                       obj_order = item.obj_order,
                                                                       obj_group = item.obj_group,
                                                                       idkey = item.idkey,
                                                                       parent = item.parent,
                                                                       query1 = item.query1,
                                                                       query2 = item.query2,
                                                                       ID_empresa = item.ID_empresa
                                                                   }
                                      ).OrderBy(a => a.obj_order).ToList();

                List<tablahijospadre> listahijasActivities = (from item in FormsMDet
                                                              select new tablahijospadre
                                                              {
                                                                  ID_details = item.ID_details,
                                                                  id_resource = item.ID_formresourcetype,
                                                                  fsource = item.fsource,
                                                                  fdescription = item.fdescription,
                                                                  fvalue = item.fvalue,
                                                                  fvalueDecimal = item.fvalueDecimal,
                                                                  fvalueText = item.fvalueText,
                                                                  ID_formM = item.ID_formM,
                                                                  ID_visit = item.ID_visit,
                                                                  original = item.original,
                                                                  obj_order = item.obj_order,
                                                                  obj_group = item.obj_group,
                                                                  idkey = item.idkey,
                                                                  parent = item.parent,
                                                                  query1 = item.query1,
                                                                  query2 = item.query2,
                                                                  ID_empresa = item.ID_empresa

                                                              }).OrderBy(a => a.obj_order).ToList();


                List<MyObj_tablapadre> categoriasListActivities = ObtenerCategoriarJerarquiaByID(listapadresActivities, listahijasActivities);

                ///
                var showbuttondynamic = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_task && item.ID_formresourcetype == 11)
                                         select item).Count();

                if (showbuttondynamic > 0)
                {
                    ViewBag.dinamicos = 1;
                    var existproducts = (from item in FormsMDet
                                         where (item.ID_visit == activity.ID_task && item.ID_formresourcetype == 3)
                                         select item).Count();
                    if (existproducts > 0)
                    {
                        ViewBag.mostrarboton = 0; //Lo ocultamos
                    }
                    else
                    {
                        ViewBag.mostrarboton = 1; //Lo mostramos
                    }

                }
                else
                {
                    ViewBag.mostrarboton = 0;
                    ViewBag.dinamicos = 0;
                }
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.task = activity.ID_task;
                ViewBag.idvisita = activity.ID_task;
                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = FormsMDet;

                Session["detailsForm"] = FormsMDet;

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });
            }


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

        public class MyObj_formtemplateMarketingReport
        {
            public string id { get; set; }
            public string text { get; set; }
            public string value { get; set; }
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
        public ActionResult DSD(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "DSD";
                ViewData["Page"] = "New Order";
                ViewBag.menunameid = "dsd_menu";
                ViewBag.submenunameid = "dsdneword_submenu";
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
                List<MyObj_lstStoresInputSel> lstCustomers = (from b in dlipro.BI_Dim_Customer where (b.SalesRep == "dsd") select new MyObj_lstStoresInputSel { id = b.id_Customer, text = b.Customer }).ToList();
                //List<MyObj_lstStoresInputSel> lstCustomers = (from b in dlipro.BI_Dim_Customer where (b.SalesRep == "VICTOR CAICEDO") select new MyObj_lstStoresInputSel { id = b.id_Customer, text = b.Customer }).ToList();
                ViewBag.lstCustomers = JsonConvert.SerializeObject(lstCustomers.ToArray());

                //var lstProducts2 = (from c in dlipro.BI_Dim_Products where (c.DSD == "Y") select c).ToList();
                //var lstProducts = (from c in dlipro.BI_Dim_Products 
                //                   join lp in dlipro.ITM1 on c.id equals lp.ItemCode
                //                   where (c.DSD == "Y" && lp.PriceList==151) //151 es la correcta //148
                //                   select new MyObj_DSD { id=c.id, Product=c.Product, price=lp.Price, srp=c.SRP,upc=c.CodeBars, category=c.category_name, subcategory=c.subcategory_name }).ToList();

                var lstProducts = (from c in dlipro.Dsd_Products
                                       //where (c.DSD == "Y" && c.PriceList == 148) //151 es la correcta //148
                                   select new MyObj_DSD { id = c.id, Product = c.Product, price = c.Price, srp = c.SRP, upc = c.CodeBars, category = c.category_name, subcategory = c.subcategory_name }).ToList();

                ViewBag.lstProducts = lstProducts;

                var lstReturnReasons = (from d in dlipro.UFD1 where (d.TableID == "RDR1" && d.FieldID == 0) select new { id = d.FldValue, text = d.Descr }).ToArray();
                ViewBag.lstReturnsR = JsonConvert.SerializeObject(lstReturnReasons);


                var lstFormasPago = (from e in dlipro.UFD1 where (e.TableID == "ORDR" && e.FieldID == 26) select new { id = e.FldValue, text = e.Descr }).ToArray();
                ViewBag.lstFormasPago = JsonConvert.SerializeObject(lstFormasPago);


                var lstCategories = (from f in dlipro.BI_Dim_Products where (f.DSD == "Y") select f.category_name).Distinct().ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in dlipro.BI_Dim_Products where (f.DSD == "Y") select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name, category = f.category_name }).Distinct().ToList();
                ViewBag.lstSubCategories = lstSubCategories;

                var lstOrders = (from o in dblim.Tb_OrdersDSD where (o.ID_Company == activeuser.ID_Company && o.Date >= filtrostartdate && o.Date <= filtroenddate) select o).ToList();

                if (lstOrders != null)
                {
                    foreach (var item in lstOrders)
                    {
                        var sumtotal = (from se in dblim.Tb_OrdersDetailsDSD where (se.ID_OrderDSD == item.ID_OrderDSD) select se.Total).Sum();
                        item.Comment = sumtotal.ToString();
                    }
                }

                ViewBag.lstOrders = lstOrders;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        [HttpPost]
        public JsonResult addSalesOrderDSD(string ID_customer, string Customer, string ID_payment, string Payment, string Doc_numP, string Doc_numCompany, string Comment, string Sign, List<MyObj_formtemplate> objectsProducts)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;

            if (Doc_numCompany == null) { Doc_numCompany = ""; }
            if (Doc_numP == null) { Doc_numP = ""; }
            if (Comment == null) { Comment = ""; }
            if (Sign == null) { Sign = ""; }


            Tb_OrdersDSD newOrderDSD = new Tb_OrdersDSD();
            newOrderDSD.ID_customer = ID_customer;
            newOrderDSD.CustomerName = Customer;
            newOrderDSD.ID_payment = ID_payment;
            newOrderDSD.Payment = Payment;
            newOrderDSD.Doc_numP = Doc_numP;
            newOrderDSD.Doc_numCompany = Doc_numCompany;
            newOrderDSD.Date = DateTime.UtcNow;
            newOrderDSD.ID_User = activeuser.ID_User;
            newOrderDSD.User_name = activeuser.Name + " " + activeuser.Lastname;
            newOrderDSD.ID_Company = activeuser.ID_Company;
            newOrderDSD.Sign = Sign;
            newOrderDSD.Comment = Comment;
            newOrderDSD.docNum_SAP = "";
            newOrderDSD.canceled = false;



            dblim.Tb_OrdersDSD.Add(newOrderDSD);
            dblim.SaveChanges();

            //Guardamos el detalle;

            foreach (var item in objectsProducts)
            {

                Tb_OrdersDetailsDSD newDetail = new Tb_OrdersDetailsDSD();

                if (item.returnReasonName == null) { item.returnReasonName = ""; }

                newDetail.ID_OrderDSD = newOrderDSD.ID_OrderDSD;
                newDetail.ID_Product = item.id;

                newDetail.Product_Name = item.text;
                newDetail.Units = Convert.ToInt32(item.units);

                newDetail.ReturnReasonID = item.returnReasonID;

                newDetail.ReturnReasonName = item.returnReasonName;

                newDetail.Price = Convert.ToDecimal(item.Price);

                newDetail.SRP = Convert.ToDecimal(item.SRP);
                if (item.UPC == null) { item.UPC = ""; }
                newDetail.UPC = item.UPC;
                newDetail.Total = Convert.ToDecimal(item.total);
                if (newDetail.Units > 0)
                {
                    newDetail.GroupDsd = "SALES";
                }
                else
                {
                    newDetail.GroupDsd = "CREDITS";
                }



                dblim.Tb_OrdersDetailsDSD.Add(newDetail);
                dblim.SaveChanges();


            }



            var result = "Success";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult editSalesOrderDSD(int orderID, string ID_customer, string Customer, string ID_payment, string Payment, string Doc_numP, string Doc_numCompany, string Comment, string Sign, List<MyObj_formtemplate> objectsProducts)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;

            if (Doc_numCompany == null) { Doc_numCompany = ""; }
            if (Doc_numP == null) { Doc_numP = ""; }
            if (Comment == null) { Comment = ""; }
            if (Sign == null) { Sign = ""; }


            Tb_PreOrdersDSD newOrderDSD = new Tb_PreOrdersDSD();

            newOrderDSD = (from a in dblim.Tb_PreOrdersDSD where (a.ID_OrderDSD == orderID) select a).FirstOrDefault();


            newOrderDSD.ID_customer = ID_customer;
            newOrderDSD.CustomerName = Customer;
            newOrderDSD.ID_payment = ID_payment;
            newOrderDSD.Payment = Payment;
            newOrderDSD.Doc_numP = Doc_numP;
            newOrderDSD.Doc_numCompany = Doc_numCompany;
            newOrderDSD.Date = DateTime.UtcNow;
            newOrderDSD.ID_User = activeuser.ID_User;
            newOrderDSD.User_name = activeuser.Name + " " + activeuser.Lastname;
            newOrderDSD.ID_Company = activeuser.ID_Company;
            newOrderDSD.Sign = Sign;
            newOrderDSD.Comment = Comment;
            newOrderDSD.docNum_SAP = "";



            dblim.Entry(newOrderDSD).State = EntityState.Modified;
            dblim.SaveChanges();

            //eliminamos detalle anterior

            var lstdel = (from c in dblim.Tb_PreOrdersDetailsDSD where (c.ID_OrderDSD == orderID) select c).ToList();

            foreach (var itemdel in lstdel)
            {
                dblim.Tb_PreOrdersDetailsDSD.Remove(itemdel);
                dblim.SaveChanges();
            }


            //Guardamos el detalle nuevo;


            if (objectsProducts != null)
            {
                foreach (var item in objectsProducts)
                {

                    Tb_PreOrdersDetailsDSD newDetail = new Tb_PreOrdersDetailsDSD();

                    if (item.returnReasonName == null) { item.returnReasonName = ""; }

                    newDetail.ID_OrderDSD = newOrderDSD.ID_OrderDSD;
                    newDetail.ID_Product = item.id;

                    newDetail.Product_Name = item.text;
                    newDetail.Units = Convert.ToInt32(item.units);

                    newDetail.ReturnReasonID = item.returnReasonID;

                    newDetail.ReturnReasonName = item.returnReasonName;

                    newDetail.Price = Convert.ToDecimal(item.Price);

                    newDetail.SRP = Convert.ToDecimal(item.SRP);
                    if (item.UPC == null) { item.UPC = ""; }
                    newDetail.UPC = item.UPC;
                    newDetail.Total = Convert.ToDecimal(item.total);
                    if (newDetail.Units > 0)
                    {
                        newDetail.GroupDsd = "SALES";
                    }
                    else
                    {
                        newDetail.GroupDsd = "CREDITS";
                    }



                    dblim.Tb_PreOrdersDetailsDSD.Add(newDetail);
                    dblim.SaveChanges();


                }

            }


            var result = "Success";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoutev2(string descriptionN, DateTime date, DateTime enddate, string listatiendas, string listarepresentantes, string idform, string listatiposactividades, string cust)
        {
            try
            {
                listatiendas=listatiendas.Substring(1);
               
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                List<int> repIds = listarepresentantes.Split(',').Select(int.Parse).ToList();
                //Comenzamos con el maestro de rutas

               

                RoutesM rutamaestra = new RoutesM();

                rutamaestra.date = date;
                rutamaestra.end_date = enddate;
                rutamaestra.query1 = "";
                rutamaestra.query2 = descriptionN;
                rutamaestra.query3 = cust;

                rutamaestra.ID_empresa = 11; //Codigo limena en comercia

                dbcmk.RoutesM.Add(rutamaestra);
                dbcmk.SaveChanges();
                //FIN ruta maestra



                //Guardamos detalle de visita
                //Se guarda el detalle por cada tienda a visitar
                List<string> storeIds = listatiendas.Split(',').Distinct().ToList();

                foreach (var store in storeIds)
                {
                    var storeSAP = (from s in dlipro.OCRD where (s.CardCode == store) select s).FirstOrDefault();
                    if (storeSAP != null)
                    {
                        VisitsM visita = new VisitsM();
                        visita.ID_customer = "";
                        visita.customer = "";
                        visita.ID_store = store;
                        visita.store = storeSAP.CardName;
                        visita.address = storeSAP.MailAddres;
                        visita.city = storeSAP.MailCity;
                        if (storeSAP.MailZipCod == null)
                        {
                            visita.zipcode = "";
                        }
                        else { visita.zipcode = storeSAP.MailZipCod; }

                        if (storeSAP.State2 == null)
                        {
                            visita.state = "";
                        }
                        else { visita.state = storeSAP.State2; }
                        visita.visit_date = date;
                        visita.ID_visitstate = 3; //On Hold
                        visita.comments = "";
                        visita.check_in = date;
                        visita.check_out = date;
                        visita.end_date = enddate;
                        visita.extra_hours = 0;
                        visita.ID_route = rutamaestra.ID_route;
                        visita.ID_empresa = 11;
                        //GEOLOCALIZACION
                        try
                        {
                            string address = storeSAP.CardName.ToString() + ", " + storeSAP.MailAddres.ToString() + ", " + storeSAP.MailCity.ToString() + ", " + storeSAP.MailZipCod.ToString();
                            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key=AIzaSyC3zDvE8enJJUHLSmhFAdWhPRy_tNSdQ6g&address={0}&sensor=false", Uri.EscapeDataString(address));

                            WebRequest request = WebRequest.Create(requestUri);
                            WebResponse response = request.GetResponse();
                            XDocument xdoc = XDocument.Load(response.GetResponseStream());

                            XElement result = xdoc.Element("GeocodeResponse").Element("result");
                            XElement locationElement = result.Element("geometry").Element("location");
                            XElement lat = locationElement.Element("lat");
                            XElement lng = locationElement.Element("lng");
                            //NO SE PORQUE LO TIRA AL REVEZ
                            visita.geoLat = lng.Value;
                            visita.geoLong = lat.Value;
                            //FIN

                        }
                        catch
                        {
                            visita.geoLong = "";
                            visita.geoLat = "";
                        }

                        dbcmk.VisitsM.Add(visita);
                        dbcmk.SaveChanges();




                        foreach (var rep in repIds)
                        {

                            if (rep != 0)
                            {
                                VisitsM_representatives repvisita = new VisitsM_representatives();

                                repvisita.ID_visit = visita.ID_visit;
                                repvisita.ID_usuario = rep;
                                repvisita.query1 = "3";
                                repvisita.ID_empresa = visita.ID_empresa;
                                dbcmk.VisitsM_representatives.Add(repvisita);
                                dbcmk.SaveChanges();
                            }

                        }


                    }

                    //FIN detalle visita




                }
                //FIn detalle de representantes

                //Evaluamos si hay que repetir

                if (cust != "NA")
                {
                    if (cust == "FW") ///First week of every month
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;
                        var remainingMonths = date.Month + 1;


                        for (int mth = remainingMonths; mth <= 12; mth++)
                        {
                            DateTime dt = new DateTime(getyear, mth, 1);
                            while (dt.DayOfWeek != getday)
                            {
                                dt = dt.AddDays(1);
                            }

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();

                                foreach (var rep in repIds)
                                {

                                    if (rep != 0)
                                    {
                                        VisitsM_representatives repvisita = new VisitsM_representatives();

                                        repvisita.ID_visit = newvisit.ID_visit;
                                        repvisita.ID_usuario = rep;
                                        repvisita.query1 = "3";
                                        repvisita.ID_empresa = newvisit.ID_empresa;
                                        dbcmk.VisitsM_representatives.Add(repvisita);
                                        dbcmk.SaveChanges();
                                    }

                                }



                            }


                        }


                    }
                    if (cust == "OW") //Once a week
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;


                        int daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
                        int daysLeftInYear = daysInYear - date.DayOfYear; // Result is in range 0-365

                        for (int wk = 7; wk <= daysLeftInYear; wk += 7)
                        {
                            DateTime dt = date;

                            dt = dt.AddDays(wk);

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();

                                foreach (var rep in repIds)
                                {

                                    if (rep != 0)
                                    {
                                        VisitsM_representatives repvisita = new VisitsM_representatives();

                                        repvisita.ID_visit = newvisit.ID_visit;
                                        repvisita.ID_usuario = rep;
                                        repvisita.query1 = "3";
                                        repvisita.ID_empresa = newvisit.ID_empresa;
                                        dbcmk.VisitsM_representatives.Add(repvisita);
                                        dbcmk.SaveChanges();
                                    }

                                }
                            }
                        }
                    }
                    if (cust == "OTW") //Once every two weeks
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;


                        int daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
                        int daysLeftInYear = daysInYear - date.DayOfYear; // Result is in range 0-365

                        for (int wk = 14; wk <= daysLeftInYear; wk += 14)
                        {
                            DateTime dt = date;

                            dt = dt.AddDays(wk);

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();


                                foreach (var rep in repIds)
                                {

                                    if (rep != 0)
                                    {
                                        VisitsM_representatives repvisita = new VisitsM_representatives();

                                        repvisita.ID_visit = newvisit.ID_visit;
                                        repvisita.ID_usuario = rep;
                                        repvisita.query1 = "3";
                                        repvisita.ID_empresa = newvisit.ID_empresa;
                                        dbcmk.VisitsM_representatives.Add(repvisita);
                                        dbcmk.SaveChanges();
                                    }

                                }
                            }
                        }
                    }

                }




                //TempData["exito"] = "Route created successfully.";
                return RedirectToAction("Routes", "Commercial", null);

            }
            catch (Exception ex)
            {
                //TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Routes", "Commercial", null);
            }
        }
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoute(string ID_routeD)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                int id = Convert.ToInt32(ID_routeD);

                //Eliminamos las visitas
                var visitas = (from a in dbcmk.VisitsM where (a.ID_empresa == 11 && a.ID_route == id) select a).ToList();
                var visitasArray = (from b in dbcmk.VisitsM where (b.ID_empresa == 11 && b.ID_route == id) select b.ID_visit).ToArray();

                foreach (var visit in visitas)
                {
                    VisitsM visita = dbcmk.VisitsM.Find(visit.ID_visit);
                    if (visita != null)
                    {
                        dbcmk.VisitsM.Remove(visita);
                        dbcmk.SaveChanges();
                    }
                }

                //Eliminamos las actividades
                var actividades = (from e in dbcmk.ActivitiesM where (e.ID_empresa == 11 && visitasArray.Contains(e.ID_visit)) select e).ToList();
                var actividadesArray = (from f in dbcmk.ActivitiesM where (f.ID_empresa == 11 && visitasArray.Contains(f.ID_visit)) select f.ID_activity).ToArray();


                foreach (var act in actividades)
                {
                    ActivitiesM actividad = dbcmk.ActivitiesM.Find(act.ID_activity);
                    if (actividad != null)
                    {
                        dbcmk.ActivitiesM.Remove(actividad);
                        dbcmk.SaveChanges();
                    }
                }
                //Eliminamos los detalles de formulario

                var detalles = (from h in dbcmk.FormsM_details where (actividadesArray.Contains(h.ID_visit)) select h).ToList();
                //var detallesArray = (from k in dbcmk.FormsM_details where (actividadesArray.Contains(k.ID_visit)) select k.ID_details).ToArray();
                foreach (var det in detalles)
                {
                    FormsM_details detalle = dbcmk.FormsM_details.Find(det.ID_details);
                    if (detalle != null)
                    {
                        dbcmk.FormsM_details.Remove(detalle);
                        dbcmk.SaveChanges();
                    }
                }

                //Eliminamos la ruta
                RoutesM ruta = dbcmk.RoutesM.Find(id);
                dbcmk.RoutesM.Remove(ruta);
                dbcmk.SaveChanges();

                //TempData["exito"] = "Route deleted successfully.";
                return RedirectToAction("Routes", "Commercial", null);
            }
            catch
            {
                //TempData["advertencia"] = "Something wrong happened, try again.";
                return RedirectToAction("Routes", "Commercial", null);
            }



        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRoutev2(string ID_routeDedt, string custedt)
        {
            try
            {
                int idroute = Convert.ToInt32(ID_routeDedt);
                RoutesM rutamaestra = (from a in dbcmk.RoutesM where (a.ID_route == idroute) select a).FirstOrDefault();
                //Comenzamos con el maestro de rutas
                rutamaestra.query3 = custedt;



                dbcmk.Entry(rutamaestra).State = EntityState.Modified;
                dbcmk.SaveChanges();
                //FIN ruta maestra
                DateTime date = rutamaestra.date;

                //FIn detalle de representantes

                //Evaluamos si hay que repetir

                if (custedt != "NA")
                {
                    var anexas = (from an in dbcmk.RoutesM where (an.query1 == ID_routeDedt) select an).ToList();

                    if (anexas.Count() > 0)
                    {


                        foreach (var item in anexas)
                        {

                            var visitas = (from a in dbcmk.VisitsM where (a.ID_empresa == 11 && a.ID_route == item.ID_route) select a).ToList();
                            var visitasArray = (from b in dbcmk.VisitsM where (b.ID_empresa == 11 && b.ID_route == item.ID_route) select b.ID_visit).ToArray();

                            foreach (var visit in visitas)
                            {
                                VisitsM visita = dbcmk.VisitsM.Find(visit.ID_visit);
                                if (visita != null)
                                {
                                    dbcmk.VisitsM.Remove(visita);
                                    dbcmk.SaveChanges();
                                }
                            }

                            RoutesM ruta = dbcmk.RoutesM.Find(item.ID_route);
                            dbcmk.RoutesM.Remove(ruta);
                            dbcmk.SaveChanges();
                        }
                    }


                    if (custedt == "FW") ///First week of every month
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;
                        var remainingMonths = date.Month + 1;


                        for (int mth = remainingMonths; mth <= 12; mth++)
                        {
                            DateTime dt = new DateTime(getyear, mth, 1);
                            while (dt.DayOfWeek != getday)
                            {
                                dt = dt.AddDays(1);
                            }

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();

                            }


                        }


                    }
                    if (custedt == "OW") //Once a week
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;


                        int daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
                        int daysLeftInYear = daysInYear - date.DayOfYear; // Result is in range 0-365

                        for (int wk = 7; wk <= daysLeftInYear; wk += 7)
                        {
                            DateTime dt = date;

                            dt = dt.AddDays(wk);

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();

                            }
                        }
                    }
                    if (custedt == "OTW") //Once every two weeks
                    {
                        var getyear = date.Year;
                        var getday = date.DayOfWeek;


                        int daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
                        int daysLeftInYear = daysInYear - date.DayOfYear; // Result is in range 0-365

                        for (int wk = 14; wk <= daysLeftInYear; wk += 14)
                        {
                            DateTime dt = date;

                            dt = dt.AddDays(wk);

                            RoutesM rutaRepetir = new RoutesM();

                            rutaRepetir.date = rutamaestra.date;
                            rutaRepetir.query1 = rutamaestra.ID_route.ToString();
                            rutaRepetir.query3 = "";
                            rutaRepetir.query2 = rutamaestra.query2;
                            rutaRepetir.end_date = rutamaestra.end_date;
                            rutaRepetir.ID_empresa = rutamaestra.ID_empresa;




                            if (rutaRepetir.date == rutaRepetir.end_date)
                            {
                                rutaRepetir.date = dt;
                                rutaRepetir.end_date = dt;
                            }
                            else
                            { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                                var d2 = (rutaRepetir.end_date - rutaRepetir.date).TotalDays;
                                rutaRepetir.date = dt;

                                rutaRepetir.end_date = dt.AddDays(d2);
                            }


                            dbcmk.RoutesM.Add(rutaRepetir);
                            dbcmk.SaveChanges();

                            //Guardamos visitas
                            foreach (var visitas in rutamaestra.VisitsM)
                            {
                                VisitsM newvisit = new VisitsM();

                                newvisit.ID_customer = visitas.ID_customer;
                                newvisit.customer = visitas.customer;
                                newvisit.ID_store = visitas.ID_store;
                                newvisit.store = visitas.store;
                                newvisit.address = visitas.address;
                                newvisit.city = visitas.city;
                                newvisit.zipcode = visitas.zipcode;
                                newvisit.state = visitas.state;
                                newvisit.ID_visitstate = visitas.ID_visitstate;
                                newvisit.comments = visitas.comments;
                                newvisit.geoLong = visitas.geoLong;
                                newvisit.geoLat = visitas.geoLat;
                                newvisit.extra_hours = visitas.extra_hours;
                                newvisit.ID_route = rutaRepetir.ID_route;
                                newvisit.ID_empresa = visitas.ID_empresa;

                                newvisit.visit_date = dt;
                                newvisit.end_date = dt;
                                newvisit.check_in = dt;
                                newvisit.check_out = dt;

                                dbcmk.VisitsM.Add(newvisit);
                                dbcmk.SaveChanges();

                            }
                        }
                    }

                }
                else
                {//Eliminamos rutas
                    var anexas = (from an in dbcmk.RoutesM where (an.query1 == ID_routeDedt) select an).ToList();

                    if (anexas.Count() > 0)
                    {


                        foreach (var item in anexas)
                        {

                            var visitas = (from a in dbcmk.VisitsM where (a.ID_empresa == 11 && a.ID_route == item.ID_route) select a).ToList();
                            var visitasArray = (from b in dbcmk.VisitsM where (b.ID_empresa == 11 && b.ID_route == item.ID_route) select b.ID_visit).ToArray();

                            foreach (var visit in visitas)
                            {
                                VisitsM visita = dbcmk.VisitsM.Find(visit.ID_visit);
                                if (visita != null)
                                {
                                    dbcmk.VisitsM.Remove(visita);
                                    dbcmk.SaveChanges();
                                }
                            }

                            RoutesM ruta = dbcmk.RoutesM.Find(item.ID_route);
                            dbcmk.RoutesM.Remove(ruta);
                            dbcmk.SaveChanges();
                        }
                    }

                }




                //TempData["exito"] = "Route updated successfully.";
                return RedirectToAction("Routes", "Commercial", null);

            }
            catch (Exception ex)
            {
                //TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Routes", "Commercial", null);
            }



        }
        [HttpPost]
        public ActionResult ChangeDates(string route, string nuevafecha)
        {
            string result2 = "";
            int idroute = Convert.ToInt32(route);
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            try
            {
                var ndate = Convert.ToDateTime(nuevafecha);
                RoutesM ruta = (from a in dbcmk.RoutesM where (a.ID_route == idroute) select a).FirstOrDefault();

                if (ruta != null)
                {
                    if (ruta.date == ruta.end_date)
                    {
                        ruta.date = ndate;
                        ruta.end_date = ndate;
                    }
                    else
                    { //Como la ruta no tiene la misma fecha de finalizacion, debemos calcular cuando terminaria

                        var d2 = (ruta.end_date - ruta.date).TotalDays;
                        ruta.date = ndate;

                        ruta.end_date = ndate.AddDays(d2);
                    }

                    //verificamos si tiene anexas
                    var anexas = (from an in dbcmk.RoutesM where (an.query1 == route) select an).Count();

                    if (anexas > 0)
                    {
                        result2 = "You can't edit this route.";
                        var result = new { result = result2 };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    //
                    var visitas = (from v in dbcmk.VisitsM where (v.ID_route == idroute) select v).ToList();
                    int flg = 0;

                    foreach (var item in visitas)
                    {
                        if (item.ID_visitstate != 3)
                        {
                            flg = 1;
                        }
                    }

                    if (flg == 1) //Ya hay visitas en progreso, canceladas o finalizadas, por lo tanto no se puede modificar
                    {



                        result2 = "You can't edit this route. Please check the visits state.";
                        var result = new { result = result2 };
                        return Json(result, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        //Modificamos las visitas
                        foreach (var item in visitas)
                        {
                            if (item.visit_date == item.end_date)
                            {
                                item.visit_date = ndate;
                                item.check_in = ndate;
                                item.end_date = ndate;
                            }
                            else
                            {

                                var d3 = (item.end_date - item.visit_date).TotalDays;
                                item.visit_date = ndate;
                                item.check_in = ndate;
                                item.end_date = ndate.AddDays(d3);
                            }

                            dbcmk.Entry(item).State = EntityState.Modified;
                            dbcmk.SaveChanges();
                        }

                        //
                        if (ruta.query1 != "")
                        {
                            ruta.query1 = "";
                        }
                        dbcmk.Entry(ruta).State = EntityState.Modified;
                        dbcmk.SaveChanges();

                        result2 = "success";
                        var result = new { result = result2 };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }



                }
                else
                {
                    result2 = "Error: no data was found.";
                    var result = new { result = result2 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
           }
            catch (Exception ex)
            {
                result2 = "Error: " + ex.Message;
                var result = new { result = result2 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetEvents(DateTime startf, DateTime endf)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                List<RoutesM> lstRoutes = new List<RoutesM>();
            
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
         
                    lstRoutes = dbcmk.RoutesM.Where(dc => dc.date >= startf && dc.end_date <= endf && dc.ID_empresa == 11).OrderByDescending(dc => dc.date).ToList();
                }
                else
                {
                    
                    lstRoutes = dbcmk.RoutesM.Where(dc => dc.date >= startf && dc.end_date <= endf && dc.ID_empresa == 11).OrderByDescending(dc => dc.date).ToList();

                    var arryruts = lstRoutes.Select(a => a.ID_route).ToArray();

                    var lstvisit = (from b in dbcmk.VisitsM where (arryruts.Contains(b.ID_route)) select b).ToList();

                    var arrvisit = lstvisit.Select(a => a.ID_visit).Distinct().ToArray();
                    var exist = (from a in dbcmk.VisitsM_representatives where (arrvisit.Contains(a.ID_visit) && a.ID_usuario == activeuser.ID_User) select a.ID_visit).ToArray();

                    arrvisit = (from b in arrvisit where (exist.Contains(b)) select b).ToArray();

                    lstvisit = (from a in lstvisit where (arrvisit.Contains(a.ID_visit)) select a).ToList();
                    arrvisit = lstvisit.Select(a => a.ID_route).Distinct().ToArray();

                    arryruts = (from c in arryruts where (arrvisit.Contains(c)) select c).ToArray();

                    lstRoutes = (from c in lstRoutes where (arryruts.Contains(c.ID_route)) select c).ToList();
                }

    

                List<Routes_calendar> rutaslst = new List<Routes_calendar>();

                foreach (var item in lstRoutes)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.ID_route + " - " + item.query2;
                    rt.url = "";
                    rt.start = item.date.ToString("yyyy-MM-dd");
                    rt.end = item.end_date.AddDays(1).ToString("yyyy-MM-dd");
                    //rt.color = "#.fc-event";//"#2081d6";
                    rt.className = ".fc-event";
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

        public class customerlist {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class brandlist {
            public string name { get; set; }
            public string id_customer { get; set; }
        }

        public ActionResult ActivitiesReport(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Activities Report";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "rep2_submenu";
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
                //var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representatives")).ToArray().AsEnumerable();

                //var activities = (from a in dbcmk.ActivitiesM where (a.date >= filtrostartdate && a.date <= filtroenddate && a.ID_empresa ==11 && a.ID_form==32) select a);
                var lstActivities = (from a in dbcmk.ActivitiesM
                                     join b in dbcmk.VisitsM on a.ID_visit equals b.ID_visit
                            

                                     where (a.date >= filtrostartdate && a.date <= filtroenddate && a.ID_empresa == 11 && a.ID_form == 32 && a.isfinished==true)
                                     //where (a.ID_customer == id && (a.date >= filtrostartdate && a.date <= filtroenddate))
                                     select new ActivitiesReport
                                     {
                                         ID_Activity = a.ID_activity,
                                         Description = a.description,
                                         Brand = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 13 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                        Customer = a.Customer,
                                        ID_Customer = a.ID_customer,
                                        ID_Store = b.ID_store,
                                        Store = b.store,
                                        Address = b.address,
                                        City = b.city,
                                        state = b.state,
                                        date=a.date,
                                        ID_SalesRep =0,
                                        SalesRep="",
                                         Category = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 30 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                         SubCategories = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 31 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault()

                                     });

                var categories = (from a in lstActivities select new customerlist { id = a.ID_Customer, name = a.Customer }).Distinct().ToList();
                var brandslst = (from a in lstActivities select new brandlist { name = a.Brand, id_customer=a.ID_Customer }).Distinct().ToList();
                ViewBag.customerlst = categories;
                ViewBag.brandlst = brandslst;

                return View(lstActivities);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }


        public ActionResult Surveys(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Surveys List";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu";
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
                var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c=>c.Name).ToList();
                List<RepsSurveys> repsSurveys = new List<RepsSurveys>();
                List<RepsSurveys> supervisors = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Supervisor")).Select(c=> new RepsSurveys {  idSAP=c.IDSAP, ID_User=c.ID_User, idSAPsupervisor="", Lastname=c.Lastname, Name=c.Name, prop01="", prop02=""}).ToList();
                var surveys = new List<SurveysTasks>();
                string[] usids;
                string[] notinlist;
                DateTime dtvalue = new DateTime(2019, 1, 1);
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    isAdmin = 1;
               
                    List<int> TagIds = new List<int>();

                    if (activeuser.Roles.Contains("Sales Supervisor"))
                    {
                        TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList();

                        repsSurveys = users.Where(d => TagIds.Contains(d.ID_User)).Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP=d.IDSAP, idSAPsupervisor=d.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();

                        surveys = (from a in dbcmk.Tasks
                                   where (TagIds.Contains(a.ID_userEnd) && a.ID_formM==45)
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
                    }
                    else {
                        repsSurveys = users.Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP = d.IDSAP, idSAPsupervisor = d.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();

                        surveys = (from a in dbcmk.Tasks
                                   where(a.ID_formM == 45) 
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
                    }



                
                }
                else
                {

                    repsSurveys = users.Where(a=>a.ID_User==activeuser.ID_User).Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP = d.IDSAP, idSAPsupervisor = d.prop01 }).ToList();
                    usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                    surveys = (from a in dbcmk.Tasks
                               where (a.ID_userEnd == activeuser.ID_User && a.ID_formM == 45)
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
                    ViewBag.customers = lstCustomer;


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



                    isAdmin = 0;

                    ViewBag.iduser = activeuser.ID_User;
                    ViewBag.user = activeuser.Name + " " + activeuser.Lastname;


                }


                //Calculos
                //ESTADISTICA DE SALES ORDERS POR ESTADO DE DETALLES
               
                foreach (var user in repsSurveys)
                {
                    decimal totalclientes = surveys.Where(d=>d.ID_userEnd==user.ID_User).Count();
                    decimal totalclientes2 = surveys.Where(d=>d.ID_userEndSAP.ToString()==user.idSAP).Count();

                    totalclientes = (totalclientes + totalclientes2);
                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceled = (from e in surveys where (e.ID_userEnd == user.ID_User && e.ID_taskstatus==4) select e).Count();

                    if (totalclientes != 0)
                    {
                        if (finishedorCanceled != 0)
                        {

                         user.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();

                        }

                        else
                        {

                            user.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        user.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";

                    }
                    else
                    {
                        user.prop01 = "0";
                        user.prop02  = "(0/ 0)";
                    }
                }


                //ESTADISTICAS POR SUPERVISOR
                foreach (var sup in supervisors) {
                    decimal totalfinalsup = 0;
                    decimal totalfinishedorcanceledsup = 0;
                    foreach (var user in repsSurveys.Where(c=>c.idSAPsupervisor==sup.idSAP))
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

                        }

                        else
                        {

                            sup.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        sup.prop02 = "(" + totalfinishedorcanceledsup + " / " + totalfinalsup + ")";

                    }
                    else
                    {
                        sup.prop01 = "0";
                        sup.prop02 = "(0/ 0)";
                    }

                }
                ViewBag.supervisors = supervisors;
                //Estadistica global
                decimal totalcustom = surveys.Count();
                var prop01 = "";
                var prop02 = "";

                    //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                    decimal finishedorCanceledcustom = (from e in surveys where (e.ID_taskstatus == 4) select e).Count();

                    if (totalcustom != 0)
                    {
                        if (finishedorCanceledcustom != 0)
                        {

                            prop01 = (((Convert.ToDecimal(finishedorCanceledcustom) / totalcustom) * 100)).ToString();

                        }

                        else
                        {

                            prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        prop02 = "(" + finishedorCanceledcustom + " / " + totalcustom + ")";

                    }
                    else
                    {
                        prop01 = "0";
                        prop02 = "(0/ 0)";
                    }



                ViewBag.prop01 = prop01;
                ViewBag.prop02 = prop02;


                ViewBag.isadmin = isAdmin;
                //ViewBag.lstSupervisors = supervisors;
                List<FormsM> activeForms = new List<FormsM>();
                activeForms = (from at in dbcmk.FormsM where (at.ID_activity == 6) select at).ToList();
                ViewBag.activeForms = activeForms;
                ViewBag.representatives = users;
                ViewBag.repsSurveys = repsSurveys;

                return View(surveys);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult SurveysCbyRep(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Surveys List";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu";
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
                var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();
                List<RepsSurveys> repsSurveys = new List<RepsSurveys>();
                List<RepsSurveys> supervisors = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Supervisor")).Select(c => new RepsSurveys { idSAP = c.IDSAP, ID_User = c.ID_User, idSAPsupervisor = "", Lastname = c.Lastname, Name = c.Name, prop01 = "", prop02 = "" }).ToList();
                var surveys = new List<ContactsCustomersByRep>();
                string[] usids;
                string[] notinlist;
                DateTime dtvalue = new DateTime(2019, 1, 1);
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    isAdmin = 1;

                    List<int> TagIds = new List<int>();

                    if (activeuser.Roles.Contains("Sales Supervisor"))
                    {
                        TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList();

                        repsSurveys = users.Where(d => TagIds.Contains(d.ID_User)).Select(d => new RepsSurveys { ID_User = d.ID_User, Lastname = d.Lastname, Name = d.Name, prop01 = "", prop02 = "", idSAP = d.IDSAP, idSAPsupervisor = d.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();

                        surveys = (from a in dlipro.BI_Dim_Customer
                                   where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C") && a.StatusCustomer == "1")
                                   select new ContactsCustomersByRep
                                   {
                                       CardCode = a.id_Customer,
                                       CardName = a.Customer,
                                       Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                       Status = 1,
                                       IsBuyer = a.IsBuyer,
                                       slpCode = a.id_SalesRep.ToString(),
                                       classification = a.Classification,
                                       statuscustomer = a.StatusCustomer
                                   }).ToList();

                    }
                    else
                    {
                        repsSurveys = users.Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();


                        surveys = (from a in dlipro.BI_Dim_Customer
                                       where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C") && a.StatusCustomer == "1")
                                       select new ContactsCustomersByRep
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           slpCode = a.id_SalesRep.ToString(),
                                           classification = a.Classification,
                                           statuscustomer = a.StatusCustomer

                                       }).ToList();

                    }
                }
                else
                {

                }

                List<ContactsCustomersByRep_cat> classificationlst = new List<ContactsCustomersByRep_cat>();

                List<string> classif = new List<string>() { "DIAMOND", "GOLD", "SILVER", "NO CLASSIFICATION" };
                //Calculos
                //ESTADISTICA DE SALES ORDERS POR ESTADO DE DETALLES

                foreach (var user in repsSurveys)
                {
                    decimal totalclientes = surveys.Where(d => d.slpCode == user.idSAP).Count();
                    decimal finishedorCanceled = 0;


                    finishedorCanceled = (from e in surveys where (e.slpCode == user.idSAP && e.IsBuyer == "Y") select e).Count();
                    

                    if (totalclientes != 0)
                    {
                        if (finishedorCanceled != 0)
                        {

                            user.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();

                        }

                        else
                        {

                            user.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        user.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";

                    }
                    else
                    {
                        user.prop01 = "0";
                        user.prop02 = "(0/ 0)";
                    }


                    ///Clasifications
                    ///
                    foreach (var cla in classif) {
                        ContactsCustomersByRep_cat newcustomercla = new ContactsCustomersByRep_cat();
                        newcustomercla.classification = cla;
                        newcustomercla.iduser = user.ID_User;
                        newcustomercla.slpCode = user.idSAP;
                        newcustomercla.prop01 = "0";
                        newcustomercla.prop02 = "";

                        decimal totalclientes2 = 0;
                        decimal finishedorCanceled2 = 0;

                        if (cla == "NO CLASSIFICATION")
                        {
                            totalclientes2 = surveys.Where(d => d.classification != "DIAMOND" && d.classification != "GOLD" && d.classification != "SILVER" && d.slpCode==user.idSAP).Count();
                            finishedorCanceled2 = (from e in surveys where (e.classification != "DIAMOND" && e.classification != "GOLD" && e.classification != "SILVER" && e.IsBuyer == "Y" && e.slpCode == user.idSAP) select e).Count();
                        }
                        else
                        {
                            totalclientes2 = surveys.Where(d => d.classification == cla && d.slpCode == user.idSAP).Count();
                            finishedorCanceled2 = (from e in surveys where (e.classification == cla && e.IsBuyer == "Y" && e.slpCode == user.idSAP) select e).Count();
                        }
                        


                        if (totalclientes2 != 0)
                        {
                            if (finishedorCanceled2 != 0)
                            {

                                newcustomercla.prop01 = (((Convert.ToDecimal(finishedorCanceled2) / totalclientes2) * 100)).ToString();

                            }

                            else
                            {

                                newcustomercla.prop01 = (Convert.ToDecimal(0)).ToString();
                            }
                            newcustomercla.prop02 = "(" + finishedorCanceled2 + " / " + totalclientes2 + ")";

                        }
                        else
                        {
                            newcustomercla.prop01 = "0";
                            newcustomercla.prop02 = "(0/ 0)";
                        }

                        classificationlst.Add(newcustomercla);
                    }




                }


                //ESTADISTICAS POR SUPERVISOR
                foreach (var sup in supervisors)
                {
                    decimal totalfinalsup = 0;
                    decimal totalfinishedorcanceledsup = 0;
                    foreach (var user in repsSurveys.Where(c => c.idSAPsupervisor == sup.idSAP))
                    {
                        decimal totalclientes = surveys.Where(d => d.slpCode == user.idSAP).Count();
                        totalfinalsup += totalclientes;

                        decimal finishedorCanceled = (from e in surveys where (e.slpCode == user.idSAP && e.IsBuyer == "Y") select e).Count();

                        totalfinishedorcanceledsup += finishedorCanceled;
                    }


                    if (totalfinalsup != 0)
                    {
                        if (totalfinishedorcanceledsup != 0)
                        {

                            sup.prop01 = (((Convert.ToDecimal(totalfinishedorcanceledsup) / totalfinalsup) * 100)).ToString();

                        }

                        else
                        {

                            sup.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        sup.prop02 = "(" + totalfinishedorcanceledsup + " / " + totalfinalsup + ")";

                    }
                    else
                    {
                        sup.prop01 = "0";
                        sup.prop02 = "(0/ 0)";
                    }

                }
                ViewBag.supervisors = supervisors;
                //Estadistica global
                decimal totalcustom = surveys.Count();
                var prop01 = "";
                var prop02 = "";

                //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                decimal finishedorCanceledcustom = (from e in surveys where (e.IsBuyer == "Y") select e).Count();

                if (totalcustom != 0)
                {
                    if (finishedorCanceledcustom != 0)
                    {

                        prop01 = (((Convert.ToDecimal(finishedorCanceledcustom) / totalcustom) * 100)).ToString();

                    }

                    else
                    {

                        prop01 = (Convert.ToDecimal(0)).ToString();
                    }
                    prop02 = "(" + finishedorCanceledcustom + " / " + totalcustom + ")";

                }
                else
                {
                    prop01 = "0";
                    prop02 = "(0/ 0)";
                }



                ViewBag.prop01 = prop01;
                ViewBag.prop02 = prop02;


                ViewBag.isadmin = isAdmin;
                //ViewBag.lstSupervisors = supervisors;
                List<FormsM> activeForms = new List<FormsM>();
                activeForms = (from at in dbcmk.FormsM where (at.ID_activity == 6) select at).ToList();
                ViewBag.activeForms = activeForms;
                ViewBag.representatives = users;
                ViewBag.repsSurveys = repsSurveys;

                return View(classificationlst);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }


        public ActionResult SurveysC(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Pepperi Online Customers List";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "tasks_submenu";
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
                var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();
                List<RepsSurveys> repsSurveys = new List<RepsSurveys>();
                List<RepsSurveys> repsSurveysTOSHOW = new List<RepsSurveys>();
                List<RepsSurveys> supervisors = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Supervisor")).Select(c => new RepsSurveys { idSAP = c.IDSAP, ID_User = c.ID_User, idSAPsupervisor = "", Lastname = c.Lastname, Name = c.Name, prop01 = "", prop02 = "" }).ToList();

                string[] usids;
                string[] notinlist;
                List<ContactsCustomersByRep> lstcustomer = new List<ContactsCustomersByRep>();
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor"))
                {
                    isAdmin = 1;

                    List<int> TagIds = new List<int>();

                    if (activeuser.Roles.Contains("Sales Supervisor"))
                    {
                        TagIds = activeuser.prop02.Split(',').Select(int.Parse).ToList();

                        repsSurveys = users.Where(h => TagIds.Contains(h.ID_User)).Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();


                        lstcustomer = (from a in dlipro.BI_Dim_Customer
                                       where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C") && a.StatusCustomer == "1")
                                       select new ContactsCustomersByRep
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           slpCode = a.id_SalesRep.ToString(),
                                           classification = a.Classification,
                                           statuscustomer = a.StatusCustomer
                                       }).ToList();

                    }
                    else
                    {
                        repsSurveys = users.Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                        usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();


                        lstcustomer = (from a in dlipro.BI_Dim_Customer
                                       where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C") && a.StatusCustomer =="1")
                                       select new ContactsCustomersByRep
                                       {
                                           CardCode = a.id_Customer,
                                           CardName = a.Customer,
                                           Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                           Status = 1,
                                           IsBuyer = a.IsBuyer,
                                           slpCode = a.id_SalesRep.ToString(),
                                           classification = a.Classification,
                                           statuscustomer = a.StatusCustomer

                                       }).ToList();
                }
            }
                else
                {
                    int? idre = Convert.ToInt32(activeuser.IDSAP);
                    repsSurveys = users.Where(a => a.ID_User == activeuser.ID_User).Select(h => new RepsSurveys { ID_User = h.ID_User, Lastname = h.Lastname, Name = h.Name, prop01 = "", prop02 = "", idSAP = h.IDSAP, idSAPsupervisor = h.prop01 }).ToList();
                    usids = repsSurveys.Select(c => c.idSAP.ToString()).ToArray();
                    lstcustomer = (from a in dlipro.BI_Dim_Customer
                                   where (usids.Contains(a.id_SalesRep.ToString()) && a.id_Customer.StartsWith("C") && a.StatusCustomer == "1")
                                   select new ContactsCustomersByRep
                                   {
                                       CardCode = a.id_Customer,
                                       CardName = a.Customer,
                                       Total = (from b in dlipro.BI_Contact_Person where (b.CardCode == a.id_Customer) select b).Count(),
                                       Status = 1,
                                       IsBuyer = a.IsBuyer,
                                       slpCode = a.id_SalesRep.ToString(),
                                       classification=a.Classification,
                                       statuscustomer = a.StatusCustomer
                                   }).ToList();
                }




                //Calculos




                List<RepsCustomersbyProfile> newlstbyprofile = new List<RepsCustomersbyProfile>();
                RepsCustomersbyProfile diamond = new RepsCustomersbyProfile();
                RepsCustomersbyProfile gold = new RepsCustomersbyProfile();
                RepsCustomersbyProfile silver = new RepsCustomersbyProfile();
                RepsCustomersbyProfile noclassification = new RepsCustomersbyProfile();
                diamond.Profile = "DIAMOND";
                diamond.prop01 = "";
                diamond.prop02 = "";
                gold.Profile = "GOLD";
                gold.prop01 = "";
                gold.prop02 = "";
                silver.Profile = "SILVER";
                silver.prop01 = "";
                silver.prop02 = "";
                noclassification.Profile = "NO CLASSIFICATION";
                noclassification.prop01 = "";
                noclassification.prop02 = "";
                newlstbyprofile.Add(diamond);
                newlstbyprofile.Add(gold);
                newlstbyprofile.Add(silver);
                newlstbyprofile.Add(noclassification);

                foreach (var classification in newlstbyprofile) {

                    decimal totalclientes = 0;
                   
                    decimal finishedorCanceled = 0;
                    if (classification.Profile == "NO CLASSIFICATION")
                    {
                        totalclientes = lstcustomer.Where(d => d.classification != "DIAMOND" && d.classification != "GOLD" && d.classification != "SILVER").Count();
                        finishedorCanceled = (from e in lstcustomer where (e.classification != "DIAMOND" && e.classification != "GOLD" && e.classification != "SILVER" && e.IsBuyer == "Y") select e).Count();
                    }
                    else {
                        totalclientes = lstcustomer.Where(d => d.classification == classification.Profile).Count();
                        finishedorCanceled = (from e in lstcustomer where (e.classification == classification.Profile && e.IsBuyer == "Y") select e).Count();
                    }
                

                    if (totalclientes != 0)
                    {
                        if (finishedorCanceled != 0)
                        {

                            classification.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();

                        }

                        else
                        {

                            classification.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        classification.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";

                    }
                    else
                    {
                        classification.prop01 = "0";
                        classification.prop02 = "(0/ 0)";
                    }


                }


                foreach (var classification in newlstbyprofile)
                {

                    //ESTADISTICA DE SALES ORDERS POR ESTADO DE DETALLES

                    foreach (var user in repsSurveys)
                    {
                        RepsSurveys nuevouser = new RepsSurveys();

                        decimal totalclientes = 0;
                        decimal finishedorCanceled = 0;
                        nuevouser.classifications = classification.Profile;
                        nuevouser.idSAP = user.idSAP;
                        nuevouser.idSAPsupervisor = user.idSAPsupervisor;
                        nuevouser.Name = user.Name;
                        nuevouser.Lastname = user.Lastname;
                        nuevouser.ID_User = user.ID_User;
                        nuevouser.prop01 = "";
                        nuevouser.prop02 = "";
                        if (classification.Profile == "NO CLASSIFICATION")
                        {
                            totalclientes = lstcustomer.Where(d => d.slpCode == user.idSAP && d.classification != "DIAMOND" && d.classification != "GOLD" && d.classification != "SILVER").Count();
                            finishedorCanceled = (from e in lstcustomer where (e.slpCode == user.idSAP && e.classification != "DIAMOND" && e.classification != "GOLD" && e.classification != "SILVER" && e.IsBuyer == "Y") select e).Count();
                        }
                        else
                        {
                            totalclientes = lstcustomer.Where(d => d.classification == classification.Profile && d.slpCode == user.idSAP).Count();
                            finishedorCanceled = (from e in lstcustomer where (e.classification == classification.Profile && e.IsBuyer == "Y" && e.slpCode == user.idSAP) select e).Count();
                        }




                        if (totalclientes != 0)
                        {
                            if (finishedorCanceled != 0)
                            {

                                nuevouser.prop01 = (((Convert.ToDecimal(finishedorCanceled) / totalclientes) * 100)).ToString();

                            }

                            else
                            {

                                nuevouser.prop01 = (Convert.ToDecimal(0)).ToString();
                            }
                            nuevouser.prop02 = "(" + finishedorCanceled + " / " + totalclientes + ")";

                        }
                        else
                        {
                            nuevouser.prop01 = "0";
                            nuevouser.prop02 = "(0/ 0)";
                        }

                        repsSurveysTOSHOW.Add(nuevouser);

                    }


                }





                //ESTADISTICAS POR SUPERVISOR
                foreach (var sup in supervisors)
                {
                    decimal totalfinalsup = 0;
                    decimal totalfinishedorcanceledsup = 0;
                    foreach (var user in repsSurveys.Where(c => c.idSAPsupervisor == sup.idSAP))
                    {
                        decimal totalclientes = lstcustomer.Where(d => d.slpCode == user.idSAP).Count();
                   

                        totalfinalsup += totalclientes;
                        //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                        decimal finishedorCanceled = (from e in lstcustomer where (e.slpCode == user.idSAP && e.IsBuyer == "Y") select e).Count();

                        totalfinishedorcanceledsup += finishedorCanceled;
                    }


                    if (totalfinalsup != 0)
                    {
                        if (totalfinishedorcanceledsup != 0)
                        {

                            sup.prop01 = (((Convert.ToDecimal(totalfinishedorcanceledsup) / totalfinalsup) * 100)).ToString();

                        }

                        else
                        {

                            sup.prop01 = (Convert.ToDecimal(0)).ToString();
                        }
                        sup.prop02 = "(" + totalfinishedorcanceledsup + " / " + totalfinalsup + ")";

                    }
                    else
                    {
                        sup.prop01 = "0";
                        sup.prop02 = "(0/ 0)";
                    }

                }
                ViewBag.supervisors = supervisors;
                //Estadistica global
                decimal totalcustom = lstcustomer.Count();
                var prop01 = "";
                var prop02 = "";

                //int finishedorCanceled = (from e in visitas where ((e.ID_visitstate == 4 || e.ID_visitstate==1) && e.ID_route == rutait.ID_route) select e).Count();
                decimal finishedorCanceledcustom = (from e in lstcustomer where (e.IsBuyer=="Y") select e).Count();

                if (totalcustom != 0)
                {
                    if (finishedorCanceledcustom != 0)
                    {

                        prop01 = (((Convert.ToDecimal(finishedorCanceledcustom) / totalcustom) * 100)).ToString();

                    }

                    else
                    {

                        prop01 = (Convert.ToDecimal(0)).ToString();
                    }
                    prop02 = "(" + finishedorCanceledcustom + " / " + totalcustom + ")";

                }
                else
                {
                    prop01 = "0";
                    prop02 = "(0/ 0)";
                }



                ViewBag.prop01 = prop01;
                ViewBag.prop02 = prop02;


                ViewBag.isadmin = isAdmin;

                ViewBag.representatives = users;
                ViewBag.newlstbyprofile = newlstbyprofile;
                //ViewBag.newlstbyprofile = newlstbyprofile;
                return View(repsSurveysTOSHOW);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult GetCustomer_reps(string ID_usuario)
        {
            try
            {
                int IDusuario = Convert.ToInt32(ID_usuario);

                var user = dblim.Sys_Users.Where(a => a.ID_User == IDusuario).FirstOrDefault();

                    var lstCustomer = (from b in dlipro.BI_Dim_Customer where (b.id_SalesRep.ToString()==user.IDSAP) select new { ID = b.id_Customer, Name = b.Customer }).OrderBy(b => b.Name).ToList();

                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    string result = javaScriptSerializer.Serialize(lstCustomer);
                    return Json(result, JsonRequestBehavior.AllowGet);
            

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Print_Activity(int id)
        {
            var total_demos = (from a in dbcmk.ActivitiesM
                                                   join b in dbcmk.VisitsM on a.ID_visit equals b.ID_visit


                                                   where (a.ID_activity ==id)
                                                   //where (a.ID_customer == id && (a.date >= filtrostartdate && a.date <= filtroenddate))
                                                   select new ActivitiesReport
                                                   {
                                                       ID_Activity = a.ID_activity,
                                                       Description = a.description,
                                                       Brand = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 13 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                                       Customer = a.Customer,
                                                       ID_Customer = a.ID_customer,
                                                       ID_Store = b.ID_store,
                                                       Store = b.store,
                                                       Address = b.address,
                                                       City = b.city,
                                                       state = b.state,
                                                       date = a.date,
                                                       ID_SalesRep = 0,
                                                       SalesRep = "",
                                                       Category = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 30 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                                       SubCategories = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 31 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault()

                                                   }).ToList();

            if (total_demos.Count > 0)
            {
                //Recuperamos los IDS de las demos en el dia especifico y del customer especifico
                int[] demo_ids = (from f in dbcmk.ActivitiesM where (f.ID_activity == id) select f.ID_activity).ToArray();

                //Existen datos
                //Buscamos los detalles

                var demo_details_items = (from b in dbcmk.FormsM_details where (demo_ids.Contains(b.ID_visit) && (b.ID_formresourcetype == 5 )) select b).OrderBy(b => b.ID_formresourcetype).ToList();
                var result = demo_details_items;


                if (result.Count > 0)

                {


                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "rptMarketingActivity.rpt"));



                    rd.SetDataSource(total_demos);

                    rd.Subreports[0].SetDataSource(result);
                    // rd.Subreports[1].SetDataSource(total_demos);

                    var filePathOriginal = Server.MapPath("/Reports/pdfReports");

                    Response.Buffer = false;

                    Response.ClearContent();

                    Response.ClearHeaders();


                    //PARA VISUALIZAR
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + "Marketing Activity.pdf; ");



                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

                    stream.Seek(0, SeekOrigin.Begin);



                    return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);

                }
                else
                {
                    TempData["advertencia"] = "No data to send.";
                    return RedirectToAction("ActivitiesReport", "Commercial", null);
                }
            }
            else
            {
                TempData["advertencia"] = "No Demos assigned for today.";
                return RedirectToAction("ActivitiesReport", "Commercial", null);
            }
        }


        public ActionResult Print_Activities(List<MyObj_formtemplateMarketingReport> objects)
        {
            List<int> list = new List<int>();
            foreach (var item in objects)
            {
                var idact = item.id.Substring(4);
                list.Add(Convert.ToInt32(idact));
            }

            var total_demos = (from a in dbcmk.ActivitiesM
                               join b in dbcmk.VisitsM on a.ID_visit equals b.ID_visit


                               where (list.Contains(a.ID_activity))
                               //where (a.ID_customer == id && (a.date >= filtrostartdate && a.date <= filtroenddate))
                               select new ActivitiesReport
                               {
                                   ID_Activity = a.ID_activity,
                                   Description = a.description,
                                   Brand = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 13 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                   Customer = a.Customer,
                                   ID_Customer = a.ID_customer,
                                   ID_Store = b.ID_store,
                                   Store = b.store,
                                   Address = b.address + b.city + b.state,
                                   City = b.city,
                                   state = b.state,
                                   date = a.date,
                                   ID_SalesRep = 0,
                                   SalesRep = "",
                                   Category = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 30 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault(),
                                   SubCategories = (from detalle in dbcmk.FormsM_details where (detalle.ID_formresourcetype == 31 && detalle.ID_visit == a.ID_activity) select detalle.fdescription).FirstOrDefault()

                               }).ToList();

            if (total_demos.Count > 0)
            {
                //Recuperamos los IDS de las demos en el dia especifico y del customer especifico
                int[] demo_ids = (from f in dbcmk.ActivitiesM where (list.Contains(f.ID_activity)) select f.ID_activity).ToArray();

                //Existen datos
                //Buscamos los detalles

                var demo_details_items = (from b in dbcmk.FormsM_details where (demo_ids.Contains(b.ID_visit) && (b.ID_formresourcetype == 5)) select b).OrderBy(b => b.ID_formresourcetype).ToList();

                foreach (var item in demo_details_items) {
                    item.fsource = Path.GetFullPath(Server.MapPath(item.fsource));
                }
                var result = demo_details_items;


                if (result.Count > 0)

                {


                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "rptMarketingActivity.rpt"));



                    rd.SetDataSource(total_demos);

                    rd.Subreports[0].SetDataSource(result);
                    // rd.Subreports[1].SetDataSource(total_demos);

                    var filePathOriginal = Server.MapPath("/Reports/pdf");

                    Response.Buffer = false;

                    Response.ClearContent();

                    Response.ClearHeaders();


                    //PARA VISUALIZAR
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + "Marketing Activity.pdf; ");



                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

                    stream.Seek(0, SeekOrigin.Begin);



                    //return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);

                    try

                    {
                        //limpiamos el directorio

                        System.IO.DirectoryInfo di = new DirectoryInfo(filePathOriginal);

                        foreach (FileInfo file in di.GetFiles())

                        {

                            file.Delete();

                        }

                        foreach (DirectoryInfo dir in di.GetDirectories())

                        {

                            dir.Delete(true);

                        }

                    }

                    catch (Exception e)

                    {

                        var mensaje = e.ToString();

                    }

                    var path2 = "";
                    var filename = "MarketingActivity" + "" + ".pdf";
                    path2 = Path.Combine(filePathOriginal, filename);
                    rd.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path2);

                    var urlcontent = Url.Content("~/Reports/pdf/" + filename + "");
                    //var other = Path.Combine(Server.MapPath("~/Reports"), "rptMarketingActivity.rpt"));
                    return Json(urlcontent);

                }
                else
                {
                    TempData["advertencia"] = "No data to send.";
                    return RedirectToAction("ActivitiesReport", "Commercial", null);
                }
            }
            else
            {
                TempData["advertencia"] = "No Demos assigned for today.";
                return RedirectToAction("ActivitiesReport", "Commercial", null);
            }
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }



        [HttpPost]
        public ActionResult RequestCreditProduct( string ItemCode, string ItemName, int IDRequest_reason, 
            string Request_reason, decimal Quantity, string idcustomer, string customerName, string comment, string returnProduct, string returnProductName, string uomlstName, string UoMCode)
        {

            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    var entero = Convert.ToInt32(Quantity);
                    var usercreate = 0;
                    var usercreatename = "";
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                    if (activeuser != null)
                    {
                        usercreate = activeuser.ID_User;
                        usercreatename = activeuser.Name + " " + activeuser.Lastname;
                    }

                        //Creamos modelo
                        Tb_CreditsRequest newRequest = new Tb_CreditsRequest();
                    newRequest.ItemCode = ItemCode;
                    newRequest.ItemName = ItemName;
                    newRequest.UoMCode = uomlstName;
                    newRequest.UoMEntry = UoMCode;
                    newRequest.IDRequest_reason = IDRequest_reason;
                    newRequest.Request_reason = Request_reason;
                    newRequest.Quantity = entero;
                    newRequest.Validated = false;
                    newRequest.DocNumSAP = "";
                    newRequest.dateCreate = DateTime.UtcNow;
                    newRequest.dateValidate = DateTime.UtcNow;
                    newRequest.userValidate = 0;
                    newRequest.userValidateName = "";
                    newRequest.userCreate = usercreate;
                    newRequest.userCreateName = usercreatename;
                    newRequest.CardCode = idcustomer;
                    newRequest.CardName = customerName;
                    newRequest.estado = 0;
                    newRequest.LineNum = 0;
                    newRequest.URL_image = "";
                    newRequest.URL_image2 = "";
                    newRequest.Error = 0;
                    newRequest.MensajeError = "";
                    if (returnProduct == "0")
                    {
                        newRequest.ItemCodeReturn = "";
                        newRequest.ItemNameReturn = "";
                    }
                    else {
                        newRequest.ItemCodeReturn = returnProduct;
                        newRequest.ItemNameReturn = returnProductName;
                    }


                    if (comment == null) {
                        comment = "";
                    }
                    newRequest.comments = comment;

                    try
                    {
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {
                            //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                            //string filename = Path.GetFileName(Request.Files[i].FileName);  

                            HttpPostedFileBase file = files[i];
                            string fname;

                            // Checking for Internet Explorer  
                            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                            {
                                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                                fname = testfiles[testfiles.Length - 1];
                            }
                            else
                            {
                                fname = file.FileName;
                            }


                            // Adding watermark to the image and saving it into the specified folder!!!!

                            //Image image = Image.FromStream(file.InputStream, true, true);


                            Image TargetImg = Image.FromStream(file.InputStream, true, true);

                            if (i > 0)
                            {

                            }
                            else
                            {
                                //try
                                //{
                                //    int or = Convert.ToInt32(orientation);

                                //    switch (or)
                                //    {
                                //        case 1: // landscape, do nothing
                                //            break;

                                //        case 8: // rotated 90 right
                                //                // de-rotate:
                                //            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                                //            break;

                                //        case 3: // bottoms up
                                //            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                                //            break;

                                //        case 6: // rotated 90 left
                                //            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                                //            break;
                                //    }

                                //}
                                //catch
                                //{

                                //}
                            }


                            DateTime time = DateTime.Now;

                            Image imagenfinal = (Image)TargetImg.Clone();
                            Bitmap bitmapImg = new Bitmap(imagenfinal);// Original Image
                            var path = "";

                            if (i > 0)
                            {

                                path = Path.Combine(Server.MapPath("~/Content/images/creditRequest"), "cr2_" + "_" + ItemCode + "_" + time.Minute + time.Second + ".jpg");
                                newRequest.URL_image2 = "~/Content/images/creditRequest/" + "cr2_" + "_" + ItemCode + "_" + time.Minute + time.Second + ".jpg";
                                Image newimage;
                                //Cambiar tamano no calidad
                                //if (orientation == "-1")
                                //{
                                   // newimage = ScaleImage(bitmapImg, 768, 1360);
                                //}
                                //else
                                //{
                                   newimage = ScaleImage(bitmapImg, 1360, 768);
                                //}

                                newimage.Save(path, ImageFormat.Jpeg);
                                //bitmapImg.Save(path, ImageFormat.Jpeg);
                                bitmapImg.Dispose();
                                newimage.Dispose();
                            }
                            else
                            {


                                path = Path.Combine(Server.MapPath("~/Content/images/creditRequest"), "cr_" + "_" + ItemCode + "_" + time.Minute + time.Second + ".jpg");
                                newRequest.URL_image = "~/Content/images/creditRequest/" + "cr_" + "_" + ItemCode + "_" + time.Minute + time.Second + ".jpg";

                                Image newimage;
                                //Cambiar tamano no calidad
                                //if (orientation == "-1")
                                //{
                                //    newimage = ScaleImage(bitmapImg, 768, 1360);
                                //}
                                //else
                                //{
                                   newimage = ScaleImage(bitmapImg, 1360, 768);
                                //}

                                newimage.Save(path, ImageFormat.Jpeg);
                                //bitmapImg.Save(path, ImageFormat.Jpeg);
                                bitmapImg.Dispose();
                                newimage.Dispose();
                            }




                        }
                    }
                    catch {

                    }


                    dblim.Tb_CreditsRequest.Add(newRequest);
                    dblim.SaveChanges();

                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public class routeCustomer
        {
            public string CardCode { get; set; }
            public string DeliveryRoute { get; set; }
        }

        public ActionResult Credits_authorize(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Credits Authorize";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "ss4_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;


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

                //FIN HEADER

                List<Tb_CreditsRequestWithRoute> lstCredits = new List<Tb_CreditsRequestWithRoute>();
                var salesordersOpen = (from c in dlipro.OpenSalesOrders select new routeCustomer {CardCode=c.CardCode, DeliveryRoute=c.DeliveryRoute }).ToArray();


                //SABER SI ES ADMIN
                int isAdmin = 0;
                if (activeuser.Roles.Contains("Super Admin") || activeuser.Roles.Contains("Sales Supervisor") || activeuser.Roles.Contains("Credits Authorizer"))
                {
                    isAdmin = 1;
                    lstCredits = (from a in dblim.Tb_CreditsRequest where(a.dateCreate >= filtrostartdate && a.dateCreate <= filtroenddate) select new Tb_CreditsRequestWithRoute { ID_creditRequest = a.ID_creditRequest, ItemCode = a.ItemCode, ItemName = a.ItemName, UoMCode = a.UoMCode, Request_reason = a.Request_reason, Quantity = a.Quantity, URL_image = a.URL_image, Validated = a.Validated, DocNumSAP = a.DocNumSAP, IDRequest_reason = a.IDRequest_reason, dateCreate = a.dateCreate, dateValidate = a.dateValidate,
                        userValidate = a.userValidate, userValidateName = a.userValidateName, userCreate = a.userCreate, userCreateName = a.userCreateName, CardCode = a.CardCode, CardName = a.CardName, estado = a.estado,
                        Route = "", comments = a.comments, LineNum=a.LineNum
                    }).ToList();



                    foreach (var item in lstCredits)
                    {
                        item.Route = salesordersOpen.Where(df => df.CardCode == item.CardCode).Select(df => df.DeliveryRoute).FirstOrDefault() == null ? "NA" : salesordersOpen.Where(df => df.CardCode == item.CardCode).Select(df => df.DeliveryRoute).FirstOrDefault();
                    }
                }
                else
                {
                    isAdmin = 0;
                    lstCredits= (from a in dblim.Tb_CreditsRequest where(a.userCreate==activeuser.ID_User && a.dateCreate >= filtrostartdate && a.dateCreate <= filtroenddate)
                                 select new Tb_CreditsRequestWithRoute
                                 {
                                     ID_creditRequest = a.ID_creditRequest,
                                     ItemCode = a.ItemCode,
                                     ItemName = a.ItemName,
                                     ReturnItemCode = a.ItemCodeReturn,
                                     ReturnItemName = a.ItemNameReturn,
                                     UoMCode = a.UoMCode,
                                     UoMEntry = a.UoMEntry,
                                     Request_reason = a.Request_reason,
                                     Quantity = a.Quantity,
                                     URL_image = a.URL_image,
                                     URL_image2 = a.URL_image2,
                                     Validated = a.Validated,
                                     DocNumSAP = a.DocNumSAP,
                                     IDRequest_reason = a.IDRequest_reason,
                                     dateCreate = a.dateCreate,
                                     dateValidate = a.dateValidate,
                                     userValidate = a.userValidate,
                                     userValidateName = a.userValidateName,
                                     userCreate = a.userCreate,
                                     userCreateName = a.userCreateName,
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     estado = a.estado,
                                     Route = "",
                                     comments=a.comments,
                                     LineNum=a.LineNum
                                 }).ToList();

                    foreach (var item in lstCredits)
                    {
                        item.Route = salesordersOpen.Where(df => df.CardCode == item.CardCode).Select(df => df.DeliveryRoute).FirstOrDefault() == null ? "NA" : salesordersOpen.Where(df => df.CardCode == item.CardCode).Select(df => df.DeliveryRoute).FirstOrDefault();
                    }
                }
                ViewBag.isadmin = isAdmin;

                return View(lstCredits);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
        public ActionResult DeleteRequestProduct(int id)
        {
            try
            {

                Tb_CreditsRequest creditReq = dblim.Tb_CreditsRequest.Find(id);
                if (creditReq != null)
                {
                    var urlimg = creditReq.URL_image;
                    dblim.Tb_CreditsRequest.Remove(creditReq);
                    dblim.SaveChanges();

                    try
                    {

                        if (System.IO.File.Exists(Server.MapPath(urlimg)))
                        {
                            try
                            {
                                System.IO.File.Delete(Server.MapPath(urlimg));
                            }
                            catch (System.IO.IOException e)
                            {
                                Console.WriteLine(e.Message);

                            }
                        }
                    }
                    catch {

                    }
                }

                return RedirectToAction("Credits_authorize", "Commercial", null);
            }
            catch
            {

                return RedirectToAction("Credits_authorize", "Commercial",null);
            }



        }

        public ActionResult AuthorizeRequestProduct(int id)
        {
            try
            {

                Tb_CreditsRequest creditReq = dblim.Tb_CreditsRequest.Find(id);
                if (creditReq != null)
                {
                    var usercreate = 0;
                    var usercreatename = "";
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                    if (activeuser != null)
                    {
                        usercreate = activeuser.ID_User;
                        usercreatename = activeuser.Name + " " + activeuser.Lastname;
                    }


                    creditReq.Validated = true;
                    creditReq.estado = 2;
                    creditReq.userValidate = usercreate;
                    creditReq.userValidateName = usercreatename;
                    creditReq.dateValidate = DateTime.UtcNow;
                    dblim.Entry(creditReq).State = EntityState.Modified;
                    dblim.SaveChanges();
                }

                return RedirectToAction("Credits_authorize", "Commercial", null);
            }
            catch
            {

                return RedirectToAction("Credits_authorize", "Commercial", null);
            }



        }

        public ActionResult DisappoveRequestProduct(int id)
        {
            try
            {

                Tb_CreditsRequest creditReq = dblim.Tb_CreditsRequest.Find(id);
                if (creditReq != null)
                {
                    var usercreate = 0;
                    var usercreatename = "";
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                    if (activeuser != null)
                    {
                        usercreate = activeuser.ID_User;
                        usercreatename = activeuser.Name + " " + activeuser.Lastname;
                    }


                    creditReq.Validated = true;
                    creditReq.estado = 1;
                    creditReq.userValidate = usercreate;
                    creditReq.userValidateName = usercreatename;
                    creditReq.dateValidate = DateTime.UtcNow;
                    dblim.Entry(creditReq).State = EntityState.Modified;
                    dblim.SaveChanges();
                }

                return RedirectToAction("Credits_authorize", "Commercial", null);
            }
            catch
            {

                return RedirectToAction("Credits_authorize", "Commercial", null);
            }



        }


    }


}
