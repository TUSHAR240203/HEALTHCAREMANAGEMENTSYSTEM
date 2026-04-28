using FluentAssertions;
using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.DTOs.Queue;
using Hms.DoctorsApi.Entities;
using Hms.DoctorsApi.Interfaces.Clients;
using Hms.DoctorsApi.Interfaces.Repository;
using Hms.DoctorsApi.Services;
using Hms.DoctorsApi.Tests.TestHelpers;
using Moq;
using Xunit;


namespace Hms.DoctorsApi.Tests.Services;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _repo = new();
    private readonly Mock<IAppointmentsApiClient> _appointments = new();
    private readonly Mock<IReceptionApiClient> _reception = new();

    private DoctorService CreateService() => new(_repo.Object, _appointments.Object, _reception.Object, TestData.CreateMapper());

    [Fact]
    public async Task CreateAsync_WhenLicenseIsUnique_CreatesActiveDoctorWithGeneratedCode()
    {
        Doctor? saved = null;
        _repo.Setup(x => x.ExistsByDoctorCodeAsync("DOC-DRJO", null)).ReturnsAsync(false);
        _repo.Setup(x => x.ExistsByLicenseNumberAsync("LIC-100", null)).ReturnsAsync(false);
        _repo.Setup(x => x.AddAsync(It.IsAny<Doctor>())).Callback<Doctor>(d => { d.Id = 1; saved = d; }).Returns(Task.CompletedTask);

        var result = await CreateService().CreateAsync(TestData.CreateDoctorRequest());

        result.Id.Should().Be(1);
        result.DoctorCode.Should().Be("DOC-DRJO");
        result.IsActive.Should().BeTrue();
        saved!.LicenseNumber.Should().Be("LIC-100");
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenLicenseAlreadyExists_ThrowsInvalidOperationException()
    {
        _repo.Setup(x => x.ExistsByDoctorCodeAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _repo.Setup(x => x.ExistsByLicenseNumberAsync("LIC-100", null)).ReturnsAsync(true);

        var act = () => CreateService().CreateAsync(TestData.CreateDoctorRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A doctor with this license number already exists.");
        _repo.Verify(x => x.AddAsync(It.IsAny<Doctor>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsInvalid_ThrowsArgumentException()
    {
        var act = () => CreateService().GetByIdAsync(0);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid doctor id.");
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenDoctorExists_MarksDoctorInactiveAndDeleted()
    {
        var doctor = TestData.Doctor();
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);

        var deleted = await CreateService().SoftDeleteAsync(1);

        deleted.Should().BeTrue();
        doctor.IsDeleted.Should().BeTrue();
        doctor.IsActive.Should().BeFalse();
        _repo.Verify(x => x.UpdateAsync(doctor), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddScheduleAsync_WhenScheduleOverlaps_ThrowsInvalidOperationException()
    {
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(TestData.Doctor());
        _repo.Setup(x => x.GetSchedulesAsync(1)).ReturnsAsync(new List<DoctorSchedule>
        {
            new() { Id = 5, DoctorId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), SlotDurationMinutes = 30 }
        });

        var request = new CreateDoctorScheduleRequestDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(13, 0),
            SlotDurationMinutes = 30
        };

        var act = () => CreateService().AddScheduleAsync(1, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A schedule already exists for this doctor during the selected time range.");
    }

    [Fact]
    public async Task AddLeaveAsync_WhenLeaveDateIsInPast_ThrowsArgumentException()
    {
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(TestData.Doctor());
        var request = new CreateDoctorLeaveRequestDto
        {
            LeaveDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-1),
            Reason = "Personal"
        };

        var act = () => CreateService().AddLeaveAsync(1, request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Leave date cannot be in the past.");
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenThereIsBreakAndBooking_ReturnsOnlyOpenSlotsOutsideBreak()
    {
        var date = TestData.Next(DayOfWeek.Monday);
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(TestData.Doctor());
        _repo.Setup(x => x.HasLeaveOnDateAsync(1, date)).ReturnsAsync(false);
        _repo.Setup(x => x.GetSchedulesAsync(1)).ReturnsAsync(new List<DoctorSchedule>
        {
            new()
            {
                DoctorId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(11, 0),
                BreakStartTime = new TimeOnly(10, 0),
                BreakEndTime = new TimeOnly(10, 30),
                SlotDurationMinutes = 30,
                IsActive = true
            }
        });
        _appointments.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(new List<AppointmentResponseDto>
        {
            new() { Id = 100, DoctorId = 1, AppointmentDate = date, SlotStartTime = new TimeOnly(9, 0), SlotEndTime = new TimeOnly(9, 30), Status = AppointmentStatus.Booked, VisitType = "OPD", UHID = "U1" }
        });

        var result = await CreateService().GetAvailableSlotsAsync(1, date, true);

        result.Slots.Should().HaveCount(3);
        result.Slots.Single(x => x.SlotStartTime == new TimeOnly(9, 0)).IsAvailable.Should().BeFalse();
        result.Slots.Should().NotContain(x => x.SlotStartTime == new TimeOnly(10, 0));
        result.Slots.Single(x => x.SlotStartTime == new TimeOnly(10, 30)).IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task StartAppointmentAsync_WhenQueueTokenIsCurrent_StartsQueueAndAppointment()
    {
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(TestData.Doctor());
        _appointments.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(new List<AppointmentResponseDto>
        {
            new() { Id = 50, DoctorId = 1, AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date), VisitType = "OPD", UHID = "U1" }
        });
        _reception.Setup(x => x.GetDoctorCurrentQueueAsync(1, It.IsAny<DateOnly>())).ReturnsAsync(new DoctorQueueCurrentResponseDto
        {
            QueueTokenId = 9,
            AppointmentId = 50,
            DoctorId = 1,
            Status = "Called",
            PatientName = "Patient",
            UHID = "U1"
        });
        _appointments.Setup(x => x.StartAppointmentAsync(50)).ReturnsAsync(new AppointmentResponseDto { Id = 50, DoctorId = 1, Status = AppointmentStatus.InConsultation, VisitType = "OPD", UHID = "U1" });

        var result = await CreateService().StartAppointmentAsync(1, 50);

        result!.Status.Should().Be(AppointmentStatus.InConsultation);
        _reception.Verify(x => x.StartQueueTokenAsync(9), Times.Once);
        _appointments.Verify(x => x.StartAppointmentAsync(50), Times.Once);
    }
}
