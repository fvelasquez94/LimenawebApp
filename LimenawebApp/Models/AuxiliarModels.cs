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
        public int? Estado { get; set; }
    }
}