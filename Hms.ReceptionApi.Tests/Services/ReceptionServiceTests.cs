using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Clients;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Services;
using Moq;
using Xunit;

namespace Hms.ReceptionApi.Tests.Services;

public class ReceptionServiceTests
{
    private readonly Mock<IPatientsApiClient> _patientsApiClientMock = new();
    private readonly Mock<IAppointmentsApiClient> _appointmentsApiClientMock = new();
    private readonly Mock<IBillingApiClient> _billingApiClientMock = new();
    private readonly Mock<ICheckInRepository> _checkInRepositoryMock = new();
    private readonly Mock<IQueueRepository> _queueRepositoryMock = new();

    private ReceptionService CreateService()
    {
        return new ReceptionService(
            _patientsApiClientMock.Object,
            _appointmentsApiClientMock.Object,
            _billingApiClientMock.Object,
            _checkInRepositoryMock.Object,
            _queueRepositoryMock.Object
        );
    }

    [Fact]
    public async Task SearchPatientsAsync_ShouldReturnPatients()
    {
        var service = CreateService();

        var request = new ReceptionPatientSearchRequestDto
        {
            MobileNumber = "9999999999"
        };

        var response = new ReceptionPatientSearchResponseDto
        {
            TotalCount = 1,
            Patients = new List<ReceptionPatientSummaryDto>
            {
                GetPatient()
            }
        };

        _patientsApiClientMock
            .Setup(x => x.SearchPatientsAsync(request))
            .ReturnsAsync(response);

        var result = await service.SearchPatientsAsync(request);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Patients);
    }

    [Fact]
    public async Task RegisterPatientAsync_ShouldReturnRegisteredPatient()
    {
        var service = CreateService();

        var request = new RegisterPatientByReceptionRequestDto
        {
            FullName = "Tushar Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = 1,
            MobileNumber = "9999999999",
            PortalAccessEnabled = true
        };

        var response = GetPatient();

        _patientsApiClientMock
            .Setup(x => x.RegisterPatientAsync(request))
            .ReturnsAsync(response);

        var result = await service.RegisterPatientAsync(request);

        Assert.Equal("Tushar Sharma", result.FullName);
    }

    [Fact]
    public async Task GetPatientSummaryAsync_ShouldReturnPatient()
    {
        var service = CreateService();

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(GetPatient());

        var result = await service.GetPatientSummaryAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.PatientId);
    }

    [Fact]
    public async Task GetPatientSummaryAsync_ShouldThrow_WhenPatientIdInvalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetPatientSummaryAsync(0));
    }

    [Fact]
    public async Task VerifyPatientAsync_ShouldReturnVerifiedTrue_WhenMobileMatches()
    {
        var service = CreateService();

        var request = new VerifyPatientRequestDto
        {
            MobileNumber = "9999999999"
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(GetPatient());

        var result = await service.VerifyPatientAsync(1, request);

        Assert.True(result.Verified);
        Assert.Equal("Patient identity verified.", result.Message);
    }

    [Fact]
    public async Task VerifyPatientAsync_ShouldReturnVerifiedFalse_WhenPatientNotFound()
    {
        var service = CreateService();

        var request = new VerifyPatientRequestDto
        {
            MobileNumber = "9999999999"
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync((ReceptionPatientSummaryDto?)null);

        var result = await service.VerifyPatientAsync(1, request);

        Assert.False(result.Verified);
        Assert.Equal("Patient not found.", result.Message);
    }

    [Fact]
    public async Task ResendPortalActivationAsync_ShouldThrowNotSupportedException()
    {
        var service = CreateService();

        var request = new ResendPortalActivationRequestDto
        {
            SendBy = "sms"
        };

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            service.ResendPortalActivationAsync(1, request));
    }

    [Fact]
    public async Task BookAppointmentAsync_ShouldBookAppointment_WhenPatientExists()
    {
        var service = CreateService();

        var request = new BookAppointmentRequestDto
        {
            PatientId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(10, 30),
            VisitType = "OPD",
            ReasonForVisit = "Fever",
            IsTeleConsultation = false
        };

        var appointment = GetAppointment();

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(GetPatient());

        _appointmentsApiClientMock
            .Setup(x => x.BookAppointmentAsync(It.IsAny<AppointmentCreateRequestDto>()))
            .ReturnsAsync(appointment);

        var result = await service.BookAppointmentAsync(request);

        Assert.Equal(1, result.PatientId);
        Assert.Equal("Booked", result.Status);
    }

    [Fact]
    public async Task BookAppointmentAsync_ShouldThrow_WhenPatientNotFound()
    {
        var service = CreateService();

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

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync((ReceptionPatientSummaryDto?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.BookAppointmentAsync(request));
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrow_WhenAppointmentIdInvalid()
    {
        var service = CreateService();

        var request = new RescheduleAppointmentRequestDto();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RescheduleAppointmentAsync(0, request));
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldReturnAppointment_WhenValid()
    {
        var service = CreateService();

        var request = new RescheduleAppointmentRequestDto
        {
            NewAppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NewSlotStartTime = new TimeOnly(11, 0),
            NewSlotEndTime = new TimeOnly(11, 30)
        };

        _appointmentsApiClientMock
            .Setup(x => x.RescheduleAppointmentAsync(1, request))
            .ReturnsAsync(GetAppointment());

        var result = await service.RescheduleAppointmentAsync(1, request);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CancelAppointmentAsync_ShouldThrow_WhenAppointmentIdInvalid()
    {
        var service = CreateService();

        var request = new CancelAppointmentRequestDto();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CancelAppointmentAsync(0, request));
    }

    [Fact]
    public async Task CancelAppointmentAsync_ShouldReturnAppointment_WhenValid()
    {
        var service = CreateService();

        var request = new CancelAppointmentRequestDto
        {
            Reason = "Patient requested cancellation"
        };

        _appointmentsApiClientMock
            .Setup(x => x.CancelAppointmentAsync(1, request))
            .ReturnsAsync(GetAppointment());

        var result = await service.CancelAppointmentAsync(1, request);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CheckInAsync_ShouldCreateCheckInAndQueueToken()
    {
        var service = CreateService();

        var request = new CheckInRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(GetPatient());

        _queueRepositoryMock
            .Setup(x => x.GetNextTokenNumberAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync(1);

        var result = await service.CheckInAsync(request);

        Assert.Equal(1, result.PatientId);
        Assert.Equal(1, result.TokenNumber);

        _checkInRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PatientCheckIn>()), Times.Once);
        _checkInRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _queueRepositoryMock.Verify(x => x.AddAsync(It.IsAny<QueueToken>()), Times.Once);
        _queueRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_ShouldThrow_WhenPatientNotFound()
    {
        var service = CreateService();

        var request = new CheckInRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync((ReceptionPatientSummaryDto?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CheckInAsync(request));
    }

    [Fact]
    public async Task GetCheckInByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var service = CreateService();

        _checkInRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((PatientCheckIn?)null);

        var result = await service.GetCheckInByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCheckInByIdAsync_ShouldReturnCheckIn_WhenFound()
    {
        var service = CreateService();

        var checkIn = new PatientCheckIn
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow,
            TokenNumber = 1,
            Status = "CheckedIn"
        };

        _checkInRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(checkIn);

        var result = await service.GetCheckInByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.CheckInId);
        Assert.Equal("CheckedIn", result.Status);
    }

    [Fact]
    public async Task GetDepartmentQueueAsync_ShouldReturnQueue()
    {
        var service = CreateService();

        var date = DateOnly.FromDateTime(DateTime.Today);

        var queueItems = new List<QueueToken>
        {
            new QueueToken
            {
                Id = 1,
                DepartmentId = 1,
                QueueDate = date,
                TokenNumber = 1,
                PatientId = 1,
                UHID = "UHID001",
                PatientName = "Tushar Sharma",
                AppointmentId = 1,
                DoctorId = 1,
                Status = "Waiting"
            }
        };

        _queueRepositoryMock
            .Setup(x => x.GetDepartmentQueueAsync(1, date))
            .ReturnsAsync(queueItems);

        var result = await service.GetDepartmentQueueAsync(1, date);

        Assert.Equal(1, result.DepartmentId);
        Assert.Single(result.Queue);
        Assert.Equal("Tushar Sharma", result.Queue[0].PatientName);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldThrow_WhenPatientIdInvalid()
    {
        var service = CreateService();

        var request = new CreateInvoiceRequestDto
        {
            PatientId = 0,
            AppointmentId = 1,
            ConsultationFee = 500
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInvoiceAsync(request));
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldThrow_WhenAppointmentIdInvalid()
    {
        var service = CreateService();

        var request = new CreateInvoiceRequestDto
        {
            PatientId = 1,
            AppointmentId = 0,
            ConsultationFee = 500
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInvoiceAsync(request));
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldThrow_WhenConsultationFeeNegative()
    {
        var service = CreateService();

        var request = new CreateInvoiceRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            ConsultationFee = -1
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInvoiceAsync(request));
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldThrow_WhenPatientNotFound()
    {
        var service = CreateService();

        var request = new CreateInvoiceRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            ConsultationFee = 500
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync((ReceptionPatientSummaryDto?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInvoiceAsync(request));
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldReturnInvoice_WhenValid()
    {
        var service = CreateService();

        var request = new CreateInvoiceRequestDto
        {
            PatientId = 1,
            AppointmentId = 1,
            ConsultationFee = 500
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientSummaryAsync(1))
            .ReturnsAsync(GetPatient());

        _billingApiClientMock
            .Setup(x => x.CreateInvoiceAsync(It.IsAny<object>()))
            .ReturnsAsync(GetInvoice());

        var result = await service.CreateInvoiceAsync(request);

        Assert.Equal(1, result.PatientId);
        Assert.Equal(500, result.TotalAmount);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_ShouldThrow_WhenInvoiceIdInvalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetInvoiceByIdAsync(0));
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_ShouldReturnInvoice_WhenValid()
    {
        var service = CreateService();

        _billingApiClientMock
            .Setup(x => x.GetInvoiceByIdAsync(1))
            .ReturnsAsync(GetInvoice());

        var result = await service.GetInvoiceByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetInvoicesByPatientIdAsync_ShouldThrow_WhenPatientIdInvalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetInvoicesByPatientIdAsync(0));
    }

    [Fact]
    public async Task GetInvoicesByPatientIdAsync_ShouldReturnInvoices_WhenValid()
    {
        var service = CreateService();

        _billingApiClientMock
            .Setup(x => x.GetInvoicesByPatientIdAsync(1))
            .ReturnsAsync(new List<InvoiceResponseDto> { GetInvoice() });

        var result = await service.GetInvoicesByPatientIdAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result[0].PatientId);
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldThrow_WhenInvoiceIdInvalid()
    {
        var service = CreateService();

        var request = new AddInvoiceItemRequestDto
        {
            ServiceName = "X-Ray",
            Amount = 500
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddInvoiceItemAsync(0, request));
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldThrow_WhenServiceNameMissing()
    {
        var service = CreateService();

        var request = new AddInvoiceItemRequestDto
        {
            ServiceName = "",
            Amount = 500
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddInvoiceItemAsync(1, request));
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldThrow_WhenAmountInvalid()
    {
        var service = CreateService();

        var request = new AddInvoiceItemRequestDto
        {
            ServiceName = "X-Ray",
            Amount = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddInvoiceItemAsync(1, request));
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldReturnInvoice_WhenValid()
    {
        var service = CreateService();

        var request = new AddInvoiceItemRequestDto
        {
            ServiceName = "X-Ray",
            Amount = 500
        };

        _billingApiClientMock
            .Setup(x => x.AddInvoiceItemAsync(1, request))
            .ReturnsAsync(GetInvoice());

        var result = await service.AddInvoiceItemAsync(1, request);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AddPaymentAsync_ShouldThrow_WhenInvoiceIdInvalid()
    {
        var service = CreateService();

        var request = new PaymentRequestDto
        {
            Amount = 500,
            PaymentMode = "Cash"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddPaymentAsync(0, request));
    }

    [Fact]
    public async Task AddPaymentAsync_ShouldThrow_WhenAmountInvalid()
    {
        var service = CreateService();

        var request = new PaymentRequestDto
        {
            Amount = 0,
            PaymentMode = "Cash"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddPaymentAsync(1, request));
    }

    [Fact]
    public async Task AddPaymentAsync_ShouldThrow_WhenPaymentModeMissing()
    {
        var service = CreateService();

        var request = new PaymentRequestDto
        {
            Amount = 500,
            PaymentMode = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddPaymentAsync(1, request));
    }

    [Fact]
    public async Task AddPaymentAsync_ShouldReturnInvoice()
    {
        var service = CreateService();

        var request = new PaymentRequestDto
        {
            Amount = 500,
            PaymentMode = "Cash"
        };

        _billingApiClientMock
            .Setup(x => x.AddPaymentAsync(1, request))
            .ReturnsAsync(GetInvoice());

        var result = await service.AddPaymentAsync(1, request);

        Assert.Equal(1, result.Id);
    }

    private static ReceptionPatientSummaryDto GetPatient()
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

    private static BookAppointmentResponseDto GetAppointment()
    {
        return new BookAppointmentResponseDto
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            DoctorId = 1,
            DoctorName = "Doctor 1",
            DepartmentId = 1,
            DepartmentName = "Department 1",
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(10, 30),
            VisitType = "OPD",
            Status = "Booked",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static InvoiceResponseDto GetInvoice()
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