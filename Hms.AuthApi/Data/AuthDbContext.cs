using Hms.AuthApi.Configurations;
using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<PatientUserLink> PatientUserLinks => Set<PatientUserLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new StaffUserConfiguration());
        modelBuilder.ApplyConfiguration(new OtpVerificationConfiguration());
        modelBuilder.ApplyConfiguration(new PatientUserLinkConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}