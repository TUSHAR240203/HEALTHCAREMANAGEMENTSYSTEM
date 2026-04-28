using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Interfaces.Repository;
using Hms.PatientsApi.Services;
using Hms.PatientsApi.Tests.TestHelpers;
using Moq;
using Xunit;

namespace Hms.PatientsApi.Tests.Services;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _repoMock;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _repoMock = new Mock<IPatientRepository>();
        _service = new PatientService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePatient()
    {
        var request = MockData.CreateRequest();

        _repoMock.Setup(x => x.ExistsByMobileAsync("9999999999", null)).ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Tushar Sharma", result.FullName);
        Assert.Equal("9999999999", result.MobileNumber);

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Hms.PatientsApi.Entities.Patient>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenMobileAlreadyExists()
    {
        var request = MockData.CreateRequest();

        _repoMock.Setup(x => x.ExistsByMobileAsync("9999999999", null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient()
    {
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MockData.Patient());

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(0));
    }

    [Fact]
    public async Task GetByUhidAsync_ShouldReturnPatient()
    {
        _repoMock.Setup(x => x.GetByUhidAsync("UHID001")).ReturnsAsync(MockData.Patient());

        var result = await _service.GetByUhidAsync("UHID001");

        Assert.NotNull(result);
        Assert.Equal("UHID001", result.UHID);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePatient()
    {
        var request = MockData.UpdateRequest();
        var patient = MockData.Patient();

        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _repoMock.Setup(x => x.ExistsByMobileAsync("8888888888", 1)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("8888888888", result.MobileNumber);

        _repoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenPatientNotFound()
    {
        var request = MockData.UpdateRequest();

        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Hms.PatientsApi.Entities.Patient?)null);

        var result = await _service.UpdateAsync(1, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnSearchResponse()
    {
        var request = new PatientSearchRequestDto();

        var response = new PatientSearchResponseDto
        {
            TotalCount = 1,
            Patients = new List<PatientResponseDto> { MockData.PatientResponse() }
        };

        _repoMock.Setup(x => x.SearchAsync(request)).ReturnsAsync(response);

        var result = await _service.SearchAsync(request);

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldReturnTrue_WhenPatientExists()
    {
        var patient = MockData.Patient();

        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

        var result = await _service.SoftDeleteAsync(1);

        Assert.True(result);
        Assert.True(patient.IsDeleted);

        _repoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldReturnFalse_WhenPatientNotFound()
    {
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Hms.PatientsApi.Entities.Patient?)null);

        var result = await _service.SoftDeleteAsync(1);

        Assert.False(result);
    }
}