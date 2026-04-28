using System.Collections.Generic;

namespace Frontend.Models.Billing
{
    public class CreateInvoiceRequestDto
    {
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public decimal ConsultationFee { get; set; }
        public List<AddInvoiceItemRequestDto> Items { get; set; } = new();
    }
}
