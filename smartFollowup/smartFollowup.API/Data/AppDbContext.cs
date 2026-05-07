using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<DoctorRequest> DoctorRequests { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<DoctorNote> DoctorNotes { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<AiAnalysis> AiAnalyses { get; set; }
        public DbSet<WoundImage> WoundImages { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
        public DbSet<MedicationAdherence> MedicationAdherences { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User → Cases (as Doctor)
            modelBuilder.Entity<Case>()
                .HasOne(c => c.Doctor)
                .WithMany(u => u.DoctorCases)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → Cases (as Patient)
            modelBuilder.Entity<Case>()
                .HasOne(c => c.Patient)
                .WithMany(u => u.PatientCases)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → DoctorProfile
            modelBuilder.Entity<DoctorProfile>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<DoctorProfile>(d => d.UserId);

            // User → PatientProfile
            modelBuilder.Entity<PatientProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(p => p.UserId);

            // DailyReport → AiAnalysis
            modelBuilder.Entity<AiAnalysis>()
                .HasOne(a => a.DailyReport)
                .WithOne(r => r.AiAnalysis)
                .HasForeignKey<AiAnalysis>(a => a.ReportId);

            // Alert → Case
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Case)
                .WithMany(c => c.Alerts)
                .HasForeignKey(a => a.CaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Alert → DailyReport
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.DailyReport)
                .WithMany(r => r.Alerts)
                .HasForeignKey(a => a.ReportId)
                .OnDelete(DeleteBehavior.Restrict);

            // WoundImage → DailyReport
            modelBuilder.Entity<WoundImage>()
                .HasOne(w => w.DailyReport)
                .WithMany(r => r.WoundImages)
                .HasForeignKey(w => w.ReportId)
                .OnDelete(DeleteBehavior.Restrict);

            // WoundImage → Case
            modelBuilder.Entity<WoundImage>()
                .HasOne(w => w.Case)
                .WithMany(c => c.WoundImages)
                .HasForeignKey(w => w.CaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Email unique index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Disable cascade delete globally for everything else
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Global Soft Delete Filters
            modelBuilder.Entity<Case>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<Prescription>()
                .HasQueryFilter(p => !p.IsDeleted);
        }
    }
}