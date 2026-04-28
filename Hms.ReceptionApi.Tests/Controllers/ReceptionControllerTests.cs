using Hms.ReceptionApi.Controllers;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.ReceptionApi.Tests.Controllers;

public class ReceptionControllerTests
{
    private readonly Mock<IReceptionService> _receptionServiceMock;
    private readonly ReceptionController _controller;

    public ReceptionControllerTests()
    {
        _receptionServiceMock = new Mock<IReceptionService>();
        _controller = new ReceptionController(_receptionServiceMock.Object);
    }

    [Fact]
    public async Task SearchPatients_ShouldReturnOk()
    {
        var request = new ReceptionPatientSearchRequestDto
        {
            MobileNumber = "9999999999"
        };

        var response = new ReceptionPatientSearchResponseDto
        {
            TotalCount = 1,
            Patients = new List<ReceptionPatientSummaryDto>
            {
                GetPatientSummary()
            }
        };

        _receptionServiceMock
            .Setup(x => x.SearchPatientsAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.SearchPatients(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RegisterPatient_ShouldReturnOk()
    {
        var request = new RegisterPatientByReceptionRequestDto
        {
            FullName = "Tushar Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = 1,
            MobileNumber = "9999999999",
            PortalAccessEnabled = true
        };

        var response = GetPatientSummary();

        _receptionServiceMock
            .Setup(x => x.RegisterPatientAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.RegisterPatient(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPatientSummary_ShouldReturnOk_WhenPatientExists()
    {
        var response = GetPatientSummary();

        _receptionServiceMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.GetPatientSummary(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPatientSummary_ShouldReturnNotFound_WhenPatientNotExists()
    {
        _receptionServiceMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync((ReceptionPatientSummaryDto?)null);

        var result = await _controller.GetPatientSummary(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task VerifyPatient_ShouldReturnOk()
    {
        var request = new VerifyPatientRequestDto
        {
            MobileNumber = "9999999999"
        };

        var response = new VerifyPatientResponseDto
        {
            PatientId = 1,
            Verified = true,
            Message = "Verified"
        };

        _receptionServiceMock
            .Setup(x => x.VerifyPatientAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.VerifyPatient(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ResendPortalActivation_ShouldReturnOk()
    {
        var request = new ResendPortalActivationRequestDto
        {
            SendBy = "sms"
        };

        _receptionServiceMock
            .Setup(x => x.ResendPortalActivationAsync(1, request))
            .Returns(Task.CompletedTask);

        var result = await _controller.ResendPortalActivation(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task BookAppointment_ShouldReturnOk()
    {
        var request = new BookAppointmentRequestDto
        {
            PatientId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(10, 30),
            VisitType = "OPD",
            IsTeleConsultation = false
        };

        var response = GetAppointmentResponse();

        _receptionServiceMock
            .Setup(x => x.BookAppointmentAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.BookAppointment(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RescheduleAppointment_ShouldReturnOk()
    {
        var request = new RescheduleAppointmentRequestDto
        {
            NewAppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NewSlotStartTime = new TimeOnly(11, 0),
            NewSlotEndTime = new TimeOnly(11, 30),
            Reason = "Time change"
        };

        var response = GetAppointmentResponse();

        _receptionServiceMock
            .Setup(x => x.RescheduleAppointmentAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.RescheduleAppointment(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CancelAppointment_ShouldReturnOk()
    {
        var request = new CancelAppointmentRequestDto
        {
            Reason = "Patient unavailable"
        };

        var response = GetAppointmentResponse();

        _receptionServiceMock
            .Setup(x => x.CancelAppointmentAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.CancelAppointment(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CheckIn_ShouldReturnOk()
    {
        var request = new CheckInRequestDto
        {
            AppointmentId = 1,
            PatientId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow
        };

        var response = new CheckInResponseDto
        {
            CheckInId = 1,
            AppointmentId = 1,
            PatientId = 1,
            UHID = "UHID001",
            TokenNumber = 101,
            QueuePosition = 1,
            Status = "Waiting",
            Message = "Checked in"
        };

        _receptionServiceMock
            .Setup(x => x.CheckInAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.CheckIn(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCheckInById_ShouldReturnOk_WhenExists()
    {
        var response = new CheckInResponseDto
        {
            CheckInId = 1,
            AppointmentId = 1,
            PatientId = 1,
            UHID = "UHID001",
            TokenNumber = 101,
            QueuePosition = 1,
            Status = "Waiting",
            Message = "Found"
        };

        _receptionServiceMock
            .Setup(x => x.GetCheckInByIdAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.GetCheckInById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCheckInById_ShouldReturnNotFound_WhenNotExists()
    {
        _receptionServiceMock
            .Setup(x => x.GetCheckInByIdAsync(1))
            .ReturnsAsync((CheckInResponseDto?)null);

        var result = await _controller.GetCheckInById(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnOk()
    {
        var request = new CreateInvoiceRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            ConsultationFee = 500
        };

        var response = GetInvoiceResponse();

        _receptionServiceMock
            .Setup(x => x.CreateInvoiceAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.CreateInvoice(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnOk_WhenExists()
    {
        var response = GetInvoiceResponse();

        _receptionServiceMock
            .Setup(x => x.GetInvoiceByIdAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.GetInvoiceById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnNotFound_WhenNotExists()
    {
        _receptionServiceMock
            .Setup(x => x.GetInvoiceByIdAsync(1))
            .ReturnsAsync((InvoiceResponseDto?)null);

        var result = await _controller.GetInvoiceById(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetInvoicesByPatientId_ShouldReturnOk()
    {
        var response = new List<InvoiceResponseDto>
        {
            GetInvoiceResponse()
        };

        _receptionServiceMock
            .Setup(x => x.GetInvoicesByPatientIdAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.GetInvoicesByPatientId(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task AddInvoiceItem_ShouldReturnOk()
    {
        var request = new AddInvoiceItemRequestDto
        {
            ServiceName = "X-Ray",
            Amount = 300
        };

        var response = GetInvoiceResponse();

        _receptionServiceMock
            .Setup(x => x.AddInvoiceItemAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.AddInvoiceItem(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task AddPayment_ShouldReturnOk()
    {
        var request = new PaymentRequestDto
        {
            Amount = 500,
            PaymentMode = "Cash"
        };

        var response = GetInvoiceResponse();

        _receptionServiceMock
            .Setup(x => x.AddPaymentAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.AddPayment(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    private static ReceptionPatientSummaryDto GetPatientSummary()
    {
        return new ReceptionPatientSummaryDto
        {
            PatientId = 1,
            UHID = "UHID001",
            FullName = "Tushar Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = 1,
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            PortalAccessEnabled = true,
            PortalActivated = false,
            Status = 1
        };
    }

    private static BookAppointmentResponseDto GetAppointmentResponse()
    {
        return new BookAppointmentResponseDto
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            DoctorId = 1,
            DoctorName = "Dr Sharma",
            DepartmentId = 1,
            DepartmentName = "Cardiology",
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(10, 30),
            VisitType = "OPD",
            Status = "Booked",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static InvoiceResponseDto GetInvoiceResponse()
    {
        return new InvoiceResponseDto
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            TotalAmount = 500,
            PaidAmount = 0,
            BalanceAmount = 500,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}