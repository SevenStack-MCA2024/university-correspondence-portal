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
        public ActionResult Login(string email, string password)
        {
            email = email?.Trim();
            password = password?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            // Map SQL result directly into StaffViewModel
            var staffData = db.Database.SqlQuery<StaffViewModel>(
                @"SELECT TOP 1 
              StaffID, 
              Name, 
              Email, 
              Phone, 
              Designation, 
              IsActive, 
              MustResetPassword
          FROM Staffs 
          WHERE Email = @p0 AND IsActive = 1", email).FirstOrDefault();

            if (staffData != null)
            {
                // You now need to fetch the full Staff record to verify the hashed password
                var fullStaff = db.Staffs.FirstOrDefault(s => s.StaffID == staffData.StaffID);

                if (fullStaff != null && PasswordHelper.VerifyPassword(password, fullStaff.PasswordHash))
                {
                    Session["StaffID"] = staffData.StaffID;
                    Session["StaffName"] = staffData.Name;
                    Session["StaffEmail"] = staffData.Email;

                    if (staffData.MustResetPassword)
                    {
                        return RedirectToAction("ResetPassword", "Staff");
                    }

                    return RedirectToAction("Profile", "Staff");
                }
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
        [HttpGet]
        public ActionResult ResetPassword()
        {
            if (Session["StaffID"] == null)
                return RedirectToAction("Login", "Staff");

            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            if (Session["StaffID"] == null)
                return RedirectToAction("Login", "Staff");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match or are empty.";
                return View();
            }

            if (newPassword == "0000")
            {
                ViewBag.Error = "You cannot reuse the default password.";
                return View();
            }

            int staffId = Convert.ToInt32(Session["StaffID"]);
            string hashed = PasswordHelper.HashPassword(newPassword);

            // ✅ Update using SQL to avoid model changes
            db.Database.ExecuteSqlCommand(
                "UPDATE Staffs SET PasswordHash = @p0, MustResetPassword = 0 WHERE StaffID = @p1",
                hashed, staffId);

            TempData["Message"] = "Password updated successfully.";
            return RedirectToAction("Profile", "Staff");
        }


    }
}