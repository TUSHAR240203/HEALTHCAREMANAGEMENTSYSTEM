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
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasIndex(x => new { x.DepartmentId, x.CheckInTimeUtc });
        });

        modelBuilder.Entity<QueueToken>(entity =>
        {
            entity.ToTable("QueueTokens");
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
        });
    }
}