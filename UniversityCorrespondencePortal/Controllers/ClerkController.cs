using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using UniversityCorrespondencePortal.Models;
using UniversityCorrespondencePortal.Models.ViewModels;

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



    }
}
