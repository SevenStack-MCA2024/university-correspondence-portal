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
    public class ClerkController : Controller
    {
        private readonly UcpDbContext db = new UcpDbContext();

        // ---------------------- LOGIN ----------------------
        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            var clerk = db.Clerks.FirstOrDefault(c => c.Email == username && c.IsActive);

            if (clerk != null && clerk.PasswordHash == password) // Plain-text password comparison
            {
                Session["ClerkID"] = clerk.ClerkID;
                Session["ClerkName"] = clerk.Name;
                Session["DepartmentID"] = clerk.DepartmentID;
                return RedirectToAction("InwardLetters");
            }

            ViewBag.Error = "Invalid username, password, or your account is inactive.";
            return View();
        }

        // ---------------------- FORGOT PASSWORD (OTP) ----------------------
        private static Dictionary<string, string> otpStore = new Dictionary<string, string>();

        [HttpPost]
        public JsonResult SendOtp(string email)
        {
            var clerk = db.Clerks.FirstOrDefault(c => c.Email == email && c.IsActive);
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
            var clerk = db.Clerks.FirstOrDefault(c => c.Email == email && c.IsActive);
            if (clerk == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            clerk.PasswordHash = password; // Save plain text
            db.SaveChanges();

            otpStore.Remove(email);
            return Json(new { success = true });
        }
        // ---------------------- PROFILE ----------------------
        // ---------------------- PROFILE ----------------------
        public ActionResult Profile()
        {
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

            string clerkId = Session["ClerkID"] as string;
            var clerk = db.Clerks.Find(clerkId);

            if (clerk == null)
                return HttpNotFound();

            var department = db.Departments.FirstOrDefault(d => d.DepartmentID == clerk.DepartmentID);
            ViewBag.DepartmentName = department != null ? department.DepartmentName : "N/A";

            return View(clerk);
        }
        //// ---------------------- REPORT ----------------------
        //public ActionResult Report()
        //{
        //    if (Session["ClerkID"] == null)
        //        return RedirectToAction("Login");

        //    return View();
        //}














        public ActionResult Report()
            {
                if (Session["ClerkID"] == null || Session["DepartmentID"] == null)
                    return RedirectToAction("Login");

                string departmentId = Session["DepartmentID"].ToString();
                var department = db.Departments.Find(departmentId);

                // Summary statistics
                ViewBag.DepartmentName = department?.DepartmentName ?? "Unknown Department";
                ViewBag.TotalStaff = db.StaffDepartments.Count(sd => sd.DepartmentID == departmentId);

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                ViewBag.LettersThisMonth = db.InwardLetters.Count(l =>
                    l.ReceiverDepartment == departmentId &&
                    l.DateReceived.Value.Month == currentMonth &&
                    l.DateReceived.Value.Year == currentYear) +
                    db.OutwardLetters.Count(o =>
                        o.DepartmentID == departmentId &&
                        o.DateReceived.Value.Month == currentMonth &&
                        o.DateReceived.Value.Year == currentYear);

                // Calculate average processing time (assuming completion date exists)
                var processedInward = db.InwardLetters
                    .Where(l => l.ReceiverDepartment == departmentId && l.DateReceived.HasValue)
                    .ToList();

                double avgDays = processedInward.Any() ?
                    processedInward.Average(l => (DateTime.Now - l.DateReceived.Value).TotalDays) : 0;
                ViewBag.AvgProcessingTime = Math.Round(avgDays, 1);

                ViewBag.PendingLetters = db.InwardLetters.Count(l =>
                    l.ReceiverDepartment == departmentId &&
                    l.LetterStaffs.Any() &&
                    !l.LetterStaffs.All(ls => /* Some completion condition */ false));

                // Letters by staff member
                var staffInDepartment = db.StaffDepartments
                    .Where(sd => sd.DepartmentID == departmentId)
                    .Select(sd => sd.Staff)
                    .ToList();

                ViewBag.StaffNames = staffInDepartment.Select(s => s.Name).ToList();
                ViewBag.LettersByStaff = staffInDepartment
                    .Select(s => s.LetterStaffs.Count(ls => ls.InwardLetter.ReceiverDepartment == departmentId))
                    .ToList();

                // Monthly data
                var months = Enumerable.Range(1, 12).Select(i =>
                    new DateTime(DateTime.Now.Year, i, 1).ToString("MMM")).ToList();
                ViewBag.Months = months;

                ViewBag.LettersByMonth = Enumerable.Range(1, 12).Select(i =>
                    db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId &&
                        l.DateReceived.Value.Month == i &&
                        l.DateReceived.Value.Year == DateTime.Now.Year) +
                    db.OutwardLetters.Count(o => o.DepartmentID == departmentId &&
                        o.DateReceived.Value.Month == i &&
                        o.DateReceived.Value.Year == DateTime.Now.Year)
                ).ToList();

                // Letter type percentages
                int totalInward = db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId);
                int totalOutward = db.OutwardLetters.Count(o => o.DepartmentID == departmentId);
                int totalLetters = totalInward + totalOutward;
                ViewBag.TotalInward = totalInward;
                ViewBag.TotalOutward = totalOutward;
                ViewBag.InwardPercentage = totalLetters > 0 ? (totalInward * 100 / totalLetters) : 0;
                ViewBag.OutwardPercentage = totalLetters > 0 ? (totalOutward * 100 / totalLetters) : 0;

                // Priority distribution
                ViewBag.PriorityLabels = new List<string> { "High", "Medium", "Low", "Urgent" };
                ViewBag.PriorityData = new List<int> {
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId && l.Priority == "High"),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId && l.Priority == "Medium"),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId && l.Priority == "Low"),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId && l.Priority == "Urgent")
        };

                // Status distribution (you'll need to implement status tracking)
                ViewBag.StatusLabels = new List<string> { "New", "In Progress", "Completed", "Pending" };
                ViewBag.StatusData = new List<int> {
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId /* && status == New */),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId /* && status == In Progress */),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId /* && status == Completed */),
            db.InwardLetters.Count(l => l.ReceiverDepartment == departmentId /* && status == Pending */)
        };

                // Sender departments
                var senderDepts = db.InwardLetters
                    .Where(l => l.ReceiverDepartment == departmentId)
                    .GroupBy(l => l.SenderDepartment)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToList();

                ViewBag.SenderDepartments = senderDepts.Select(g => g.Key).ToList();
                ViewBag.SenderDepartmentCounts = senderDepts.Select(g => g.Count()).ToList();

                // Processing time trends (last 6 months)
                var processingMonths = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i).ToString("MMM yyyy"))
                    .Reverse()
                    .ToList();

                ViewBag.ProcessingTimeMonths = processingMonths;
                ViewBag.ProcessingTimeData = Enumerable.Range(0, 6)
                    .Select(i => {
                        var month = DateTime.Now.AddMonths(-i).Month;
                        var year = DateTime.Now.AddMonths(-i).Year;
                        var letters = db.InwardLetters
                            .Where(l => l.ReceiverDepartment == departmentId &&
                                        l.DateReceived.Value.Month == month &&
                                        l.DateReceived.Value.Year == year)
                            .ToList();
                        return letters.Any() ?
                            letters.Average(l => (DateTime.Now - l.DateReceived.Value).TotalDays) : 0;
                    })
                    .Reverse()
                    .ToList();

            // Recent activity
            String departmentID = (string)(Session["DepartmentID"]);

            var inwardLetters = db.InwardLetters
                .Where(l => l.ReceiverDepartment == departmentId)
                .OrderByDescending(l => l.DateReceived)
                .Take(10)
                .Select(l => new InwardLetter
                {
                    LetterStaffs = l.LetterStaffs,
                    Priority = "High",
                    Subject = l.Subject,
                    SenderName = string.Join(", ", l.LetterStaffs.Select(ls => ls.Staff.Name)),
                    DateReceived = l.DateReceived,
                    InwardNumber = l.Priority ?? "High",
                    
                });

            var outwardLetters = db.OutwardLetters
                .Where(o => o.DepartmentID == departmentId)
                .OrderByDescending(o => o.DateReceived)
                .Take(10)
                .Select(o => new OutwardLetterSerialTracker
                {
                    
                    
              
                    DepartmentID = o.Staff != null ? o.Staff.Name : "Unassigned",
                  
                });

           

            return View();
            }
 
























        // ---------------------- LOGOUT ----------------------
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // =========================== STAFF MANAGEMENT ===========================
        public ActionResult Staff(string searchTerm, string designationFilter, string departmentFilter, string statusFilter, int? editId)
        {
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

            string clerkId = Session["ClerkID"].ToString();
            var clerk = db.Clerks.Find(clerkId);
            if (clerk == null)
                return RedirectToAction("Login");

            string clerkDeptId = clerk.DepartmentID;

            // ✅ Pass clerk's department info to view
            ViewBag.ClerkDepartmentID = clerk.DepartmentID;
            ViewBag.ClerkDepartmentName = db.Departments
                .Where(d => d.DepartmentID == clerk.DepartmentID)
                .Select(d => d.DepartmentName)
                .FirstOrDefault();

            // ✅ Base query: only staff belonging to the same department as the clerk
            var staffQuery = db.Staffs
                .Where(s => s.StaffDepartments.Any(sd => sd.DepartmentID == clerkDeptId))
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                staffQuery = staffQuery.Where(s =>
                    s.Name.Contains(searchTerm) ||
                    s.StaffID.ToString().Contains(searchTerm) ||
                    s.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(designationFilter))
            {
                staffQuery = staffQuery.Where(s => s.Designation == designationFilter);
            }

            if (!string.IsNullOrEmpty(departmentFilter))
            {
                staffQuery = staffQuery.Where(s => s.StaffDepartments.Any(sd => sd.Department.DepartmentName == departmentFilter));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                bool isActive = statusFilter == "Active";
                staffQuery = staffQuery.Where(s => s.IsActive == isActive);
            }

            var staffData = staffQuery
                .Select(s => new
                {
                    s.StaffID,
                    s.Name,
                    s.Email,
                    s.Phone,
                    s.Designation,
                    s.IsActive,
                    StaffDepartments = s.StaffDepartments.Select(sd => sd.Department.DepartmentName)
                })
                .ToList();

            var staffList = staffData
                .Select(s => new StaffViewModel
                {
                    StaffID = s.StaffID,
                    Name = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    Designation = s.Designation,
                    Departments = string.Join(", ", s.StaffDepartments),
                    IsActive = s.IsActive
                })
                .ToList();

            ViewBag.DesignationList = db.Staffs
                .Where(s => s.StaffDepartments.Any(sd => sd.DepartmentID == clerkDeptId))
                .Select(s => s.Designation)
                .Distinct()
                .ToList();

            ViewBag.DepartmentList = db.Departments
                .Where(d => d.DepartmentID == clerkDeptId)
                .ToList();

            ViewBag.StatusList = new List<string> { "Status(All)", "Active", "Inactive" };
            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.DesignationFilter = designationFilter;
            ViewBag.DepartmentFilter = departmentFilter;

            if (editId.HasValue)
            {
                TempData["EditID"] = editId.Value;
            }

            return View(staffList);
        }



        [HttpPost]
        public ActionResult CreateStaff(Staff staff, string DepartmentID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(staff.Email) && string.IsNullOrWhiteSpace(staff.Phone))
                {
                    TempData["Error"] = "Email or Phone is required.";
                    return RedirectToAction("Staff");
                }

                // 🔍 Check if the staff already exists by email OR phone
                var existingStaff = db.Staffs
                    .FirstOrDefault(s => s.Email == staff.Email || s.Phone == staff.Phone);

                if (existingStaff != null)
                {
                    // ✅ Found existing staff, now check if already linked to department
                    bool alreadyLinked = db.StaffDepartments.Any(sd =>
                        sd.StaffID == existingStaff.StaffID && sd.DepartmentID == DepartmentID);

                    if (!alreadyLinked)
                    {
                        // 🔗 Link existing staff to this new department
                        db.StaffDepartments.Add(new StaffDepartment
                        {
                            StaffID = existingStaff.StaffID,
                            DepartmentID = DepartmentID
                        });
                        db.SaveChanges();

                        TempData["Message"] = "Existing staff linked to this department.";
                    }
                    else
                    {
                        TempData["Error"] = "Staff already exists in this department.";
                    }
                }
                else
                {
                    // ✳️ Completely new staff → Add to Staff + StaffDepartment
                    staff.PasswordHash = "0000";
                    db.Staffs.Add(staff);
                    db.SaveChanges();

                    db.StaffDepartments.Add(new StaffDepartment
                    {
                        StaffID = staff.StaffID,
                        DepartmentID = DepartmentID
                    });
                    db.SaveChanges();

                    TempData["Message"] = "New staff added successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Staff");
        }

        [HttpPost]
        public ActionResult UpdateStaff(int StaffID, string Name, string Email, string Phone, string Designation)
        {
            try
            {
                var staff = db.Staffs.FirstOrDefault(s => s.StaffID == StaffID);
                if (staff == null)
                {
                    TempData["Error"] = "Staff not found.";
                    return RedirectToAction("Staff");
                }

                // 🔁 Check if new Email or Phone is already used by another staff
                var duplicateEmail = db.Staffs.Any(s => s.StaffID != StaffID && s.Email == Email);
                var duplicatePhone = db.Staffs.Any(s => s.StaffID != StaffID && s.Phone == Phone);

                if (duplicateEmail)
                {
                    TempData["Error"] = "Email is already used by another staff.";
                    return RedirectToAction("Staff");
                }

                if (duplicatePhone)
                {
                    TempData["Error"] = "Phone number is already used by another staff.";
                    return RedirectToAction("Staff");
                }

                // ✅ Update
                staff.Name = Name;
                staff.Email = Email;
                staff.Phone = Phone;
                staff.Designation = Designation;

                db.SaveChanges();
                TempData["Message"] = "Staff updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
            }

            return RedirectToAction("Staff");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStaffStatus(int staffId)
        {
            try
            {
                var staff = db.Staffs.Find(staffId);
                if (staff == null)
                {
                    TempData["Error"] = "Staff member not found";
                    return RedirectToAction("Staff"); // ✅ Fixed redirect
                }

                staff.IsActive = !staff.IsActive;
                db.SaveChanges();

                TempData["Message"] = $"Staff member {(staff.IsActive ? "activated" : "deactivated")} successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating staff status: " + ex.Message;
            }

            return RedirectToAction("Staff"); // ✅ Stay in Clerk's Staff panel
        }
        //Inward letter
        public ActionResult InwardLetters()
        {
            if (Session["ClerkID"] == null || Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            string clerkDept = Session["DepartmentID"].ToString();

            var letters = db.InwardLetters
                .Include(l => l.LetterStaffs.Select(ls => ls.Staff))
                .Where(l => l.ReceiverDepartment == clerkDept)
                .ToList();

            var viewModelList = letters.Select(l => new InwardLetterViewModel
            {
                LetterID = l.LetterID,
                InwardNumber = l.InwardNumber,
                OutwardNumber = l.OutwardNumber,
                DateReceived = l.DateReceived,
                TimeReceived = l.TimeReceived,
                DeliveryMode = l.DeliveryMode,
                SenderDepartment = l.SenderDepartment,
                SenderName = l.SenderName,
                ReferenceID = l.ReferenceID,
                Subject = l.Subject,
                Remarks = l.Remarks,
                Priority = l.Priority,
                ReceiverDepartment = l.ReceiverDepartment,
                StaffNames = string.Join(", ", l.LetterStaffs.Select(ls => ls.Staff.Name)),
                SelectedStaffIDs = l.LetterStaffs.Select(ls => ls.StaffID).ToList()
            }).ToList();

            ViewBag.Departments = db.Departments.ToList(); // for sender department dropdown
            ViewBag.AllStaff = db.Staffs.Where(s => s.StaffDepartments.Any(sd => sd.DepartmentID == clerkDept)).ToList();

            return View(viewModelList);
        }
        private string GenerateInwardNumber(string departmentId)
        {
            // Get the latest letter for this department
            var latest = db.InwardLetters
                .Where(l => l.ReceiverDepartment == departmentId)
                .OrderByDescending(l => l.LetterID)
                .FirstOrDefault();

            int nextNumber = 1;

            if (latest != null && !string.IsNullOrEmpty(latest.InwardNumber))
            {
                string[] parts = latest.InwardNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
                    nextNumber = lastNum + 1;
            }

            return $"{departmentId}-{nextNumber:D4}";
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInwardLetter(InwardLetterViewModel model)
        {
            if (Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            string deptId = Session["DepartmentID"].ToString();
            DateTime today = DateTime.Now;
            string todayFormatted = today.ToString("yyyy/MM/dd");

            // Get the existing tracker for this department
            var existingTracker = db.InwardLetterSerialTrackers
                .FirstOrDefault(t => t.DepartmentID == deptId);

            int nextSerial = 1;

            if (existingTracker != null && int.TryParse(existingTracker.LastSerialNumber, out int lastSerial))
            {
                nextSerial = lastSerial + 1;

                // 🧹 Delete the old tracker record
                db.InwardLetterSerialTrackers.Remove(existingTracker);
                db.SaveChanges();
            }

            string paddedSerial = nextSerial.ToString("D3");
            string inwardNumber = $"{deptId}-INW-{todayFormatted}-{paddedSerial}";

            var letter = new InwardLetter
            {
                InwardNumber = inwardNumber,
                OutwardNumber = model.OutwardNumber,
                DateReceived = today.Date,
                TimeReceived = today.TimeOfDay,
                DeliveryMode = model.DeliveryMode,
                SenderDepartment = model.SenderDepartment,
                SenderName = model.SenderName,
                ReferenceID = model.ReferenceID,
                Subject = model.Subject,
                Remarks = model.Remarks,
                Priority = model.Priority,
                ReceiverDepartment = deptId,
                LetterStaffs = new List<LetterStaff>()
            };

            if (model.SelectedStaffIDs != null)
            {
                foreach (int staffId in model.SelectedStaffIDs)
                {
                    letter.LetterStaffs.Add(new LetterStaff
                    {
                        StaffID = staffId
                    });
                }
            }

            db.InwardLetters.Add(letter);
            db.SaveChanges();

            // 🆕 Add the new tracker record
            var tracker = new InwardLetterSerialTracker
            {
                DepartmentID = deptId,
                Date = today,
                LastSerialNumber = paddedSerial,
                LetterID = letter.LetterID
            };

            db.InwardLetterSerialTrackers.Add(tracker);
            db.SaveChanges();

            var emailService = new EmailService();

            if (model.SelectedStaffIDs != null)
            {
                var staffList = db.Staffs
                    .Where(s => model.SelectedStaffIDs.Contains(s.StaffID))
                    .Select(s => new { s.Email, s.Name })
                    .ToList();

                foreach (var staff in staffList)
                {
                    string subject = "📨 New Inward Letter Assigned";
                    string body = $@"
                <p>Dear {staff.Name},</p>
                <p>You have been assigned a new inward letter.</p>
                <p>
                    <strong>Letter No:</strong> {inwardNumber}<br/>
                    <strong>Subject:</strong> {model.Subject}<br/>
                    <strong>Sender:</strong> {model.SenderDepartment} ({model.SenderName})<br/>
                    <strong>Reference ID:</strong> {model.ReferenceID}
                </p>
                <p>Please log in to the UCP portal to view the details.</p>
                <br/>
                <p>Regards,<br/>UCP System</p>
            ";

                    try
                    {
                        emailService.SendEmail(staff.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Some emails could not be sent: " + ex.Message;
                    }
                }
            }

            TempData["Message"] = "Inward Letter created and email sent.";
            return RedirectToAction("InwardLetters");
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateInwardLetter(InwardLetterViewModel model)
        {
            // Fetch the original letter and related staff mapping
            var letter = db.InwardLetters
                .Include(l => l.LetterStaffs)
                .FirstOrDefault(l => l.LetterID == model.LetterID);

            if (letter == null)
                return HttpNotFound();

            // ✅ Update only allowed editable fields
            letter.OutwardNumber = model.OutwardNumber;
            letter.SenderDepartment = model.SenderDepartment;
            letter.SenderName = model.SenderName;
            letter.ReferenceID = model.ReferenceID;
            letter.Remarks = model.Remarks;

            // ❌ Do NOT update fields like:
            // letter.InwardNumber
            // letter.DateReceived
            // letter.TimeReceived
            // letter.Subject
            // letter.Priority
            // letter.ReceiverDepartment
            // letter.DeliveryMode

            // ✅ Update LetterStaffs (remove old and add new)
            db.LetterStaffs.RemoveRange(letter.LetterStaffs);

            if (model.SelectedStaffIDs != null && model.SelectedStaffIDs.Any())
            {
                letter.LetterStaffs = model.SelectedStaffIDs.Select(staffId => new LetterStaff
                {
                    LetterID = letter.LetterID,
                    StaffID = staffId
                }).ToList();
            }

            db.SaveChanges();
            TempData["Message"] = "Inward Letter updated successfully.";

            return RedirectToAction("InwardLetters");
        }



        //outward letters
        [HttpGet]
        public ActionResult OutwardLetters()
        {
            if (Session["ClerkID"] == null || Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            string deptId = Session["DepartmentID"].ToString();

            var letters = db.OutwardLetters
                .Where(l => l.DepartmentID == deptId)
                .Include(l => l.Staff)
                .Include(l => l.Department)
                .OrderByDescending(l => l.OutwardLetterID)
                .ToList();

            var model = letters.Select(l => new OutwardLetterViewModel
            {
                OutwardLetterID = l.OutwardLetterID,
                OutwardNumber = l.OutwardNumber,
                LetterNumber = l.LetterNumber,
                DateReceived = l.DateReceived,
                TimeReceived = l.TimeReceived,
                DeliveryMode = l.DeliveryMode,
                ReferenceID = l.ReferenceID,
                Subject = l.Subject,
                Remarks = l.Remarks,
                Priority = l.Priority,
                SenderDepartment = l.SenderDepartment,
                ReceiverDepartments = l.ReceiverDepartments,
                ReceiverNames = l.ReceiverNames,
                StaffID = l.StaffID,
                StaffName = l.Staff != null ? l.Staff.Name : "",
                DepartmentID = l.DepartmentID,
                DepartmentName = l.Department != null ? l.Department.DepartmentName : ""
            }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOutwardLetter(OutwardLetterViewModel model)
        {
            if (Session["ClerkID"] == null || Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            string deptId = Session["DepartmentID"].ToString();

            // 🔍 Get Department details
            var dept = db.Departments.Find(deptId);
            if (dept == null)
                return RedirectToAction("OutwardLetters");

            // 🔁 Delete last outward letter and tracker for this department
            var lastLetter = db.OutwardLetters
                .Where(l => l.DepartmentID == deptId)
                .OrderByDescending(l => l.OutwardLetterID)
                .FirstOrDefault();

            if (lastLetter != null)
            {
                db.OutwardLetters.Remove(lastLetter);

                var lastTracker = db.OutwardLetterSerialTrackers
                    .FirstOrDefault(t => t.DepartmentID == deptId && t.LetterID == lastLetter.OutwardLetterID);

                if (lastTracker != null)
                    db.OutwardLetterSerialTrackers.Remove(lastTracker);

                db.SaveChanges();
            }

            // 🔢 Generate new serial
            string prefix = dept.DepartmentCode + "/OUT/";
            int newSerial = 1;

            var lastSerialTracker = db.OutwardLetterSerialTrackers
                .Where(t => t.DepartmentID == deptId)
                .OrderByDescending(t => t.TrackerID)
                .FirstOrDefault();

            if (lastSerialTracker != null && int.TryParse(lastSerialTracker.LastSerialNumber?.Split('/').Last(), out int parsed))
            {
                newSerial = parsed + 1;
            }

            string outwardNumber = prefix + newSerial.ToString("D3");

            // 📨 Create new OutwardLetter
            var newLetter = new OutwardLetter
            {
                OutwardNumber = outwardNumber,
                LetterNumber = model.LetterNumber,
                DateReceived = model.DateReceived,
                TimeReceived = model.TimeReceived,
                DeliveryMode = model.DeliveryMode,
                ReferenceID = model.ReferenceID,
                Subject = model.Subject,
                Remarks = model.Remarks,
                Priority = model.Priority,
                SenderDepartment = model.SenderDepartment,
                ReceiverDepartments = model.ReceiverDepartments,
                ReceiverNames = model.ReceiverNames,
                StaffID = model.StaffID,
                DepartmentID = deptId
            };

            db.OutwardLetters.Add(newLetter);
            db.SaveChanges(); // Save first to get newLetter ID

            // 📌 Create new tracker
            var newTracker = new OutwardLetterSerialTracker
            {
                DepartmentID = deptId,
                Date = DateTime.Now,
                LastSerialNumber = outwardNumber,
                LetterID = newLetter.OutwardLetterID
            };

            db.OutwardLetterSerialTrackers.Add(newTracker);
            db.SaveChanges();

            return RedirectToAction("OutwardLetters");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOutwardLetter(OutwardLetterViewModel model)
        {
            var letter = db.OutwardLetters.Find(model.OutwardLetterID);
            if (letter == null) return RedirectToAction("OutwardLetters");

            letter.LetterNumber = model.LetterNumber;
            letter.DateReceived = model.DateReceived;
            letter.TimeReceived = model.TimeReceived;
            letter.DeliveryMode = model.DeliveryMode;
            letter.ReferenceID = model.ReferenceID;
            letter.Subject = model.Subject;
            letter.Remarks = model.Remarks;
            letter.Priority = model.Priority;
            letter.SenderDepartment = model.SenderDepartment;
            letter.ReceiverDepartments = model.ReceiverDepartments;
            letter.ReceiverNames = model.ReceiverNames;
            letter.StaffID = model.StaffID;

            db.SaveChanges();
            return RedirectToAction("OutwardLetters");
        }


    }
}
