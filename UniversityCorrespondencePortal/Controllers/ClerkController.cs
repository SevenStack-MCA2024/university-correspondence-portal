//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Net;
//using System.Web.Mvc;
//using UniversityCorrespondencePortal.Models;
//using UniversityCorrespondencePortal.Models.ViewModels;

//namespace UniversityCorrespondencePortal.Controllers
//{
//    public class ClerkController : Controller
//    {
//        private readonly UcpDbContext db = new UcpDbContext();

//        // ---------------------- LOGIN ----------------------
//        public ActionResult Login()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult Login(string username, string password)
//        {
//            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
//            {
//                ViewBag.Error = "Please enter both username and password.";
//                return View();
//            }

//            // Encode the input password using the same method used for storing
//            string encodedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

//            var clerk = db.Clerks.FirstOrDefault(c =>
//                c.Email == username && c.PasswordHash == encodedPassword
//            );

//            if (clerk != null)
//            {
//                Session["ClerkID"] = clerk.ClerkID;
//                Session["ClerkName"] = clerk.Name;

//                return RedirectToAction("InwardLetter");
//            }

//            ViewBag.Error = "Invalid username or password.";
//            return View();
//        }

//        // ---------------------- OutwardLetter ----------------------

//        public ActionResult OutwardLetter()
//        {
//            return View();
//        }

//        // ---------------------- PROFILE ----------------------
//        public ActionResult Profile()
//        {
//            if (Session["ClerkID"] == null)
//                return RedirectToAction("Login");

//            string clerkId = Session["ClerkID"] as string;
//            var clerk = db.Clerks.Find(clerkId); // now matches the string ClerkID

//            if (clerk == null)
//                return HttpNotFound();

//            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", clerk.DepartmentID);
//            return View(clerk);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Profile(Clerk model, string NewPassword)
//        {
//            if (Session["ClerkID"] == null)
//                return RedirectToAction("Login");

//            var existingClerk = db.Clerks.Find(model.ClerkID);
//            if (existingClerk == null)
//                return HttpNotFound();

//            if (ModelState.IsValid)
//            {
//                existingClerk.Name = model.Name;
//                existingClerk.Email = model.Email;
//                existingClerk.Phone = model.Phone;
//                existingClerk.DepartmentID = model.DepartmentID;

//                if (!string.IsNullOrEmpty(NewPassword))
//                {
//                    existingClerk.PasswordHash = HashPassword(NewPassword); // Or store directly if not hashing
//                }

//                db.SaveChanges();
//                TempData["Success"] = "Profile updated successfully.";
//                return RedirectToAction("Profile");

//            }

//            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", model.DepartmentID);
//            return View(model);
//        }

//        private string HashPassword(string password)
//        {
//            // Replace with secure hash method in production
//            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
//        }

//        // ---------------------- InwardLETTER MANAGEMENT ----------------------
//        public ActionResult InwardLetter()
//        {
//            if (Session["ClerkID"] == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            string clerkId = Session["ClerkID"].ToString();

//            using (var db = new UcpDbContext())
//            {
//                var clerk = db.Clerks.Include(c => c.Department).FirstOrDefault(c => c.ClerkID == clerkId);
//                if (clerk == null)
//                {
//                    return RedirectToAction("Login", "Account");
//                }

//                string clerkDepartment = clerk.Department.DepartmentName;

//                var letters = (from letter in db.InwardLetters
//                               where letter.ReceiverDepartment == clerkDepartment
//                               join staff in db.Staffs on letter.StaffID equals staff.StaffID into staffJoin
//                               from staff in staffJoin.DefaultIfEmpty()
//                               select new InwardLetterViewModel
//                               {
//                                   LetterID = letter.LetterID,
//                                   InwardNumber = letter.InwardNumber,
//                                   DateReceived = letter.DateReceived,
//                                   TimeReceived = letter.TimeReceived,
//                                   SenderDepartment = letter.SenderDepartment,
//                                   DeliveryMode = letter.DeliveryMode,
//                                   OutwardNumber = letter.OutwardNumber,
//                                   SenderName = letter.SenderName,
//                                   ReceiverDepartment = letter.ReceiverDepartment,
//                                   ReferenceID = letter.ReferenceID,
//                                   Subject = letter.Subject,
//                                   Remarks = letter.Remarks,
//                                   Priority = letter.Priority,
//                                   Status = letter.Status,
//                                   StaffName = staff != null ? staff.Name : "Not Allotted"
//                               }).ToList();

//                return View("InwardLetter", letters);
//            }
//        }
              

//        public ActionResult Index()
//        {
//            // Load all letters with related Staff using eager loading
//            var letters = db.InwardLetters.Include("Staff").ToList();

//            // Convert to ViewModel
//            var viewModel = letters.Select(letter => new InwardLetterViewModel
//            {
//                LetterID = letter.LetterID,
//                InwardNumber = letter.InwardNumber,
//                OutwardNumber = letter.OutwardNumber,
//                DateReceived = letter.DateReceived,
//                TimeReceived = letter.TimeReceived,
//                DeliveryMode = letter.DeliveryMode,
//                SenderDepartment = letter.SenderDepartment,
//                SenderName = letter.SenderName,
//                ReferenceID = letter.ReferenceID,
//                Subject = letter.Subject,
//                Remarks = letter.Remarks,
//                Status = letter.Status,
//                Priority = letter.Priority,
//                ReceiverDepartment = letter.ReceiverDepartment,
//                StaffName = letter.Staff?.Name ?? "Unassigned"
//            })
//            .OrderByDescending(vm => vm.DateReceived)
//            .ThenByDescending(vm => vm.TimeReceived) // Secondary sort by time
//            .ToList();

//            return View(viewModel);
//        }




//        // ---------------------- REPORT ----------------------
//        public ActionResult Report()
//        {
//            if (Session["ClerkID"] == null)
//                return RedirectToAction("Login");

//            return View();
//        }

//        // ---------------------- LOGOUT ----------------------
//        public ActionResult Logout()
//        {
//            Session.Clear();
//            return RedirectToAction("Login");
//        }
//        // =========================== STAFF ===========================

//        public ActionResult Staff(string searchTerm, string designationFilter, string departmentFilter, int? editId)
//        {
//            if (Session["ClerkID"] == null)
//                return RedirectToAction("Login");

//            string clerkId = Session["ClerkID"].ToString();
//            var clerk = db.Clerks.Find(clerkId);

//            if (clerk == null)
//                return HttpNotFound("Clerk not found.");

//            string clerkDeptId = clerk.DepartmentID;

//            ViewBag.ClerkDepartmentID = clerkDeptId;
//            ViewBag.ClerkDepartmentName = db.Departments
//                .Where(d => d.DepartmentID == clerkDeptId)
//                .Select(d => d.DepartmentName)
//                .FirstOrDefault();

//            // Get all staff linked to the Clerk's department
//            var staffList = db.StaffDepartments
//                .Where(sd => sd.DepartmentID == clerkDeptId)
//                .Select(sd => sd.Staff)
//                .Distinct()
//                .ToList()
//                .Select(s => new StaffViewModel
//                {
//                    StaffID = s.StaffID,
//                    Name = s.Name,
//                    Email = s.Email,
//                    Phone = s.Phone,
//                    Designation = s.Designation,
//                    Departments = string.Join(", ", s.StaffDepartments.Select(d => d.Department.DepartmentName))
//                });

//            // Apply Search Filter
//            if (!string.IsNullOrEmpty(searchTerm))
//            {
//                string loweredSearch = searchTerm.ToLower();

//                staffList = staffList.Where(s =>
//                    s.Name != null && s.Name.ToLower().Contains(loweredSearch) ||
//                    s.StaffID.ToString().Contains(loweredSearch) ||
//                    s.Email != null && s.Email.ToLower().Contains(loweredSearch));
//            }

//            // Apply Designation Filter
//            if (!string.IsNullOrEmpty(designationFilter))
//                staffList = staffList.Where(s => s.Designation == designationFilter);

//            //// Apply Department Filter
//            //if (!string.IsNullOrEmpty(departmentFilter))
//            //    staffList = staffList.Where(s => s.Departments.Contains(departmentFilter));

//            // Dropdown for Designation Filters
//            ViewBag.DesignationList = db.StaffDepartments
//                .Where(sd => sd.DepartmentID == clerkDeptId)
//                .Select(sd => sd.Staff.Designation)
//                .Distinct()
//                .ToList();

//            // Dropdown for Department Filters (will only show Clerk's department)
//            ViewBag.DepartmentList = db.Departments
//                .Where(d => d.DepartmentID == clerkDeptId)
//                .ToList();

//            if (editId.HasValue)
//                TempData["EditID"] = editId.Value;

//            return View(staffList.ToList());
//        }




//        [HttpPost]
//        public ActionResult CreateStaff(Staff staff)
//        {
//            try
//            {
//                staff.PasswordHash = "0000";
//                db.Staffs.Add(staff);
//                db.SaveChanges();

//                string clerkId = Session["ClerkID"].ToString();
//                var clerk = db.Clerks.Find(clerkId);

//                db.StaffDepartments.Add(new StaffDepartment
//                {
//                    StaffID = staff.StaffID,
//                    DepartmentID = clerk.DepartmentID
//                });

//                db.SaveChanges();
//                TempData["Message"] = "Staff added successfully.";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Error: " + ex.Message;
//            }

//            return RedirectToAction("Staff");
//        }


//        [HttpPost]
//        public ActionResult UpdateStaff(int StaffID, string Email, string Phone, string Designation)
//        {
//            try
//            {
//                var staff = db.Staffs.FirstOrDefault(s => s.StaffID == StaffID);
//                if (staff == null)
//                    return HttpNotFound();

//                staff.Email = Email;
//                staff.Phone = Phone;
//                staff.Designation = Designation;

//                db.SaveChanges();
//                TempData["Message"] = "Staff updated successfully.";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Update failed: " + ex.Message;
//            }

//            return RedirectToAction("Staff");
//        }


//    }
//}

