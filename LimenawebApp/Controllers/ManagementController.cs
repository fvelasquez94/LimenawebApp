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
using System.Data.SqlClient;
using LimenawebApp.Controllers.Session;

namespace LimenawebApp.Controllers
{
    public class ManagementController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();
        private Cls_session cls_session = new Cls_session();
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

        public ActionResult SendToSharepoint(int? id, string emailresp)
        {
            try
            {
                var user = (from a in dblim.Tb_NewCustomers where (a.ID_customer == id) select a).FirstOrDefault();
                if (user != null) {
                    user.Validated = true;
                    user.status = 1;
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

                        List oList = CollList.GetByTitle("A_Customer_Web");
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
                        //Se utiilizara para enviar imagenes
                        
                        var imgName1 = user.url_imageTAXCERT.Substring(19);
                        var imgName2 = "";
                        try
                        {
                            imgName2 = user.url_imageTAXCERNUM.Substring(19);
                        }
                        catch {
                            imgName2 = "";
                        }
                        


                        oListItem["ilow"] = imgName1;
                        //Etnias
                        //Se utilizara para enviar imagenes
                        oListItem["_x0066_dt3"] = imgName2;
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
                        //if (user.Muelle_descarga == true)
                        //{

                        oListItem["MuelleDeCarga"] = user.Muelle_descarga;
                        //}
                        //else {
                        //    oListItem["MuelleDeCarga"] = "NO";
                        //}

                        //Store Size
                        oListItem["rzmy"] = user.Store_size;
                        //Validated
                        oListItem["Validated1"] = true;
                        //OnSharepoint
                        oListItem["NewColumn3"] = true;
                        ////Modificado
                        //oListItem["Modified"] = DateTime.UtcNow;
                        ////Creado
                        //oListItem["Created"] = DateTime.UtcNow;
                        ////Creador
                        //oListItem["Author"] = "Limena";
                        ////Editor
                        //oListItem["Editor"] = "Limena";


                        //Servicios
                        List<string> servicesIDs = user.StoreServices.Split(',').ToList();
                        if (servicesIDs.Contains("s1")) { oListItem["_x0033_7_GROCERY_DRY_PRODUCT"] = true; } else { oListItem["_x0033_7_GROCERY_DRY_PRODUCT"] = false; }
                        if (servicesIDs.Contains("s2")) { oListItem["_x0033_8_DAIRY_x0020_SECTION"] = true; } else { oListItem["_x0033_8_DAIRY_x0020_SECTION"] = false; }
                        if (servicesIDs.Contains("s3")) { oListItem["_x0033_9_FROZEN_SECTION"] = true; } else { oListItem["_x0033_9_FROZEN_SECTION"] = false; }
                        if (servicesIDs.Contains("s4")) { oListItem["_x0034_0_PRODUCE"] = true; } else { oListItem["_x0034_0_PRODUCE"] = false; }
                        if (servicesIDs.Contains("s5")) { oListItem["_x0034_1_MEAT_DEPARTAMENT"] = true; } else { oListItem["_x0034_1_MEAT_DEPARTAMENT"] = false; }
                        if (servicesIDs.Contains("s6")) { oListItem["_x0034_2_RESTAURANT"] = true; } else { oListItem["_x0034_2_RESTAURANT"] = false; }
                        if (servicesIDs.Contains("s7")) { oListItem["_x0034_3_MONEY_SERVICES"] = true; } else { oListItem["_x0034_3_MONEY_SERVICES"] = false; }
                        if (servicesIDs.Contains("s8")) { oListItem["_x0034_4_OTC"] = true; } else { oListItem["_x0034_4_OTC"] = false; }
                        if (servicesIDs.Contains("s9")) { oListItem["_x0034_5_KITCHENWARE"] = true; } else { oListItem["_x0034_5_KITCHENWARE"] = false; }
                        if (servicesIDs.Contains("s10")) { oListItem["_x0034_6_ETHNIC_SOURVENIRS"] = true; } else { oListItem["_x0034_6_ETHNIC_SOURVENIRS"] = false; }
                        if (servicesIDs.Contains("s11")) { oListItem["_x0034_7_CLOTHING"] = true; } else { oListItem["_x0034_7_CLOTHING"] = false; }
                        if (servicesIDs.Contains("s12")) { oListItem["_x0034_8_JEWELRY"] = true; } else { oListItem["_x0034_8_JEWELRY"] = false; }
                        if (servicesIDs.Contains("s13")) { oListItem["_x0034_9_CELLPHONE_STORE"] = true; } else { oListItem["_x0034_9_CELLPHONE_STORE"] = false; }
                        //Etnias
                        List<string> etniasIDs = user.Etnias.Split(',').ToList();
                        if (etniasIDs.Contains("e1")) { oListItem["_x0035_5_ELSALVADOR"] = true; } else { oListItem["_x0035_5_ELSALVADOR"] = false; }
                        if (etniasIDs.Contains("e2")) { oListItem["_x0035_6_GUATEMALA"] = true; } else { oListItem["_x0035_6_GUATEMALA"] = false; }
                        if (etniasIDs.Contains("e3")) { oListItem["_x0035_7_COSTARICA"] = true; } else { oListItem["_x0035_7_COSTARICA"] = false; }
                        if (etniasIDs.Contains("e4")) { oListItem["_x0035_8_MEXICO"] = true; } else { oListItem["_x0035_8_MEXICO"] = false; }
                        if (etniasIDs.Contains("e5")) { oListItem["_x0035_9_COLOMBIA"] = true; } else { oListItem["_x0035_9_COLOMBIA"] = false; }
                        if (etniasIDs.Contains("e6")) { oListItem["_x0036_0_PERU"] = true; } else { oListItem["_x0036_0_PERU"] = false; }
                        if (etniasIDs.Contains("e7")) { oListItem["_x0036_1_VENEZUELA"] = true; } else { oListItem["_x0036_1_VENEZUELA"] = false; }
                        if (etniasIDs.Contains("e8")) { oListItem["_x0036_2_CUBA"] = true; } else { oListItem["_x0036_2_CUBA"] = false; }
                        if (etniasIDs.Contains("e9")) { oListItem["_x0036_3_PUERTORICO"] = true; } else { oListItem["_x0036_3_PUERTORICO"] = false; }
                        if (etniasIDs.Contains("e10")) { oListItem["_x0036_4_HONDURAS"] = true; } else { oListItem["_x0036_4_HONDURAS"] = false; }


                        oListItem.Update();

                        ClienteCTX.ExecuteQuery();

                        int newID = oListItem.Id; //Here I reference the ID from the newly created list item
                        var commercialid = newID + 22;
                        var urltosharepoint = "https://limenainc.sharepoint.com/sites/DatosMaestrosFlujos/Lists/Customer_Commercial/DispForm.aspx?ID=" + commercialid;

                        user.urlsharepoint = urltosharepoint;
                        user.idsharepoint = newID;
                        user.status = 2;
                        user.OnSharepoint = true;
                        dblim.Entry(user).State = EntityState.Modified;
                        dblim.SaveChanges();

                        //Enviar correo a supervisor asignado o si es gerencia comercial, enviarselo a JOHAN 05/08/2020

                        TempData["exito"] = "User uploaded to Sharepoint successfully.";

                        try
                        { //Enviamos correos
                            if (emailresp == "supervisors")
                            {
                                var sup = user.Supervisor.ToString();
                                var emailsupervisor = (from g in dblim.Sys_Users where (g.IDSAP == sup && g.Roles.Contains("Sales Supervisor")) select g).FirstOrDefault();

                                if (emailsupervisor != null) {
                                    //Send the email
                                    dynamic semail = new Email("email_notificationEnrollSharepoint");
                                    semail.To = emailsupervisor.Email;
                                    semail.From = "donotreply@limenainc.net";
                                    semail.customer = user.CardName;
                                    semail.url = urltosharepoint;

                                    semail.Send();
                                }
        
                            }
                            else {
   
                                //Send the email
                                dynamic semail = new Email("email_notificationEnrollSharepoint");
                                semail.To = emailresp;
                                semail.From = "donotreply@limenainc.net";
                                semail.customer = user.CardName;
                                semail.url = urltosharepoint;
  
                                semail.Send();
                            }

                        }
                        catch {

                        }



                    }
                    catch (Exception ex) {
                        TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                    }

                }

                return RedirectToAction("New_customers", "Management", null);
            }
            catch (Exception ex2)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex2.Message;
                return RedirectToAction("New_customers", "Management", null);
            }


        }
        //0 - espera
        //1 - validado
        //2 - sharepoint
        //3 - edit
        //4 - rechazado
        public ActionResult SendMessageEdit(int? id, string mensaje)
        {
            try
            {



                var user = (from a in dblim.Tb_NewCustomers where (a.ID_customer == id) select a).FirstOrDefault();


                //Send the email
                dynamic semail = new Email("email_notificationEnrollEdit");
                semail.To = user.E_MailL.ToString();
                semail.From = "donotreply@limenainc.net";
                semail.user = user.FirstName + " " + user.LastName;
                //semail.user = user.FirstName + " " + user.LastName;

                semail.url = "https://limenainc.net/Home/Enroll_edit/?request=" + user.ID_customer;
                semail.message = mensaje;
                if (user.emailRep == "") { semail.ccrep = "donotreply@limenainc.net"; } else { semail.ccrep = user.emailRep; }
                
                semail.Send();

                user.Validated = false;
                    user.status = 3;
                    dblim.Entry(user).State = EntityState.Modified;
                    dblim.SaveChanges();
               

                        TempData["exito"] = "Message sent to customer successfully.";

      

                return RedirectToAction("New_customers", "Management", null);
            }
            catch (Exception ex2)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex2.Message;
                return RedirectToAction("New_customers", "Management", null);
            }


        }


        public ActionResult DeleteRequest(int id)
        {
            try
            {

                Tb_NewCustomers newCustomer = dblim.Tb_NewCustomers.Find(id);
                if (newCustomer != null)
                {
                    var urlimg1 = newCustomer.url_imageTAXCERNUM;
                    var urlimg2 = newCustomer.url_imageTAXCERT;
                    dblim.Tb_NewCustomers.Remove(newCustomer);
                    dblim.SaveChanges();

                    try
                    {

                        if (System.IO.File.Exists(Server.MapPath(urlimg1)))
                        {
                            try
                            {
                                System.IO.File.Delete(Server.MapPath(urlimg1));
                            }
                            catch (System.IO.IOException e)
                            {
                                Console.WriteLine(e.Message);

                            }
                        }
                        if (System.IO.File.Exists(Server.MapPath(urlimg2)))
                        {
                            try
                            {
                                System.IO.File.Delete(Server.MapPath(urlimg2));
                            }
                            catch (System.IO.IOException e)
                            {
                                Console.WriteLine(e.Message);

                            }
                        }
                    }
                    catch
                    {

                    }
                }
                TempData["exito"] = "User deleted successfully.";
                return RedirectToAction("New_customers", "Management", null);
            }
            catch(Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
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

                List<Tb_NewCustomers> lstCustomers = new List<Tb_NewCustomers>();

                    lstCustomers = (from a in dblim.Tb_NewCustomers where(a.FirstName !="" && a.E_Mail !="" && a.OnSharepoint==false)  select a).ToList();

                foreach (var item in lstCustomers) {
                    var slp = (from des in dlipro.OSLP where (des.SlpCode == item.idSAPRep) select des).FirstOrDefault();
                    if (slp != null) {
                        item.emailRep = slp.SlpName;
                    }
                }

             

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
                User.ID_Truck = "";
                User.Truck_name = "";
                User.Url_image = "";
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


                ViewBag.ID_activity = new SelectList(dbcmk.ActivitiesM_types.Where(x=>x.ID_activity==1 || x.ID_activity==6), "ID_activity", "description");
                //Seleccionamos los tipos de recursos a utilizar en el caso de Merchandising

                List<string> uids = new List<string>() { "1", "3", "5", "6", "8", "9", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25","28","29","30","31","38" };

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

            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Management";
                ViewData["Page"] = "Form Builder Preview";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSurvey(string ID_form, string ID_Vendor, string ID_rep)
        {
            try
            {
                int IDForm = Convert.ToInt32(ID_form);

                //CREAMOS LA ESTRUCTURA DE LA ACTIVIDAD
                Tasks nuevaActividad = new Tasks();

                nuevaActividad.ID_formM = Convert.ToInt32(ID_form);
                nuevaActividad.ID_Store = "";
                nuevaActividad.store = "";
                nuevaActividad.ID_brands = "";
                nuevaActividad.Brands = "";
                nuevaActividad.ID_Customer = ID_Vendor;

                var vendor = (from c in dlipro.OCRD where (c.CardCode == ID_Vendor) select c).FirstOrDefault();
                if (vendor != null)
                {
                    nuevaActividad.Customer = vendor.CardName;
                }


                nuevaActividad.desnormalizado = false;
                nuevaActividad.ID_Store = "";
                nuevaActividad.store = "";
                nuevaActividad.address = "";
                nuevaActividad.city = "";
                nuevaActividad.zipcode = "";

                nuevaActividad.state = "";

                nuevaActividad.geoLong = "";
                nuevaActividad.geoLat = "";


                //FIN
                int idrepint = Convert.ToInt32(ID_rep);
                nuevaActividad.ID_taskstatus = 3;
                nuevaActividad.comments = "";
                nuevaActividad.check_in = DateTime.Today.Date;
                nuevaActividad.end_date = DateTime.Today.Date;
                nuevaActividad.ID_empresa = 11;
                nuevaActividad.ID_userCreate = 0;
                nuevaActividad.visit_date = DateTime.Today.Date;
                nuevaActividad.ID_formM = Convert.ToInt32(ID_form);
                nuevaActividad.ID_userEnd = idrepint;
                nuevaActividad.ID_ExternalUser = "";
                nuevaActividad.extra_hours = 0;

                //1 - Inventario
                //2 - Survey
                nuevaActividad.ID_taskType = 2;
                nuevaActividad.TaskType = "UPDATE DATA FORM";
                nuevaActividad.Task_description = "PEPPERI ONLINE CUSTOMER";

                var usuario = (from a in dblim.Sys_Users where (a.ID_User == idrepint) select a).FirstOrDefault();

                nuevaActividad.UserName = usuario.Name + " " + usuario.Lastname;

                //guardamos
                dbcmk.Tasks.Add(nuevaActividad);
                dbcmk.SaveChanges();

                //LUEGO ASIGNAMOS LA PLANTILLA DE FORMULARIO A LA ACTIVIDAD
                //Guardamos el detalle
                var detalles_acopiar = (from a in dbcmk.FormsM_details where (a.ID_formM == IDForm && a.original == true) select a).AsNoTracking().ToList();
                List<FormsM_detailsTasks> targetList = detalles_acopiar.Select(c=> new FormsM_detailsTasks {
                    ID_details = c.ID_details,
                    ID_formresourcetype =c.ID_formresourcetype,
                    fsource = c.fsource,
                    fdescription=c.fdescription,
                    fvalue=c.fvalue,
                    fvalueDecimal=c.fvalueDecimal,
                    fvalueText=c.fvalueText,
                    ID_formM=c.ID_formM,
                    ID_visit= nuevaActividad.ID_task,
                    original=false,
                    obj_order=c.obj_order,
                    obj_group=c.obj_group,
                    idkey=c.idkey,
                    parent=c.parent,
                    query1=c.query1,
                    query2=c.query2,
                    ID_empresa=11


            }).ToList();

                
                dbcmk.BulkInsert(targetList);

                TempData["exito"] = "Form created successfully.";


                return RedirectToAction("SurveyFormC", "Commercial", new { id= nuevaActividad.ID_task});
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("SurveysC", "Commercial", null);
            }




        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateContact(string name, string lastname, string tel, string email, string position, string idcustomer)
        {
            try
            {
                var existe = 0;
                var contacts = (from a in dlipro.BI_Contact_Person
                                where (a.Email==email)
                                select a).Count();

                if (contacts > 0) { existe = 1; }

                var contactsLocal = (from a in internadli.Tb_customerscontacts
                                     where (a.Email == email && a.Accion !=3)
                                     select a).Count();

                if (contactsLocal > 0) { existe = 1; }

                if (existe == 0)
                {
                    Tb_customerscontacts newcontact = new Tb_customerscontacts();
                    newcontact.Name = name.ToUpper();
                    newcontact.LastName = lastname.ToUpper();
                    newcontact.Telephone = tel;
                    newcontact.Email = email.ToUpper();
                    newcontact.Position = position.ToUpper();
                    newcontact.Error = 0;
                    newcontact.ErrorMessage = "";
                    newcontact.DocEntry = "";

                    newcontact.IDSAP = 0;
                    newcontact.CardCode = idcustomer;
                    newcontact.Accion = 1;
                    newcontact.creationdate = DateTime.UtcNow;
                    internadli.Tb_customerscontacts.Add(newcontact);
                    internadli.SaveChanges();

                    TempData["exito"] = "Contact created successfully.";
                    return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomer });
                }
                else {

                    TempData["advertencia"] = "This email is already registered.";
                    return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomer });
                }

            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomer });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditContact(string nameED, string lastnameED, string telED, string emailED, string positionED, string idcustomerED, string idsapcontactED)
        {
            try
            {
                int idsapexiste = Convert.ToInt32(idsapcontactED);

                var existe = internadli.Tb_customerscontacts.Where(a => a.IDSAP==idsapexiste).FirstOrDefault();
                if (existe == null)
                {
                 
                }
                else {

                    internadli.Tb_customerscontacts.Remove(existe);
                    internadli.SaveChanges();
                }
                Tb_customerscontacts newcontact = new Tb_customerscontacts();

                newcontact.Name = nameED.ToUpper();
                newcontact.LastName = lastnameED.ToUpper();
                newcontact.Telephone = telED;
                newcontact.Email = emailED.ToUpper();
                newcontact.Position = positionED.ToUpper();
                newcontact.Error = 0;
                newcontact.ErrorMessage = "";
                newcontact.DocEntry = "";
                newcontact.creationdate = DateTime.UtcNow;
                newcontact.CardCode = idcustomerED;
                newcontact.IDSAP = Convert.ToInt32(idsapcontactED);
                    newcontact.Accion = 2;

                internadli.Tb_customerscontacts.Add(newcontact);
                internadli.SaveChanges();
                TempData["exito"] = "Contact saved successfully.";
                return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomerED });
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomerED });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteContact(string nameDEL, string lastnameDEL, string telDEL, string emailDEL, string positionDEL, string idcustomerDEL, string idsapcontactDEL)
        {
            try
            {
                int idsapexiste = Convert.ToInt32(idsapcontactDEL);

                var existe = internadli.Tb_customerscontacts.Where(a => a.IDSAP == idsapexiste).FirstOrDefault();
                if (existe == null)
                {

                }
                else
                {

                    internadli.Tb_customerscontacts.Remove(existe);
                    internadli.SaveChanges();
                }
                Tb_customerscontacts newcontact = new Tb_customerscontacts();

                newcontact.Name = nameDEL.ToUpper();
                newcontact.LastName = lastnameDEL.ToUpper();
                newcontact.Telephone = telDEL;
                newcontact.Email = emailDEL.ToUpper();
                newcontact.Position = positionDEL.ToUpper();
                newcontact.Error = 0;
                newcontact.ErrorMessage = "";
                newcontact.creationdate = DateTime.UtcNow;
                newcontact.DocEntry = "";
                newcontact.CardCode = idcustomerDEL;
                newcontact.IDSAP = Convert.ToInt32(idsapcontactDEL);
                newcontact.Accion = 3;
                internadli.Tb_customerscontacts.Add(newcontact);
                internadli.SaveChanges();

                TempData["exito"] = "Contact deleted successfully.";
                return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomerDEL });
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Contacts_details", "Commercial", new { id = idcustomerDEL });
            }

        }


        public ActionResult DeleteTask(int id)
        {
            try
            {

                //Eliminamos detalle
                var deletelist = (from a in dbcmk.FormsM_detailsTasks where (a.ID_visit == id) select a).ToList();
                dbcmk.BulkDelete(deletelist);

                Tasks activity = dbcmk.Tasks.Find(id);
                dbcmk.Tasks.Remove(activity);
                dbcmk.SaveChanges();

                


                TempData["exito"] = "Survey deleted successfully.";
                return RedirectToAction("Surveys", "Commercial", null);

            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something wrong happened, try again." + ex.Message;
                return RedirectToAction("Surveys", "Commercial", null);
            }


        }
    }
}