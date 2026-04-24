<<<<<<< HEAD
﻿using Hms.PatientsApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
=======
using Hms.PatientsApi.Entities;
using Microsoft.EntityFrameworkCore;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.PatientsApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
<<<<<<< HEAD
=======
    public DbSet<MobileNumberChangeRequest> MobileNumberChangeRequests => Set<MobileNumberChangeRequest>();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
<<<<<<< HEAD

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");

            entity.HasKey(x => x.Id);

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

            entity.Property(x => x.FullName)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.MobileNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(x => x.MobileNumber);

            entity.Property(x => x.Email)
                .HasMaxLength(150);

            entity.Property(x => x.BloodGroup)
                .HasMaxLength(10);

            entity.Property(x => x.MaritalStatus)
                .HasMaxLength(50);

            entity.Property(x => x.AddressLine1).HasMaxLength(200);
            entity.Property(x => x.AddressLine2).HasMaxLength(200);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(100);
            entity.Property(x => x.Country).HasMaxLength(100);
            entity.Property(x => x.PostalCode).HasMaxLength(20);

            entity.Property(x => x.EmergencyContactName).HasMaxLength(150);
            entity.Property(x => x.EmergencyContactNumber).HasMaxLength(20);
            entity.Property(x => x.EmergencyContactRelation).HasMaxLength(50);

            entity.Property(x => x.AadhaarNumber).HasMaxLength(20);
            entity.Property(x => x.InsuranceProvider).HasMaxLength(150);
            entity.Property(x => x.InsurancePolicyNumber).HasMaxLength(100);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
=======
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    }
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    //}
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
