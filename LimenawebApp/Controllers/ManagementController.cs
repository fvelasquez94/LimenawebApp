using LimenawebApp.Models;
using Newtonsoft.Json;
using Postal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Client;
using System.Security;

namespace LimenawebApp.Controllers
{
    public class ManagementController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();
        public class repsU {
            public int id_Sales_Rep { get; set; }
            public string Slp_name { get; set; }
            public int issel { get; set; }
        }

        public ActionResult Users()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Users";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "users_submenu";
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

                List<Sys_Users> lstUsers = (from a in dblim.Sys_Users select a).ToList();

                return View(lstUsers);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        public ActionResult SendToSharepoint(int? id)
        {
            try
            {
                var user = (from a in dblim.Tb_NewCustomers where (a.ID_customer == id) select a).FirstOrDefault();
                if (user != null) {
                    user.Validated = true;
                    dblim.Entry(user).State = EntityState.Modified;
                    dblim.SaveChanges();
                    try //Enviamos a lista de Sharepoint
                    {
                        var contrasena = "VaL3nZuEl@2017";
                        ClientContext ClienteCTX = new ClientContext("https://limenainc.sharepoint.com/sites/DatosMaestrosFlujos");
                        var seguridad = new SecureString();

                        foreach (Char item in contrasena) {
                            seguridad.AppendChar(item);
                        }

                        ClienteCTX.Credentials = new SharePointOnlineCredentials("s.valenzuela@limenainc.net", seguridad);
                        Web oWebsite = ClienteCTX.Web;
                        ListCollection CollList = oWebsite.Lists;

                        List oList = CollList.GetByTitle("Customer_Web");
                        ClienteCTX.Load(oList);
                        ClienteCTX.ExecuteQuery();

                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        ListItem oListItem = oList.AddItem(itemCreateInfo);
                        oListItem["Title"] = user.ID_customer.ToString();
                        //Cardname
                        oListItem["_x006a_525"] = user.CardName;
                        //Phone1
                        oListItem["f5nt"] = user.Phone1;
                        //Email
                        oListItem["_x007a_vg9"] = user.E_Mail;
                        //Website
                        oListItem["g0qp"] = user.IntrntSite;
                        //TAXID
                        oListItem["_x006a_357"] = user.TAXID;
                        //TAC CERT NUM
                        oListItem["svcl"] = user.TAXCERTNUM;
                        //FirstName
                        oListItem["jhpd"] = user.FirstName;
                        //Lastname
                        oListItem["_x0076_ai5"] = user.LastName;
                        //Position
                        oListItem["_x0073_qi8"] = user.Position;
                        //Tel1
                        oListItem["tlej"] = user.Tel1;
                        //EmailL
                        oListItem["b3yn"] = user.E_MailL;
                        //Street
                        oListItem["tila"] = user.Street;
                        //City
                        oListItem["geny"] = user.City;
                        //State
                        oListItem["bnwd"] = user.State;
                        //ZipCode
                        oListItem["s0nc"] = user.ZipCode;
                        //Country
                        oListItem["x2nn"] = user.Country;
                        //StoreServices
                        oListItem["ilow"] = user.StoreServices;
                        //Etnias
                        oListItem["_x0066_dt3"] = user.Etnias;
                        //URL Image TAX ID
                        oListItem["hzfz"] = user.url_imageTAXCERT;
                        //URL IMAGE TAX CERT NUM
                        oListItem["iiah"] = user.url_imageTAXCERNUM;
                        //Operation Time
                        oListItem["yvmx"] = user.OperationTime;
                        //Recibo Mercaderia Dia
                        oListItem["odzu"] = user.ReciboMercaderia_dia;
                        //Recibo Mercaderia Area
                        oListItem["tzqf"] = user.ReciboMercaderia_area;
                        //Muelle descarga //Tipo Boolean Evaluamos que cadena enviar
                        if (user.Muelle_descarga == true)
                        {
                            oListItem["_x007a_xh4"] = "YES";
                        }
                        else {
                            oListItem["_x007a_xh4"] = "NO";
                        }
                      
                        //Store Size
                        oListItem["rzmy"] = user.Store_size;
                        //Validated
                        oListItem["yufn"] = "YES";
                        //OnSharepoint
                        oListItem["li8o"] = "YES";
                        ////Modificado
                        //oListItem["Modified"] = DateTime.UtcNow;
                        ////Creado
                        //oListItem["Created"] = DateTime.UtcNow;
                        ////Creador
                        //oListItem["Author"] = "Limena";
                        ////Editor
                        //oListItem["Editor"] = "Limena";

                        oListItem.Update();

                        ClienteCTX.ExecuteQuery();

                        user.OnSharepoint = true;

                        dblim.Entry(user).State = EntityState.Modified;
                        dblim.SaveChanges();

                    }
                    catch(Exception ex) {

                    }

                }

                return RedirectToAction("New_customers", "Management", null);
            }
            catch
            {
                return RedirectToAction("New_customers", "Management", null);
            }


        }

        public ActionResult New_customers()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "New Customers";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "newcustomers_submenu";
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

                List<Tb_NewCustomers> lstCustomers = (from a in dblim.Tb_NewCustomers select a).ToList();

                return View(lstCustomers);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

        }

        //MERCHANDISING
        public ActionResult deleteUser(string id_user)
        {

            try
            {

                var id = Convert.ToInt32(id_user);
                var user = dblim.Sys_Users.Where(c => c.ID_User == id).Select(c => c).FirstOrDefault();

                if (user != null)
                {
                    dblim.Sys_Users.Remove(user);
                    dblim.SaveChanges();

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
        public class userData
        {
            public int ID_User { get; set; }
            public string Name { get; set; }
            public string Lastname { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Telephone { get; set; }
            public string Position { get; set; }
            public bool Active { get; set; }
            public string Departments { get; set; }
            public string Roles { get; set; }
            
        }
        public ActionResult getUser(string id_user)
        {

            try
            {

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                var id = Convert.ToInt32(id_user);
                var user = (from a in dblim.Sys_Users where(a.ID_User == id) select new userData { ID_User=a.ID_User, Name= a.Name, Lastname =a.Lastname, Email=a.Email, Password = a.Password, Telephone=a.Telephone, Position=a.Position,Active=a.Active, Departments=a.Departments, Roles=a.Roles }  ).ToArray();
                var user2 = (from a in dblim.Sys_Users
                             where (a.ID_User == id)
                             select a).FirstOrDefault();

                var result2 = javaScriptSerializer.Serialize(user);
                var lstReps = new List<repsU>();
                lstReps = (from b in dlipro.SalesRep select new repsU { id_Sales_Rep = b.id_SalesRep, Slp_name = b.Slp_Name, issel = 0 }).ToList();

                if (user2.prop01 != null) {
                    if (user2.prop01 == "")
                    {

                    }
                    else {

                        List<int> TagIds = user2.prop01.Split(',').Select(int.Parse).ToList();


                        if (TagIds.Count() > 0)
                        {
                            foreach (var item in lstReps)
                            {

                                if (TagIds.Contains(item.id_Sales_Rep))
                                {
                                    item.issel = 1;
                                }
                            }
                        }
                    }

                }

      
                
                var result = new { result = result2, result2 = javaScriptSerializer.Serialize(lstReps.ToArray()) };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string result = "ERROR: " + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CreateUser(string name, string lastname, string email, string password, string telephone, string position, string departments, string roles)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            Sys_Users User = new Sys_Users();
            try
            {
                

                User.Name = name;
                User.Lastname = lastname;
                User.Email = email;
                User.Password = password;
                User.Telephone = telephone;
                User.Position = position;
                User.Departments = departments;
                User.Roles = roles;
                User.BolsaValor = 0;
                User.PorcentajeBolsa = 0;
                User.IDSAP = "";
                if (User.Position == null)
                {
                    User.Position = "";
                }
                if (User.Departments == null)
                {
                    User.Departments = "";
                }
                if (User.Roles == null)
                {
                    User.Roles = "";
                }

                if (User.Telephone == null)
                {
                    User.Telephone = "";
                }
                if (User.prop01 == null)
                {
                    User.prop01 = "";

                }

                if (User.prop02 == null)
                {
                    User.prop02 = "";

                }
                User.ID_Company = activeuser.ID_Company;
                User.Registration_Date = DateTime.UtcNow;
                User.Active = true;

                dblim.Sys_Users.Add(User);
                dblim.SaveChanges();

                //Send the email
                dynamic semail = new Email("email_confirmation");
                semail.To = User.Email.ToString();
                semail.From = "donotreply@limenainc.net";
                semail.user = User.Name + " " + User.Lastname;
                semail.email = User.Email;
                semail.password = User.Password;

                semail.Send();

                //FIN email


                return Json(new { Result = "Success" });
            }
            catch (Exception ex){          
                return Json(new { Result = "Something wrong happened, try again. " + ex.Message});
            }

        }

        public JsonResult EditUser(string id,string name, string lastname, string email, string password, string telephone, string position, string departments, string roles, string reps)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            int idr = Convert.ToInt32(id);
            Sys_Users User = (from a in dblim.Sys_Users where (a.ID_User == idr) select a).FirstOrDefault();
            try
            {


                User.Name = name;
                User.Lastname = lastname;
                User.Email = email;
                User.Password = password;
                User.Telephone = telephone;
                User.Position = position;
                User.Departments = departments;
                User.Roles = roles;
                User.prop01 = reps;
                if (User.Position == null)
                {
                    User.Position = "";
                }
                if (User.Departments == null)
                {
                    User.Departments = "";
                }
                if (User.Roles == null)
                {
                    User.Roles = "";
                }

                if (User.Telephone == null)
                {
                    User.Telephone = "";
                }

                if (User.prop01 == null)
                {
                    User.prop01 = "";

                }

                if (User.prop02 == null)
                {
                    User.prop02 = "";

                }
                dblim.Entry(User).State = EntityState.Modified;
                dblim.SaveChanges();

                //Send the email
                //dynamic semail = new Email("email_confirmation");
                //semail.To = User.Email.ToString();
                //semail.From = "donotreply@limenainc.net";
                //semail.user = User.Name + " " + User.Lastname;
                //semail.email = User.Email;
                //semail.password = User.Password;

                //semail.Send();

                //FIN email


                return Json(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Something wrong happened, try again. " + ex.Message });
            }

        }

        public ActionResult FormsM()
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Forms";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "forms_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;


                var forms = (from a in dbcmk.FormsM where(a.ID_empresa==11) select a


                             );

                foreach (var item in forms)
                {
                    var tp = (from b in dbcmk.ActivitiesM_types where (b.ID_activity == item.ID_activity) select b).FirstOrDefault();
                    if (tp == null) { item.query1 = ""; }
                    else
                    {
                        item.query1 = tp.description;
                    }
                }


                ViewBag.ID_activity = new SelectList(dbcmk.ActivitiesM_types.Where(x=>x.ID_activity==1), "ID_activity", "description");
                //Seleccionamos los tipos de recursos a utilizar en el caso de Merchandising

                List<string> uids = new List<string>() { "1", "3", "5", "6", "8", "9", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25","28","29","30","31" };

                ViewBag.ID_formresourcetype = new SelectList(dbcmk.form_resource_type.Where(c => uids.Contains(c.ID_formresourcetype.ToString())).OrderBy(c => c.fdescription), "ID_formresourcetype", "fdescription");

                //PARA RECURSOS DE RETAIL AUDIT O COLUMN
                List<string> uidsColumn = new List<string>() { "16", "21", "3" };

                ViewBag.ID_formresourcetypeRetail = new SelectList(dbcmk.form_resource_type.Where(c => uidsColumn.Contains(c.ID_formresourcetype.ToString())).OrderBy(c => c.fdescription), "ID_formresourcetype", "fdescription");

                ViewBag.vendors = (from b in dlipro.OCRD where (b.Series == 61 && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();

                //ViewBag.displaylist = (from d in dbcmk.Items_displays where (d.ID_empresa == GlobalVariables.ID_EMPRESA_USUARIO) select d).ToList();

                ViewBag.formslist = forms.ToList();
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        //CREACION DE JERARQUIAS Y OBJETOS
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



        public ActionResult Template_preview(int? id)
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (activeuser != null)
            {

                //HEADER
                //PAGINAS ACTIVAS
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Forms (Preview)";
                ViewBag.menunameid = "manag_menu";
                ViewBag.submenunameid = "forms_submenu";
                List<string> d = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(d);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                FormsM formsM = dbcmk.FormsM.Find(id);

                //LISTADO DE CLIENTES
                //VERIFICAMOS SI SELECCIONARON CLIENTE PREDEFINIDO


                var customers = (from b in dlipro.OCRD where (b.Series == 61 && b.CardName != null && b.CardName != "") select b).OrderBy(b => b.CardName).ToList();
                ViewBag.customers = customers.ToList();

                //NUEVO
                //ID VISIT SE UTILIZA COMO RELACION
                List<MyObj_tablapadre> listapadresActivities = (from item in dbcmk.FormsM_details
                                                                where (item.ID_formM == id && item.parent == 0 && item.original == true)
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
                                      ).ToList();


                List<tablahijospadre> listahijasActivities = (from item in dbcmk.FormsM_details
                                                              where (item.ID_formM == id && item.original == true)
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

                                                              }).ToList();


                List<MyObj_tablapadre> categoriasListActivities = ObtenerCategoriarJerarquiaByID(listapadresActivities, listahijasActivities);

                ///

                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

                ViewBag.idvisitareal = "0";
                ViewBag.idvisita = "0";

                ViewBag.details = categoriasListActivities;

                ViewBag.detailssql = (from a in dbcmk.FormsM_details where (a.ID_formM == id && a.original == true) select a).ToList();
                return View();
            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
    }
}