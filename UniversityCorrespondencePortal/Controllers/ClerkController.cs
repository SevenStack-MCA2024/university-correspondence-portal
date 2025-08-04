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
                c.Email == username && c.PasswordHash == password
            );

            if (clerk != null)
            {
                Session["ClerkID"] = clerk.ClerkID;
                Session["ClerkName"] = clerk.Name;
                Session["DepartmentID"] = clerk.DepartmentID;

                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }


        // ---------------------- PROFILE ----------------------
        public ActionResult Profile()
        {
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

            string clerkId = Session["ClerkID"] as string;
            var clerk = db.Clerks.Find(clerkId);

            if (clerk == null)
                return HttpNotFound();

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", clerk.DepartmentID);
            return View(clerk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(Clerk model, string NewPassword)
        {
            if (Session["ClerkID"] == null)
                return RedirectToAction("Login");

            var existingClerk = db.Clerks.Find(model.ClerkID);
            if (existingClerk == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                existingClerk.Name = model.Name;
                existingClerk.Email = model.Email;
                existingClerk.Phone = model.Phone;
                existingClerk.DepartmentID = model.DepartmentID;

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    existingClerk.PasswordHash = HashPassword(NewPassword);
                }

                db.SaveChanges();
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", model.DepartmentID);
            return View(model);
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
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
                return HttpNotFound("Clerk not found.");

            string clerkDeptId = clerk.DepartmentID;

            ViewBag.ClerkDepartmentID = clerkDeptId;
            ViewBag.ClerkDepartmentName = db.Departments
                .Where(d => d.DepartmentID == clerkDeptId)
                .Select(d => d.DepartmentName)
                .FirstOrDefault();

            // Get staff with status
            var staffList = db.StaffDepartments
                .Where(sd => sd.DepartmentID == clerkDeptId)
                .Select(sd => sd.Staff)
                .Distinct()
                .ToList()
                .Select(s => new StaffViewModel
                {
                    StaffID = s.StaffID,
                    Name = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    Designation = s.Designation,
                    Departments = string.Join(", ", s.StaffDepartments.Select(d => d.Department.DepartmentName)),
                    IsActive = s.IsActive
                });

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string loweredSearch = searchTerm.ToLower();
                staffList = staffList.Where(s =>
                    (s.Name != null && s.Name.ToLower().Contains(loweredSearch)) ||
                    s.StaffID.ToString().Contains(loweredSearch) ||
                    (s.Email != null && s.Email.ToLower().Contains(loweredSearch)));
            }

            if (!string.IsNullOrEmpty(designationFilter))
                staffList = staffList.Where(s => s.Designation == designationFilter);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = bool.Parse(statusFilter);
                staffList = staffList.Where(s => s.IsActive == isActive);
            }

            // Prepare dropdowns
            ViewBag.DesignationList = db.StaffDepartments
                .Where(sd => sd.DepartmentID == clerkDeptId)
                .Select(sd => sd.Staff.Designation)
                .Distinct()
                .ToList();

            ViewBag.DepartmentList = db.Departments
                .Where(d => d.DepartmentID == clerkDeptId)
                .ToList();

            if (editId.HasValue)
                TempData["EditID"] = editId.Value;

            return View(staffList.ToList());
        }

        [HttpPost]
        public ActionResult CreateStaff(Staff staff)
        {
            try
            {
                staff.PasswordHash = "0000";
                staff.IsActive = true;
                db.Staffs.Add(staff);
                db.SaveChanges();

                // Add department association
                db.StaffDepartments.Add(new StaffDepartment
                {
                    StaffID = staff.StaffID,
                    DepartmentID = Request.Form["DepartmentID"]
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
        public ActionResult UpdateStaff(int StaffID, string Email, string Phone, string Designation, bool IsActive)
        {
            try
            {
                var staff = db.Staffs.FirstOrDefault(s => s.StaffID == StaffID);
                if (staff == null)
                    return HttpNotFound();

                staff.Email = Email;
                staff.Phone = Phone;
                staff.Designation = Designation;
                staff.IsActive = IsActive;

                db.SaveChanges();
                TempData["Message"] = "Staff updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
            }

            return RedirectToAction("Staff");
        }

        
        
    }
}