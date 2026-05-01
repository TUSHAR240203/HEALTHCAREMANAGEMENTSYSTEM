using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.AuthApi.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoginId)
            .HasMaxLength(100);
        builder.Property(x => x.FullName)
    .HasMaxLength(150);

        builder.HasIndex(x => x.LoginId)
            .IsUnique()
            .HasFilter("[LoginId] IS NOT NULL");

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsPasswordLoginEnabled)
            .HasDefaultValue(false);

        builder.Property(x => x.IsOtpLoginEnabled)
            .HasDefaultValue(true);

        builder.Property(x => x.IsFirstLoginCompleted)
            .HasDefaultValue(false);

        // Family members can share the same mobile number, so this is intentionally not unique.
        builder.HasIndex(x => x.MobileNumber);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}