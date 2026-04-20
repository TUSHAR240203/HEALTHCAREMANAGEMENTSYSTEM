using Hms.AppointmentsApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.AppointmentsApi.Data;

public class AppointmentsDbContext : DbContext
{
    public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DoctorName).HasMaxLength(150);
            entity.Property(x => x.DepartmentName).HasMaxLength(150);

            entity.Property(x => x.VisitType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReasonForVisit).HasMaxLength(500);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.CompletionNotes).HasMaxLength(1000);

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.DoctorId);
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.AppointmentDate);

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.AppointmentDate,
                x.SlotStartTime,
                x.SlotEndTime
            });

            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}