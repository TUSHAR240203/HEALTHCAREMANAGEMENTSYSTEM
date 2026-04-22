using Hms.PatientsApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> entity)
    {
        entity.ToTable("Patients");

        entity.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasIndex(x => x.MobileNumber);
    }
}