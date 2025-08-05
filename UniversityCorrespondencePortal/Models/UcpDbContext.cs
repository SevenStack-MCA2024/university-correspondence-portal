using System.Data.Entity;

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
        public DbSet<LetterSerialTracker> LetterSerialTrackers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<InwardLetterSerialTracker> InwardLetterSerialTrackers { get; set; }
        public DbSet<OutwardLetterSerialTracker> OutwardLetterSerialTrackers { get; set; }


        // 🔁 Add the join table
        public DbSet<LetterStaff> LetterStaffs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Composite key for StaffDepartment
            modelBuilder.Entity<StaffDepartment>()
                .HasKey(sd => new { sd.StaffID, sd.DepartmentID });

            // Composite key for LetterStaff
            modelBuilder.Entity<LetterStaff>()
                .HasKey(ls => new { ls.LetterID, ls.StaffID });

            // Configure LetterStaff ↔ InwardLetter
            modelBuilder.Entity<LetterStaff>()
                .HasRequired(ls => ls.InwardLetter)
                .WithMany(il => il.LetterStaffs)
                .HasForeignKey(ls => ls.LetterID)
                .WillCascadeOnDelete(false); // Optional

            // Configure LetterStaff ↔ Staff
            modelBuilder.Entity<LetterStaff>()
                .HasRequired(ls => ls.Staff)
                .WithMany(s => s.LetterStaffs)
                .HasForeignKey(ls => ls.StaffID)
                .WillCascadeOnDelete(false); // Optional

            base.OnModelCreating(modelBuilder);
        }
    }
}
