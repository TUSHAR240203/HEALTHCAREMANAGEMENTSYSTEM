using Hms.BillingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ServiceCatalog> ServiceCatalog => Set<ServiceCatalog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UHID).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.BalanceAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.IsClosed).IsRequired().HasDefaultValue(false);

            // ── Task 4: InvoiceNumber ─────────────────────────────────────────
            entity.Property(x => x.InvoiceNumber)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(string.Empty);

            entity.HasIndex(x => x.InvoiceNumber)
                .IsUnique()
                .HasFilter("[InvoiceNumber] <> ''");   // exclude empty (before generation)

            // ── Task 3: Filtered unique index on AppointmentId ────────────────
            // Prevents duplicate invoices for the same appointment at DB level.
            // NULL values are excluded so no-appointment invoices are not affected.
            entity.HasIndex(x => x.AppointmentId)
                .IsUnique()
                .HasFilter("[AppointmentId] IS NOT NULL");

            entity.HasIndex(x => x.PatientId);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Type)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Consultation");

            entity.Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Quantity);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Invoice)
                  .WithMany(x => x.Items)
                  .HasForeignKey(x => x.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.Invoice)
                  .WithMany(x => x.Payments)
                  .HasForeignKey(x => x.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Task 2: ServiceCatalog ────────────────────────────────────────────
        modelBuilder.Entity<ServiceCatalog>(entity =>
        {
            entity.ToTable("ServiceCatalog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            // Seed default services so the catalog is usable immediately
            entity.HasData(
                new ServiceCatalog { Id = 1, Name = "Blood Test (CBC)", Price = 350, Type = "Test", IsActive = true },
                new ServiceCatalog { Id = 2, Name = "Urine Test", Price = 200, Type = "Test", IsActive = true },
                new ServiceCatalog { Id = 3, Name = "X-Ray", Price = 800, Type = "Test", IsActive = true },
                new ServiceCatalog { Id = 4, Name = "ECG", Price = 500, Type = "Test", IsActive = true },
                new ServiceCatalog { Id = 5, Name = "Ultrasound", Price = 1200, Type = "Test", IsActive = true },
                new ServiceCatalog { Id = 6, Name = "Paracetamol 500mg", Price = 50, Type = "Medicine", IsActive = true },
                new ServiceCatalog { Id = 7, Name = "Amoxicillin 250mg", Price = 120, Type = "Medicine", IsActive = true },
                new ServiceCatalog { Id = 8, Name = "Pantoprazole 40mg", Price = 90, Type = "Medicine", IsActive = true }
            );
        });
    }
}