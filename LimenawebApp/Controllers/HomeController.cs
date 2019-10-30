﻿using LimenawebApp.Models;
using Postal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net;

namespace LimenawebApp.Controllers
{
    public class HomeController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult test_webservice()
        {
            //string url = string.Format("http://test.limenainc.net:81/Servicio.svc?singleWsdl");
            //WebRequest request = HttpWebRequest.Create(url);
            //using (WebResponse response = request.GetResponse())
            //{
            //    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //    {
            //        string urlText = reader.ReadToEnd();
            //        //Do whatever you need to do
            //    }
            //}

            var resultado = ExampleWebMethod(4, 5);

            return View();
        }

        private static WebService ExampleAPI = new WebService("http://test.limenainc.net:81/Servicio.svc?singleWsdl");    // DEFAULT location of the WebService, containing the WebMethods

        public static void ChangeUrl(string webserviceEndpoint)
        {
            ExampleAPI = new WebService(webserviceEndpoint);
        }

        public static string ExampleWebMethod(int num1, int num2)
        {
            ExampleAPI.PreInvoke();

            ExampleAPI.AddParameter("Num1", num1.ToString());                    // Case Sensitive! To avoid typos, just copy the WebMethod's signature and paste it
            ExampleAPI.AddParameter("Num2", num2.ToString());     // all parameters are passed as strings
            try
            {
                ExampleAPI.Invoke("GetSuma");                // name of the WebMethod to call (Case Sentitive again!)
            }
            finally { ExampleAPI.PosInvoke(); }

            return ExampleAPI.ResultString;                           // you can either return a string or an XML, your choice
        }



        public ActionResult Test_barcode()
        {

            return View();



        }        public ActionResult Test_barcode2()
        {

            return View();



        }
        public ActionResult Services()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendMessage(string Nombre, string Empresa, string email, string Mensaje)
        {

            RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper();
            if (string.IsNullOrEmpty(recaptchaHelper.Response))
            {
                ModelState.AddModelError("reCAPTCHA", "Please complete the reCAPTCHA");
                return RedirectToAction("Contactus", "Home", new { token = 3 });
            }
            else
            {
                RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();
                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    ModelState.AddModelError("reCAPTCHA", "The reCAPTCHA is incorrect");
                    return RedirectToAction("Contactus", "Home", new { token = 4 });
                }
            }


            //Send the email
            dynamic semail = new Email("emailContact");
            semail.To = "customercare@limenainc.net";
                semail.From = "donotreply@limenainc.net";
                semail.name = Nombre;
                semail.email = email;
                semail.company = Empresa;
                semail.message = Mensaje;
                semail.Send();

            return RedirectToAction("Contactus","Home", new { token=1});
        }

        public ActionResult Brands()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult Contactus(int token=0)
        {
            
            ViewBag.showmessage = token;
            return View();
        }

        public ActionResult Enroll(int token = 0)
        {

            var tipotamana = dlipro.UFD1.Where(c => c.TableID.Contains("OCRD") && c.FieldID == 50 && !c.Descr.Contains("Not Assigned")).ToList();
            ViewBag.tamanostienda = tipotamana;
            ViewBag.showmessage = token;
            var users = dblim.Sys_Users.Where(a => a.Roles.Contains("Sales Representative")).OrderBy(c => c.Name).ToList();
            ViewBag.lstreps = users;
            return View();
        }
        public ActionResult Enroll_edit(int token = 0, int request=0)
        {

            var tipotamana = dlipro.UFD1.Where(c => c.TableID.Contains("OCRD") && c.FieldID == 50 && !c.Descr.Contains("Not Assigned")).ToList();
            ViewBag.tamanostienda = tipotamana;
            ViewBag.showmessage = token;

            var requestdetails = (from a in dblim.Tb_NewCustomers where (a.ID_customer == request) select a).FirstOrDefault();
            ViewBag.idcustomer = request;
            return View(requestdetails);
        }

        public ActionResult UnderConstruction()
        {
            return View();
        }
        public ActionResult Login(bool access =true, int? logpage=0)
        {
            if (access == false) {
                if (logpage == 0)
                {
                    TempData["advertencia"] = "Session expired.";
                }
                else if (logpage == 1)
                {
                    TempData["advertencia"] = "Wrong email or password.";
                }
               
            }

            HttpCookie aCookieCorreo = Request.Cookies["correo"];
            HttpCookie aCookiePassword = Request.Cookies["pass"];
            HttpCookie aCookieRememberme = Request.Cookies["rememberme"];

            try
            {
                var correo = Server.HtmlEncode(aCookieCorreo.Value).ToString();
                var pass = Server.HtmlEncode(aCookiePassword.Value).ToString();
                int remember =Convert.ToInt32(Server.HtmlEncode(aCookieRememberme.Value));

                if (remember == 1) { ViewBag.remember = true; } else { ViewBag.remember = false; }
                ViewBag.correo = correo;
                ViewBag.pass = pass;
              
            }
            catch {
               ViewBag.remember = false; 
  
            }
           


            return View();
        }
        public ActionResult Log_in(string email, string password, string date, bool rememberme)
        {
 

            Session["activeUser"] = (from a in dblim.Sys_Users where (a.Email == email && a.Password == password && a.Active == true) select a).FirstOrDefault();
            if (Session["activeUser"] != null)
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                ///PARA RECORDAR DATOS
                if (rememberme == true)
                {
                    if (Request.Cookies["correo"] != null)
                    {
                        //COMO YA EXISTE NO NECESITAMOS RECREARLA
                    }
                    else
                    {
                        HttpCookie aCookie = new HttpCookie("correo");
                        aCookie.Value = activeuser.Email.ToString();
                        aCookie.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie2 = new HttpCookie("pass");
                        aCookie2.Value = activeuser.Password.ToString();
                        aCookie2.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie3 = new HttpCookie("rememberme");
                        aCookie3.Value = "1";
                        aCookie3.Expires = DateTime.Now.AddMonths(3);


                        Response.Cookies.Add(aCookie);
                        Response.Cookies.Add(aCookie2);
                        Response.Cookies.Add(aCookie3);
                    }
                }

                    ////Save log
                    //Sys_LogCon newLog = new Sys_LogCon();
                    //try
                    //{
                    //    newLog.ID_Company = activeuser.ID_Company;
                    //    newLog.ID_User = activeuser.ID_User;
                    //    newLog.date = Convert.ToDateTime(date);

                    //    newLog.City = "";
                    //    newLog.Country_Code = "";
                    //    newLog.Country_Name = "";
                    //    newLog.Continent_Name = "";
                    //    newLog.Region_Code = "";
                    //    newLog.Region_Name = "";
                    //    newLog.IP = "";
                    //    newLog.TypeH = "";
                    //    newLog.Hostname = "";
                    //    newLog.Lat = "";
                    //    newLog.Long = "";

                    //    dblim.Sys_LogCon.Add(newLog);
                    //    dblim.SaveChanges();
                    //}
                    //catch {

                    //}


                    if (activeuser.Departments.Contains("Sales")) {
                    return RedirectToAction("Dashboard_sales", "Main", null);
                }
                else if (activeuser.Departments.Contains("DSD"))
                {
                    return RedirectToAction("Dashboard_dsd", "Main", null);

                }
                else if (activeuser.Departments.Contains("Operations"))
                {
                    return RedirectToAction("Dashboard_operations", "Main", null);

                }
                else if (activeuser.Departments.Contains("Customer"))
                {
                    return RedirectToAction("Dashboard_customers", "Main", null);

                }
                else if (activeuser.Departments.Contains("Inventory"))
                {
                    return RedirectToAction("Dashboard_Inventory", "Main", null);

                }

            }

            return RedirectToAction("Login", "Home", new { access= false, logpage = 1 });
        }

        public ActionResult Log_out()
        {
            Session.RemoveAll();
            //Global_variables.active_user.Name = null;
            //Global_variables.active_Departments = null;
            //Global_variables.active_Roles = null;
            if (Request.Cookies["correo"] != null)
            {
                var c = new HttpCookie("correo");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            if (Request.Cookies["pass"] != null)
            {
                var c = new HttpCookie("pass");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            if (Request.Cookies["rememberme"] != null)
            {
                var c = new HttpCookie("rememberme");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }



            return RedirectToAction("Login","Home", new { access=true});
        }

        public ActionResult Reset_password(bool token=false, bool email=false)
        {
            if (token == true)
            {
                ViewData["msgnotexist"] = "";
                ViewBag.msg = "Your password has been reset successfully! Your new password has been sent to your email address.";
            }
            else {
                if (email == true)
                {
                    ViewData["msgnotexist"] = "This email does not exist.";
                }
                else {
                    ViewData["msgnotexist"] = "";
                }
                
                ViewBag.msg = "";
            }
            return View();
        }

        public ActionResult Reset_pass(string email)
        {


            Sys_Users User = (from a in dblim.Sys_Users where (a.Email == email) select a).FirstOrDefault();
            if (User != null)
            {
                User.Password = "dli2019";
                dblim.Entry(User).State = EntityState.Modified;
                dblim.SaveChanges();


                //Send the email
                dynamic semail = new Email("reset_password");
                semail.To = User.Email.ToString();
                semail.From = "donotreply@limenainc.net";
                semail.user = User.Name + " " + User.Lastname;
                semail.email = User.Email;
                semail.password = User.Password;

                semail.Send();

                //FIN email
            
                return RedirectToAction("Reset_password", "Home", new { token = true, email=false });

            }
            
            return RedirectToAction("Reset_password", "Home", new { token = false, email=true });
        }

        [HttpPost]
        public ActionResult UploadInformationNewCustomer(string CardName,string Phone1,string E_Mail,string IntrntSite,string TAXID,string TAXCERTNUM,string FirstName,string LastName,string Position,string Tel1,string E_MailL,string countryId,string stateId,string cityId,string Street,string ZipCode,string servicesSelected,string etniasselected,string HorarioOperacion,string ReciboMercaderiaDia,string ReciboMercaderiaArea,Boolean MuelleDescarga,string TamanoTienda, int idrep)
        {


            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //Creamos modelo
                    Tb_NewCustomers newCustomer = new Tb_NewCustomers();
                    newCustomer.CardName = CardName.ToUpper();
                    newCustomer.Phone1 = Phone1;
                    newCustomer.E_Mail = E_Mail.ToUpper();
                    newCustomer.IntrntSite = IntrntSite.ToUpper();
                    newCustomer.TAXID = TAXID.ToUpper();
                    newCustomer.TAXCERTNUM = TAXCERTNUM.ToUpper();
                    newCustomer.FirstName = FirstName.ToUpper();
                    newCustomer.LastName = LastName.ToUpper();
                    newCustomer.Position = Position.ToUpper();
                    newCustomer.Tel1 = Tel1;
                    newCustomer.E_MailL = E_MailL.ToUpper();
                    newCustomer.Street = Street.ToUpper();
                    newCustomer.City = cityId.ToUpper();
                    newCustomer.State = stateId.ToUpper();
                    newCustomer.ZipCode = ZipCode.ToUpper();
                    newCustomer.Country = countryId.ToUpper();
                    newCustomer.StoreServices = servicesSelected;
                    newCustomer.Etnias = etniasselected;
                    newCustomer.OperationTime = HorarioOperacion;
                    newCustomer.ReciboMercaderia_dia = ReciboMercaderiaDia.ToUpper();
                    newCustomer.ReciboMercaderia_area = ReciboMercaderiaArea.ToUpper();
                    newCustomer.Muelle_descarga = MuelleDescarga;
                    newCustomer.Store_size = TamanoTienda;
                    newCustomer.Validated = false;
                    newCustomer.OnSharepoint = false;
                    newCustomer.status = 0;
                    newCustomer.urlsharepoint = "";
                    newCustomer.idsharepoint = 0;
                    newCustomer.url_imageTAXCERNUM = "";
                    newCustomer.url_imageTAXCERT = "";
                    newCustomer.idRep = 0;
                    newCustomer.idSAPRep = 0;
                    newCustomer.emailRep = "";

                    var usuario = dblim.Sys_Users.Where(a => a.ID_User == idrep).FirstOrDefault();
                    if (usuario != null)
                    {
                        newCustomer.idRep = usuario.ID_User;
                        if (usuario.IDSAP == "" || usuario.IDSAP==null)
                        {
                            newCustomer.idSAPRep = 0;
                        }
                        else {
                            newCustomer.idSAPRep = Convert.ToInt32(usuario.IDSAP);
                        }
                        
                        newCustomer.emailRep = usuario.Email;
                    }
                    else {
                        newCustomer.idRep =0;
                        newCustomer.idSAPRep = 0;
                        newCustomer.emailRep = "";
                    }
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

                        Image TargetImg = Image.FromStream(file.InputStream, true, true);
                        DateTime time = DateTime.Now;
                        Image imagenfinal = (Image)TargetImg.Clone();
                        Bitmap bitmapImg = new Bitmap(imagenfinal);// Original Image
                        var path = "";
                        if (i == 0)
                        {
                            path = Path.Combine(Server.MapPath("~/SharedContent/images/enroll"), TAXID + "_1" + ".jpg");
                            newCustomer.url_imageTAXCERT = "~/SharedContent/images/enroll/" + TAXID + "_1" + ".jpg";
                        }
                        else {
                            path = Path.Combine(Server.MapPath("~/SharedContent/images/enroll"), TAXCERTNUM + "_2" + ".jpg");
                            newCustomer.url_imageTAXCERNUM= "~/SharedContent/images/enroll/" + TAXCERTNUM + "_2" + ".jpg";
                        }
                            
                        bitmapImg.Save(path, ImageFormat.Jpeg);

                        bitmapImg.Dispose();
                        
                       

                    }

                    //validar rep
                    var idsup = 19;
                    newCustomer.Supervisor =idsup;
                    try
                    {
                        List<ZipcodeStatesSupervisor> statesList = dlipro.Database.SqlQuery<ZipcodeStatesSupervisor>("SELECT * FROM SDK_State_and_Supervisor").ToList();
                        var statess = stateId.ToUpper();
                        var existe = statesList.Where(c => c.State == statess).ToList();
                        if (existe.Count > 0)
                        {
                            idsup= existe[0].Supervisor;
                            newCustomer.Supervisor = idsup;
                        }
                    }
                    catch {

                    }

                    dblim.Tb_NewCustomers.Add(newCustomer);
                    dblim.SaveChanges();
                    try
                    {
                        var idstring = idsup.ToString();
                        //Enviamos correo a supervisor
                        var supervisor = (from a in dblim.Sys_Users where (a.IDSAP == idstring && a.Roles.Contains("Sales Supervisor")) select a).FirstOrDefault();
                        if (supervisor != null)
                        {
                            //Send the email
                            dynamic semail = new Email("email_notificationEnrollSup");
                            semail.To = supervisor.Email.ToString();
                            semail.From = "donotreply@limenainc.net";
                            semail.user = newCustomer.FirstName + " " + newCustomer.LastName;
                            semail.company = newCustomer.CardName;

                            semail.Send();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        //Send the email
                        dynamic semail = new Email("email_notificationEnrollNC");
                        semail.To = newCustomer.E_MailL.ToString();
                        semail.From = "donotreply@limenainc.net";
                        semail.user = newCustomer.FirstName + " " + newCustomer.LastName;
                        //semail.ccrep = newCustomer.emailRep;
                        if (newCustomer.emailRep == "") { semail.ccrep = "donotreply@limenainc.net"; } else { semail.ccrep = newCustomer.emailRep; }


                        semail.Send();
                    }
                    catch(Exception ex) {

                    }



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

        [HttpPost]
        public ActionResult UpdateInformationNewCustomer(int idcustomer,string CardName, string Phone1, string E_Mail, string IntrntSite, string TAXID, string TAXCERTNUM, string FirstName, string LastName, string Position, string Tel1, string E_MailL, string countryId, string stateId, string cityId, string Street, string ZipCode, string servicesSelected, string etniasselected, string HorarioOperacion, string ReciboMercaderiaDia, string ReciboMercaderiaArea, Boolean MuelleDescarga, string TamanoTienda)
        {


            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //Creamos modelo
                    Tb_NewCustomers newCustomer = dblim.Tb_NewCustomers.Find(idcustomer);

                    newCustomer.CardName = CardName.ToUpper();
                    newCustomer.Phone1 = Phone1;
                    newCustomer.E_Mail = E_Mail.ToUpper();
                    newCustomer.IntrntSite = IntrntSite.ToUpper();
                    newCustomer.TAXID = TAXID.ToUpper();
                    newCustomer.TAXCERTNUM = TAXCERTNUM.ToUpper();
                    newCustomer.FirstName = FirstName.ToUpper();
                    newCustomer.LastName = LastName.ToUpper();
                    newCustomer.Position = Position.ToUpper();
                    newCustomer.Tel1 = Tel1;
                    newCustomer.E_MailL = E_MailL.ToUpper();
                    newCustomer.Street = Street.ToUpper();
                    newCustomer.City = cityId.ToUpper();
                    newCustomer.State = stateId.ToUpper();
                    newCustomer.ZipCode = ZipCode.ToUpper();
                    newCustomer.Country = countryId.ToUpper();
                    newCustomer.StoreServices = servicesSelected;
                    newCustomer.Etnias = etniasselected;
                    newCustomer.OperationTime = HorarioOperacion;
                    newCustomer.ReciboMercaderia_dia = ReciboMercaderiaDia.ToUpper();
                    newCustomer.ReciboMercaderia_area = ReciboMercaderiaArea.ToUpper();
                    newCustomer.Muelle_descarga = MuelleDescarga;
                    newCustomer.Store_size = TamanoTienda;
                    newCustomer.Validated = false;
                    newCustomer.OnSharepoint = false;
                    newCustomer.status = 0;
                    newCustomer.urlsharepoint = "";
                    newCustomer.idsharepoint = 0;

                    try
                    {

                        if (System.IO.File.Exists(Server.MapPath(newCustomer.url_imageTAXCERNUM)))
                        {
                            try
                            {
                                System.IO.File.Delete(Server.MapPath(newCustomer.url_imageTAXCERNUM));
                            }
                            catch (System.IO.IOException e)
                            {
                                Console.WriteLine(e.Message);

                            }
                        }
                        if (System.IO.File.Exists(Server.MapPath(newCustomer.url_imageTAXCERT)))
                        {
                            try
                            {
                                System.IO.File.Delete(Server.MapPath(newCustomer.url_imageTAXCERT));
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

                        Image TargetImg = Image.FromStream(file.InputStream, true, true);
                        DateTime time = DateTime.Now;
                        Image imagenfinal = (Image)TargetImg.Clone();
                        Bitmap bitmapImg = new Bitmap(imagenfinal);// Original Image
                        var path = "";
                        if (i == 0)
                        {
                            path = Path.Combine(Server.MapPath("~/SharedContent/images/enroll"), TAXID + "_1" + ".jpg");
                            newCustomer.url_imageTAXCERT = "~/SharedContent/images/enroll/" + TAXID + "_1" + ".jpg";
                        }
                        else
                        {
                            path = Path.Combine(Server.MapPath("~/SharedContent/images/enroll"), TAXCERTNUM + "_2" + ".jpg");
                            newCustomer.url_imageTAXCERNUM = "~/SharedContent/images/enroll/" + TAXCERTNUM + "_2" + ".jpg";
                        }

                        bitmapImg.Save(path, ImageFormat.Jpeg);

                        bitmapImg.Dispose();



                    }

                    //validar rep
                    var idsup = 19;
                    newCustomer.Supervisor = idsup;
                    try
                    {
                        List<ZipcodeStatesSupervisor> statesList = dlipro.Database.SqlQuery<ZipcodeStatesSupervisor>("SELECT * FROM SDK_State_and_Supervisor").ToList();
                        var statess = stateId.ToUpper();
                        var existe = statesList.Where(c => c.State == statess).ToList();
                        if (existe.Count > 0)
                        {
                            idsup = existe[0].Supervisor;
                            newCustomer.Supervisor = idsup;
                        }
                    }
                    catch
                    {

                    }

                  
                    dblim.Entry(newCustomer).State = EntityState.Modified;
                    dblim.SaveChanges();
                    try
                    {
                        var idstring = idsup.ToString();
                        //Enviamos correo a supervisor
                        var supervisor = (from a in dblim.Sys_Users where (a.IDSAP == idstring && a.Roles.Contains("Sales Supervisor")) select a).FirstOrDefault();
                        if (supervisor != null)
                        {
                            //Send the email
                            dynamic semail = new Email("email_notificationEnrollSup");
                            semail.To = supervisor.Email.ToString();
                            semail.From = "donotreply@limenainc.net";
                            semail.user = newCustomer.FirstName + " " + newCustomer.LastName;
                            semail.company = newCustomer.CardName;

                            semail.Send();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        //Send the email
                        dynamic semail = new Email("email_notificationEnrollNC");
                        semail.To = newCustomer.E_MailL.ToString();
                        semail.From = "donotreply@limenainc.net";
                        semail.ccrep = newCustomer.emailRep;
                        semail.user = newCustomer.FirstName + " " + newCustomer.LastName;
                        if (newCustomer.emailRep == "") { semail.ccrep = "donotreply@limenainc.net"; } else { semail.ccrep = newCustomer.emailRep; }

                        semail.Send();
                    }
                    catch (Exception ex)
                    {

                    }



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
    }
}