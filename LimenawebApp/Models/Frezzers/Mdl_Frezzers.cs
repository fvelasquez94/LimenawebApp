using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Frezzers
{
    public class GetFrezzers_api
    {
        public int? code { get; set; }
        public string message { get; set; }
        public List<Frezzers_api> data { get; set; }
    }
    public class GetFrezzer_api
    {
        public int? code { get; set; }
        public string message { get; set; }
        public Frezzers_api data { get; set; }
    }
    public class Frezzers_api
    {
        public string code { get; set; }
        public string name { get; set; }
        public string whs { get; set; }
        public string active { get; set; }
    }
    public class Mdl_Frezzers
    {
    }
}