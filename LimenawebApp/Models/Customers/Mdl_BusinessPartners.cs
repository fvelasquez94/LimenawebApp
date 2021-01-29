using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Customers
{
    public class Mdl_BusinessPartners
    {
    }

    public class GetBusinessPartners_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<BusinessPartners_api> data { get; set; }
    }
    public class BusinessPartners_api
    {
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public string cardType { get; set; }
        public int? groupCode { get; set; }
        public string addres { get; set; }
        public string zipCode { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public decimal balance { get; set; }
        public int? groupNum { get; set; }
        public decimal creditLine { get; set; }
        public int? listNum { get; set; }
        public int? slpCode { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string e_Mail { get; set; }
        public string cardFName { get; set; }
        public DateTime createDate { get; set; }
        public DateTime updateDate { get; set; }
        public int? userSign { get; set; }
        public string validFor { get; set; }
        public string frozenFor { get; set; }
        public string state { get; set; }
        public int? shipType { get; set; }
        public int? supervisorCode { get; set; }
        public int? territory { get; set; }
        public int? series { get; set; }
        public decimal budget { get; set; }
        public string supportTrailerType { get; set; }
        public string loadingDock { get; set; }
        public string receivingHours { get; set; }
        public string receivingDays { get; set; }
        public string displayProducts { get; set; }
        public string deliveryPointStore { get; set; }
        public string barcodeRequerid { get; set; }
        public string salesDayWeekA { get; set; }
        public string salesDayWeekB { get; set; }
        public string preparationDayWeekA { get; set; }
        public string preparationDayWeekB { get; set; }
        public string deliveryDayWeekA { get; set; }
        public string deliveryDayWeekB { get; set; }
        public string idSalesRoute { get; set; }
        public string idDeliveryRoute { get; set; }
        public int? visitFrequency { get; set; }
        public string storeSize { get; set; }
        public int? timeWinStart { get; set; }
        public int? timeWinEnd { get; set; }
        public string warehouses { get; set; }
    }
}