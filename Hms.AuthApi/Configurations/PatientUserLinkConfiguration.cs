using Hms.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.AuthApi.Configurations;

public class PatientUserLinkConfiguration : IEntityTypeConfiguration<PatientUserLink>
{
    public void Configure(EntityTypeBuilder<PatientUserLink> builder)
    {
        builder.ToTable("PatientUserLinks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UHID)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => x.PatientId)
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}