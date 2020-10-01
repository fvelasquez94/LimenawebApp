using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.Creditmemos_api;
using LimenawebApp.Models.Invoices;
using LimenawebApp.Models.Returnreasons_api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LimenawebApp.Controllers.Warehouse
{
    public class WarehouseController : Controller
    {
        private dbLimenaEntities db = new dbLimenaEntities();
        private Cls_session cls_session = new Cls_session();
        private Cls_Creditmemos cls_creditmemos = new Cls_Creditmemos();
        private Cls_Returnreasons cls_returnreasons = new Cls_Returnreasons();
        private cls_invoices Cls_invoices = new cls_invoices();
        private Cls_Authorizations cls_Authorizations = new Cls_Authorizations();
        public ActionResult ReceiveCredits(string fstartd, string fendd)
        {

            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Warehouse";
                ViewData["Page"] = "Receive Credits";
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


                var creditmemos = cls_creditmemos.GetCreditMemos(0, "",filtrostartdate, filtroenddate, true);
                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits")) {
                        roles.Add("FD");
                    }
                  

                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                      
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow= returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                List<Returnreason_api> lstreturnsreasons = new List<Returnreason_api>();
                var response = cls_returnreasons.GetReturnreasons();
                if (response.data != null) { lstreturnsreasons = response.data.Where(c => c.visibleFor != null && c.visibleFor.Contains("SDA") && c.active == "Y" && roles.Contains(c.authorizedBy)).ToList(); }

                ViewBag.returnreasonschange = lstreturnsreasons;


                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                string[] rrfilter;
                if (creditmemos.data != null) {

                    var sss= creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
             details.
             OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                    rrfilter = (from creditmemo in creditmemos.data
                                      from reason in creditmemo.details
                                      select reason.returnReasonCode).Distinct().ToArray();

                    returnsToshow = returnsToshow.Where(c => rrfilter.Contains(c.reasonCode)).Select(c => c).ToList();
                }
                //filtramos nuevamente
              

                ViewBag.returnreasons = returnsToshow;

                return View(creditmemos.data);



            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }

     
        }


        public ActionResult getCreditmemoDetails(int DocEntry)
        {
            try
            {
                var creditmemos = cls_creditmemos.GetCreditMemos(DocEntry, "", null, null, true);

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                string result = javaScriptSerializer.Serialize(creditmemos.data.FirstOrDefault().details);

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult ReceivedCredits(string fstartd, string fendd)
        {

            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Warehouse";
                ViewData["Page"] = "Received Credits";
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


                var creditmemos = cls_creditmemos.GetCreditMemosOriginals(0, "", filtrostartdate, filtroenddate, true);

                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("FD");
                    }


                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow = returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                List<Returnreason_api> lstreturnsreasons = new List<Returnreason_api>();
                var response = cls_returnreasons.GetReturnreasons();
                if (response.data != null) { lstreturnsreasons = response.data.Where(c => c.visibleFor != null && c.visibleFor.Contains("SDA") && c.active == "Y" && roles.Contains(c.authorizedBy)).ToList(); }

                ViewBag.returnreasonschange = lstreturnsreasons;


                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                string[] rrfilter;
                if (creditmemos.data != null)
                {

                    var sss = creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
              details.
              OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                    rrfilter = (from creditmemo in creditmemos.data
                                from reason in creditmemo.details
                                select reason.returnReasonCode).Distinct().ToArray();

                    returnsToshow = returnsToshow.Where(c => rrfilter.Contains(c.reasonCode)).Select(c => c).ToList();
                }
                //filtramos nuevamente


                ViewBag.returnreasons = returnsToshow;

                return View(creditmemos.data);




            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }


        }

        public ActionResult Put_creditMemoDetail(PutDetailsCreditmemos_api Item, int DocentryCredit)
        {
            try
            {
                var response = cls_creditmemos.PutCreditmemo(Item, DocentryCredit);

                if (response.IsSuccessful == true)
                {
                  
                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var result = "Error";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Put_creditMemoDetailNoshow(PutDetailsCreditmemos_api Item, int DocentryCredit, PutDetailsCreditmemos_apiNOSHOW newrow)
        {
            try
            {
                var response = cls_creditmemos.PutCreditmemo(Item, DocentryCredit);

                if (response.IsSuccessful == true)
                {
                    //Agregamos nueva linea negativa
                    var response2 = cls_creditmemos.CancelCreditmemoDetail(newrow, DocentryCredit);

                    if (response.IsSuccessful == true)
                    {
                        var result = "SUCCESS";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else {
                        var result = "Error";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
              
                }
                else
                {

                    var result = "Error";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Get_creditMemoDetail(int DocentryCredit)
        {
            try
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));

                var creditmemos = cls_creditmemos.GetCreditMemos(DocentryCredit, "", null, null, true);



                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("FD");
                    }


                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow = returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                if (creditmemos.data != null)
                {

                    var sss = creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
              details.
              OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                }


                var result = new { details = creditmemos.data.FirstOrDefault().details, showsubmit = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                var result = new { error = "Error", errormsg = resulterror };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }


        public ActionResult Get_creditMemoDetailOriginal(int DocentryCredit)
        {
            try
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
            
                var creditmemos = cls_creditmemos.GetCreditMemoOriginal(DocentryCredit,true);



                var returnreason = cls_returnreasons.GetReturnreasons();
                var roles = new List<string>();
                if (s.Contains("Front Desk"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("FD");
                    }


                }
                else if (s.Contains("Operations"))
                {
                    if (r.Contains("Receive Credits"))
                    {
                        roles.Add("OP");
                        roles.Add("QC");
                    }

                }

                if (r.Contains("Super Admin"))
                {
                    roles.Add("FD");
                    roles.Add("OP");
                    roles.Add("QC");
                }

                var returnsToshow = returnreason.data.Where(c => roles.Contains(c.authorizedBy)).ToList();

                //filtramos data a mostrar
                var codes = returnsToshow.Select(c => c.reasonCode).ToArray();
                if (creditmemos.data != null)
                {

                    var sss = creditmemos.data.Where(c => c.details.Any(d => codes.Contains(d.returnReasonCode))).OrderBy(p => p.
              details.
              OrderBy(c => c.returnReasonCode).Select(c => c.returnReasonCode).FirstOrDefault()).ToList();
                    creditmemos.data = sss;

                }


                var result = new { details= creditmemos.data.FirstOrDefault().details, showsubmit= 0 };           
                return Json(result, JsonRequestBehavior.AllowGet);
            

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                var result = new {error="Error", errormsg= resulterror };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }


        public ActionResult Transform_creditMemo(int docentryCreditMemo, int docEntryInv)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //Verificamos si no existen mas Creditos sin recibir
                var creditmemos = cls_creditmemos.GetCreditMemos(docEntryInv, "", null, null, true);
                var faltavalidar = false;

                foreach (var item in creditmemos.data) {
                    foreach (var detail in item.details.Where(c=>c.quantity>0)) {
                        if (detail.received == false && detail.noShow==false) {
                            faltavalidar = true;
                        }
                    }
                }

                if (faltavalidar == false)
                {
                    var response = cls_creditmemos.TransformCreditMemo(docentryCreditMemo, 1);

                    if (response.IsSuccessful == true)
                    {
                        //Validamos que no haya autorizacion de NO recibir pago o incoming payments
                        //1.Validamos estado de orden 
                        //Si el estado es =2, quiere decir que aun esta en Operaciones y el Vendedor NO ha realizado pagos
                        var invoice = Cls_invoices.GetInvoice(docEntryInv, 0, false).data.FirstOrDefault();
                        PUT_Invoices_api newput = new PUT_Invoices_api();

                        //Si hay pagos, va para finanzas
                        if (invoice.paymentsDraft > 0)
                        {
                            newput.stateSd = 3;
                        }
                        else {
                            //Si no hay pagos y NO existe autorizacion, la dejamos en estado 6 Waiting for payment
                            //Si autorizacion CODIGO 1 (deja producto sin recibir pago) se aprueba, colocar estado 6 y verificar que estado de factura NO sea 3 (sino quiere decir que el vendedor cambio ele stado)
                            //Authorizations
                            var authorizations = cls_Authorizations.GetAuthorizations(docEntryInv,"","",null,null);
                            if (authorizations.data != null)
                            {
                                var existe = false;
                                foreach (var auth in authorizations.data)
                                {
                                    if (auth.idReason == 1)
                                    {
                                        existe = true;
                                    }
                                }
                                if (existe == true)
                                {
                                    newput.stateSd = 6;
                                }
                                else {
                                    //Si NO existe, NO hay pagos, no hay otra evaluacion que hacer
                                    newput.stateSd = 6;
                                }
                            }


                        }

                        var response2 = Cls_invoices.PutInvoice(docEntryInv, newput);
                        var result = "SUCCESS";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        var result = "Error";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else {
                    var result = "VALIDAR";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }



            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }
    }
}