using System;
using System.Collections.Concurrent;
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

        // GET: Staff/Login1
        public ActionResult Login(string email, string password)
        {
            try
            {
                email = email?.Trim();
                password = password?.Trim();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Please enter both your email and password to continue.";
                    return View();
                }

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
                    var fullStaff = db.Staffs.FirstOrDefault(s => s.StaffID == staffData.StaffID);

                    if (fullStaff != null && PasswordHelper.VerifyPassword(password, fullStaff.PasswordHash))
                    {
                        Session["StaffID"] = staffData.StaffID;
                        Session["StaffName"] = staffData.Name;
                        Session["StaffEmail"] = staffData.Email;

                        if (staffData.MustResetPassword)
                        {
                            TempData["Info"] = "Your account requires a password reset before logging in.";
                            return RedirectToAction("ResetPassword", "Staff");
                        }

                        return RedirectToAction("InwardLetter", "Staff");
                    }
                }

                ViewBag.Error = "Incorrect email or password. Please try again.";
                return View();
            }
            catch (Exception)
            {
                // You can log `ex` for debugging, but don't show details to the user
                ViewBag.Error = "Something went wrong while logging you in. Please try again in a few minutes.";
                return View();
            }
        }


        private static Dictionary<string, string> otpStore = new Dictionary<string, string>();

        [HttpPost]
        public JsonResult SendOtp(string email)
        {
            var clerk = db.Staffs.FirstOrDefault(c => c.Email == email && c.IsActive);
            if (clerk == null)
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
            var clerk = db.Staffs.FirstOrDefault(c => c.Email == email && c.IsActive);
            if (clerk == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            clerk.PasswordHash = PasswordHelper.HashPassword(password); // Save plain text
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




        

        public ActionResult InwardLetter()
        {
            if (Session["StaffID"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            int loggedInStaffID = Convert.ToInt32(Session["StaffID"]);

            // Get all DepartmentIDs assigned to logged-in staff
            var staffDepartmentIds = db.StaffDepartments
                                       .Where(sd => sd.StaffID == loggedInStaffID)
                                       .Select(sd => sd.DepartmentID)
                                       .ToList();

            // Fetch InwardLetters where ReceiverDepartment (which stores DepartmentID) matches any staff's department
            var inwardLetters = db.InwardLetters
                                  .Where(il => staffDepartmentIds.Contains(il.ReceiverDepartment))
                                  .ToList();

            // Map to ViewModel
            var model = inwardLetters.Select(il => new UniversityCorrespondencePortal.Models.ViewModels.InwardLetterViewModel
            {
                LetterID = il.LetterID,
                InwardNumber = il.InwardNumber,
                OutwardNumber = il.OutwardNumber,
                DateReceived = il.DateReceived,
                TimeReceived = il.TimeReceived,
                DeliveryMode = il.DeliveryMode,
                SenderDepartment = il.SenderDepartment,
                SenderName = il.SenderName,
                ReferenceID = il.ReferenceID,
                Subject = il.Subject,
                Remarks = il.Remarks,
                Priority = il.Priority,
                ReceiverDepartment = il.ReceiverDepartment  // This holds DepartmentID
            }).ToList();

            return View(model);
        }


        public ActionResult OutwardLetter()
        {
            if (Session["StaffID"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            int loggedInStaffID = Convert.ToInt32(Session["StaffID"]);
            var loggedInStaff = db.Staffs.FirstOrDefault(s => s.StaffID == loggedInStaffID);
            if (loggedInStaff == null)
            {
                return HttpNotFound();
            }

            string staffName = loggedInStaff.Name;

            var outwardLetters = db.OutwardLetters
                .Where(ol =>ol.SenderName == staffName)
                .Select(ol => new StaffOutwardLetterViewModel
                {
                    Type = "Outward",
                    LetterID = ol.LetterID,
                    OutwardNumber = ol.OutwardNumber,
                    DateSent = ol.Date,
                    DeliveryMode = ol.DeliveryMode,
                    SenderDepartment = ol.SenderDepartment,
                    SenderName = ol.SenderName,
                    ReceiverDepartment = ol.ReceiverDepartment,
                    ReceiverName = ol.ReceiverName,
                    ReferenceID = ol.ReferenceID,
                    Subject = ol.Subject,
                    Remarks = ol.Remarks,
                    Priority = ol.Priority
                })
                .ToList();

            return View(outwardLetters);
        }


    }
}