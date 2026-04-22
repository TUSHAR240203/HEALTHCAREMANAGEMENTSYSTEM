using Hms.PatientsApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.PatientsApi.Configurations;

public class MobileNumberChangeRequestConfiguration : IEntityTypeConfiguration<MobileNumberChangeRequest>
{
    public void Configure(EntityTypeBuilder<MobileNumberChangeRequest> entity)
    {
        entity.ToTable("MobileNumberChangeRequests");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.NewMobileNumber)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(x => x.OtpCode)
            .HasMaxLength(10)
            .IsRequired();

        entity.HasIndex(x => new { x.PatientId, x.NewMobileNumber, x.IsConsumed });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
