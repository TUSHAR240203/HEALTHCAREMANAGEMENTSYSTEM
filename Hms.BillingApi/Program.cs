using Hms.BillingApi.Data;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Middleware;
using Hms.BillingApi.Repositories;
using Hms.BillingApi.Services;
using Hms.BillingApi.Mappings;
using Hms.BillingApi.Validators;

using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

// 🔥 Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 🔥 Use Serilog
builder.Host.UseSerilog();

// 🔥 Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 DB Context
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// 🔥 DI
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBillingService, BillingService>();

// 🔥 AutoMapper
builder.Services.AddAutoMapper(typeof(BillingProfile));

// 🔥 FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

// 🔥 Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// 🔥 Serilog request logging
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();