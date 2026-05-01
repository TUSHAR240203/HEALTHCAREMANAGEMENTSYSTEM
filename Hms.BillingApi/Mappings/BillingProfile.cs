using AutoMapper;
using Hms.BillingApi.Entities;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Mappings;

public class BillingProfile : Profile
{
    public BillingProfile()
    {
        // ── Request → Entity ──────────────────────────────────────────────────
        CreateMap<CreateInvoiceRequestDto, Invoice>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<CreateFromAppointmentRequestDto, Invoice>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"))
            .ForMember(dest => dest.IsClosed, opt => opt.MapFrom(_ => false));

        // NOTE: AddInvoiceItemRequestDto is NOT mapped to InvoiceItem.
        // BillingService builds InvoiceItem manually from ServiceCatalog (Task 2).

        CreateMap<PaymentRequestDto, Payment>();

        // ── Entity → Response ─────────────────────────────────────────────────
        CreateMap<InvoiceItem, InvoiceItemResponseDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<Payment, PaymentResponseDto>();

        CreateMap<Invoice, InvoiceResponseDto>()
            .ForMember(dest => dest.IsClosed, opt => opt.MapFrom(src => src.IsClosed))
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber));

        CreateMap<ServiceCatalog, ServiceCatalogResponseDto>();
    }
}