using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SABLANCE.Controllers
{
    public class PolicyController : Controller
    {
        // GET: Policy
        public ActionResult Warranty()
        {
            return View();
        }
        public ActionResult Lookupwarranty()
        {
            return View();
        }
        public ActionResult Exchange()
        {
            return View();
        }
        public ActionResult Delivery()
        {
            return View();
        }
        public ActionResult GiftCard()
        {
            return View();
        }
        public ActionResult TermsOfUse()
        {
            return View();
        }

    }
}