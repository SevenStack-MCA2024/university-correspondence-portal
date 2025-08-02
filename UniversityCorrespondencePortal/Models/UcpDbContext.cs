using System.Data.Entity;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class UcpDbContext : DbContext
    {
        public UcpDbContext()
        : base("name=UcpDbConnection") // This name should match your connection string in Web.config
        {
        }

       
        public DbSet<Department> Departments { get; set; }
        public DbSet<Clerk> Clerks { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<StaffDepartment> StaffDepartments { get; set; }
        public DbSet<InwardLetter> InwardLetters { get; set; }
        public DbSet<OutwardLetter> OutwardLetters { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<LetterSerialTracker> LetterSerialTrackers { get; set; }
        public DbSet<Admin> Admins { get; set; }




        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Composite key for StaffDepartment
            modelBuilder.Entity<StaffDepartment>()
                .HasKey(sd => new { sd.StaffID, sd.DepartmentID });

            base.OnModelCreating(modelBuilder);
        }
    }
}