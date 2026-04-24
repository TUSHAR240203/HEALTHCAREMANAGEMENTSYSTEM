using FluentValidation;
using FluentValidation.AspNetCore;
using Hms.PatientsApi.Data;
using Hms.PatientsApi.Interfaces.Repository;
using Hms.PatientsApi.Interfaces.Services;
using Hms.PatientsApi.Middleware;
using Hms.PatientsApi.Repositories;
using Hms.PatientsApi.Services;
using Hms.PatientsApi.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CreatePatientRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
