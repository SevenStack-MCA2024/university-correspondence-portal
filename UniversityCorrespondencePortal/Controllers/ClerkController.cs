using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UniversityCorrespondencePortal.Controllers
{
    public class ClerkController : Controller
    {
        // GET: /Clerk/Login
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