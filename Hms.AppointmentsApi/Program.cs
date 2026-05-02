using Hms.AppointmentsApi.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hms.AppointmentsApi.Common;
using Hms.AppointmentsApi.Data;
using Hms.AppointmentsApi.Interfaces.Repository;
using Hms.AppointmentsApi.Interfaces.Services;
using Hms.AppointmentsApi.Middleware;
using Hms.AppointmentsApi.Repositories;
using Hms.AppointmentsApi.Services;
using Hms.AppointmentsApi.Validators.Appointments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Hms.AppointmentsApi.Clients;
using Hms.AppointmentsApi.Interfaces.Clients;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
    };
});


// ✅ Doctors API client (FIXED — no AddRetry issue)
builder.Services.AddHttpClient<IDoctorsApiClient, DoctorsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorsApi"]!);
})
.AddStandardResilienceHandler();


// ✅ Billing API client (FIXED)
builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BillingApi"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.AddJwtSwaggerSecurity("Hms.AppointmentsApi"));

builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

// Background service
builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHmsJwtSecurity(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();