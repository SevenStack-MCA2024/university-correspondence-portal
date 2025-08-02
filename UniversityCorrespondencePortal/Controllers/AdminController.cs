using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Controllers
{
    public class AdminController : Controller
    {
        private UcpDbContext db = new UcpDbContext();

        // GET: Admin/Login
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Find the admin from the DB using email as username
            var admin = db.Admins.FirstOrDefault(a => a.Email == username);

            if (admin != null && admin.PasswordHash == password)
            {
                TempData["Message"] = $"Welcome {admin.Name}!";
                Session["AdminID"] = admin.AdminID; // Optional: store session
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult AddStaff()
        {
            return View();
        }

        public ActionResult AddDepartment()
        {
            return View();
        }

        public ActionResult AddClerk()
        {
            return View();
        }
        public ActionResult Report()
        {
            if (Session["AdminID"] == null)
                return RedirectToAction("Login");

            ViewBag.ReportMessage = "This is the Admin Reports page. Reports will be shown here.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Clear session data
            return RedirectToAction("Index", "Home"); // Redirect to portal home page
        }

        public ActionResult Profile()
        {
            string adminId = Session["AdminID"] as string;

            if (adminId == null)
                return RedirectToAction("Login");

            var admin = db.Admins.FirstOrDefault(a => a.AdminID == adminId);

            if (admin == null)
                return HttpNotFound();

            return View(admin);
        }

    }
}