using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Interfaces;
using SmartFollowUp.API.Models;


namespace SmartFollowUp.API.Data
{
    public class AppDbContext : DbContext, IAppDbContext
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
            base.OnModelCreating(modelBuilder);

            // Every entity now has its own IEntityTypeConfiguration<T> file
            // under Data/Configurations/. This picks all of them up.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Safety net: force Restrict on any relationship a future entity
            // might add without an explicit configuration, so cascade deletes
            // never accidentally turn on.
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}