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

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceName)
                .HasMaxLength(200)
                .IsRequired();

            // ✅ FIXED
            entity.Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Quantity);

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

            //entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}