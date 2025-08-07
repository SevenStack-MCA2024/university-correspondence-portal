using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;
using UniversityCorrespondencePortal.Services;

namespace UniversityCorrespondencePortal.Controllers
{
    public class StaffController : Controller
    {
        private readonly UcpDbContext db = new UcpDbContext();

        // GET: Staff/Login
        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            email = email?.Trim();
            password = password?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var staff = db.Staffs.FirstOrDefault(s => s.Email == email && s.IsActive);

            // Plain text password check
            if (staff != null && staff.PasswordHash == password)
            {
                Session["StaffID"] = staff.StaffID;
                Session["StaffName"] = staff.Name;
                Session["StaffEmail"] = staff.Email;
                return RedirectToAction("Letter", "Staff");
            }

            ViewBag.Error = "Invalid email, password, or your account is inactive.";
            return View();
        }

        // OTP storage (static for simplicity)
        private static Dictionary<string, string> otpStore = new Dictionary<string, string>();

        [HttpPost]
        public JsonResult SendOtp(string email)
        {
            var staff = db.Staffs.FirstOrDefault(s => s.Email == email && s.IsActive);
            if (staff == null)
            {
                return Json(new { success = false, message = "Email not found or account inactive." });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            otpStore[email] = otp;

            OptService emailService = new OptService();
            emailService.SendOtpEmail(email, otp);

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult VerifyOtp(string email, string otp)
        {
            if (otpStore.ContainsKey(email) && otpStore[email] == otp)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid OTP." });
        }

        [HttpPost]
        public JsonResult UpdatePassword(string email, string password)
        {
            var clerk = db.Clerks.FirstOrDefault(c => c.Email == email && c.IsActive);
            if (clerk == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            clerk.PasswordHash = password; // Store plain text password
            db.SaveChanges();

            otpStore.Remove(email);
            return Json(new { success = true });
        }
        




















        public ActionResult GetReceiverDepartmentChart()
        {
            int currentStaffId = Convert.ToInt32(Session["StaffID"]);

            var receiverData = db.InwardLetters
                .Where(l => l.LetterStaffs.Any(ls => ls.StaffID == currentStaffId))
                .GroupBy(l => l.ReceiverDepartment)
                .Select(g => new
                {
                    ReceiverDepartment = g.Key,
                    LetterCount = g.Count()
                })
                .ToList();

            return Json(receiverData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Profile()
        {
            // 🔐 Replace with real authentication logic
            string loggedInEmail = Session["StaffEmail"]?.ToString();

            if (string.IsNullOrEmpty(loggedInEmail))
            {
                return RedirectToAction("Login", "Staff");
            }

            var staff = db.Staffs
                          .Where(s => s.Email == loggedInEmail)
                          .FirstOrDefault();

            if (staff == null)
            {
                return HttpNotFound();
            }

            var departmentNames = db.StaffDepartments
                .Where(sd => sd.StaffID == staff.StaffID)
                .Select(sd => sd.Department.DepartmentName)
                .ToList();

            var viewModel = new StaffProfileViewModel
            {
                StaffID = staff.StaffID,
                Name = staff.Name,
                Designation = staff.Designation,
                Email = staff.Email,
                Phone = staff.Phone,
                IsActive = staff.IsActive,
                Departments = departmentNames
            };

            return View(viewModel);
        }


    }
}