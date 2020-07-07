using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.Purchases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers.Purchases
{
    public class OTBController : Controller
    {
        private dbLimenaEntities db = new dbLimenaEntities();
        private Cls_session cls_session = new Cls_session();
        private MatrizComprasEntities dbMatriz = new MatrizComprasEntities();

        public ActionResult OTB(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Purchases";
                ViewData["Page"] = "OTB";
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

                var data = (from a in db.Purchase_data where (a.Date_create >= filtrostartdate && a.Date_create <= filtroenddate && a.query2.Contains("OTB")) select a).ToList();

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult OTB_create()
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Purchases";
                ViewData["Page"] = "SOP";
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



                List<view_MatrizMadre> data = (from b in dbMatriz.view_MatrizMadre select b).ToList();

                return View(data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult new_purchaseData(int id)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Purchases";
                ViewData["Page"] = "SOP";
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

                var existe = (from j in db.Purchase_data_details where (j.ID_purchaseData == id) select j).Count();
                if (existe > 0)
                {
                    return RedirectToAction("edit_purchaseData", "OTB", new { id = id });
                }


                //Seleccionamos los productos de la submatriz
                var products = (from a in db.Purchase_data_details where (a.ID_purchaseData == id) select a).ToList();
                var IDsproducts = (from g in products select g.ProdCodigo).ToArray();
                //Seleccionamos los datos de los productos en la submatriz, generada por el proceso de compras en db_matrizCompras
                List<view_MatrizMadre> data = (from b in dbMatriz.view_MatrizMadre where (IDsproducts.Contains(b.ProdCodigo)) select b).ToList();

                ViewBag.iddata = id;
                ViewBag.data = data;
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult edit_purchaseData(int id)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Purchases";
                ViewData["Page"] = "OTB";
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

                //List<Purchase_data_details> data = (from b in dblim.Purchase_data_details where(b.ID_purchaseData==id) select b).OrderBy(b=>b.num).ToList();
                List<Mdl_Matriz> data = (from b in db.Purchase_data_details
                                          where (b.ID_purchaseData == id)
                                          select new Mdl_Matriz
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
                                              B1 = b.B1,
                                              B2 = b.B2,
                                              B3 = b.B3,
                                              B4 = b.B4,
                                              B5 = b.B5,
                                              InventarioIngresoPO = b.InventarioIngresoPO,

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

                foreach (var item in data)
                {
                    item.transito = (from a in dbMatriz.Transito_Final where (a.ProdCodigo == item.ProdCodigo) select a).Count();
                }

                ViewBag.purchasedataID = id;
                ViewBag.data = data;

                var header = (from c in db.Purchase_data where (c.ID_purchaseData == id) select c).FirstOrDefault();
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
    }
}