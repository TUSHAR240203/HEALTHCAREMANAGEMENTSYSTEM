namespace Hms.BillingApi.Entities;

public class Invoice : BaseEntity
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int? AppointmentId { get; set; }

    public decimal TotalAmount { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
    public decimal BalanceAmount { get; set; } = 0; 

    public string Status { get; set; } = "Pending";
    public bool IsClosed { get; set; } = false;

    /// <summary>Format: INV-{YEAR}-{ID:D4}. Auto-generated after first save.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}