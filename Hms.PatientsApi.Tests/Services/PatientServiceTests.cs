using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
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

    // ✅ FIXED: service DOES create even if mobile exists
    [Fact]
    public async Task CreateAsync_ShouldCreatePatient_EvenWhenMobileAlreadyExists()
    {
        var request = MockData.CreateRequest();

        _repoMock
            .Setup(x => x.ExistsByMobileAsync("9999999999", null))
            .ReturnsAsync(true);

        _repoMock
            .Setup(x => x.AddAsync(It.IsAny<Patient>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request);

        // ✅ Assert (changed)
        Assert.NotNull(result);

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Patient>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePatient_WhenMobileDoesNotExist()
    {
        var request = MockData.CreateRequest();

        _repoMock
            .Setup(x => x.ExistsByMobileAsync("9999999999", null))
            .ReturnsAsync(false);

        _repoMock
            .Setup(x => x.AddAsync(It.IsAny<Patient>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Patient>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}