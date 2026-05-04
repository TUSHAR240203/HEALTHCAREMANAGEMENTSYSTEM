using Hms.BillingApi.Security;
using Hms.BillingApi.Data;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Middleware;
using Hms.BillingApi.Repositories;
using Hms.BillingApi.Services;
using Hms.BillingApi.Mappings;
using Hms.BillingApi.Validators;
using Hms.BillingApi.Finance;
//using Hms.BillingApi.Clients;
using Microsoft.Extensions.Http.Resilience;
using Polly;

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
builder.Services.AddSwaggerGen(options => options.AddJwtSwaggerSecurity("Hms.BillingApi"));
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();
// 🔥 DB Context
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// 🔥 DI
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IFinanceCalculator, FinanceCalculator>();
builder.Services.AddScoped<IServiceCatalogRepository, ServiceCatalogRepository>();

// 🔥 DoctorsApi HttpClient (for fetching ConsultationFee) + Polly retry
builder.Services.AddHttpClient<IDoctorsApiClient, DoctorsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorsApi"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("doctors-api-retry", (pipeline, ctx) =>
{
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,   // 2s → 4s → 8s
        UseJitter = true,
        ShouldHandle = static args =>
            ValueTask.FromResult(
                args.Outcome.Exception is HttpRequestException ||
                (args.Outcome.Result?.StatusCode >= System.Net.HttpStatusCode.InternalServerError)),
        OnRetry = static args =>
        {
            Log.Warning(
                "DoctorsApi retry attempt {Attempt} after {Delay}s. Reason: {Reason}",
                args.AttemptNumber + 1,
                args.RetryDelay.TotalSeconds,
                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
            return ValueTask.CompletedTask;
        }
    });
});

// 🔥 AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(BillingProfile).Assembly);
// 🔥 FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddHmsJwtSecurity(builder.Configuration);

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
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }