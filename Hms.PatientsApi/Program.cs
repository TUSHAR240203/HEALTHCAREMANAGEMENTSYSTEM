<<<<<<< HEAD
=======
using FluentValidation;
using FluentValidation.AspNetCore;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.PatientsApi.Data;
using Hms.PatientsApi.Interfaces.Repository;
using Hms.PatientsApi.Interfaces.Services;
using Hms.PatientsApi.Middleware;
using Hms.PatientsApi.Repositories;
using Hms.PatientsApi.Services;
<<<<<<< HEAD
=======
using Hms.PatientsApi.Validators;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
builder.Services.AddControllers();
=======
builder.Services
    .AddControllers();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CreatePatientRequestValidator>();

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

var app = builder.Build();

<<<<<<< HEAD
=======
app.UseMiddleware<ExceptionMiddleware>();

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
<<<<<<< HEAD
app.UseMiddleware<ExceptionMiddleware>();
app.Run();
=======
app.Run();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
