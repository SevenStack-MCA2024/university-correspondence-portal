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
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            var clerk = db.Clerks.FirstOrDefault(c =>
                c.Email == username && c.PasswordHash == password && c.IsActive
            );

            if (clerk != null)
            {
                Session["ClerkID"] = clerk.ClerkID;
                Session["ClerkName"] = clerk.Name;
                Session["DepartmentID"] = clerk.DepartmentID;

                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Invalid username, password, or your account is inactive.";
            return View();
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
        // ---------------------- REPORT ----------------------
        public ActionResult Report()
        {
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

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

            // ✅ Pass clerk's department ID and department name to view
            ViewBag.ClerkDepartmentID = clerk.DepartmentID;
            ViewBag.ClerkDepartmentName = db.Departments
                .Where(d => d.DepartmentID == clerk.DepartmentID)
                .Select(d => d.DepartmentName)
                .FirstOrDefault();

            // Base query
            var staffQuery = db.Staffs.AsQueryable();

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

            ViewBag.DesignationList = db.Staffs.Select(s => s.Designation).Distinct().ToList();
            ViewBag.DepartmentList = db.Departments.ToList();
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
                staff.PasswordHash = "0000";
                db.Staffs.Add(staff);
                db.SaveChanges();

                db.StaffDepartments.Add(new StaffDepartment
                {
                    StaffID = staff.StaffID,
                    DepartmentID = DepartmentID
                });

                db.SaveChanges();
                TempData["Message"] = "Staff added successfully.";
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
                    return HttpNotFound();

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

            // ✅ Use DbFunctions to compare only date
            var lastTracker = db.InwardLetterSerialTrackers
                .Where(t => t.DepartmentID == deptId && DbFunctions.TruncateTime(t.Date) == today.Date)
                .OrderByDescending(t => t.LastSerialNumber)
                .FirstOrDefault();

            int nextSerial = 1;

            if (lastTracker != null && int.TryParse(lastTracker.LastSerialNumber, out int lastSerial))
            {
                nextSerial = lastSerial + 1;
            }

            string paddedSerial = nextSerial.ToString("D3"); // 001, 002, ...
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


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateInwardLetter(InwardLetterViewModel model)
        //{
        //    if (Session["DepartmentID"] == null)
        //        return RedirectToAction("Login");

        //    string deptId = Session["DepartmentID"].ToString();

        //    // ✅ STEP 1: Only delete the previous serial tracker (not the letter)
        //    var oldTracker = db.InwardLetterSerialTrackers
        //        .Where(t => t.DepartmentID == deptId)
        //        .OrderByDescending(t => t.Date)
        //        .FirstOrDefault();

        //    if (oldTracker != null)
        //    {
        //        db.InwardLetterSerialTrackers.Remove(oldTracker);
        //        db.SaveChanges();
        //    }

        //    // ✅ STEP 2: Generate new serial number & Inward Number
        //    string serialNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(); // Can be changed to number-based
        //    string inwardNumber = $"{deptId}-INW-{serialNumber}";

        //    // ✅ STEP 3: Create the new letter
        //    var letter = new InwardLetter
        //    {
        //        InwardNumber = inwardNumber,
        //        OutwardNumber = model.OutwardNumber,
        //        DateReceived = DateTime.Now.Date,
        //        TimeReceived = DateTime.Now.TimeOfDay,
        //        DeliveryMode = model.DeliveryMode,
        //        SenderDepartment = model.SenderDepartment,
        //        SenderName = model.SenderName,
        //        ReferenceID = model.ReferenceID,
        //        Subject = model.Subject,
        //        Remarks = model.Remarks,
        //        Priority = model.Priority,
        //        ReceiverDepartment = deptId,
        //        LetterStaffs = new List<LetterStaff>()
        //    };

        //    if (model.SelectedStaffIDs != null)
        //    {
        //        foreach (int staffId in model.SelectedStaffIDs)
        //        {
        //            letter.LetterStaffs.Add(new LetterStaff
        //            {
        //                StaffID = staffId
        //            });
        //        }
        //    }

        //    // Save the new letter
        //    db.InwardLetters.Add(letter);
        //    db.SaveChanges();

        //    // ✅ STEP 4: Add new tracker entry
        //    var newTracker = new InwardLetterSerialTracker
        //    {
        //        DepartmentID = deptId,
        //        Date = DateTime.Now,
        //        LastSerialNumber = serialNumber,
        //        LetterID = letter.LetterID
        //    };

        //    db.InwardLetterSerialTrackers.Add(newTracker);
        //    db.SaveChanges();

        //    // ✅ STEP 5: Send Email
        //    var emailService = new EmailService();

        //    if (model.SelectedStaffIDs != null)
        //    {
        //        var staffList = db.Staffs
        //            .Where(s => model.SelectedStaffIDs.Contains(s.StaffID))
        //            .Select(s => new { s.Email, s.Name })
        //            .ToList();

        //        foreach (var staff in staffList)
        //        {
        //            string subject = "📨 New Inward Letter Assigned";
        //            string body = $@"
        //        <p>Dear {staff.Name},</p>
        //        <p>You have been assigned a new inward letter.</p>
        //        <p>
        //            <strong>Letter No:</strong> {inwardNumber}<br/>
        //            <strong>Subject:</strong> {model.Subject}<br/>
        //            <strong>Sender:</strong> {model.SenderDepartment} ({model.SenderName})<br/>
        //            <strong>Reference ID:</strong> {model.ReferenceID}
        //        </p>
        //        <p>Please log in to the UCP portal to view the details.</p>
        //        <br/>
        //        <p>Regards,<br/>UCP System</p>
        //    ";

        //            try
        //            {
        //                emailService.SendEmail(staff.Email, subject, body);
        //            }
        //            catch (Exception ex)
        //            {
        //                TempData["Error"] = "Some emails could not be sent: " + ex.Message;
        //            }
        //        }
        //    }

        //    TempData["Message"] = "Inward Letter created and email sent.";
        //    return RedirectToAction("InwardLetters");
        //}


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


    }
}
