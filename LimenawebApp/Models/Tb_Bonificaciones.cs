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
    
    public partial class Tb_Bonificaciones
    {
        public int ID_bonificacion { get; set; }
        public string CodPedido { get; set; }
        public string CodCliente { get; set; }
        public string Cliente { get; set; }
        public string CodProducto { get; set; }
        public string Producto { get; set; }
        public string ID_brand { get; set; }
        public string Brand { get; set; }
        public string CodUOM { get; set; }
        public string UOM { get; set; }
        public string CodVendedor { get; set; }
        public string Vendedor { get; set; }
        public decimal Cantidad { get; set; }
        public int Estado { get; set; }
        public string DocNum { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public System.DateTime FechaValidacion { get; set; }
        public string ID_Vendor { get; set; }
        public string Vendor { get; set; }
        public string CodUOMAnt { get; set; }
        public string UOMAnt { get; set; }
        public int TipoIngreso { get; set; }
        public decimal CantidadFinal { get; set; }
        public string LineNum { get; set; }
        public decimal Currency { get; set; }
        public decimal CurrencyFinal { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal PorcentajeFinal { get; set; }
        public int CodAutorizo { get; set; }
        public string Autorizo { get; set; }
        public bool OrderClosed { get; set; }
        public int CantidadPedido { get; set; }
        public string DocPepperi { get; set; }
        public string RutaDef { get; set; }
        public bool deleted { get; set; }
        public int closedOrderTimes { get; set; }
        public int DeletedSAP { get; set; }
    }
}
