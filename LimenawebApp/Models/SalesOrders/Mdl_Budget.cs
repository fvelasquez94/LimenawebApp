using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.SalesOrders
{
    public class Mdl_Budget
    {
    }
    public class GetBudget_api
    {
        public int statusCode { get; set; }
        public int totalRecords { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public List<Budget_api> data { get; set; }
    }
    public class Budget_api
    {
        public string cardCode { get; set; }
        public decimal amount { get; set; }
        public int SlpCode { get; set; }
        public int week { get; set; }
        public int year { get; set; }
        public string period { get; set; }
        public string dayName { get; set; }
        public DateTime date { get; set; }
        public Boolean active { get; set; }
    }

    public class Budget_BP {
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public int time { get; set; }
        public decimal Budget { get; set; }
        public decimal Sales { get; set; }
        public decimal Scope { get; set; }
        public int SlpCode { get; set; }
        public string SlpName { get; set; }
    }
    public class Budget_BP_extra
    {
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public string VisitDay { get; set; }
        public int time { get; set; }
        public decimal Sales { get; set; }
        public int SlpCode { get; set; }
        public string SlpName { get; set; }

    }
}