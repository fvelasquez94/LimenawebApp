using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers
{
    public class FinanceController : Controller
    {
        // GET: Finance
        public ActionResult Authorizations()
        {
            ViewData["Menu"] = "Finance";
            ViewData["Page"] = "Authorizations for Drivers";
            return View();
        }

        public ActionResult ReceivePayment()
        {
            ViewData["Menu"] = "Finance";
            ViewData["Page"] = "Receive Payments";
            return View();
        }
    }
}