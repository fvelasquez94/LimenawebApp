using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.SalesOrders
{
    public class Post_priceChange {
        public int docentry { get; set; }
        public List<details_priceChange> details { get; set; }
    }
    public class details_priceChange {
        public int visorder { get; set; }
        public decimal price { get; set; }
    }
    public class Mdl_SalesOrders
    {
       
    }
    public class SendSOAPI
    {
        public int DocEntry { get; set; }
        public string IdDeliveryRoute { get; set; }
        public DateTime InvDate { get; set; }
        public string IdDriver { get; set; }
        public string IdHelper { get; set; }
        public string IdTruck { get; set; }
        public int RouteNumber { get; set; }
        public int StopNumber { get; set; }
    }
}