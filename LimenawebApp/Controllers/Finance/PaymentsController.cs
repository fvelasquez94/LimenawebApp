using LimenawebApp.Controllers.API;
using LimenawebApp.Controllers.Session;
using LimenawebApp.Models;
using LimenawebApp.Models.Invoices;
using LimenawebApp.Models.Payments;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static LimenawebApp.Models.Journal.Mdl_Journal;
using static LimenawebApp.Models.Payments.Mdl_PaymentsPOSTPUT;

namespace LimenawebApp.Controllers.Finance
{
    public class PaymentsController : Controller
    {
        private Cls_session cls_session = new Cls_session();
        private cls_payments Cls_payments = new cls_payments();
        private cls_invoices cls_invoices = new cls_invoices();
        private Cls_Creditmemos cls_Creditmemos = new Cls_Creditmemos();
        private Cls_Journals cls_Journals = new Cls_Journals();
        private dbLimenaEntities db = new dbLimenaEntities();
        private Cls_Banks cls_Banks = new Cls_Banks();
        public ActionResult ReceivePayment(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Finance";
                ViewData["Page"] = "Payments";
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
                var sunday = now;
                var saturday = sunday.AddDays(1).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                List<Payments_routesmain> lstcustomers = new List<Payments_routesmain>();
                List<Invoices_api> invoices = new List<Invoices_api>();
                //Authorizations
                var payments = Cls_payments.GetPayments("", 0, filtrostartdate, filtroenddate, false);

                //if (payments != null)
                //{
                //    if (payments.data != null)
                //    {
                //        invoices = cls_invoices.GetInvoices("", "", false, filtrostartdate, filtroenddate, false);
                //        var docnums = payments.data.Select(c => c.docEntryInv).Distinct().ToArray();

                //        invoices.data = invoices.data.Where(c => docnums.Contains(c.docEntry)).ToList();
                //    }
                //}
                

                //if (invoices != null)
                //{
                //    if (invoices.data != null)
                //    {




                //        lstcustomers = (from b in invoices.data
                //                        select new Payments_routesmain
                //                        {

                //                            Id_Delivery = b.id_Delivery,
                //                            Id_Driver = b.id_Driver,
                //                            Id_Truck = b.id_Truck,
                //                            OrdersC = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus == 7) select e).Count(),
                //                            OrdersP = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus == 1) select e).Count(),
                //                            OrdersF = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus != 1 && e.invoiceStatus != 7) select e).Count(),
                //                            TotalOrders = (from f in invoices.data where (f.cardCode == b.cardCode) select f).Count(),
                //                            DeliveryDate = b.deliveryDate,
                //                            DeliveryRoute = b.deliveryRoute,
                //                            Driver = b.driver,
                //                            Truck = b.truck,
                //                            TotalAR = (from e in invoices.data where (e.cardCode == b.cardCode && e.isAR == false) select e.balanceCustomer).Sum(),
                //                            TotalAR_paid = (from d in cls_invoices.GetInvoices("", b.cardCode, true, null, null, false).data select d.paymentsDraft).Sum(),
                //                            TotalAmount = (from e in invoices.data where (e.cardCode == b.cardCode && e.isAR == false) select e.docTotal).Sum(),
                //                            TotalIncomingPayments = (from e in invoices.data where (e.cardCode == b.cardCode) select e.paymentsDraft).Sum()
                                            

                //                        }).GroupBy(y => y.Id_Delivery).Select(i => i.FirstOrDefault()).ToList();
                //    }
                //}
                if (payments != null)
                {
                    if (payments.data != null)
                    {

                        var docnums = payments.data.Select(c => c.docEntryInv).Distinct().ToArray();

                        foreach (var item in docnums)
                        {
                            var invoice = cls_invoices.GetInvoice(item,0, false);
                            invoices.Add(invoice.data.FirstOrDefault());
                        }


                    }
                }


                if (invoices != null)
                {
                    if (invoices != null)
                    {

                        lstcustomers = (from b in invoices
                                        where (b.paymentsDraft > 0 && (b.invoiceStatus == 3 || b.invoiceStatus==4 || b.invoiceStatus == 5))
                                        select new Payments_routesmain
                                        {

                                            Id_Delivery = b.id_Route,
                                            Id_Driver = b.id_Driver,
                                            Id_Truck = b.id_Truck,
                                            OrdersC = (from e in invoices where (e.cardCode == b.cardCode && e.invoiceStatus == 7) select e).Count(),
                                            OrdersP = (from e in invoices where (e.cardCode == b.cardCode && e.invoiceStatus == 1) select e).Count(),
                                            OrdersF = (from e in invoices where (e.cardCode == b.cardCode && e.invoiceStatus != 1 && e.invoiceStatus != 7) select e).Count(),
                                            TotalOrders = (from f in invoices where (f.cardCode == b.cardCode) select f).Count(),
                                            DeliveryDate = b.deliveryDate,
                                            DeliveryRoute = b.deliveryRoute,
                                            Driver = b.driver,
                                            Truck = b.truck,
                                            TotalAR = (from e in invoices where (e.cardCode == b.cardCode && e.isAR == false) select e.balanceCustomer).Sum(),
                                            TotalAR_paid = (from d in cls_invoices.GetInvoices("", b.cardCode, "",0,true, null, null, false).data select d.paymentsDraft).Sum(),
                                            TotalAmount = (from e in invoices where (e.cardCode == b.cardCode && e.isAR == false) select e.docTotal).Sum(),
                                            TotalIncomingPayments = (from e in invoices where (e.cardCode == b.cardCode) select e.paymentsDraft).Sum()


                                        }).GroupBy(y => y.Id_Delivery).Select(i => i.FirstOrDefault()).ToList();
                    }
                }
                return View(lstcustomers);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult ReceivePayments(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Finance";
                ViewData["Page"] = "Payments";
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
                var sunday = now;
                var saturday = sunday.AddHours(23).AddMinutes(59);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                //List<Payments_routesmain> lstcustomers = new List<Payments_routesmain>();
                //List<Invoices_api> invoices = new List<Invoices_api>();
             
                //var payments = Cls_payments.GetPayments("", 0, filtrostartdate, filtroenddate, false);
                var payments = Cls_payments.GetPaymentsv2();

                var routes = (from a in db.Tb_Planning where (a.Departure >= filtrostartdate && a.Departure <= filtroenddate) select a).ToList();
                //if (payments != null && payments.data !=null) {
                //    if (payments.data.Count > 0) {
                //        foreach (var item in payments.data) {
                //            var invoice = cls_invoices.GetInvoice(item.docEntryInv,0,false).data.FirstOrDefault();
                //            if (invoice != null) {
                //                item.series = invoice.invoiceStatus;
                //                item.cardCode = item.cardCode + " - " + invoice.cardName;
                //                item.userWeb = item.idRoute + " - " + invoice.deliveryRoute;
                //            }
                            
                //        }
                //    }
                //}
                ViewBag.routes = routes;
                //Banks
                var banks = cls_Banks.GetBanks();
                ViewBag.banks = banks.data;

                return View(payments.data);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult ReceivedPayment(string fstartd, string fendd)
        {
            if (cls_session.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Finance";
                ViewData["Page"] = "Payments";
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
                var sunday = now;
                var saturday = sunday.AddDays(1).AddHours(23);

                if (fstartd == null || fstartd == "") { filtrostartdate = sunday; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = saturday; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                GetPayments_api paymentshistory = new GetPayments_api();
              
                //Authorizations
               paymentshistory = Cls_payments.GetPaymentsOriginals("", 0, filtrostartdate, filtroenddate, false);


                return View(paymentshistory);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public ActionResult Get_CustomersRoute(string iddelivery)
        {
            try
            {
                List<Drivers_customersmain> lstcustomers = new List<Drivers_customersmain>();

                var invoices = cls_invoices.GetInvoices("", "", iddelivery,0, false, null, null, false);
                if (invoices != null)
                {
                    if (invoices.data != null)
                    {
                        lstcustomers = (from b in invoices.data
                                        where (b.paymentsDraft> 0 && (b.invoiceStatus == 3 || b.invoiceStatus == 4 || b.invoiceStatus == 5))
                                        select new Drivers_customersmain
                                        {
                                            CardCode = b.cardCode,
                                            CardName = b.cardName,
                                            SlpCode = b.slpCode,
                                            SlpName = b.slpName,
                                            Id_Delivery = b.id_Route,
                                            Id_Driver = b.id_Driver,
                                            Id_Truck = b.id_Truck,
                                            OrdersC = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus == 7) select e).Count(),
                                            OrdersP = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus == 1) select e).Count(),
                                            OrdersF = (from e in invoices.data where (e.cardCode == b.cardCode && e.invoiceStatus != 1 && e.invoiceStatus != 7) select e).Count(),
                                            TotalOrders = (from f in invoices.data where (f.cardCode == b.cardCode) select f).Count(),
                                            DeliveryDate = b.deliveryDate,
                                            DeliveryRoute = b.deliveryRoute,
                                            Driver = b.driver,
                                            Truck = b.truck,
                                            TotalAR = (from e in invoices.data where (e.cardCode == b.cardCode && e.isAR == false) select e.balanceCustomer).Sum(),
                                            TotalAR_paid = (from d in cls_invoices.GetInvoices("", b.cardCode,"",0, true, null, null, false).data where (d.paymentsDraft > 0 && d.invoiceStatus == 3) select d.paymentsDraft).Sum(),
                                            TotalReturns = (from e in invoices.data where (e.cardCode == b.cardCode && e.isAR == false) select e.returnsDraft).Sum(),
                                            TotalAmount = (from e in invoices.data where (e.cardCode == b.cardCode && e.isAR == false) select e.docTotal).Sum(),
                                            TotalIncomingPayments = (from e in invoices.data where (e.cardCode == b.cardCode) select e.paymentsDraft).Sum(),
                                          

                                        }).GroupBy(y => y.CardCode).Select(i => i.FirstOrDefault()).ToList();
                    }
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                var result = new { details = lstcustomers};

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                var result = new { error = "Error", errormsg = resulterror };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult Get_CustomersRoutePayments(string idcustomer)
        {
            try
            {
                var payments = Cls_payments.GetPayments(idcustomer,0,null,null,false);

                foreach (var item in payments.data) {
                    var creditmemos = cls_invoices.GetInvoice(item.docEntryInv,0, false).data.Sum(c=>c.returnsDraft);
                    item.remarks = creditmemos.ToString();
                }

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                var result = new { details = payments.data };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                var result = new { error = "Error", errormsg = resulterror };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult Get_Paymentsinfo(int docenty)
        {
            try
            {
                var payments = Cls_payments.GetPayment(docenty);

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                var result = new { details = payments.data };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                var result = new { error = "Error", errormsg = resulterror };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult Transform_payment(int docentry, int docentryinvoice)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                var invoice = cls_invoices.GetInvoice(docentryinvoice, 0, false).data.FirstOrDefault();
                var payment = Cls_payments.GetPayment(docentry).data[0];

              
                    if (invoice != null)
                    {
                        PUT_Invoices_api newput = new PUT_Invoices_api();

                        if (payment.paymentType == 2 || payment.paymentType == 3)
                        {

                        Payment_put newupdate = new Payment_put();
                        newupdate.hasIssue = 0;
                        newupdate.webStatus = 1; //Estado solo para cheques
                        newupdate.sumApplied = payment.sumApplied;
                        newupdate.paymentType = payment.paymentType;
                        newupdate.remarks = payment.remarks;
                        newupdate.journalRemarks = payment.journalRemarks;

                        switch (payment.paymentType)
                        {
                            case 1: //cash
                                newupdate.cash = new PaymentPost_cash();
                                newupdate.cash.cashAccount = payment.cash.cashAcct;
                                newupdate.cash.denominations = payment.cash.denominations;
                                break;
                            case 2://check
                                PaymentPost_check_moneyorder newcheck = new PaymentPost_check_moneyorder();
                                newcheck.bankCode = payment.check.bankCode;
                                newcheck.trnsfrable = payment.check.trnsfrable;
                                newcheck.checkAccount = payment.check.checkAct;
                                newcheck.endorse = payment.check.endorse;
                                newcheck.fullName = payment.check.fullName;
                                newcheck.checkNumber = payment.check.checkNum;

                                newupdate.check = newcheck;

                                break;
                            case 3: //money order
                                newupdate.moneyOrder = new PaymentPost_check_moneyorder();
                                newupdate.moneyOrder.bankCode = payment.moneyOrder.bankCode;
                                newupdate.moneyOrder.trnsfrable = payment.moneyOrder.trnsfrable;
                                newupdate.moneyOrder.checkAccount = payment.moneyOrder.checkAct;
                                newupdate.moneyOrder.endorse = payment.moneyOrder.endorse;
                                newupdate.moneyOrder.fullName = payment.moneyOrder.fullName;
                                newupdate.moneyOrder.checkNumber = payment.moneyOrder.checkNum;
                                break;
                            case 4://credit card
                                newupdate.creditCard = new PaymentPost_creditCard();
                                newupdate.creditCard.voucherNum = payment.creditCard.voucherNum;
                                newupdate.creditCard.transaction = payment.creditCard.transaction;
                                break;
                            case 5: //ach
                                newupdate.ach = new PaymentPost_ach_wire();
                                newupdate.ach.fullName = payment.ach.fullName;
                                newupdate.ach.transferAccount = payment.ach.trsfrAcct;
                                newupdate.ach.idTransaction = "-";
                                break;
                            case 6: //wire
                                newupdate.wire = new PaymentPost_ach_wire();
                                newupdate.wire.fullName = payment.wire.fullName;
                                newupdate.wire.transferAccount = payment.wire.trsfrAcct;
                                newupdate.wire.idTransaction = "-";
                                break;

                        }


                        var responsefac = Cls_payments.PutPayment(newupdate, docentry);

                        var response = Cls_payments.Transform_payment(docentry, activeuser.ID_User);


                        newput.stateSd = 5; //for funds
                            var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                        }

                    else {
                        var response = Cls_payments.Transform_payment(docentry, activeuser.ID_User);

                        if (response.IsSuccessful == true)
                        {
                            var payments = Cls_payments.GetPayments("", docentryinvoice, null, null, false).data;
                            if (payments != null && payments.Count > 0) //Verificamos que hayan mas pagos que realizar
                            {

                                newput.stateSd = 3;
                                var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                            }
                            else
                            {
                                var paymentsOriginals = Cls_payments.GetPaymentsOriginals("", docentryinvoice, null, null, false).data;

                                if (paymentsOriginals != null && paymentsOriginals.Count > 0)
                                {
                                    var issue = false;
                                    foreach (var doc in paymentsOriginals)
                                    {
                                        if (doc.hasIssue == 1)
                                        {
                                            issue = true;
                                        }
                                    }

                                    if (issue == true)
                                    {
                                        newput.stateSd = 4; //Cambiamos estado, PERO no hacemos nada porque se sigue investigando
                                    }
                                    else
                                    {
                                        if ((invoice.docTotal - invoice.returns) == invoice.paymentsDraft)
                                        {
                                            newput.stateSd = 9;
                                            var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                        }
                                        else
                                        {
                                            newput.stateSd = 6;
                                            var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                        }
                                    }

                                }
                                else
                                {
                                    if ((invoice.docTotal - invoice.returns) == invoice.paymentsDraft)
                                    {
                                        newput.stateSd = 9;
                                        var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                    }
                                    else
                                    {
                                        newput.stateSd = 6;
                                        var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                    }
                                }


                            }
                        }
                        else {
                            if (response.Content.Contains("cliente es  inactivo"))
                            {
                                var result2 = "Inactive Customer, please contact Finance department";
                                return Json(result2, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var result2 = "Error";
                                return Json(result2, JsonRequestBehavior.AllowGet);
                            }
                          
                        }


                        }


                  
                       

                    }

                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                
     


            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Delete_Payment(int docentry, int docentryinvoice)
        {
            try
            {
                var response = Cls_payments.DeletePayment(docentry);

                if (response.IsSuccessful == true)
                {
                    //Verificamos si tiene otros pagos pendientes o si era el ultimo
                    //Si tiene pagos pendientes no se hace nada
                    //Si NO tiene pagos pendientes, se coloca en waiting for payment
                    var payments = Cls_payments.GetPayments("", docentryinvoice, null, null, false).data;
                    PUT_Invoices_api newput = new PUT_Invoices_api();
                    if (payments != null && payments.Count > 0) //Verificamos que hayan mas pagos que realizar
                    {//Si hay, NO hacemos nada ya que se mantiene el estado Waiting for finantials
                    }
                    else
                    {
                        //Si NO hay pasa a Waiting for payment
                        newput.stateSd = 6;
                        var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                    }

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


        public ActionResult Put_payment(int docentry, Payment_putNew Item)
        {
            try
            {
                if (Item != null)
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                    var payment = Cls_payments.GetPayment(docentry).data[0];
                    //En lugar de actualizar, eliminaremos el draft y crearemos otro
                    /*
                                public int docEntry { get; set; }
            public int series { get; set; }
            public int invoiceType { get; set; }
            public DateTime docDate { get; set; }
            public DateTime docDueDate { get; set; }
            public DateTime vatDate { get; set; }
            public DateTime taxDate { get; set; }
            public string cardCode { get; set; }
            public int paymentType { get; set; }
            public decimal sumApplied { get; set; }
            public string userWeb { get; set; }
            public string remarks { get; set; }
            public string journalRemarks { get; set; }
                    */
                    Payment_post newupdate = new Payment_post();
                    newupdate.series = 17;
                    newupdate.userWeb = "0";
                    if (activeuser != null) { newupdate.userWeb = activeuser.ID_User.ToString(); }
                    newupdate.cardCode = payment.cardCode;
                    newupdate.docDate = payment.docDate;
                    newupdate.docDueDate = payment.docDueDate;
                    newupdate.taxDate = payment.taxDate;
                    newupdate.vatDate = payment.vatDate;
                    newupdate.invoiceType = 1;
                    newupdate.docEntry = payment.docEntryInv;
                    newupdate.sumApplied = payment.sumApplied;
                    newupdate.paymentType = Convert.ToInt32(Item.paymentType);
                    newupdate.remarks = payment.remarks;
                    newupdate.journalRemarks = payment.journalRemarks;

                    switch (Item.paymentType)
                    {
                        case 1: //cash
                            PaymentPost_cash newcash = new PaymentPost_cash();
                            newcash.cashAccount = "10112002";
                            newcash.denominations = "";
                            newupdate.cash = newcash;
                            break;
                        case 2://check
                            PaymentPost_check_moneyorder newcheck = new PaymentPost_check_moneyorder();
                            newcheck.bankCode = Item.check.bankCode;
                            newcheck.trnsfrable = false;
                            newcheck.checkAccount = "10112003";
                            newcheck.endorse = false;
                            newcheck.fullName = Item.check.fullName;
                            newcheck.checkNumber = Item.check.checkNumber;

                            newupdate.check = newcheck;

                            break;
                        case 3: //money order
                            newupdate.moneyOrder = new PaymentPost_check_moneyorder();
                            newupdate.moneyOrder.bankCode = Item.moneyOrder.bankCode;
                            newupdate.moneyOrder.trnsfrable = false;
                            newupdate.moneyOrder.checkAccount = "10112003";
                            newupdate.moneyOrder.endorse = false;
                            newupdate.moneyOrder.fullName = Item.moneyOrder.fullName;
                            newupdate.moneyOrder.checkNumber = Item.moneyOrder.checkNumber;
                            break;
                        case 4://credit card
                            newupdate.creditCard = new PaymentPost_creditCard();
                            newupdate.creditCard.voucherNum = Item.creditCard.voucherNum;
                            newupdate.creditCard.transaction = "";
                           
                            break;
                        case 5: //ach
                            newupdate.ach = new PaymentPost_ach_wire();
                            newupdate.ach.fullName = Item.ach.fullName;
                            newupdate.ach.transferAccount = "10112002";
                            newupdate.ach.idTransaction = "-";
                            break;
                        case 6: //wire
                            newupdate.wire = new PaymentPost_ach_wire();
                            newupdate.wire.fullName = Item.wire.fullName;
                            newupdate.wire.transferAccount = "10112002";
                            newupdate.wire.idTransaction = "-";
                            break;

                    }
                    ////Evaluamos lo anterior
                    //switch (payment.paymentType)
                    //{
                    //    case 1: //cash
                    //        PaymentPost_cash newcash = new PaymentPost_cash();
                    //        newcash.cashAccount = payment.cash.cashAcct;
                    //        newcash.denominations = payment.cash.denominations;
                    //        newupdate.cash = newcash;
                    //        break;
                    //    case 2://check
                    //        PaymentPost_check_moneyorder newcheck = new PaymentPost_check_moneyorder();
                    //        newcheck.bankCode = payment.check.bankCode;
                    //        newcheck.trnsfrable = payment.check.trnsfrable;
                    //        newcheck.checkAccount = payment.check.checkAct;
                    //        newcheck.endorse = payment.check.endorse;
                    //        newcheck.fullName = payment.check.fullName;
                    //        newcheck.checkNumber = payment.check.checkNum;

                    //        newupdate.check = newcheck;

                    //        break;
                    //    case 3: //money order
                    //        newupdate.moneyOrder = new PaymentPost_check_moneyorder();
                    //        newupdate.moneyOrder.bankCode = payment.moneyOrder.bankCode;
                    //        newupdate.moneyOrder.trnsfrable = payment.moneyOrder.trnsfrable;
                    //        newupdate.moneyOrder.checkAccount = payment.check.checkAct;
                    //        newupdate.moneyOrder.endorse = payment.moneyOrder.endorse;
                    //        newupdate.moneyOrder.fullName = payment.moneyOrder.fullName;
                    //        newupdate.moneyOrder.checkNumber = payment.moneyOrder.checkNum;
                    //        break;
                    //    case 4://credit card
                    //        newupdate.creditCard = new PaymentPost_creditCard();
                    //        newupdate.creditCard.voucherNum = payment.creditCard.voucherNum;
                    //        newupdate.creditCard.transaction = payment.creditCard.transaction;
                    //        break;
                    //    case 5: //ach
                    //        newupdate.ach = new PaymentPost_ach_wire();
                    //        newupdate.ach.fullName = Item.ach.fullName;
                    //        newupdate.ach.transferAccount = payment.ach.trsfrAcct;
                    //        newupdate.ach.idTransaction = "-";
                    //        break;
                    //    case 6: //wire
                    //        newupdate.wire = new PaymentPost_ach_wire();
                    //        newupdate.wire.fullName = payment.wire.fullName;
                    //        newupdate.wire.transferAccount = payment.wire.trsfrAcct;
                    //        newupdate.wire.idTransaction = "-";
                    //        break;

                    //}

                    //Eliminamos draft anterior
                    if (payment.remarks == null) { newupdate.remarks = "-"; }
                    if (payment.journalRemarks == null) { newupdate.journalRemarks = ""; }

                    //Creamos nuevo draft
                    var responsefac = Cls_payments.Postpayment(newupdate);



                    if (responsefac.IsSuccessful == true)
                    {
                        var respondedel = Cls_payments.DeletePayment(payment.docEntry);
                        var result = "SUCCESS";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result2 = "Error";
                        return Json(result2, JsonRequestBehavior.AllowGet);
                    }

                }
                else {
                    var result2 = "Error";
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }
        //CHEQUES
        public ActionResult Validate_payment(int docentry, int docentryinvoice, int status)
        {
            try
            {

                var payment = Cls_payments.GetPaymentOriginal(docentry).data[0];


                if (payment != null)
                {
                    Payment_putOriginal newupdate = new Payment_putOriginal();
                    newupdate.hasIssue = 0;
                    newupdate.webStatus = status;
                    newupdate.remarks = payment.remarks;
                    newupdate.journalRemarks = payment.journalRemarks;

                   
                    var responsefac = Cls_payments.PutPaymentOriginal(newupdate, docentry);

                    if (status == 2)
                    {//Verificamos si no hay mas pagos pendientes y cerramos, sino enviamos 3(finanzas)
                        var payments = Cls_payments.GetPayments("", docentryinvoice, null, null, false).data;
                        var paymentsOriginales = Cls_payments.GetPaymentsOriginals("", docentryinvoice, null, null, false).data;
                        if (payments != null && payments.Count > 0) //Verificamos que hayan mas pagos draft que realizar
                        {
                            if (paymentsOriginales ==null || paymentsOriginales.Count<1)
                            {
                                PUT_Invoices_api newput = new PUT_Invoices_api(); //hay pagos draft pendientes

                                newput.stateSd = 3;
                                var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                            }
                            else
                            {
                                    var allisok = true;
                                    foreach(var paymentor in paymentsOriginales)
                                    {
                                        if (paymentor.webStatus == 1) {
                                            allisok = false;
                                        }
                                        
                                    }

                                    if (allisok)
                                    {
                                        PUT_Invoices_api newput = new PUT_Invoices_api(); //hay pagos draft pendientes
                                        if (payments != null && payments.Count > 0)
                                        {
                                            newput.stateSd = 3;
                                        }
       
                                            
                                        var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                    }
                            }



                        }
                        else
                        {
                            //No hay pagos DRAFT

                            //Evaluar pagos originales

                            if (paymentsOriginales != null && paymentsOriginales.Count > 0)
                            {
                                var allisok = true;
                                foreach (var paymentor in paymentsOriginales)
                                {
                                    if (paymentor.webStatus == 1)
                                    {
                                        allisok = false;
                                    }

                                }

                                if (allisok)
                                {
                                    PUT_Invoices_api newput = new PUT_Invoices_api(); //no hay pagos originales pendientes
                                    newput.stateSd = 9;
                                    var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                                }
                            }
                            else {

                                PUT_Invoices_api newput = new PUT_Invoices_api(); //no hay pagos originales pendientes

                                newput.stateSd = 9;
                                var response2 = cls_invoices.PutInvoice(docentryinvoice, newput);
                            }

                  
                        }

                    }
                    else if (status == 3) {
                        //No hay fondos - Estado 6 y eliminamos payment



                        //Actualizamos factura
                        PUT_Invoices_api newput = new PUT_Invoices_api();
                        newput.stateSd = 6;
                        var response3 = cls_invoices.PutInvoice(docentryinvoice, newput);

                        var response4 = Cls_payments.DeletePaymentOriginal(docentry);
                    }


                }

                var result = "SUCCESS";

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult addJournalEntry(int docentry, int docentryinvoice, decimal amount, string cardcode)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //solicitamos pago
                var payment = Cls_payments.GetPayment(docentry).data[0];
   
                //Actualizamos pago
                Payment_put newupdate = new Payment_put();
                newupdate.hasIssue = 1;
                newupdate.webStatus = 0;
                newupdate.sumApplied = payment.sumApplied - amount;
                newupdate.paymentType = payment.paymentType;
                newupdate.remarks = payment.remarks;
                newupdate.journalRemarks = payment.journalRemarks;
  
                switch (payment.paymentType) {
                    case 1: //cash
                        newupdate.cash = new PaymentPost_cash();
                        newupdate.cash.cashAccount = "10142002";
                        newupdate.cash.denominations = payment.cash.denominations;
                        break;
                    case 2://check
                        PaymentPost_check_moneyorder newcheck = new PaymentPost_check_moneyorder();
                        newcheck.bankCode = payment.check.bankCode;
                        newcheck.trnsfrable = payment.check.trnsfrable;
                        newcheck.checkAccount = "10142002";
                        newcheck.endorse = payment.check.endorse;
                        newcheck.fullName = payment.check.fullName;
                        newcheck.checkNumber = payment.check.checkNum;

                        newupdate.check = newcheck;

                        break;
                    case 3: //money order
                        newupdate.moneyOrder = new PaymentPost_check_moneyorder();
                        newupdate.moneyOrder.bankCode = payment.moneyOrder.bankCode;
                        newupdate.moneyOrder.trnsfrable = payment.moneyOrder.trnsfrable;
                        newupdate.moneyOrder.checkAccount = "10142002";
                        newupdate.moneyOrder.endorse = payment.moneyOrder.endorse;
                        newupdate.moneyOrder.fullName = payment.moneyOrder.fullName;
                        newupdate.moneyOrder.checkNumber = payment.moneyOrder.checkNum;
                        break;
                    case 4://credit card
                        newupdate.creditCard = new PaymentPost_creditCard();
                        newupdate.creditCard.voucherNum = payment.creditCard.voucherNum;
                        newupdate.creditCard.transaction = payment.creditCard.transaction;
                        break;
                    case 5: //ach
                        newupdate.ach = new PaymentPost_ach_wire();
                        newupdate.ach.fullName = payment.ach.fullName;
                        newupdate.ach.transferAccount = "10142002";
                        newupdate.ach.idTransaction = "-";
                        break;
                    case 6: //wire
                        newupdate.wire = new PaymentPost_ach_wire();
                        newupdate.wire.fullName = payment.wire.fullName;
                        newupdate.wire.transferAccount = "10142002";
                        newupdate.wire.idTransaction = "-";
                        break;

                }

                //Actualizamos DRAFT
                var responsefac = Cls_payments.PutPayment(newupdate, docentry);
                //Validamos que saldos cuadren, sino haremos un rollback
                //Solicitamos invoice
                var invoice = cls_invoices.GetInvoice(docentryinvoice, 0, false).data.FirstOrDefault();

                if (invoice.balance - invoice.paymentsDraft == amount)
                {
                    //Agregamos partida
                    Journal_api newjournal = new Journal_api();
                    newjournal.cardCode = cardcode;
                    newjournal.docDate = DateTime.Now;
                    newjournal.indicator = "SD";
                    newjournal.comments = "Add Journal from SD";
                    newjournal.docEntryInv = docentryinvoice;
                    newjournal.journalTotal = amount;
                    newjournal.reconcileBp = true;
                    Journals_details credito = new Journals_details();
                    credito.accountOrCardCode = cardcode;
                    credito.credit = amount;
                    credito.debit = 0;
                    Journals_details debito = new Journals_details();
                    debito.accountOrCardCode = "10111001";
                    debito.credit = 0;
                    debito.debit = amount;
                    newjournal.details = new List<Journals_details>();
                    newjournal.details.Add(credito);
                    newjournal.details.Add(debito);

                    var response = Cls_payments.Transform_payment(docentry, activeuser.ID_User);


                    //Si todo salio bien, podemos seguir el proceso
                    if (response.IsSuccessful == true)
                    {
                        //Creamos partida
                        var response2 = cls_Journals.PostJournal(newjournal);

                        if (response2.IsSuccessful == true)
                        {

                            //Actualizamos factura
                            PUT_Invoices_api newput = new PUT_Invoices_api();
                            newput.stateSd = 4;
                            var response3 = cls_invoices.PutInvoice(docentryinvoice, newput);



                            var result = "SUCCESS";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

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
                else {

                    newupdate.hasIssue = 0;
                    newupdate.webStatus = 0;
                    newupdate.sumApplied = newupdate.sumApplied + amount;
                    //Actualizamos DRAFT
                    var respwhenfail = Cls_payments.PutPayment(newupdate, docentry);

                    var result = "Saldosnocuadran";
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