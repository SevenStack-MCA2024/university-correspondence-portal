using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;
using System.IO;
using System.Web.Script.Serialization;

namespace UniversityCorrespondencePortal.Controllers
{
    public class AdminController : Controller
    {
        private UcpDbContext db = new UcpDbContext();

        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var admin = db.Admins.FirstOrDefault(a => a.Email == username);

            if (admin != null && admin.PasswordHash == password)
            {
                Session["AdminID"] = admin.AdminID;
                TempData["Message"] = $"Welcome {admin.Name}!";
                return RedirectToAction("Report");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Report()
        {
            if (Session["AdminID"] == null)
                return RedirectToAction("Login");

            ViewBag.ReportMessage = "This is the Admin Reports page. Reports will be shown here.";
            return View();
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

            return RedirectToAction("AddStaff");
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

            return RedirectToAction("AddStaff");
        }

        // =========================== CLERK ===========================

        public ActionResult AddClerk(string searchTerm, string departmentFilter, string editId)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClerk(Clerk clerk)
        {
            try
            {
                clerk.PasswordHash = "0000";
                clerk.IsActive = true;
                db.Clerks.Add(clerk);
                db.SaveChanges();
                TempData["Message"] = "Clerk added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error adding clerk: " + ex.Message;
            }

            return RedirectToAction("AddClerk");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateClerk(string ClerkID, string Name, string Email, string Phone)
        {
            try
            {
                var clerk = db.Clerks.FirstOrDefault(c => c.ClerkID == ClerkID);
                if (clerk == null)
                {
                    TempData["Error"] = "Clerk not found.";
                    return RedirectToAction("AddClerk");
                }

                clerk.Name = Name;
                clerk.Email = Email;
                clerk.Phone = Phone;

                db.SaveChanges();
                TempData["Message"] = "Clerk updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
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
        public ActionResult AddDepartmentModal(string DepartmentID, string DepartmentName, string DepartmentCode)
        {
            if (string.IsNullOrWhiteSpace(DepartmentID) || string.IsNullOrWhiteSpace(DepartmentName))
            {
                TempData["Error"] = "Department ID and Name are required.";
                return RedirectToAction("AddDepartment");
            }

            if (db.Departments.Any(d => d.DepartmentID == DepartmentID))
            {
                TempData["Error"] = "Department ID already exists.";
                return RedirectToAction("AddDepartment");
            }

            db.Departments.Add(new Department
            {
                DepartmentID = DepartmentID,
                DepartmentName = DepartmentName,
                DepartmentCode = DepartmentCode
            });

            db.SaveChanges();
            TempData["Message"] = "Department added successfully.";
            return RedirectToAction("AddDepartment");
        }

        [HttpPost]
        public ActionResult UpdateDepartment(string DepartmentID, string DepartmentName, string DepartmentCode)
        {
            try
            {
                var department = db.Departments.FirstOrDefault(d => d.DepartmentID == DepartmentID);
                if (department == null)
                {
                    TempData["Error"] = "Department not found.";
                    return RedirectToAction("AddDepartment");
                }

                department.DepartmentName = DepartmentName;
                department.DepartmentCode = DepartmentCode;
                db.SaveChanges();

                TempData["Message"] = "Department updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
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
