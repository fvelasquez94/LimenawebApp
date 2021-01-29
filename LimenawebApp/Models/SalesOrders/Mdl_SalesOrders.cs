using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.SalesOrders
{
    public class Post_priceChange {
        public int docentry { get; set; }
        public List<details_priceChange> details { get; set; }
    }
    public class Delete_priceChange
    {
        public List<details_priceChangeDelete> details { get; set; }
    }
    public class details_priceChange {
        public int visorder { get; set; }
        public decimal price { get; set; }
    }
    public class details_priceChangeDelete
    {
        public int docEntry { get; set; }
        public int[] LineNumbers { get; set; }
    }
    public class Mdl_SalesOrders
    {

    }
    public class SendSOAPI
    {
        public int DocEntry { get; set; }
        public string IdDeliveryRoute { get; set; }
        public DateTime InvDate { get; set; }
        public string IdDriver { get; set; }
        public string IdHelper { get; set; }
        public string IdTruck { get; set; }
        public int RouteNumber { get; set; }
        public int StopNumber { get; set; }
    }

    public class GetSalesOrders_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<SalesOrders_api> data { get; set; }
    }

    public class SalesOrders_api
    {
        public int DocEntry { get; set; }
        public int docNum { get; set; }
        public string canceled { get; set; }
        public string printed { get; set; }
        public string docStatus { get; set; }
        public string objType { get; set; }
        public DateTime docDate { get; set; }
        public DateTime docDueDate { get; set; }
        public string cardCode { get; set; }
        public string numAtCard { get; set; }
        public decimal discPrcnt { get; set; }
        public decimal discSum { get; set; }
        public decimal docTotal { get; set; }
        public decimal paidToDate { get; set; }
        public decimal grosProfit { get; set; }
        public string ref1 { get; set; }
        public string comments { get; set; }
        public string jrnlMemo { get; set; }
        public int? docTime { get; set; }
        public int? slpCode { get; set; }
        public DateTime updateDate { get; set; }
        public DateTime createDate { get; set; }
        public decimal totalExpns { get; set; }
        public int? blocked { get; set; }
        public string idDeliveryRoute { get; set; }
        public DateTime invDate { get; set; }
        public string idDriver { get; set; }
        public string idHelper { get; set; }
        public string idCompany { get; set; }
        public string joined { get; set; }
        public string idTruck { get; set; }
        public string status { get; set; }
        public int? analyzed { get; set; }
        public string routeNumber { get; set; }
        public int? idAuthorized { get; set; }
        public string salesOrderType { get; set; }
        public string stateSd { get; set; }
        public int? stopNumber { get; set; }
        public List<SalesOrdersDetails_api> details { get; set; }
    }
    public class SalesOrdersDetails_api
    {
        public int docEntry { get; set; }
        public int lineNum { get; set; }
        public int targetType { get; set; }
        public int trgetEntry { get; set; }
        public string baseRef { get; set; }
        public int baseType { get; set; }
        public int baseEntry { get; set; }
        public int baseLine { get; set; }
        public string lineStatus { get; set; }
        public string itemCode { get; set; }
        public string dscription { get; set; }
        public decimal quantity { get; set; }
        public decimal openQty { get; set; }
        public decimal price { get; set; }
        public decimal discPrcnt { get; set; }
        public decimal lineTotal { get; set; }
        public string serialNum { get; set; }
        public string whsCode { get; set; }
        public int slpCode { get; set; }
        public string treeType { get; set; }
        public decimal priceBefDi { get; set; }
        public DateTime docDate { get; set; }
        public string codeBars { get; set; }
        public decimal grssProfit { get; set; }
        public int visOrder { get; set; }
        public string backOrdr { get; set; }
        public string freeTxt { get; set; }
        public decimal baseQty { get; set; }
        public string unitMsr { get; set; }
        public decimal numPerMsr { get; set; }
        public decimal stockPrice { get; set; }
        public string basePrice { get; set; }
        public decimal stockValue { get; set; }
        public int uomEntry { get; set; }
        public string uomCode { get; set; }
        public decimal invQty { get; set; }
        public string returnReason { get; set; }
        public string validated { get; set; }
        public decimal deliveredQty { get; set; }
    }

}