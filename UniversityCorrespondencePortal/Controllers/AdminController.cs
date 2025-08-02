using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin/Login
        public ActionResult Login()
        {
            return View();
        }

    
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Dummy credentials
            if (username == "clerk" && password == "654321")
            {
                TempData["Message"] = "Login successful!";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }
    }
}