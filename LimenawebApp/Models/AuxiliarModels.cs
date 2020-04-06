using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models
{
    public class HistorialVentas
    {
        public string CardCode { get; set; }
        public decimal Invoices { get; set; }
        public int SKUs { get; set; }
        public int PreviousPeriod { get; set; }

    }

    public class ActivitiesReport
    {
        public int ID_Activity { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public string Customer { get; set; }
        public string ID_Customer { get; set; }
        public string ID_Store { get; set; }
        public string Store { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string state { get; set; }
        public DateTime date { get; set; }
        public int ID_SalesRep { get; set; }
        public string SalesRep { get; set; }
        public string Category { get; set; }
        public string SubCategories { get; set; }
        


    }

    public class Inv_ProjectsTasks_mod
    {
        public int ID_projects_task { get; set; }
        public int ID_binloc { get; set; }
        public string Bin_location { get; set; }
        public System.DateTime creation_date { get; set; }
        public int ID_userEnd { get; set; }
        public string UserName { get; set; }
        public int ID_status { get; set; }
        public string comments { get; set; }
        public System.DateTime end_date { get; set; }
        public int Count { get; set; }
        public bool isselected { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UoM_code { get; set; }
        public int UoM_entry { get; set; }
        public decimal Quantity { get; set; }
        public string UoM_code2 { get; set; }
        public int UoM_entry2 { get; set; }
        public decimal Quantity2 { get; set; }
        public string UoM_code3 { get; set; }
        public int UoM_entry3 { get; set; }
        public decimal Quantity3 { get; set; }
        public string UoM_code4 { get; set; }
        public int UoM_entry4 { get; set; }
        public decimal Quantity4 { get; set; }
        public decimal Final_quantity { get; set; }
        public int ID_empresa { get; set; }
        public int ID_project { get; set; }
        public string Area { get; set; }
        public int Internal_sort { get; set; }
        public string Type { get; set; }
        public string Aisle { get; set; }
        public decimal unitcost { get; set; }
        public decimal stock { get; set; }
        public decimal casescost { get; set; }
        public decimal casesstock { get; set; }
    }


    public class ZipcodeStatesSupervisor
    {
        public int Supervisor { get; set; }
        public string StateCode { get; set; }
        public string State { get; set; }   
    }


    public class Pedidos_precios
    {
        public int LineNum { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public string UomCode { get; set; }
        public Nullable<int> UomEntry { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int DocNum { get; set; }
        public Nullable<System.DateTime> DocDate { get; set; }
        public string CardCode { get; set; }
        public Nullable<decimal> Price { get; set; }
        public decimal? NewPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? Total { get; set; }
        public decimal? FactorBonif { get; set; }
        public decimal? CantBonif { get; set; }
        public int? Estado { get; set; }
        public int? id_brand { get; set; }
        public int? id_subcategory { get; set; }
        public string subcategory_name { get; set; }
        public string category_name { get; set; }
        public string Brand_Name { get; set; }
        public string Bonificable { get; set; }
        public Boolean? deleted { get; set; }
    }

    public class BI_Dim_ProductsUOM
    {
        public string id { get; set; }
        public string Product { get; set; }
        public string Brand_Name { get; set; }
        public Nullable<int> id_subcategory { get; set; }
        public string subcategory_name { get; set; }
        public string id_category { get; set; }
        public string category_name { get; set; }
        public string Vendor { get; set; }
        public Nullable<decimal> LeadTime { get; set; }
        public string BuyUnitMsr { get; set; }
        public Nullable<decimal> NumInBuy { get; set; }
        public string Active { get; set; }
        public string Blocked { get; set; }
        public string Pepperi { get; set; }
        public string CatCommission { get; set; }
        public Nullable<decimal> CommPerc { get; set; }
        public string CodeBars { get; set; }
        public string DSD { get; set; }
        public Nullable<decimal> SRP { get; set; }
        public string id_Vendor { get; set; }
        public Nullable<int> id_brand { get; set; }
        public Nullable<decimal> unitCost { get; set; }
        public int MinPorcent { get; set; }
        public Nullable<decimal> MinPrice { get; set; }
        public string item_name { get; set; }
        public string Vendor_Name { get; set; }
        public string Bonificables { get; set; }
        public string MinPercIndiv { get; set; }
        public decimal CantBonif { get; set; }
        public decimal FactorBonif { get; set; }
        public string Credits { get; set; }
        public List<UomLstBIDIM> lstuom { get; set; }
    }

    public class UomLstBIDIM
    {
        public decimal? Units { get; set; }
        public string UomCode { get; set; }
    }

    public class Tb_CreditsRequestWithRoute
    {
        public int ID_creditRequest { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ReturnItemCode { get; set; }
        public string ReturnItemName { get; set; }
        public string UoMCode { get; set; }
        public string UoMEntry { get; set; }
        public string Request_reason { get; set; }
        public int Quantity { get; set; }
        public string URL_image { get; set; }
        public string URL_image2 { get; set; }
        public bool Validated { get; set; }
        public string DocNumSAP { get; set; }
        public int IDRequest_reason { get; set; }
        public System.DateTime dateCreate { get; set; }
        public System.DateTime dateValidate { get; set; }
        public int userValidate { get; set; }
        public string userValidateName { get; set; }
        public int userCreate { get; set; }
        public string userCreateName { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int estado { get; set; }
        public string Route { get; set; }
        public string comments { get; set; }
        public int LineNum { get; set; }
    }


    //Clase para Surveys

    public class SurveysTasks
    {
        public int ID_task { get; set; }
        public string ID_Customer { get; set; }
        public string Customer { get; set; }
        public System.DateTime visit_date { get; set; }
        public int? ID_userEnd { get; set; }
        public int? ID_userEndSAP { get; set; }
        public int ID_taskstatus { get; set; }
        public int ID_taskType { get; set; }
        public string TaskType { get; set; }
        public string Task_description { get; set; }

    }

    public class RepsSurveys {
        public int ID_User { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string prop01 { get; set; }
        public string prop02 { get; set; }
        public string idSAP { get; set; }
        public string idSAPsupervisor{ get; set; }
        public string classifications{ get; set; }

    }

    public class RepsCustomersbyProfile
    {
        public string Profile { get; set; }
        public string prop01 { get; set; }
        public string prop02 { get; set; }

    }


    public class QualityControl_SO {
        public int ID_Route { get; set; }
        public string Route_name { get; set; }
        public string ID_driver { get; set; }
        public string Driver_name { get; set; }
        public string ID_routeleader { get; set; }
        public string Routeleader_name { get; set; }
        public string ID_truck { get; set; }
        public string Truck_name { get; set; }
        public System.DateTime Departure { get; set; }
        public bool isfinished { get; set; }
        public string ID_SAPRoute { get; set; }
        public string query1 { get; set; }
        public string query2 { get; set; }
        public decimal query3 { get; set; }
        public string query4 { get; set; }
        public string query5 { get; set; }
        public decimal query6 { get; set; }
        public System.DateTime Date { get; set; }
        public bool Invoiced { get; set; }
        public System.DateTime DateCheckIn { get; set; }
        public System.DateTime DateCheckOut { get; set; }
        public int ID_userValidate { get; set; }
        public string Warehouse { get; set; }
        public int transferred { get; set; }
    }
}