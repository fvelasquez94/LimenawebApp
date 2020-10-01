using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Payments
{
    public class Mdl_Payments
    {
    }
    public class GetPayments_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Payments_api> data { get; set; }
    }
    public class GetPayments_apiv2
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Payments_apiv2> data { get; set; }
    }
    public class Payments_api
    {
        public int docEntry { get; set; }
        public int docNum { get; set; }
        public int? series { get; set; }
        public int? invoiceType { get; set; }
        public DateTime docDate { get; set; }
        public DateTime docDueDate { get; set; }
        public DateTime vatDate { get; set; }
        public DateTime taxDate { get; set; }
        public DateTime CreateTime { get; set; }
        public int? docTime { get; set; }
        public string cardCode { get; set; }
        public int docEntryInv { get; set; }
        public int docNumInv { get; set; }
        public decimal docTotalInv { get; set; }
        public int? paymentType { get; set; }
        public decimal sumApplied { get; set; }
        public string userWeb { get; set; }
        public int? idRoute { get; set; }
        public string remarks { get; set; }
        public string journalRemarks { get; set; }
        public int? hasIssue { get; set; }
        public int? webStatus { get; set; }
        public PaymentMethod_cash cash { get; set; }
        public PaymentMethod_check_moneyorder check { get; set; }
        public PaymentMethod_check_moneyorder moneyOrder { get; set; }
        public PaymentMethod_creditCard creditCard { get; set; }
        public PaymentMethod_ach_wire ach { get; set; }
        public PaymentMethod_ach_wire wire { get; set; }

    }


    public class Payments_apiv2
    {
        public int docEntry { get; set; }
        public int docNum { get; set; }
        public DateTime docDate { get; set; }
        public DateTime createDate { get; set; }
        public int? docTime { get; set; }
        public string cardCode { get; set; }
        public int docEntryInv { get; set; }
        public int? paymentType { get; set; }
        public decimal? sumApplied { get; set; }
        public string userWeb { get; set; }
        public int? idRoute { get; set; }
        public int? hasIssue { get; set; }
        public int? webStatus { get; set; }
        public string canceled { get; set; }
        public int? appliedBy { get; set; }
        public Invoices_api_forpayments invoice { get; set; }
        public PaymentMethod_cash cash { get; set; }
        public PaymentMethod_check_moneyorder check { get; set; }
        public PaymentMethod_check_moneyorder moneyOrder { get; set; }
        public PaymentMethod_creditCard creditCard { get; set; }
        public PaymentMethod_ach_wire ach { get; set; }
        public PaymentMethod_ach_wire wire { get; set; }

    }

    public class PaymentMethod_cash
    {
        public int docEntry { get; set; }
        public string cashAcct { get; set; }
        public decimal cashSum { get; set; }
        public string denominations { get; set; }
        /*
         [{
  "id": n,
  "Denomination: n,
  "Value": n
}]
         */
    }

    public class PaymentMethod_check_moneyorder
    {
        public int docEntry { get; set; }
        public int checkNum { get; set; }
        public string bankCode { get; set; }
        public string countryCod { get; set; }
        public Boolean trnsfrable { get; set; }
        public string checkAct { get; set; }
        public Boolean endorse { get; set; }
        public string fullName { get; set; }
        public string currency { get; set; }
        public decimal checkSum { get; set; }
    }

    public class PaymentMethod_creditCard
    {
        public int docEntry { get; set; }
        public int creditCard { get; set; }
        public string creditAcct { get; set; }
        public DateTime cardValid { get; set; }
        public string voucherNum { get; set; }
        public string transaction { get; set; }
        public string cardNumber { get; set; }
        public decimal creditSum { get; set; }
        public string currency { get; set; }
        public int methodCode { get; set; }
    }
    public class PaymentMethod_ach_wire
    {
        public int docEntry { get; set; }
        public string trsfrAcct { get; set; }
        public decimal trsfrSum { get; set; }
        public string trsfrRef { get; set; }
        public string transfType { get; set; }
        public string fullName { get; set; }
    }

    public class Payments_routesmain
    {
        public decimal? TotalAmount { get; set; }
        public decimal? TotalIncomingPayments { get; set; }
        public decimal? TotalAR { get; set; }
        public decimal? TotalAR_paid { get; set; }
        public string Id_Truck { get; set; }
        public string Id_Driver { get; set; }
        public string Id_Delivery { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public string Truck { get; set; }
        public string Driver { get; set; }
        public string DeliveryRoute { get; set; }
        public int TotalOrders { get; set; }
        public int OrdersF { get; set; } //finalizadas
        public int OrdersP { get; set; } //pendientes
        public int OrdersC { get; set; } //canceladas
    }
    public class Payments_customersmain
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int? SlpCode { get; set; }
        public string SlpName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalIncomingPayments { get; set; }
        public decimal? TotalAR { get; set; }
        public decimal? TotalAR_paid { get; set; }
        public string Id_Truck { get; set; }
        public string Id_Driver { get; set; }
        public string Id_Delivery { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public string Truck { get; set; }
        public string Driver { get; set; }
        public string DeliveryRoute { get; set; }
        public int TotalOrders { get; set; }
        public int OrdersF { get; set; } //finalizadas
        public int OrdersP { get; set; } //pendientes
        public int OrdersC { get; set; } //canceladas
    }

    public class Drivers_customersmain
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int? SlpCode { get; set; }
        public string SlpName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalIncomingPayments { get; set; }
        public decimal? TotalAR { get; set; }
        public decimal? TotalAR_paid { get; set; }
        public decimal? TotalReturns { get; set; }
        public string Id_Truck { get; set; }
        public string Id_Driver { get; set; }
        public string Id_Delivery { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public string Truck { get; set; }
        public string Driver { get; set; }
        public string DeliveryRoute { get; set; }
        public int TotalOrders { get; set; }
        public int OrdersF { get; set; } //finalizadas
        public int OrdersP { get; set; } //pendientes
        public int OrdersC { get; set; } //canceladas
 
    }



    public class Invoices_api_forpayments
    {
        public int docEntry { get; set; }
        public int docNum { get; set; }
        public System.DateTime docDate { get; set; }
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public decimal docTotal { get; set; }
        public string id_Route { get; set; }
        public string docStatus { get; set; }
        public decimal paidToDate { get; set; }
        public string id_Truck { get; set; }
        public string truck { get; set; }
        public string id_Driver { get; set; }
        public string driver { get; set; }
        public string id_Helper { get; set; }
        public string id_Delivery { get; set; }
        public string deliveryRoute { get; set; }
        public System.DateTime deliveryDate { get; set; }
        public int? slpCode { get; set; }
        public string slpName { get; set; }
        public decimal paymentsDraft { get; set; } //Total pagos $$ que se han realizado en la app
        public decimal retursDraft { get; set; } //Total returns $$ que se han realizado en la app
        public decimal returns { get; set; }
        public int? invoiceStatus { get; set; }
        public decimal? balance { get; set; } //update 06/10/2020
        public Boolean isAR { get; set; }
        public System.DateTime expireDate { get; set; }  //Esta es la fecha que se tomara para saber si esta expirada una invoice
        public int? extraDays { get; set; }
        public int? expireDays { get; set; }
        public decimal? balanceCustomer { get; set; }
        public int? idAuthorized { get; set; }
  
    }
}