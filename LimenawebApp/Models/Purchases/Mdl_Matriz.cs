using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Purchases
{
    public class Mdl_Matriz
    {

  
            public int num { get; set; } //GENERADO POR DATATABLE

            public string ProdCodigo { get; set; } //Matriz compra
            public string ProdNombre { get; set; } //fin Matriz compra

            public Nullable<double> FactorCompra { get; set; } //MICRO CATALOGO
            public Nullable<double> FactorUnidadCompra { get; set; } //MICRO CATALOGO
            public Nullable<double> FactorCompra_quiebre { get; set; } //MICRO CATALOGO
            public Nullable<double> Politica_cobertura { get; set; } //FIN MICRO CATALOGO

            public string Marca { get; set; } //Filtros
            public string SubCategory { get; set; }
            public string Category { get; set; }
            public string ProvCodigo { get; set; }
            public string ProvNombre { get; set; }//fin filtros

            public string UnidadMedidaLetras { get; set; } //UoM Group //Matriz compra
            public Nullable<decimal> InventarioEach { get; set; }
            public Nullable<decimal> InventarioCajas { get; set; }
            public Nullable<double> Promedio { get; set; }
            public Nullable<double> Desviacion { get; set; }
            public Nullable<double> Maximo { get; set; }
            public Nullable<double> Minimo { get; set; }
            public Nullable<double> PronosticoPeriodoActual { get; set; }
            public Nullable<double> Promedio_AA { get; set; } //Pronostico periodo actual AA
            public Nullable<double> VentaB1 { get; set; } //Periodo Anterior
            public Nullable<double> Variacion { get; set; }
            public Nullable<decimal> TendenciaPeriodoActual { get; set; }
            public Nullable<double> PronosticoSiguiente1 { get; set; }
            public Nullable<double> PronosticoSiguiente2 { get; set; }
            public Nullable<double> PronosticoSiguiente3 { get; set; }
            public Nullable<double> PronosticoSiguiente4 { get; set; }
            public Nullable<double> CoberturaActual { get; set; }
            public Nullable<double> CoberturaProyectada { get; set; }

            public Nullable<double> OTB { get; set; }
            public Nullable<double> Pedido { get; set; } //Orden a colocar
            public Nullable<DateTime> DeliveryDate { get; set; }
            public Nullable<DateTime> DocumentDate { get; set; }
            public Nullable<decimal> U_TI { get; set; } //TIER
            public Nullable<decimal> U_HI { get; set; } //HEIGHT
            public Nullable<decimal> U_PalletCount { get; set; } //CASES PER PALLET
            public Nullable<double> PalletsdeOrden { get; set; }
            public Nullable<double> CoberturaProyectadaNume { get; set; }
            public Nullable<double> InventarioIngresoPO { get; set; }
            public Nullable<double> CoberturaIngresoPO { get; set; }
            public Nullable<double> Costo { get; set; }
            public Nullable<decimal> DescuentoAp { get; set; } //DESCUENTO /ALLOWANCE (%)
            public Nullable<decimal> DescuentoAn { get; set; } // ... ($)
            public Nullable<double> CostoconDescuento { get; set; }
            public Nullable<double> MontoPO { get; set; }
            public string Comentarios { get; set; }
            //No se muestran pero se utilizane en formulas
            public Nullable<double> CoberturaProyectadaDeno { get; set; }
            public Nullable<double> VentaF1 { get; set; }
            public Nullable<double> Cobertura_OTB { get; set; }
            public int LeadTime { get; set; }
            public Nullable<int> transito { get; set; }

            public Nullable<double> B1 { get; set; }
            public Nullable<double> B2 { get; set; }
            public Nullable<double> B3 { get; set; }
            public Nullable<double> B4 { get; set; }
            public Nullable<double> B5 { get; set; }

        
    }
}