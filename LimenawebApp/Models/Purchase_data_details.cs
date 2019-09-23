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
    
    public partial class Purchase_data_details
    {
        public int ID_detail { get; set; }
        public string ProdCodigo { get; set; }
        public string ProdNombre { get; set; }
        public string UnidadMedidaLetras { get; set; }
        public string Marca { get; set; }
        public string SubCategory { get; set; }
        public string Category { get; set; }
        public string ProvNombre { get; set; }
        public Nullable<decimal> InventarioEach { get; set; }
        public Nullable<decimal> InventarioCajas { get; set; }
        public int LeadTime { get; set; }
        public Nullable<decimal> U_TI { get; set; }
        public Nullable<decimal> U_HI { get; set; }
        public Nullable<decimal> U_PalletCount { get; set; }
        public Nullable<double> Promedio { get; set; }
        public Nullable<double> Desviacion { get; set; }
        public Nullable<double> Maximo { get; set; }
        public Nullable<double> Minimo { get; set; }
        public Nullable<double> PronosticoPeriodoActual { get; set; }
        public Nullable<double> PronosticoSiguiente1 { get; set; }
        public Nullable<double> PronosticoSiguiente2 { get; set; }
        public Nullable<double> PronosticoSiguiente3 { get; set; }
        public Nullable<double> PronosticoSiguiente4 { get; set; }
        public int ID_purchaseData { get; set; }
        public string Comentarios { get; set; }
        public Nullable<decimal> Descuento_allowancep { get; set; }
        public Nullable<decimal> Descuento_allowanced { get; set; }
        public Nullable<decimal> TendenciaPeriodoActual { get; set; }
        public Nullable<double> Promedio_AA { get; set; }
        public Nullable<double> VentaB1 { get; set; }
        public Nullable<double> VentaF1 { get; set; }
        public Nullable<double> CoberturaActual { get; set; }
        public Nullable<double> CoberturaProyectada { get; set; }
        public Nullable<double> CoberturaProyectadaNume { get; set; }
        public Nullable<double> CoberturaProyectadaDeno { get; set; }
        public Nullable<int> num { get; set; }
        public Nullable<double> FactorCompra { get; set; }
        public Nullable<double> FactorCompra_quiebre { get; set; }
        public Nullable<double> Politica_cobertura { get; set; }
        public Nullable<double> Variacion { get; set; }
        public Nullable<double> OTB { get; set; }
        public Nullable<double> Pedido { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public Nullable<double> PalletsdeOrden { get; set; }
        public Nullable<double> CoberturaIngresoPO { get; set; }
        public Nullable<double> Costo { get; set; }
        public Nullable<double> CostoconDescuento { get; set; }
        public Nullable<double> MontoPO { get; set; }
        public Nullable<double> Cobertura_OTB { get; set; }
        public Nullable<double> FactorUnidadCompra { get; set; }
        public Nullable<double> B1 { get; set; }
        public Nullable<double> B2 { get; set; }
        public Nullable<double> B3 { get; set; }
        public Nullable<double> B4 { get; set; }
        public Nullable<double> B5 { get; set; }
        public Nullable<double> InventarioIngresoPO { get; set; }
    
        public virtual Purchase_data Purchase_data { get; set; }
    }
}
