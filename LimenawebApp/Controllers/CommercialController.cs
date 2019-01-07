using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
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




            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result2 = javaScriptSerializer.Serialize(lst);
            var result = new { result = result2, porcentaje = porcentaje, sel = rt.query3 };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Routes(string fstartd, string fendd)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Routes";
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

                //filtros de fecha
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var saturday = sunday.AddDays(6).AddHours(23);
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
                //FIN FILTROS*******************

                visitas = dbcmk.VisitsM.Where(dc => dc.visit_date >= filtrostartdate && dc.end_date <= filtroenddate && dc.ID_empresa==11).ToList();
                rutas = dbcmk.RoutesM.Where(dc => dc.date >= filtrostartdate && dc.end_date <= filtroenddate && dc.ID_empresa == 11).OrderByDescending(dc => dc.date).ToList();


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
                var stod = (from b in dlipro.OCRD
                            where (b.Series == 61 && b.CardName != null && b.CardName != "" && b.validFor == "Y") select b);
                ArrayList myArrList2 = new ArrayList();
                myArrList2.AddRange((from b in stod
                             
                              select new 
                              {
                                  id = b.CardCode,
                                  text = b.CardName.Replace("'", "").Replace("\"", "").Replace(@"""", @"\""")
                              }).ToList());
                ViewBag.stores = JsonConvert.SerializeObject(myArrList2);
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
        public ActionResult Marketing_activities(string user, string fstartd, string fendd, string store)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {
                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Commercial";
                ViewData["Page"] = "Marketing activities";
                ViewBag.menunameid = "marketing_menu";
                ViewBag.submenunameid = "markact_submenu";
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
                //List<ActivitiesM> activities = new List<ActivitiesM>();
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

                }
                else
                {

                    ArrayUsers = (from b in dblim.Sys_Users where (b.Active == true && b.Departments == "Sales" && b.Roles == "Sales Representative" && b.ID_Company == activeuser.ID_Company) select new MyObj_lstUsers { id = b.ID_User.ToString(), text = b.Name.ToUpper() + " " + b.Lastname.ToUpper() }).ToArray();
                }

                ViewBag.lstUsers = JsonConvert.SerializeObject(ArrayUsers);

                //
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
                ////
                //if ((user != null || user !="") && (store !=null || store !="")) {
                //    activities = (from a in dbcmk.ActivitiesM where(a.visit_date >= filtrostartdate && a.end_date <= filtroenddate))
                //}

                //CREACION DE ACTIVIDADES//
                var activeForms = (from at in dbcmk.FormsM where (at.ID_empresa == 11) select at).ToList();
                ViewBag.activeForms = activeForms;

                var lstCustomer = (from b in dlipro.OCRD where (b.CardType == "S" && b.CardName != null && b.CardName != "" && b.GroupCode == 102) select new { id = b.CardCode, text = b.CardName }).OrderBy(b => b.text).ToArray();
                ViewBag.lstCustomer = JsonConvert.SerializeObject(lstCustomer);
                ///END

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }
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


        public ActionResult GetVisitData(string UserId, string VisitId)
        {
            try
            {
                if ((UserId != null || UserId != "") && (VisitId != null || VisitId != ""))
                {
                    int IDU = Convert.ToInt32(UserId);
                    int IDV = Convert.ToInt32(VisitId);

                    List<ActivitiesM> lstActivities = (from a in dbcmk.ActivitiesM where (a.ID_visit == IDV && a.ID_usuarioEnd == IDU) select a).ToList();

                    List<VisitsM> selVisit = (from b in dbcmk.VisitsM where (b.ID_visit == IDV) select b).ToList();
                    var areas = dbcmk.VisitsM.Where(x => x.ID_visit == IDV).Select(a => new { ID_Visit = a.ID_visit, storeName = a.store, ID_Store = a.ID_store, storeAddress = (a.address + ", " + a.state + ", " + a.city + ", " + a.zipcode), ID_visitstate = a.ID_visitstate, lng = a.geoLong, lat = a.geoLat });

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

        public ActionResult CreateActivity(string ID_form, string ID_customer, string Customer, string ID_visita, string ID_rep)
        {
            try
            {
                int IDForm = Convert.ToInt32(ID_form);
                int IDRep = 0;

                int esDemoUser = 0;//Variable para identificar si el usuario seleccionado es demo o es un reps

                try
                {
                    IDRep = Convert.ToInt32(ID_rep);
                    esDemoUser = 0;
                }
                catch
                {
                    esDemoUser = 1;
                }


                //CREAMOS LA ESTRUCTURA DE LA ACTIVIDAD
                ActivitiesM nuevaActividad = new ActivitiesM();

                nuevaActividad.ID_form = Convert.ToInt32(ID_form);
                nuevaActividad.ID_visit = Convert.ToInt32(ID_visita);
                nuevaActividad.ID_customer = ID_customer;
                nuevaActividad.Customer = Customer;
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

                    dbcmk.FormsM_details.Add(nuevodetalle);
                    dbcmk.SaveChanges();
                }


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

                foreach (var item in lista_eliminar)
                {
                    FormsM_details detailstodelete = dbcmk.FormsM_details.Find(item.ID_details);
                    dbcmk.FormsM_details.Remove(detailstodelete);
                    dbcmk.SaveChanges();

                }
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



                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in dbcmk.FormsM_details
                                                                where (item.parent == 0 && item.ID_visit == activity.ID_activity && item.original == false)
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

                List<tablahijospadre> listahijasActivities = (from item in dbcmk.FormsM_details
                                                              where (item.ID_visit == activity.ID_activity && item.original == false)
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
                var showbuttondynamic = (from item in dbcmk.FormsM_details
                                         where (item.ID_visit == activity.ID_activity && item.ID_formresourcetype == 3)
                                         select item).Count();

                if (showbuttondynamic > 0)
                {

                    ViewBag.mostrarboton = 1;
                }
                else
                {
                    ViewBag.mostrarboton = 0;
                }
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.idvisitareal = activity.ID_visit;
                ViewBag.idvisita = activity.ID_activity;
                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = (from a in dbcmk.FormsM_details where (a.ID_visit == activity.ID_activity && a.original == false) select a).ToList();

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
                List<string> storeIds = listatiendas.Split(',').ToList();

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

    }
}