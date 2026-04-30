using FluentValidation;
using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Clients;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Interfaces.Services;
using Hms.ReceptionApi.Validators;
namespace Hms.ReceptionApi.Services;

public class ReceptionService : IReceptionService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IAppointmentsApiClient _appointmentsApiClient;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IBillingApiClient _billingApiClient;

    public ReceptionService(
        IPatientsApiClient patientsApiClient,
        IAppointmentsApiClient appointmentsApiClient,
        IBillingApiClient billingApiClient,
        ICheckInRepository checkInRepository,
        IQueueRepository queueRepository)
    {
        _patientsApiClient = patientsApiClient;
        _appointmentsApiClient = appointmentsApiClient;
        _billingApiClient = billingApiClient;
        _checkInRepository = checkInRepository;
        _queueRepository = queueRepository;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        if (request.AppointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        if (request.ConsultationFee < 0)
            throw new ArgumentException("Consultation fee cannot be negative.");

        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var invoiceRequest = new
        {
            patientId = patient.PatientId,
            uhid = patient.UHID,
            appointmentId = request.AppointmentId,
            consultationFee = request.ConsultationFee
        };

        return await _billingApiClient.CreateInvoiceAsync(invoiceRequest);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        return await _billingApiClient.GetInvoiceByIdAsync(invoiceId);
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        return await _billingApiClient.GetInvoicesByPatientIdAsync(patientId);
    }

    public async Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (string.IsNullOrWhiteSpace(request.ServiceName))
            throw new ArgumentException("ServiceName is required.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        return await _billingApiClient.AddInvoiceItemAsync(invoiceId, request);
    }

    public async Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.PaymentMode))
            throw new ArgumentException("PaymentMode is required.");

        return await _billingApiClient.AddPaymentAsync(invoiceId, request);
    }

    public async Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(
     ReceptionPatientSearchRequestDto request)
    {
        var result = await _patientsApiClient.SearchPatientsAsync(request);

        if (result?.Patients == null)
        {
            return new ReceptionPatientSearchResponseDto
            {
                Patients = new List<ReceptionPatientSummaryDto>()
            };
        }

        foreach (var patient in result.Patients)
        {
            var effectivePatientId = patient.PatientId > 0
                ? patient.PatientId
                : patient.Id;

            patient.Id = effectivePatientId;
            patient.PatientId = effectivePatientId;
        }

        return result;
    }

    public async Task<ReceptionPatientSummaryDto> RegisterPatientAsync(RegisterPatientByReceptionRequestDto request)
    {
        var patient = await _patientsApiClient.RegisterPatientAsync(request);

        // Portal activation OTP was removed. Patient portal user is created automatically
        // when the patient requests their first login OTP.

        return patient;
    }

    public async Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        return await _patientsApiClient.GetPatientSummaryAsync(patientId);
    }

    public async Task<VerifyPatientResponseDto> VerifyPatientAsync(int patientId, VerifyPatientRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(patientId);

        if (patient == null)
        {
            return new VerifyPatientResponseDto
            {
                PatientId = patientId,
                Verified = false,
                Message = "Patient not found."
            };
        }

        var dobMatched = !request.DateOfBirth.HasValue || patient.DateOfBirth == request.DateOfBirth.Value;
        var mobileMatched = string.IsNullOrWhiteSpace(request.MobileNumber) || patient.MobileNumber == request.MobileNumber.Trim();

        var verified = dobMatched && mobileMatched;

        return new VerifyPatientResponseDto
        {
            PatientId = patientId,
            Verified = verified,
            Message = verified ? "Patient identity verified." : "Patient verification failed."
        };
    }

    public Task ResendPortalActivationAsync(int patientId, ResendPortalActivationRequestDto request)
    {
        throw new NotSupportedException("Portal activation OTP was removed. Use patient send-login-otp for first-time and later logins.");
    }

    public async Task<BookAppointmentResponseDto> BookAppointmentAsync(BookAppointmentRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");


        //IValidator<BookAppointmentRequestDto> validator1 = new BookAppointmentRequestDtoValidator();
        /////BookAppointmentRequestDtoValidator validator = new BookAppointmentRequestDtoValidator();
        ///var res = await validator1.ValidateAsync(request);
        //if (!res.IsValid)
        //{
        //   throw new ValidationException(res.Errors);
        //}
        var appointmentRequest = new AppointmentCreateRequestDto
        {
            PatientId = patient.PatientId,   
            UHID = patient.UHID,
            DoctorId = request.DoctorId,
            DoctorName = $"Doctor {request.DoctorId}",
            DepartmentId = request.DepartmentId,
            DepartmentName = $"Department {request.DepartmentId}",
            AppointmentDate = request.AppointmentDate,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            VisitType = request.VisitType,
            ReasonForVisit = request.ReasonForVisit,
            IsTeleConsultation = request.IsTeleConsultation
        };

        var result = await _appointmentsApiClient.BookAppointmentAsync(appointmentRequest);

        return result;
    }

    public async Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequestDto request)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        return await _appointmentsApiClient.RescheduleAppointmentAsync(appointmentId, request);
    }

    public async Task<BookAppointmentResponseDto> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDto request)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        return await _appointmentsApiClient.CancelAppointmentAsync(appointmentId, request);
    }

    public async Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto request)
    {
        if (request.PatientId <= 0)
        {
            throw new InvalidOperationException("Patient is required for check-in.");
        }

        if (request.AppointmentId <= 0)
        {
            throw new InvalidOperationException("Appointment is required for check-in.");
        }

        if (request.DoctorId <= 0)
        {
            throw new InvalidOperationException("Doctor is required for check-in.");
        }

        if (request.DepartmentId <= 0)
        {
            throw new InvalidOperationException("Department is required for check-in.");
        }

        var queueDate = request.QueueDate == default
            ? DateOnly.FromDateTime(DateTime.Today)
            : request.QueueDate;

        var checkInTimeUtc = request.CheckInTimeUtc == default
            ? DateTime.UtcNow
            : request.CheckInTimeUtc;

        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);

        if (patient == null)
        {
            throw new InvalidOperationException("Patient not found.");
        }

        var existingToken = await _queueRepository.GetByAppointmentIdAsync(request.AppointmentId);

        if (existingToken != null)
        {
            throw new InvalidOperationException("This appointment is already checked in.");
        }

        var nextToken = await _queueRepository.GetNextTokenNumberAsync(
            request.DepartmentId,
            queueDate);

        var checkIn = new PatientCheckIn
        {
            PatientId = request.PatientId,
            UHID = patient.UHID,
            AppointmentId = request.AppointmentId,
            DoctorId = request.DoctorId,
            DepartmentId = request.DepartmentId,
            CheckInTimeUtc = checkInTimeUtc,
            TokenNumber = nextToken,
            Status = "CheckedIn",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };

        await _checkInRepository.AddAsync(checkIn);

        var queueToken = new QueueToken
        {
            DepartmentId = request.DepartmentId,
            QueueDate = queueDate,
            TokenNumber = nextToken,
            PatientId = request.PatientId,
            UHID = patient.UHID,
            PatientName = patient.FullName,
            AppointmentId = request.AppointmentId,
            DoctorId = request.DoctorId,
            Status = "Waiting",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };

        await _queueRepository.AddAsync(queueToken);

        await _checkInRepository.SaveChangesAsync();
        await _queueRepository.SaveChangesAsync();

        return new CheckInResponseDto
        {
            CheckInId = checkIn.Id,
            AppointmentId = checkIn.AppointmentId,
            PatientId = checkIn.PatientId,
            UHID = checkIn.UHID,
            TokenNumber = checkIn.TokenNumber,
            QueuePosition = checkIn.TokenNumber,
            Status = checkIn.Status,
            Message = "Patient checked in successfully."
        };
    }

    public async Task<CheckInResponseDto?> GetCheckInByIdAsync(int checkInId)
    {
        var checkIn = await _checkInRepository.GetByIdAsync(checkInId);
        if (checkIn == null)
            return null;

        return new CheckInResponseDto
        {
            CheckInId = checkIn.Id,
            AppointmentId = checkIn.AppointmentId,
            PatientId = checkIn.PatientId,
            UHID = checkIn.UHID,
            TokenNumber = checkIn.TokenNumber,
            QueuePosition = checkIn.TokenNumber,
            Status = checkIn.Status,
            Message = "Check-in record fetched successfully."
        };
    }

    public async Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date)
    {
        var items = await _queueRepository.GetDepartmentQueueAsync(departmentId, date);

        return new DepartmentQueueResponseDto
        {
            DepartmentId = departmentId,
            DepartmentName = $"Department {departmentId}",
            Date = date,
            Queue = items.Select(x => new QueueItemDto
            {
                TokenNumber = x.TokenNumber,
                PatientId = x.PatientId,
                UHID = x.UHID,
                PatientName = x.PatientName,
                Status = x.Status
            }).ToList()
        };
    }
    public async Task<List<TodayAppointmentForCheckInDto>> GetTodayAppointmentsForCheckInAsync(DateOnly date)
    {
        var result = await _appointmentsApiClient.SearchAsync(new AppointmentSearchRequestDto
        {
            AppointmentDate = date,
            PageNumber = 1,
            PageSize = 5
        });

        var appointments = result.Appointments
            .Where(x =>
                string.Equals(x.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Status, "Booked", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.SlotStartTime)
            .ToList();

        var list = new List<TodayAppointmentForCheckInDto>();

        foreach (var appointment in appointments)
        {
            string? patientName = null;

            try
            {
                var patient = await _patientsApiClient.GetPatientSummaryAsync(appointment.PatientId);
                patientName = patient?.FullName;
            }
            catch
            {
                patientName = $"Patient #{appointment.PatientId}";
            }

            list.Add(new TodayAppointmentForCheckInDto
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                UHID = appointment.UHID,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.DoctorName,
                DepartmentId = appointment.DepartmentId,
                DepartmentName = appointment.DepartmentName,
                AppointmentDate = appointment.AppointmentDate,
                SlotStartTime = appointment.SlotStartTime,
                SlotEndTime = appointment.SlotEndTime,
                Status = appointment.Status
            });
        }

        return list;
    }
}