using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Document_Approval_Project_BE.Models;

namespace Document_Approval_Project_BE.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }
    }
}
