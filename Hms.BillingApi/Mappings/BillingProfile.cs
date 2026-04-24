using AutoMapper;
using Hms.BillingApi.Entities;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Mappings;

public class BillingProfile : Profile
{
    public BillingProfile()
    {
        // Request → Entity
        CreateMap<CreateInvoiceRequestDto, Invoice>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<AddInvoiceItemRequestDto, InvoiceItem>();
        CreateMap<PaymentRequestDto, Payment>();

        // 🔥 IMPORTANT: Entity → Response
        CreateMap<InvoiceItem, InvoiceItemResponseDto>();
        CreateMap<Payment, PaymentResponseDto>();

        CreateMap<Invoice, InvoiceResponseDto>();
    }
}