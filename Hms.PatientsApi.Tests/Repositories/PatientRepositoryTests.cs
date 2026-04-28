using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Repositories;
using Hms.PatientsApi.Tests.TestHelpers;
using Xunit;

namespace Hms.PatientsApi.Tests.Repositories;

public class PatientRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddPatient()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new PatientRepository(context);

        var patient = MockData.Patient();

        await repo.AddAsync(patient);
        await repo.SaveChangesAsync();

        Assert.Single(context.Patients);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient()
    {
        using var context = TestDbContextFactory.Create();

        context.Patients.Add(MockData.Patient());
        await context.SaveChangesAsync();

        var repo = new PatientRepository(context);

        var result = await repo.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Tushar Sharma", result.FullName);
    }

    [Fact]
    public async Task GetByUhidAsync_ShouldReturnPatient()
    {
        using var context = TestDbContextFactory.Create();

        context.Patients.Add(MockData.Patient());
        await context.SaveChangesAsync();

        var repo = new PatientRepository(context);

        var result = await repo.GetByUhidAsync("UHID001");

        Assert.NotNull(result);
        Assert.Equal("UHID001", result.UHID);
    }

    [Fact]
    public async Task ExistsByMobileAsync_ShouldReturnTrue_WhenMobileExists()
    {
        using var context = TestDbContextFactory.Create();

        context.Patients.Add(MockData.Patient());
        await context.SaveChangesAsync();

        var repo = new PatientRepository(context);

        var result = await repo.ExistsByMobileAsync("9999999999");

        Assert.True(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnPatients()
    {
        using var context = TestDbContextFactory.Create();

        context.Patients.Add(MockData.Patient());
        await context.SaveChangesAsync();

        var repo = new PatientRepository(context);

        var request = new PatientSearchRequestDto
        {
            Name = "Tushar",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.SearchAsync(request);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Patients);
    }
}