using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models
{
    public class ResumeSO_DSD
    {
        public int ID_OrderDSD { get; set; }
        public string ID_customer { get; set; }
        public string CustomerName { get; set; }
        public string ID_payment { get; set; }
        public string Payment { get; set; }
        public string Doc_numP { get; set; }
        public string Doc_numCompany { get; set; }
        public string Date { get; set; }
        public int ID_User { get; set; }
        public string User_name { get; set; }
        public int ID_Company { get; set; }
        public string docNum_SAP { get; set; }
        public string Comment { get; set; }
        public string Sign { get; set; }
        public decimal Total { get; set; }
        public string Direction { get; set; }
    }
}