<<<<<<< HEAD
=======
using Hms.AuthApi.Configurations;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Data;

public class AuthDbContext : DbContext
{
<<<<<<< HEAD
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
=======
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    {
    }

    public DbSet<User> Users => Set<User>();
<<<<<<< HEAD
=======
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<PatientUserLink> PatientUserLinks => Set<PatientUserLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
<<<<<<< HEAD
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MobileNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Role).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => x.MobileNumber);
        });

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.ToTable("OtpVerifications");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MobileNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.OtpCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => new { x.PatientId, x.MobileNumber, x.Purpose });
        });

        modelBuilder.Entity<PatientUserLink>(entity =>
        {
            entity.ToTable("PatientUserLinks");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();

            entity.HasIndex(x => x.PatientId).IsUnique();
            entity.HasIndex(x => x.UserId).IsUnique();
        });
=======
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new OtpVerificationConfiguration());
        modelBuilder.ApplyConfiguration(new PatientUserLinkConfiguration());

        base.OnModelCreating(modelBuilder);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    }
}