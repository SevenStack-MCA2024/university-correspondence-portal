using System.Data;
using System.Data.Entity;

namespace UniversityCorrespondencePortal.Models
{
    public class UcpDbContext : DbContext
    {
        public UcpDbContext()
            : base("name=UcpDbConnection") // This name should match your connection string in Web.config
        {
            Database.SetInitializer<UcpDbContext>(null);
        }
        public DbSet<OutwardLetter> OutwardLetters { get; set; }
        public DbSet<OutwardLetterStaff> OutwardLetterStaffs { get; set; }
        public DbSet<OutwardLetterSerialTracker> OutwardLetterSerialTrackers { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Clerk> Clerks { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<StaffDepartment> StaffDepartments { get; set; }
        public DbSet<InwardLetter> InwardLetters { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<InwardLetterSerialTracker> InwardLetterSerialTrackers { get; set; }


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

            modelBuilder.Entity<Staff>()
        .HasIndex(s => s.Email)
        .IsUnique()
        .HasName("IX_Staff_Email");

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Phone)
                .IsUnique()
                .HasName("IX_Staff_Phone");
            base.OnModelCreating(modelBuilder);




            modelBuilder.Entity<OutwardLetterStaff>()
       .HasKey(ols => new { ols.LetterID, ols.StaffID });

            modelBuilder.Entity<OutwardLetterStaff>()
                .HasRequired(ols => ols.OutwardLetter)
                .WithMany(ol => ol.OutwardLetterStaffs)
                .HasForeignKey(ols => ols.LetterID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OutwardLetterStaff>()
                .HasRequired(ols => ols.Staff)
                .WithMany(s => s.OutwardLetterStaffs)
                .HasForeignKey(ols => ols.StaffID)
                .WillCascadeOnDelete(false);

            
            // Unique constraint on DepartmentID
    modelBuilder.Entity<Department>()
        .HasIndex(d => d.DepartmentID)
        .IsUnique();

            // Unique constraint on DepartmentName
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DepartmentName)
                .IsUnique();
        }
    }
}
