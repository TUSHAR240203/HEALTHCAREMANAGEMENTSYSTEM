using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.AuthApi.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("OtpVerifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.OtpCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.PatientId,
            x.MobileNumber,
            x.Purpose
        });
    }
}