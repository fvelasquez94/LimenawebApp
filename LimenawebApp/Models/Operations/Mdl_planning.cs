using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Operations
{
    public class Mdl_planning
    {
        public class Routes_calendar
        {
            public string title { get; set; }
            public string url { get; set; }
            public string start { get; set; }
            public string route_leader { get; set; }
            public string className { get; set; }
            public string driver { get; set; }
            public string truck { get; set; }
            public string departure { get; set; }
            public string amount { get; set; }
            public string orderscount { get; set; }
            public string customerscount { get; set; }
            public string isfinished { get; set; }
            public string extra { get; set; }
            public string totalEach { get; set; }
            public string totalCase { get; set; }
            public string totalPack { get; set; }
            public string totalLbs { get; set; }
            public string AVGEach { get; set; }
            public string Warehouse { get; set; }
            public string driver_WHS { get; set; }
            public string truck_WHS { get; set; }
        }

        public class Routes_calendarPlanning
        {
            public string title { get; set; }
            public string url { get; set; }
            public DateTime start { get; set; }
            public string route_leader { get; set; }
            public string className { get; set; }
            public string driver { get; set; }
            public string truck { get; set; }
            public DateTime departure { get; set; }
            public decimal? amount { get; set; }
            public int orderscount { get; set; }
            public int customerscount { get; set; }
            public string isfinished { get; set; }
            public string extra { get; set; }
            public string totalEach { get; set; }
            public string totalCase { get; set; }
            public string totalPack { get; set; }
            public string totalLbs { get; set; }
            public string AVGEach { get; set; }
            public string Warehouse { get; set; }
            public string driver_WHS { get; set; }
            public string truck_WHS { get; set; }
        }

    }
}