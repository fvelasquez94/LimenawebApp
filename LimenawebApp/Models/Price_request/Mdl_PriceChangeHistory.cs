using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Price_request
{
    public class Mdl_PriceChangeHistory
    {
        public class Help_BolsaUtilizada
        {

            public string id_Period { get; set; }
            public string Period_Name { get; set; }
            public int ID_user { get; set; }
            public string UserName { get; set; }
            public decimal? Utilizado { get; set; }
        }

        public class Bolsa_SalesR
        {
            public int ID_user { get; set; }
            public string ID_userSAP { get; set; }
            public string Username { get; set; }
            public decimal? Asignado { get; set; }
            public decimal? Utilizado { get; set; }
            public decimal? Disponible { get; set; }
        }


        public class PeriodoActivo
        {
            public string PeriodCode { get; set; }
            public string PeriodName { get; set; }
            public DateTime BeginDate { get; set; }
            public DateTime EndDate { get; set; }

        }

        public class PeriodoActivoSemana
        {
            public Int16 YearDLI { get; set; }
            public string PeriodCode { get; set; }
            public string PeriodName { get; set; }
            public DateTime PeriodBeginDate { get; set; }
            public DateTime PeriodEndDate { get; set; }
            public Int16 WeekDLI { get; set; }
            public DateTime WeekBeginDate { get; set; }
            public DateTime WeekEndDate { get; set; }

        }

        public class Help_bolsautilizada
        {
            public string id_Period { get; set; }
            public string Period_Name { get; set; }
            public int ID_user { get; set; }
            public string UserName { get; set; }
            public decimal Utilizado { get; set; }

        }
        public class Tb_AutorizacionExport
        {
            public int ID_detalle { get; set; }
            public int DocNum { get; set; }
            public string ItemCode { get; set; }
            public string Producto { get; set; }
            public string CodUOM { get; set; }
            public string UOM { get; set; }
            public decimal Cantidad { get; set; }
            public decimal PrecioPedido { get; set; }
            public decimal PrecioMin { get; set; }
            public decimal NuevoPrecio { get; set; }
            public decimal Resultado { get; set; }
            public string DocNumSAP { get; set; }
            public string PeriodCode { get; set; }
            public string PeriodName { get; set; }
            public Int16 WeekDLI { get; set; }
            public System.DateTime FechaIngreso { get; set; }
            public System.DateTime FechaValidacion { get; set; }
            public int LineNum { get; set; }
            public string UserName { get; set; }
            public string DocPepperi { get; set; }
            public string CodCustomer { get; set; }
            public string Customer { get; set; }
        }
    }
}