using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Repositories;
using Hms.ReceptionApi.Tests.TestHelpers;
using Xunit;

namespace Hms.ReceptionApi.Tests.Repositories;

public class CheckInRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddCheckIn()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new CheckInRepository(context);

        var checkIn = new PatientCheckIn
        {
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow,
            TokenNumber = 1,
            Status = "CheckedIn"
        };

        await repo.AddAsync(checkIn);
        await repo.SaveChangesAsync();

        Assert.Single(context.PatientCheckIns);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCheckIn_WhenExists()
    {
        using var context = TestDbContextFactory.Create();

        var checkIn = new PatientCheckIn
        {
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow,
            TokenNumber = 1,
            Status = "CheckedIn",
            IsDeleted = false
        };

        context.PatientCheckIns.Add(checkIn);
        await context.SaveChangesAsync();

        var repo = new CheckInRepository(context);

        var result = await repo.GetByIdAsync(checkIn.Id);

        Assert.NotNull(result);
        Assert.Equal(checkIn.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCheckInIsDeleted()
    {
        using var context = TestDbContextFactory.Create();

        var checkIn = new PatientCheckIn
        {
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow,
            TokenNumber = 1,
            Status = "CheckedIn",
            IsDeleted = true
        };

        context.PatientCheckIns.Add(checkIn);
        await context.SaveChangesAsync();

        var repo = new CheckInRepository(context);

        var result = await repo.GetByIdAsync(checkIn.Id);

        Assert.Null(result);
    }
}