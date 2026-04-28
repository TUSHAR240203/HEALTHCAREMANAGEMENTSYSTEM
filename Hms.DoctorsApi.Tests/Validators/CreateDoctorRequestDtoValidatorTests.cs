using FluentValidation.TestHelper;
using Hms.DoctorsApi.Tests.TestHelpers;
using Hms.DoctorsApi.Validators;

namespace Hms.DoctorsApi.Tests.Validators;

public class CreateDoctorRequestDtoValidatorTests
{
    private readonly CreateDoctorRequestDtoValidator _validator = new();

    [Fact]
    public void ValidRequest_ShouldNotHaveValidationErrors()
    {
        var result = _validator.TestValidate(TestData.CreateDoctorRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidRequest_ShouldValidateRequiredAndRangeFields()
    {
        var request = TestData.CreateDoctorRequest();
        request.FullName = "";
        request.Specialization = "";
        request.DepartmentId = 0;
        request.DepartmentName = "";
        request.ConsultationFee = -1;
        request.ExperienceYears = 100;
        request.Email = "not-an-email";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
        result.ShouldHaveValidationErrorFor(x => x.Specialization);
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        result.ShouldHaveValidationErrorFor(x => x.DepartmentName);
        result.ShouldHaveValidationErrorFor(x => x.ConsultationFee);
        result.ShouldHaveValidationErrorFor(x => x.ExperienceYears);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
