using Hms.PatientsApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hms.PatientsApi.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> entity)
    {
        entity.ToTable("Patients");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PatientIdentifier)
            .HasMaxLength(30)
            .IsRequired();

        entity.HasIndex(x => x.PatientIdentifier)
            .IsUnique();

        entity.Property(x => x.UHID)
            .HasMaxLength(30)
            .IsRequired();

        entity.HasIndex(x => x.UHID)
            .IsUnique();

        entity.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.MiddleName)
            .HasMaxLength(100);

        entity.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.MobileNumber)
            .HasMaxLength(20)
            .IsRequired();

        entity.HasIndex(x => x.MobileNumber);

        entity.Property(x => x.Email).HasMaxLength(150);
        entity.Property(x => x.BloodGroup).HasMaxLength(10);
        entity.Property(x => x.MaritalStatus).HasMaxLength(50);
        entity.Property(x => x.AddressLine1).HasMaxLength(200);
        entity.Property(x => x.AddressLine2).HasMaxLength(200);
        entity.Property(x => x.City).HasMaxLength(100);
        entity.Property(x => x.State).HasMaxLength(100);
        entity.Property(x => x.PostalCode).HasMaxLength(20);
        entity.Property(x => x.EmergencyContactName).HasMaxLength(150);
        entity.Property(x => x.EmergencyContactNumber).HasMaxLength(20);
        entity.Property(x => x.EmergencyContactRelation).HasMaxLength(50);
        entity.Property(x => x.AadhaarNumber).HasMaxLength(20);
        entity.Property(x => x.InsuranceProvider).HasMaxLength(150);
        entity.Property(x => x.InsurancePolicyNumber).HasMaxLength(100);

        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
