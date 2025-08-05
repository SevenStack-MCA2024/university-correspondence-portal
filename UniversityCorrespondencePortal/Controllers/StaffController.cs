using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;

namespace UniversityCorrespondencePortal.Controllers
{
    public class StaffController : Controller
    {
        private readonly UcpDbContext db = new UcpDbContext();

        // GET: Staff/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Staff/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var staff = db.Staffs.FirstOrDefault(s =>
                s.Email == email && s.PasswordHash == password && s.IsActive
            );

            if (staff != null)
            {
                Session["StaffID"] = staff.StaffID;
                Session["StaffName"] = staff.Name;
                Session["StaffEmail"] = staff.Email;

                return RedirectToAction("Letter", "Staff"); // Change to your actual dashboard
            }

            ViewBag.Error = "Invalid email, password, or your account is inactive.";
            return View();
        }

        public ActionResult Letter(string search, string senderDept, string receiverDept, string priority, DateTime? fromDate, DateTime? toDate)
        {
            int currentStaffId = Convert.ToInt32(Session["StaffID"]);

            // Inward Letters: staff is assigned through LetterStaff
            var inwardLetters = db.InwardLetters
                .Where(l => l.LetterStaffs.Any(ls => ls.StaffID == currentStaffId))
                .Include(l => l.LetterStaffs.Select(ls => ls.Staff))
                .AsQueryable();

            // Outward Letters: staff is sender
            var outwardLetters = db.OutwardLetters
                .Where(o => o.StaffID == currentStaffId)
                .Include(o => o.Staff)
                .AsQueryable();

            // Apply filters to Inward
            if (!string.IsNullOrEmpty(search))
            {
                inwardLetters = inwardLetters.Where(l =>
                    l.InwardNumber.Contains(search) ||
                    l.OutwardNumber.Contains(search) ||
                    l.SenderName.Contains(search) ||
                    l.SenderDepartment.Contains(search) ||
                    l.ReferenceID.Contains(search) ||
                    l.Subject.Contains(search) ||
                    l.Remarks.Contains(search));
            }

            if (!string.IsNullOrEmpty(senderDept))
                inwardLetters = inwardLetters.Where(l => l.SenderDepartment == senderDept);

            if (!string.IsNullOrEmpty(receiverDept))
                inwardLetters = inwardLetters.Where(l => l.ReceiverDepartment == receiverDept);

            if (!string.IsNullOrEmpty(priority))
                inwardLetters = inwardLetters.Where(l => l.Priority == priority);

            if (fromDate.HasValue)
                inwardLetters = inwardLetters.Where(l => l.DateReceived >= fromDate.Value);

            if (toDate.HasValue)
                inwardLetters = inwardLetters.Where(l => l.DateReceived <= toDate.Value);

            var inwardList = inwardLetters.ToList().Select(l => new StaffLetterViewModel
            {
                Type = "Inward",
                LetterID = l.LetterID,
                InwardNumber = l.InwardNumber,
                OutwardNumber = l.OutwardNumber,
                DateReceived = l.DateReceived,
                DeliveryMode = l.DeliveryMode,
                SenderDepartment = l.SenderDepartment,
                SenderName = l.SenderName,
                ReferenceID = l.ReferenceID,
                Subject = l.Subject,
                Remarks = l.Remarks,
                Priority = l.Priority,
                ReceiverDepartment = l.ReceiverDepartment,
                AssignedStaffNames = l.LetterStaffs.Select(ls => ls.Staff.Name).ToList()
            }).ToList();

            // Apply same filters to Outward
            if (!string.IsNullOrEmpty(search))
            {
                outwardLetters = outwardLetters.Where(o =>
                    o.OutwardNumber.Contains(search) ||
                    o.SenderDepartment.Contains(search) ||
                    o.ReferenceID.Contains(search) ||
                    o.Subject.Contains(search) ||
                    o.Remarks.Contains(search));
            }

            if (!string.IsNullOrEmpty(senderDept))
                outwardLetters = outwardLetters.Where(o => o.SenderDepartment == senderDept);

            if (!string.IsNullOrEmpty(receiverDept))
                outwardLetters = outwardLetters.Where(o => o.ReceiverDepartments.Contains(receiverDept));

            if (!string.IsNullOrEmpty(priority))
                outwardLetters = outwardLetters.Where(o => o.Priority == priority);

            if (fromDate.HasValue)
                outwardLetters = outwardLetters.Where(o => o.DateReceived >= fromDate.Value);

            if (toDate.HasValue)
                outwardLetters = outwardLetters.Where(o => o.DateReceived <= toDate.Value);

            var outwardList = outwardLetters.ToList().Select(o => new StaffLetterViewModel
            {
                Type = "Outward",
                LetterID = o.OutwardLetterID,
                InwardNumber = "", // Not applicable
                OutwardNumber = o.OutwardNumber,
                DateReceived = o.DateReceived,
                DeliveryMode = o.DeliveryMode,
                SenderDepartment = o.SenderDepartment,
                SenderName = o.Staff != null ? o.Staff.Name : "",
                ReferenceID = o.ReferenceID,
                Subject = o.Subject,
                Remarks = o.Remarks,
                Priority = o.Priority,
                ReceiverDepartment = o.ReceiverDepartments,
                AssignedStaffNames = new List<string> { o.Staff?.Name ?? "" }
            }).ToList();

            var allLetters = inwardList.Concat(outwardList)
                .OrderByDescending(l => l.DateReceived)
                .ToList();

            ViewBag.SenderDepartments = db.InwardLetters.Select(l => l.SenderDepartment).Distinct().ToList();
            ViewBag.ReceiverDepartments = db.InwardLetters.Select(l => l.ReceiverDepartment).Distinct().ToList();
            ViewBag.Priorities = db.InwardLetters.Select(l => l.Priority).Distinct().ToList();

            return View(allLetters);
        }






        public ActionResult Report()
        {
            int currentStaffId = Convert.ToInt32(Session["StaffID"]);

            var inwardLetters = db.InwardLetters
                .Where(l => l.LetterStaffs.Any(ls => ls.StaffID == currentStaffId))
                .ToList();

            var outwardLetters = db.OutwardLetters
                .Where(l => l.StaffID == currentStaffId)
                .ToList();

            var inwardGrouped = inwardLetters
                .Where(l => l.DateReceived.HasValue)
                .GroupBy(l => l.DateReceived.Value.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Year, g => g.Count);

            var outwardGrouped = outwardLetters
                .Where(l => l.DateReceived.HasValue)
                .GroupBy(l => l.DateReceived.Value.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Year, g => g.Count);

            var allYears = inwardGrouped.Keys.Union(outwardGrouped.Keys).OrderBy(y => y).ToList();

            var yearlyStats = allYears.Select(year => new YearlyLetterStats
            {
                Year = year,
                InwardCount = inwardGrouped.ContainsKey(year) ? inwardGrouped[year] : 0,
                OutwardCount = outwardGrouped.ContainsKey(year) ? outwardGrouped[year] : 0
            }).ToList();

            var viewModel = new StaffReportViewModel
            {
                TotalInward = inwardLetters.Count,
                TotalOutward = outwardLetters.Count,
                YearlyStats = yearlyStats
            };


            return View(viewModel);
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