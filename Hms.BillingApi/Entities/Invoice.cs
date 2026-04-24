namespace Hms.BillingApi.Entities;

public class Invoice : BaseEntity
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int AppointmentId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}