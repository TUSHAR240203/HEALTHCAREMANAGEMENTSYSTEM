using FluentValidation.TestHelper;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Validators;

namespace Hms.DoctorsApi.Tests.Validators;

public class CreateDoctorScheduleRequestDtoValidatorTests
{
    private readonly CreateDoctorScheduleRequestDtoValidator _validator = new();

    [Fact]
    public void EndTimeBeforeStartTime_ShouldHaveValidationError()
    {
        var request = new CreateDoctorScheduleRequestDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(9, 0),
            SlotDurationMinutes = 30
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void BreakOutsideSchedule_ShouldHaveValidationError()
    {
        var request = new CreateDoctorScheduleRequestDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0),
            BreakStartTime = new TimeOnly(12, 30),
            BreakEndTime = new TimeOnly(13, 0),
            SlotDurationMinutes = 30
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x);
    }
}
