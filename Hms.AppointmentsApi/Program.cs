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
builder.Services.AddHttpClient<IDoctorsApiClient, DoctorsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorsApi"]!);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

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
