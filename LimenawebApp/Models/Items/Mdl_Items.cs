using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Items
{
    public class GetUOM_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<UOM_api> data { get; set; }
    }

    public class UOM_api
    {
        public string itemCode { get; set; }
        public int uomEntry { get; set; }
        public string uomCode { get; set; }
        public decimal units { get; set; }

    }
    public class GetItems_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Items_api> data { get; set; }
    }

    public class Items_api
    {
        public string itemCode { get; set; }
        public string descriptionLarge { get; set; }
        public string description { get; set; }
        public int idBrand { get; set; }
        public int idSubcategory { get; set; }
        public string idCategory { get; set; }
        public string idVendor { get; set; }
        public decimal leadTime { get; set; }
        public string buyUnitMsr { get; set; }
        public decimal numInBuy { get; set; }
        public string active { get; set; }
        public string blocked { get; set; }
        public string pepperi { get; set; }
        public string catCommission { get; set; }
        public decimal commPerc { get; set; }
        public string codeBars { get; set; }
        public string dsd { get; set; }
        public decimal srp { get; set; }
        public decimal unitCost { get; set; }
        public int minPorcent { get; set; }
        public decimal minPrice { get; set; }
        public string bonificables { get; set; }
        public string minPercIndiv { get; set; }
        public decimal cantBonif { get; set; }
        public decimal factorBonif { get; set; }
        public string credits { get; set; }
        public string storageType { get; set; }
        public decimal stock { get; set; }
        public decimal stockBonif { get; set; }
        public string uomGroup { get; set; }
        public string inventoryItem { get; set; }
        public decimal unitPrice { get; set; }

    }
}