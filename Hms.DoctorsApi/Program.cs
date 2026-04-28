using FluentValidation;
using FluentValidation.AspNetCore;
using Hms.DoctorsApi.Clients;
using Hms.DoctorsApi.Common;
using Hms.DoctorsApi.Data;
using Hms.DoctorsApi.Interfaces.Clients;
using Hms.DoctorsApi.Interfaces.Repository;
using Hms.DoctorsApi.Interfaces.Services;
using Hms.DoctorsApi.Middleware;
using Hms.DoctorsApi.Repositories;
using Hms.DoctorsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/doctors-api-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request value." : e.ErrorMessage))
                .ToList();

            var response = ApiResponse<object>.Fail("Validation failed.", errors, context.HttpContext.TraceIdentifier);
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<DoctorsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddHttpClient<IAppointmentsApiClient, AppointmentsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AppointmentsApi"]
        ?? throw new InvalidOperationException("AppointmentsApi base URL is missing."));
});

builder.Services.AddHttpClient<IReceptionApiClient, ReceptionApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ReceptionApi"]
        ?? throw new InvalidOperationException("ReceptionApi base URL is missing."));
});

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
