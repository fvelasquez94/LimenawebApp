using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimenawebApp.Controllers
{
    public class FinanceController : Controller
    {

        public ActionResult ReceivePayment()
        {
            ViewData["Menu"] = "Finance";
            ViewData["Page"] = "Receive Payments";
            return View();
        }
    }
}