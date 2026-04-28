using Hms.AuthApi.Common;
using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.AuthApi.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();

        builder.HasData(
            new Role
            {
                Id = 1,
                Name = AppRoles.Admin,
                NormalizedName = AppRoles.Admin.ToUpper()
            },
            new Role
            {
                Id = 2,
                Name = AppRoles.Patient,
                NormalizedName = AppRoles.Patient.ToUpper()
            },
            new Role
            {
                Id = 3,
                Name = AppRoles.Doctor,
                NormalizedName = AppRoles.Doctor.ToUpper()
            },
            new Role
            {
                Id = 4,
                Name = AppRoles.Receptionist,
                NormalizedName = AppRoles.Receptionist.ToUpper()
            }
        );
    }
}