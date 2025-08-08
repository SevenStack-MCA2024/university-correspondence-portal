using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;
using UniversityCorrespondencePortal.Services;
using UniversityCorrespondencePortal.ViewModels;

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

            if (clerk != null && PasswordHelper.VerifyPassword(password, clerk.PasswordHash))
            {
                if (clerk.MustResetPassword)
                {
                    Session["ClerkID"] = clerk.ClerkID;
                    return RedirectToAction("ResetPassword", "Clerk");
                }

                Session["ClerkID"] = clerk.ClerkID;
                Session["ClerkName"] = clerk.Name;
                Session["DepartmentID"] = clerk.DepartmentID;
                return RedirectToAction("InwardLetters");
            }



            ViewBag.Error = "Invalid username, password, or your account is inactive.";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string clerkId = Session["ClerkID"]?.ToString();
            if (string.IsNullOrEmpty(clerkId))
                return RedirectToAction("Login");

            var clerk = db.Clerks.Find(clerkId);
            if (clerk == null)
                return RedirectToAction("Login");

            // Hash new password
            clerk.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            clerk.MustResetPassword = false; // ✅ no longer need to reset

            db.SaveChanges();

            TempData["Message"] = "Password reset successfully.";
            return RedirectToAction("InwardLetters");
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            // Optionally check session
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

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

        //public ActionResult OutwardLetter()
        //{
        //    if (Session["DepartmentID"] == null)
        //        return RedirectToAction("Login");

        //    string deptId = Session["DepartmentID"].ToString();
        //        var deptName = db.Departments
        //             .Where(d => d.DepartmentID == deptId)
        //             .Select(d => d.DepartmentName)
        //             .FirstOrDefault();

        //    ViewBag.SenderDepartment = deptName; ;


        //    // 📨 Fetch outward letters of the logged-in department
        //    var letters = db.OutwardLetters
        //        .Where(l => l.DepartmentID == deptId)
        //        .OrderByDescending(l => l.Date)
        //        .ToList();

        //    // 👥 Load staff of this department via StaffDepartments
        //    var staffList = db.StaffDepartments
        //        .Where(sd => sd.DepartmentID == deptId)
        //        .Select(sd => sd.Staff)
        //        .Distinct()
        //        .ToList();

        //    // 🏢 Load all departments
        //    var departmentList = db.Departments.ToList();

        //    // 📄 Prepare ViewModel
        //    var model = new OutwardLetterPageViewModel
        //    {
        //        NewLetter = new OutwardLetterViewModel
        //        {
        //            DepartmentID = deptId,
        //            SenderDepartment = Session["DepartmentName"]?.ToString()
        //        },
        //        OutwardLetters = letters
        //    };

        //    ViewBag.StaffList = staffList;
        //    ViewBag.DepartmentList = departmentList;

        //    return View("OutwardLetter", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateOutwardLetter(OutwardLetterViewModel model)
        //{
        //    if (Session["DepartmentID"] == null)
        //        return RedirectToAction("Login");

        //    if (!ModelState.IsValid)
        //        return RedirectToAction("OutwardLetter");

        //    string deptId = Session["DepartmentID"].ToString();
        //    string todayFormatted = DateTime.Now.ToString("yyyy/MM/dd");
        //    int nextSerial = 1;

        //    // 🔁 Get and update tracker
        //    var existingTracker = db.OutwardLetterSerialTrackers.FirstOrDefault(t => t.DepartmentID == deptId && t.Date == DateTime.Today);
        //    if (existingTracker != null && int.TryParse(existingTracker.LastSerialNumber, out int lastSerial))
        //    {
        //        nextSerial = lastSerial + 1;
        //        db.OutwardLetterSerialTrackers.Remove(existingTracker);
        //        db.SaveChanges();
        //    }

        //    string paddedSerial = nextSerial.ToString("D3");
        //    string outwardNumber = $"{deptId}-OUT-{todayFormatted}-{paddedSerial}";
        //    var staff = db.Staffs.FirstOrDefault(s => s.StaffID == model.AssignedStaffID);
        //    string senderName = staff != null ? staff.Name : "Unknown";


        //    // 📄 Create new outward letter
        //    var letter = new OutwardLetter
        //    {
        //        LetterNo = model.LetterNo,
        //        OutwardNumber = outwardNumber,
        //        Date = DateTime.Now.Date,
        //        Time = DateTime.Now.TimeOfDay,
        //        DeliveryMode = model.DeliveryMode,
        //        ReferenceID = model.ReferenceID,
        //        Subject = model.Subject,
        //        Remarks = model.Remarks,
        //        Priority = model.Priority,
        //        SenderDepartment = db.Departments
        // .Where(d => d.DepartmentID == deptId)
        // .Select(d => d.DepartmentName)
        // .FirstOrDefault(),
        //        SenderName = senderName,
        //        ReceiverDepartment = model.ReceiverDepartment == "Other" ? model.ReceiverDepartmentOther : model.ReceiverDepartment,
        //        ReceiverName = model.ReceiverName,
        //        DepartmentID = deptId
        //    };


        //    db.OutwardLetters.Add(letter);
        //    db.SaveChanges();

        //    // 📌 Save new tracker
        //    var newTracker = new OutwardLetterSerialTracker
        //    {
        //        DepartmentID = deptId,
        //        Date = DateTime.Today,
        //        LastSerialNumber = nextSerial.ToString()
        //    };
        //    db.OutwardLetterSerialTrackers.Add(newTracker);
        //    db.SaveChanges();

        //    // Save single staff
        //    db.OutwardLetterStaffs.Add(new OutwardLetterStaff
        //    {
        //        LetterID = letter.LetterID,
        //        StaffID = model.AssignedStaffID
        //    });
        //    db.SaveChanges();

        //    // Send email to that staff


        //    if (staff != null && !string.IsNullOrEmpty(staff.Email))
        //    {
        //        var emailService = new EmailService();
        //        string subject = "New Outward Letter Assigned";
        //        string body = $"Dear {staff.Name},<br/><br/>You have been assigned as the sender for Letter No: <strong>{letter.LetterNo}</strong>.<br/>Subject: {letter.Subject}<br/>Please take note.<br/><br/>Regards,<br/>University Portal";
        //        emailService.SendEmail(staff.Email, subject, body);
        //    }


        //    TempData["OutwardSuccess"] = true;
        //    return RedirectToAction("OutwardLetter");
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditOutwardLetter(OutwardLetter model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existing = db.OutwardLetters.Find(model.LetterID);
        //        if (existing != null)
        //        {
        //            existing.DeliveryMode = model.DeliveryMode;
        //            existing.ReferenceID = model.ReferenceID;
        //            existing.Remarks = model.Remarks;
        //            existing.ReceiverDepartment = model.ReceiverDepartment;
        //            existing.ReceiverName = model.ReceiverName;

        //            db.SaveChanges();
        //            TempData["OutwardSuccess"] = "Outward letter updated.";
        //        }
        //    }

        //    return RedirectToAction("OutwardLetter");
        //}

        //-----------------------------Outward Letter methods -------------------------------
        public ActionResult OutwardLetter()
        {
            if (Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            string deptId = Session["DepartmentID"].ToString();
            var deptName = db.Departments
                 .Where(d => d.DepartmentID == deptId)
                 .Select(d => d.DepartmentName)
                 .FirstOrDefault();

            ViewBag.SenderDepartment = deptName; ;


            // 📨 Fetch outward letters of the logged-in department
            var letters = db.OutwardLetters
                .Where(l => l.DepartmentID == deptId)
                .OrderByDescending(l => l.Date)
                .ToList();

            // 👥 Load staff of this department via StaffDepartments
            var staffList = db.StaffDepartments
                .Where(sd => sd.DepartmentID == deptId)
                .Select(sd => sd.Staff)
                .Distinct()
                .ToList();

            // 🏢 Load all departments
            var departmentList = db.Departments.ToList();

            // 📄 Prepare ViewModel
            var model = new OutwardLetterPageViewModel
            {
                NewLetter = new OutwardLetterViewModel
                {
                    DepartmentID = deptId,
                    SenderDepartment = Session["DepartmentName"]?.ToString()
                },
                OutwardLetters = letters
            };

            ViewBag.StaffList = staffList;
            ViewBag.DepartmentList = departmentList;

            return View("OutwardLetter", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateOutwardLetterInline(OutwardLetter updated)
        {
            if (updated == null || updated.LetterID <= 0)
                return Json(new { success = false, message = "Invalid data" });

            var letter = db.OutwardLetters.FirstOrDefault(l => l.LetterID == updated.LetterID);
            if (letter == null)
                return Json(new { success = false, message = "Letter not found" });

            // Update fields
            letter.LetterNo = updated.LetterNo;
            letter.SenderName = updated.SenderName;
            letter.ReceiverName = updated.ReceiverName;
            letter.ReceiverDepartment = updated.ReceiverDepartment;
            letter.ReferenceID = updated.ReferenceID;

            db.SaveChanges();

            return Json(new
            {
                success = true,
                data = new
                {
                    letter.LetterNo,
                    letter.SenderName,
                    letter.ReceiverName,
                    letter.ReceiverDepartment,
                    letter.ReferenceID
                }
            });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOutwardLetter(OutwardLetterViewModel model)
        {
            if (Session["DepartmentID"] == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return RedirectToAction("OutwardLetter");

            string deptId = Session["DepartmentID"].ToString();
            string todayFormatted = DateTime.Now.ToString("yyyy/MM/dd");
            int nextSerial = 1;

            // 🔁 Get and update tracker
            var existingTracker = db.OutwardLetterSerialTrackers.FirstOrDefault(t => t.DepartmentID == deptId && t.Date == DateTime.Today);
            if (existingTracker != null && int.TryParse(existingTracker.LastSerialNumber, out int lastSerial))
            {
                nextSerial = lastSerial + 1;
                db.OutwardLetterSerialTrackers.Remove(existingTracker);
                db.SaveChanges();
            }

            string paddedSerial = nextSerial.ToString("D3");
            string outwardNumber = $"{deptId}-OUT-{todayFormatted}-{paddedSerial}";
            var staff = db.Staffs.FirstOrDefault(s => s.StaffID == model.AssignedStaffID);
            string senderName = staff != null ? staff.Name : "Unknown";


            // 📄 Create new outward letter
            var letter = new OutwardLetter
            {
                LetterNo = model.LetterNo,
                OutwardNumber = outwardNumber,
                Date = DateTime.Now.Date,
                Time = DateTime.Now.TimeOfDay,
                DeliveryMode = model.DeliveryMode,
                ReferenceID = model.ReferenceID,
                Subject = model.Subject,
                Remarks = model.Remarks,
                Priority = model.Priority,
                SenderDepartment = db.Departments
         .Where(d => d.DepartmentID == deptId)
         .Select(d => d.DepartmentName)
         .FirstOrDefault(),
                SenderName = senderName,
                ReceiverDepartment = model.ReceiverDepartment == "Other" ? model.ReceiverDepartmentOther : model.ReceiverDepartment,
                ReceiverName = model.ReceiverName,
                DepartmentID = deptId
            };


            db.OutwardLetters.Add(letter);
            db.SaveChanges();

            // 📌 Save new tracker
            var newTracker = new OutwardLetterSerialTracker
            {
                DepartmentID = deptId,
                Date = DateTime.Today,
                LastSerialNumber = nextSerial.ToString()
            };
            db.OutwardLetterSerialTrackers.Add(newTracker);
            db.SaveChanges();

            // Save single staff
            db.OutwardLetterStaffs.Add(new OutwardLetterStaff
            {
                LetterID = letter.LetterID,
                StaffID = model.AssignedStaffID
            });
            db.SaveChanges();

            // Send email to that staff


            if (staff != null && !string.IsNullOrEmpty(staff.Email))
            {
                var emailService = new EmailService();
                string subject = "New Outward Letter Assigned";
                string body = $"Dear {staff.Name},<br/><br/>You have been assigned as the sender for Letter No: <strong>{letter.LetterNo}</strong>.<br/>Subject: {letter.Subject}<br/>Please take note.<br/><br/>Regards,<br/>University Portal";
                emailService.SendEmail(staff.Email, subject, body);
            }


            TempData["OutwardSuccess"] = true;
            return RedirectToAction("OutwardLetter");
        }
    }
}
