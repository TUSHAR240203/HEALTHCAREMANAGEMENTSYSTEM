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
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/patients-api-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionMiddleware>();

app.Run();