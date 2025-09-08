using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;

namespace UniversityCorrespondencePortal.Controllers
{
    public class AdminController : Controller
    {
        private UcpDbContext db = new UcpDbContext();

        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            username = username?.Trim();
            password = password?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            var admin = db.Admins.FirstOrDefault(a => a.Email == username);

            if (admin != null && admin.PasswordHash == password) // Plain text comparison
            {
                Session["AdminID"] = admin.AdminID;
                TempData["Message"] = $"Welcome {admin.Name}!";
                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }








        // ------------------------Report -------------------------------


        //    public ActionResult Report()
        //    {
        //        // Total active and deactivated departments
        //        ViewBag.TotalDepartments = db.Departments.Count();
        //        ViewBag.ActiveDepartments = db.Departments.Count(d => d.IsActive);
        //        ViewBag.DeactivatedDepartments = db.Departments.Count(d => !d.IsActive);

        //        // Total active/deactive clerks (assuming Clerk is a role)
        //        ViewBag.ActiveClerksCount = db.Clerks.Count(u => u.IsActive);
        //        ViewBag.DeactiveClerksCount = db.Clerks.Count(u => !u.IsActive);

        //        // Total active/deactive staff
        //        ViewBag.ActiveStaffCount = db.Staffs.Count(u => u.IsActive);
        //        ViewBag.DeactiveStaffCount = db.Staffs.Count(u => !u.IsActive);

        //        // Total inward and outward letters
        //        ViewBag.TotalInwardLetters = db.InwardLetters.Count();
        //        ViewBag.TotalOutwardLetters = db.OutwardLetters.Count();

        //        // Total inward and outward by department
        //        ViewBag.InwardByDepartment = db.InwardLetters
        //            .GroupBy(i => i.ReceiverDepartment)
        //            .Select(g => new { Department = g.Key, Count = g.Count() })
        //            .ToList();

        //        ViewBag.OutwardByDepartment = db.OutwardLetters
        //            .GroupBy(o => o.Department.DepartmentName)
        //            .Select(g => new { Department = g.Key, Count = g.Count() })
        //            .ToList();

        //        // Month-wise count of inward and outward letters
        //        var currentYear = DateTime.Now.Year;

        //        ViewBag.MonthlyInward = db.InwardLetters
        //.Where(i => i.DateReceived.HasValue && i.DateReceived.Value.Year == currentYear)
        //.GroupBy(i => i.DateReceived.Value.Month)
        //.Select(g => new { Month = g.Key, Count = g.Count() })
        //.OrderBy(g => g.Month)
        //.ToList();

        //        ViewBag.MonthlyOutward = db.OutwardLetters
        //.Where(o => o.Date.HasValue && o.Date.Value.Year == currentYear)
        //.GroupBy(o => o.Date.Value.Month)
        //.Select(g => new { Month = g.Key, Count = g.Count() })
        //.OrderBy(g => g.Month)
        //.ToList();

        //        // Recent activity (both inward and outward)
        //        var recentInward = db.InwardLetters
        //            .Where(i => i.DateReceived.HasValue)
        //.OrderByDescending(i => i.DateReceived.Value)
        //            .Take(10)
        //            .Select(i => new
        //            {
        //                LetterNumber = i.InwardNumber,
        //                Type = "Inward",
        //                Subject = i.Subject,
        //                Department = i.SenderDepartment,
        //                Date = i.DateReceived,
        //                Status = "Received",
        //                StatusClass = "success"
        //            }).ToList();





        //        var recentOutward = db.OutwardLetters
        //            .OrderByDescending(o => o.Date)
        //            .Take(10)
        //            .Select(o => new
        //            {
        //                LetterNumber = o.OutwardNumber,
        //                Type = "Outward",
        //                Subject = o.Subject,
        //                Department = o.Department.DepartmentName,
        //                Date = o.Date,
        //                Status = "Dispatched",
        //                StatusClass = "info"
        //            }).ToList();

        //        var recentLetters = recentInward.Concat(recentOutward)
        //            .OrderByDescending(l => l.Date)
        //            .Take(10)
        //            .ToList();

        //        ViewBag.RecentLetters = recentLetters;

        //        return View();
        //    }




        public ActionResult Report()
        {
            // Total active and deactivated departments
            ViewBag.TotalDepartments = db.Departments.Count();
            ViewBag.ActiveDepartments = db.Departments.Count(d => d.IsActive);
            ViewBag.DeactivatedDepartments = db.Departments.Count(d => !d.IsActive);

            // Clerks
            ViewBag.ActiveClerksCount = db.Clerks.Count(u => u.IsActive);
            ViewBag.DeactiveClerksCount = db.Clerks.Count(u => !u.IsActive);

            // Staff
            ViewBag.ActiveStaffCount = db.Staffs.Count(u => u.IsActive);
            ViewBag.DeactiveStaffCount = db.Staffs.Count(u => !u.IsActive);

            // Letters
            ViewBag.TotalInwardLetters = db.InwardLetters.Count();
            ViewBag.TotalOutwardLetters = db.OutwardLetters.Count();

            // Group by Department (Receiver side for Inward)
            ViewBag.InwardByDepartment = db.InwardLetters
                .Where(i => i.ReceiverDepartment != null)
                .GroupBy(i => i.ReceiverDepartment.ToString())
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.OutwardByDepartment = db.OutwardLetters
                .Where(o => o.Department != null)
                .GroupBy(o => o.Department.DepartmentName)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            // Month-wise
            var currentYear = DateTime.Now.Year;

            ViewBag.MonthlyInward = db.InwardLetters
                .Where(i => i.DateReceived.HasValue && i.DateReceived.Value.Year == currentYear)
                .GroupBy(i => i.DateReceived.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToList();

            ViewBag.MonthlyOutward = db.OutwardLetters
                .Where(o => o.Date.HasValue && o.Date.Value.Year == currentYear)
                .GroupBy(o => o.Date.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToList();

            // Recent activity
            var recentInward = db.InwardLetters
                .Where(i => i.DateReceived.HasValue)
                .OrderByDescending(i => i.DateReceived.Value)
                .Take(10)
                .Select(i => new
                {
                    LetterNumber = i.InwardNumber,
                    Type = "Inward",
                    Subject = i.Subject,
                    Department = i.SenderDepartment != null ? i.SenderDepartment.ToString() : "N/A",
                    Date = i.DateReceived,
                    Status = "Received",
                    StatusClass = "success"
                }).ToList();

            var recentOutward = db.OutwardLetters
                .Where(o => o.Date.HasValue)
                .OrderByDescending(o => o.Date.Value)
                .Take(10)
                .Select(o => new
                {
                    LetterNumber = o.OutwardNumber,
                    Type = "Outward",
                    Subject = o.Subject,
                    Department = o.Department != null ? o.Department.DepartmentName : "N/A",
                    Date = o.Date,
                    Status = "Dispatched",
                    StatusClass = "info"
                }).ToList();

            var recentLetters = recentInward.Concat(recentOutward)
                .OrderByDescending(l => l.Date)
                .Take(10)
                .ToList();

            ViewBag.RecentLetters = recentLetters;

            return View();
        }












        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //public ActionResult Report()
        //{
        //    if (Session["AdminID"] == null)
        //        return RedirectToAction("Login");

        //    ViewBag.ReportMessage = "This is the Admin Reports page. Reports will be shown here.";
        //    return View();
        //}

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

        // =========================== STAFF ===========================
        public ActionResult AddStaff(string searchTerm, string designationFilter, string departmentFilter, string statusFilter, int? editId)
        {
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
                // 1️⃣ Always set default password before validation so [Required] passes
                if (string.IsNullOrWhiteSpace(staff.PasswordHash))
                {
                    staff.PasswordHash = PasswordHelper.HashPassword("0000");
                    staff.MustResetPassword = true;
                }

                // 2️⃣ Manual DataAnnotations validation
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(staff, serviceProvider: null, items: null);
                bool isValid = Validator.TryValidateObject(staff, validationContext, validationResults, true);

                if (!isValid)
                {
                    var errors = string.Join(" ", validationResults.Select(vr => vr.ErrorMessage));
                    throw new ValidationException(errors);
                }

                // 3️⃣ Check for existing email or phone
                var existingStaff = db.Staffs.FirstOrDefault(s =>
                    s.Email == staff.Email || s.Phone == staff.Phone);

                if (existingStaff != null)
                {
                    bool alreadyAssigned = db.StaffDepartments.Any(sd =>
                        sd.StaffID == existingStaff.StaffID && sd.DepartmentID == DepartmentID);

                    if (!alreadyAssigned)
                    {
                        db.StaffDepartments.Add(new StaffDepartment
                        {
                            StaffID = existingStaff.StaffID,
                            DepartmentID = DepartmentID
                        });
                        db.SaveChanges();
                        TempData["Message"] = "Staff already exists. Added to selected department.";
                    }
                    else
                    {
                        throw new InvalidOperationException("Staff already exists and is already assigned to this department.");
                    }
                }
                else
                {
                    // 4️⃣ Save new staff
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
            }
            catch (ValidationException vex)
            {
                TempData["Error"] = vex.Message;
            }
            catch (InvalidOperationException ioex)
            {
                TempData["Error"] = ioex.Message;
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "A database error occurred. Please check your data.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred. Please contact the administrator.";
            }

            return RedirectToAction("AddStaff");
        }

        // =========================== CLERK ===========================

        public ActionResult AddClerk(string searchTerm, string departmentFilter, string editId)
        {
            try
            {
                var clerks = db.Clerks.Include("Department").AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    clerks = clerks.Where(c =>
                        c.Name.Contains(searchTerm) ||
                        c.ClerkID.Contains(searchTerm) ||
                        c.Email.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(departmentFilter))
                {
                    clerks = clerks.Where(c => c.Department.DepartmentID == departmentFilter);
                }

                ViewBag.DepartmentList = db.Departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentID,
                    Text = d.DepartmentName
                }).ToList();

                ViewBag.EditID = editId;

                var viewModel = clerks.OrderBy(c => c.Name)
                    .Select(c => new ClerkViewModel
                    {
                        ClerkID = c.ClerkID,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        DepartmentName = c.Department.DepartmentName,
                        IsActive = c.IsActive
                    }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load clerk list. Please try again later. Error: " + ex.Message;
                return RedirectToAction("ErrorPage"); // Optional: redirect to a friendly error page
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClerk(Clerk clerk)
        {
            try
            {
                // Basic validation
                if (clerk == null)
                    throw new ArgumentNullException(nameof(clerk), "Clerk details are required.");

                if (string.IsNullOrWhiteSpace(clerk.ClerkID) || !System.Text.RegularExpressions.Regex.IsMatch(clerk.ClerkID, "^[a-zA-Z0-9]+$"))
                    throw new ArgumentException("Clerk ID must contain only letters and numbers.");

                if (string.IsNullOrWhiteSpace(clerk.Name))
                    throw new ArgumentException("Clerk name is required.");

                if (!new EmailAddressAttribute().IsValid(clerk.Email))
                    throw new ArgumentException("Invalid email format.");

                if (string.IsNullOrWhiteSpace(clerk.DepartmentID))
                    throw new ArgumentException("Department selection is required.");

                // Default setup
                clerk.PasswordHash = PasswordHelper.HashPassword("0000");
                clerk.MustResetPassword = true;
                clerk.IsActive = true;

                db.Clerks.Add(clerk);
                db.SaveChanges();

                TempData["Message"] = "Clerk added successfully.";
            }
            catch (ArgumentException argEx)
            {
                TempData["Error"] = "Validation error: " + argEx.Message;
            }
            catch (DbUpdateException dbEx)
            {
                TempData["Error"] = "Database update failed. Please check the entered data. Details: " + dbEx.InnerException?.Message ?? dbEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unexpected error while adding clerk: " + ex.Message;
            }

            return RedirectToAction("AddClerk");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateClerk(string ClerkID, string Name, string Email, string Phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClerkID))
                    throw new ArgumentException("Clerk ID is required for update.");

                var clerk = db.Clerks.FirstOrDefault(c => c.ClerkID == ClerkID);
                if (clerk == null)
                    throw new KeyNotFoundException("Clerk not found.");

                if (string.IsNullOrWhiteSpace(Name))
                    throw new ArgumentException("Name cannot be empty.");

                if (!string.IsNullOrWhiteSpace(Email) && !new EmailAddressAttribute().IsValid(Email))
                    throw new ArgumentException("Invalid email format.");

                clerk.Name = Name;
                clerk.Email = Email;
                clerk.Phone = Phone;

                db.SaveChanges();
                TempData["Message"] = "Clerk updated successfully.";
            }
            catch (ArgumentException argEx)
            {
                TempData["Error"] = "Validation error: " + argEx.Message;
            }
            catch (KeyNotFoundException notFoundEx)
            {
                TempData["Error"] = notFoundEx.Message;
            }
            catch (DbUpdateException dbEx)
            {
                TempData["Error"] = "Database update failed. Please try again. Details: " + dbEx.InnerException?.Message ?? dbEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unexpected error while updating clerk: " + ex.Message;
            }

            return RedirectToAction("AddClerk");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateClerk(string clerkId)
        {
            if (string.IsNullOrEmpty(clerkId))
                return RedirectToAction("AddClerk");

            var clerk = db.Clerks.Find(clerkId);
            if (clerk != null)
            {
                clerk.IsActive = true;
                db.SaveChanges();
                TempData["Message"] = "Clerk activated successfully.";
            }
            else
            {
                TempData["Error"] = "Clerk not found.";
            }

            return RedirectToAction("AddClerk");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeactivateClerk(string clerkId)
        {
            if (string.IsNullOrEmpty(clerkId))
                return RedirectToAction("AddClerk");

            var clerk = db.Clerks.Find(clerkId);
            if (clerk != null)
            {
                clerk.IsActive = false;
                db.SaveChanges();
                TempData["Message"] = "Clerk deactivated successfully.";
            }
            else
            {
                TempData["Error"] = "Clerk not found.";
            }

            return RedirectToAction("AddClerk");
        }

        // =========================== DEPARTMENT ===========================

        [HttpGet]
        public ActionResult AddDepartment(string search, string filter = "All", string editId = "")
        {
            var departments = db.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                departments = departments.Where(d =>
                    d.DepartmentName.Contains(search) ||
                    d.DepartmentID.Contains(search));
            }

            if (filter == "Active")
                departments = departments.Where(d => d.IsActive);
            else if (filter == "Inactive")
                departments = departments.Where(d => !d.IsActive);

            ViewBag.Search = search;
            ViewBag.Filter = filter;
            TempData["EditID"] = editId;

            return View(departments.ToList());
        }

        [HttpPost]
        public ActionResult ToggleDepartmentStatus(string departmentId)
        {
            var dept = db.Departments.Find(departmentId);
            if (dept != null)
            {
                dept.IsActive = !dept.IsActive;
                db.SaveChanges();
                TempData["Message"] = $"Department '{dept.DepartmentName}' is now {(dept.IsActive ? "Active" : "Inactive")}.";
            }
            else
            {
                TempData["Error"] = "Department not found.";
            }
            return RedirectToAction("AddDepartment");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartmentModal(string DepartmentID, string DepartmentName, string DepartmentCode)
        {
            try
            {
                // 1️⃣ Required fields check
                if (string.IsNullOrWhiteSpace(DepartmentID) || string.IsNullOrWhiteSpace(DepartmentName))
                    throw new ArgumentException("Department ID and Name are required.");

                // 2️⃣ Format validation for DepartmentID (letters + numbers only)
                if (!System.Text.RegularExpressions.Regex.IsMatch(DepartmentID, @"^[A-Za-z0-9]+$"))
                    throw new ArgumentException("Department ID can only contain letters and numbers.");

                // 3️⃣ Format validation for DepartmentName (letters + spaces only)
                if (!System.Text.RegularExpressions.Regex.IsMatch(DepartmentName, @"^[A-Za-z\s]+$"))
                    throw new ArgumentException("Department name can only contain letters and spaces.");

                // 4️⃣ Format validation for DepartmentCode (uppercase letters only)
                if (!string.IsNullOrWhiteSpace(DepartmentCode) &&
                    !System.Text.RegularExpressions.Regex.IsMatch(DepartmentCode, @"^[A-Z]{2,10}$"))
                    throw new ArgumentException("Department code must be 2–10 uppercase letters.");

                // 5️⃣ Uniqueness checks
                if (db.Departments.Any(d => d.DepartmentID == DepartmentID))
                    throw new InvalidOperationException("Department ID already exists.");

                if (db.Departments.Any(d => d.DepartmentName.ToLower() == DepartmentName.ToLower()))
                    throw new InvalidOperationException("Department name already exists.");

                // 6️⃣ Add to database
                db.Departments.Add(new Department
                {
                    DepartmentID = DepartmentID,
                    DepartmentName = DepartmentName,
                    DepartmentCode = DepartmentCode,
                    IsActive = true
                });

                db.SaveChanges();

                TempData["Message"] = "Department added successfully.";
            }
            catch (ArgumentException ex) // Validation-related
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex) // Business rule violation
            {
                TempData["Error"] = ex.Message;
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx) // DB constraint issues
            {
                // Likely a unique index violation
                TempData["Error"] = "Database error: possible duplicate or constraint violation.";
                // Optional: log dbEx.InnerException for debugging
            }
            catch (Exception ex) // Unexpected errors
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
                // Optional: log the full exception
            }

            return RedirectToAction("AddDepartment");
        }

        [HttpPost]
        public ActionResult UpdateStaff(int StaffID, string Name, string Email, string Phone, string Designation)
        {
            try
            {
                var tempStaff = new Staff
                {
                    StaffID = StaffID,
                    Name = Name?.Trim(),
                    Email = Email?.Trim(),
                    Phone = Phone?.Trim(),
                    Designation = Designation?.Trim()
                };

                var validationResults = new List<ValidationResult>();

                // ✅ Validate only Name, Email, and Phone
                var propsToValidate = new[] { "Name", "Email", "Phone" };
                foreach (var prop in propsToValidate)
                {
                    var context = new ValidationContext(tempStaff) { MemberName = prop };
                    Validator.TryValidateProperty(
                        tempStaff.GetType().GetProperty(prop)?.GetValue(tempStaff),
                        context,
                        validationResults
                    );
                }

                if (validationResults.Any())
                {
                    throw new ValidationException(string.Join(" ", validationResults.Select(vr => vr.ErrorMessage)));
                }

                var staff = db.Staffs.FirstOrDefault(s => s.StaffID == StaffID);
                if (staff == null)
                    throw new KeyNotFoundException("Staff record not found.");

                bool duplicateExists = db.Staffs.Any(s =>
                    s.StaffID != StaffID &&
                    (s.Email == tempStaff.Email || s.Phone == tempStaff.Phone));

                if (duplicateExists)
                    throw new InvalidOperationException("Another staff member already has the same email or phone number.");

                staff.Name = tempStaff.Name;
                staff.Email = tempStaff.Email;
                staff.Phone = tempStaff.Phone;
                staff.Designation = tempStaff.Designation;

                db.SaveChanges();
                TempData["Message"] = "Staff updated successfully.";
            }
            catch (ValidationException vex)
            {
                TempData["Error"] = vex.Message;
            }
            catch (KeyNotFoundException knfEx)
            {
                TempData["Error"] = knfEx.Message;
            }
            catch (InvalidOperationException ioex)
            {
                TempData["Error"] = ioex.Message;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "The record was modified by another user. Please reload and try again.";
            }
            catch (DbUpdateException dbEx)
            {
                TempData["Error"] = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred while updating the staff record.";
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return RedirectToAction("AddStaff");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDepartment(string DepartmentID, string DepartmentName, string DepartmentCode)
        {
            try
            {
                // 1️⃣ Validate required fields
                if (string.IsNullOrWhiteSpace(DepartmentID))
                    throw new ArgumentException("Department ID is required.");

                if (string.IsNullOrWhiteSpace(DepartmentName))
                    throw new ArgumentException("Department name is required.");

                // 2️⃣ Format validation for DepartmentName (letters + spaces only)
                if (!System.Text.RegularExpressions.Regex.IsMatch(DepartmentName, @"^[A-Za-z\s]+$"))
                    throw new ArgumentException("Department name can only contain letters and spaces.");

                // 3️⃣ Format validation for DepartmentCode (uppercase letters only, optional field)
                if (!string.IsNullOrWhiteSpace(DepartmentCode) &&
                    !System.Text.RegularExpressions.Regex.IsMatch(DepartmentCode, @"^[A-Z]{2,10}$"))
                    throw new ArgumentException("Department code must be 2–10 uppercase letters.");

                // 4️⃣ Check if department exists
                var department = db.Departments.FirstOrDefault(d => d.DepartmentID == DepartmentID);
                if (department == null)
                    throw new KeyNotFoundException("Department not found.");

                // 5️⃣ Check for duplicate DepartmentName (case-insensitive, excluding current record)
                bool nameExists = db.Departments.Any(d => d.DepartmentName.ToLower() == DepartmentName.ToLower()
                                                       && d.DepartmentID != DepartmentID);
                if (nameExists)
                    throw new InvalidOperationException("Another department with the same name already exists.");

                // 6️⃣ Update department
                department.DepartmentName = DepartmentName;
                department.DepartmentCode = DepartmentCode;
                db.SaveChanges();

                TempData["Message"] = "Department updated successfully.";
            }
            catch (ArgumentException ex) // Input validation errors
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex) // Duplicate name or business rule errors
            {
                TempData["Error"] = ex.Message;
            }
            catch (KeyNotFoundException ex) // Record not found
            {
                TempData["Error"] = ex.Message;
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx) // DB constraint violations
            {
                TempData["Error"] = "Database update failed. Possible duplicate or constraint violation.";
                // Optionally log dbEx.InnerException for details
            }
            catch (Exception ex) // Unexpected errors
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("AddDepartment");
        }


        public class DepartmentStatusToggleModel
        {
            public string DepartmentId { get; set; }
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
                    return RedirectToAction("AddStaff");
                }

                staff.IsActive = !staff.IsActive;
                db.SaveChanges();

                TempData["Message"] = $"Staff member {(staff.IsActive ? "activated" : "deactivated")} successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating staff status: " + ex.Message;
            }

            return RedirectToAction("AddStaff");
        }
    }
}
