using Hms.ReceptionApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.ReceptionApi.Data;

public class ReceptionDbContext : DbContext
{
    public ReceptionDbContext(DbContextOptions<ReceptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientCheckIn> PatientCheckIns => Set<PatientCheckIn>();
    public DbSet<QueueToken> QueueTokens => Set<QueueToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PatientCheckIn>(entity =>
        {
            entity.ToTable("PatientCheckIns");
<<<<<<< HEAD
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasIndex(x => new { x.DepartmentId, x.CheckInTimeUtc });
=======

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasIndex(x => x.DoctorId);
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => new { x.DepartmentId, x.CheckInTimeUtc });
            entity.HasIndex(x => new { x.PatientId, x.AppointmentId }).IsUnique();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        });

        modelBuilder.Entity<QueueToken>(entity =>
        {
            entity.ToTable("QueueTokens");
<<<<<<< HEAD
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PatientName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => new { x.DepartmentId, x.QueueDate, x.TokenNumber }).IsUnique();
        });
        modelBuilder.Entity<QueueToken>(entity =>
        {
            entity.ToTable("QueueTokens");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PatientName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasIndex(x => new { x.DepartmentId, x.QueueDate, x.TokenNumber }).IsUnique();
=======

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.PatientName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            entity.HasIndex(x => new { x.DepartmentId, x.QueueDate, x.TokenNumber }).IsUnique();
            entity.HasIndex(x => new { x.DepartmentId, x.QueueDate, x.Status });
            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasIndex(x => x.DoctorId);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        });
    }
}