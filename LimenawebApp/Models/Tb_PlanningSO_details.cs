//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LimenawebApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tb_PlanningSO_details
    {
        public int ID_detail { get; set; }
        public int Line_num { get; set; }
        public string Bin_loc { get; set; }
        public int Quantity { get; set; }
        public string ID_UOM { get; set; }
        public string UOM { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string StockWhs01 { get; set; }
        public bool isvalidated { get; set; }
        public string ID_storagetype { get; set; }
        public string Storage_type { get; set; }
        public int ID_salesorder { get; set; }
        public string query1 { get; set; }
        public string query2 { get; set; }
        public string ID_picker { get; set; }
        public string Picker_name { get; set; }
    
        public virtual Tb_PlanningSO Tb_PlanningSO { get; set; }
    }
}
