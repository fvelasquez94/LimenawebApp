using LimenawebApp.Models.Invoices_api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Creditmemos_api
{
    public class Mdl_Creditmemos
    {
    }

    public class GetCreditmemos_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Creditmemos_api> data { get; set; }
    }

    public class Creditmemos_api
    {
        public int docEntry { get; set; }
        public int docNum { get; set; }
        public System.DateTime docDate { get; set; }
        public int docTime { get; set; }
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public decimal docTotal { get; set; }
        public string id_Route { get; set; }
        public string id_Truck { get; set; }
        public string truck { get; set; }
        public string id_Driver { get; set; }
        public string driver { get; set; }
        public string id_Helper { get; set; }
        public string id_Delivery { get; set; }
        public string deliveryRoute { get; set; }
        public System.DateTime deliveryDate { get; set; }
        public int slpCode { get; set; }
        public string slpName { get; set; }
        public int? docEntryInv { get; set; }
        public List<Creditmemos_Details> details { get; set; }
    }

    public class Creditmemos_Details : InvoiceDetails
    {
        //No pertenecen
        public int? baseEntry { get; set; }
        public string returnReasonCode { get; set; }
        public string returnReasonName { get; set; }
        public bool received { get; set; }
        public bool noShow { get; set; }
        public decimal? originalQty { get; set; }
    }

    public class CreditMemo_reconciliation {
        public string cardCode { get; set; }
     public int docNumInvoice { get; set; }
     public int   docNumCredit { get; set; }
       public decimal totalCredit{ get; set; }
}

    public class PostCreditmemos_api
    {
        public int docEntryInv { get; set; }
        public List<PostDetailsCreditmemos_api> details { get; set; }
    }
    public class PostDetailsCreditmemos_api
    {
        public string itemCode { get; set; }
        public decimal quantity { get; set; }
        public int uomEntry { get; set; }
        public string returnReasonCode { get; set; }
        public int baseLine { get; set; }
    }

    public class PutDetailsCreditmemos_api : PostDetailsCreditmemos_api
    {
        public int? lineNum { get; set; }
        public int? visOrder { get; set; }
        public bool deleted { get; set; }
        public bool received { get; set; }
        public bool noShow { get; set; }

    }
    public class PutDetailsCreditmemos_apiNew
    {
        public int quantity { get; set; }
        public int uomEntry { get; set; }
        public string returnReasonCode { get; set; }
        public bool received { get; set; }
        public bool changeOriginalQty { get; set; }

    }
    public class PutDetailsCreditmemos_apiNOSHOW
    {
        public string itemCode { get; set; }
        public int quantity { get; set; }
        public int uomEntry { get; set; }
        public string returnReasonCode { get; set; }
        public decimal price { get; set; }
        public int? visOrder { get; set; }

    }

}