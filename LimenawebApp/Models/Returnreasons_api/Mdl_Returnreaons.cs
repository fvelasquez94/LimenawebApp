using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Returnreasons_api
{
    public class GetReturnreason_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Returnreason_api> data { get; set; }
    }

    public class Returnreason_api
    {
        public string idReturnReason { get; set; }
        public string name { get; set; }
        public string reasonCode { get; set; }
        public string reason { get; set; }
        public string toWhs { get; set; }
        public string authorizedBy { get; set; }
        public string visibleFor { get; set; }
        public string active { get; set; }
    }
    public class Mdl_Returnreaons
    {
    }
}