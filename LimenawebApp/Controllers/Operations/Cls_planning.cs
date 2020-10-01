using LimenawebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static LimenawebApp.Models.Operations.Mdl_planning;

namespace LimenawebApp.Controllers.Operations
{
    public class Cls_planning
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        public List<Routes_calendarPlanning> GetRoutes(string Whs, DateTime fstartd, DateTime fendd)
        {
            List<Routes_calendarPlanning> actividades_calendar = new List<Routes_calendarPlanning>();
            var today = DateTime.Today;

            if (Whs == "" || Whs == null)
            {

                actividades_calendar = (from a in dblim.Tb_Planning
                                        where (a.Departure >= fstartd && a.Departure <= fendd)
                                        select new Routes_calendarPlanning
                                        {
                                            title = a.ID_Route + " - " + a.Route_name,
                                            url = "",
                                            start = a.Departure,
                                            //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd"),
                                            route_leader = a.Routeleader_name.ToUpper(),
                                            className = a.isfinished == true ? "block b-t b-t-2x b-success" : "block b-t b-t-2x b-warning",
                                            driver = a.Driver_name.ToUpper(),
                                            driver_WHS = a.Driver_name_whs.ToUpper(),
                                            truck = a.Truck_name,
                                            truck_WHS = a.Truck_name_whs,
                                            departure = a.Departure,
                                            Warehouse = a.Warehouse,
                                            extra = "0.0", //(from extra in dblim.Tb_Planning_extra where (extra.ID_Route == a.ID_Route) select extra.Value).Sum().ToString(),
                                            totalEach = "",
                                            totalCase = "",
                                            totalPack = "",
                                            totalLbs = "",
                                            AVGEach = "",
                                            isfinished = a.isfinished == true ? "Y" : "N",
                                            amount = a.Tb_PlanningSO.Select(c => c.Amount).Sum(),
                                            customerscount = a.Tb_PlanningSO.Select(c => c.Customer_name).Distinct().Count(),
                                            orderscount = a.Tb_PlanningSO.Count()
                                        }).ToList();
            }
            else 
            {
                actividades_calendar = (from a in dblim.Tb_Planning
                                        where (a.Departure >= fstartd && a.Departure <= fendd && a.Warehouse == Whs || a.query1.Contains("from"))
                                        select new Routes_calendarPlanning
                                        {
                                            title = a.ID_Route + " - " + a.Route_name,
                                            url = "",
                                            start = a.Departure,
                                            //rt.end = item.Departure.AddDays(1).ToString("yyyy-MM-dd"),
                                            route_leader = a.Routeleader_name.ToUpper(),
                                            className = a.isfinished == true ? "block b-t b-t-2x b-success" : "block b-t b-t-2x b-warning",
                                            driver = a.Driver_name.ToUpper(),
                                            driver_WHS = a.Driver_name_whs.ToUpper(),
                                            truck = a.Truck_name,
                                            truck_WHS = a.Truck_name_whs,
                                            departure = a.Departure,
                                            Warehouse = a.Warehouse,
                                            extra = "0.0", //(from extra in dblim.Tb_Planning_extra where (extra.ID_Route == a.ID_Route) select extra.Value).Sum().ToString(),
                                            totalEach = "",
                                            totalCase = "",
                                            totalPack = "",
                                            totalLbs = "",
                                            AVGEach = "",
                                            isfinished = a.isfinished == true ? "Y" : "N",
                                            amount = a.Tb_PlanningSO.Select(c => c.Amount).Sum(),
                                            customerscount = a.Tb_PlanningSO.Select(c => c.Customer_name).Distinct().Count(),
                                            orderscount = a.Tb_PlanningSO.Count()
                                        }).ToList();

            }


            return actividades_calendar;
        }
    }
}