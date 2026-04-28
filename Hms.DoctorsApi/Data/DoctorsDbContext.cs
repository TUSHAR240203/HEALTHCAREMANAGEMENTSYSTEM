using Hms.DoctorsApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.DoctorsApi.Data;

public class DoctorsDbContext : DbContext
{
    public DoctorsDbContext(DbContextOptions<DoctorsDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<DoctorLeave> DoctorLeaves => Set<DoctorLeave>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DoctorCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.Gender).HasMaxLength(20);
            entity.Property(x => x.Qualification).HasMaxLength(150);
            entity.Property(x => x.Specialization).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DepartmentName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LicenseNumber).HasMaxLength(50);
            entity.Property(x => x.RoomNumber).HasMaxLength(20);
            entity.Property(x => x.ConsultationFee).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => x.DoctorCode).IsUnique();
            entity.HasIndex(x => x.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
            entity.HasIndex(x => x.DepartmentId);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.ToTable("DoctorSchedules");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Doctor)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<DoctorLeave>(entity =>
        {
            entity.ToTable("DoctorLeaves");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(250);
            entity.HasOne(x => x.Doctor)
                .WithMany(x => x.Leaves)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DoctorId, x.LeaveDate });
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
