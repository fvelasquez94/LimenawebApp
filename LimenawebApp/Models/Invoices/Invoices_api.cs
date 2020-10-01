using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Invoices
{
    public class GetInvoices_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Invoices_api> data { get; set; }
    }

    public class Invoices_api
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
        public System.DateTime? deliveryDate { get; set; }
        public int slpCode { get; set; }
        public string slpName { get; set; }
        public decimal paymentsDraft { get; set; } //Total pagos $$ que se han realizado en la app
        public decimal returnsDraft { get; set; } //Total returns $$ que se han realizado en la app
        public decimal returns { get; set; }
        public int invoiceStatus { get; set; }
        public decimal balance { get; set; } //update 06/10/2020
        public Boolean isAR { get; set; }
        public System.DateTime expireDate { get; set; }  //Esta es la fecha que se tomara para saber si esta expirada una invoice
        public int extraDays { get; set; }
        public int expireDays { get; set; }
        public decimal balanceCustomer { get; set; }
        public int? idAuthorized { get; set; }
        public List<InvoiceDetails> details { get; set; }
    }

    public class InvoiceDetails
    {
        public string id_Detail { get; set; }
        public int docEntry { get; set; }
        public int docNum { get; set; }
        public string itemCode { get; set; }
        public string itemName { get; set; }
        public decimal quantity { get; set; }
        public decimal openQty { get; set; }
        public int uomEntry { get; set; }
        public string unitMsr { get; set; }
        public decimal numPerMsr { get; set; }
        public decimal priceBefDi { get; set; }
        public decimal discPrcnt { get; set; }
        public decimal unitPrice { get; set; }
        public decimal unitCost { get; set; }
        public decimal unitProfit { get; set; }
        public decimal totalSale { get; set; }
        public decimal totalCost { get; set; }
        public decimal totalProfit { get; set; }
        public string whsCode { get; set; }
        public string treeType { get; set; }
        public int lineNum { get; set; }
        public int visOrder { get; set; }
        public int baseType { get; set; }
        public int baseLine { get; set; }
        public int targetType { get; set; }
        public int? trgetEntry { get; set; }
        public string docSubType { get; set; }
        public decimal returnsDraftQty { get; set; }

    }

    public class PUT_Invoices_api
    {
        public string id_Delivery { get; set; }
        public System.DateTime? deliveryDate { get; set; }
        public string idDriver { get; set; }
        public string idHelper { get; set; }
        public string idCompany { get; set; }
        public string idTruck { get; set; }
        public string routeNumber { get; set; }
        public int stateSd { get; set; }
        public int idAuthorized { get; set; }

    }

}