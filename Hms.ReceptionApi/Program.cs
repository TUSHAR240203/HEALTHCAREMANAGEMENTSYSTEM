using FluentValidation;
using FluentValidation.AspNetCore;
using Hms.ReceptionApi.Clients;
using Hms.ReceptionApi.Data;
using Hms.ReceptionApi.Interfaces.Clients;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Interfaces.Services;
using Hms.ReceptionApi.Middleware;
using Hms.ReceptionApi.Repositories;
using Hms.ReceptionApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddFluentValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value != null && x.Value.Errors.Count > 0)
            .Select(x => new
            {
                field = x.Key,
                errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
            })
            .ToList();

        return new BadRequestObjectResult(new
        {
            statusCode = 400,
            message = "Validation failed.",
            errors
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ReceptionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();

builder.Services.AddHttpClient<IPatientsApiClient, PatientsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PatientsApi"]!);
});

builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BillingApi"]!);
});

builder.Services.AddHttpClient<IAppointmentsApiClient, AppointmentsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AppointmentsApi"]!);
});

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthApi"]!);
});

builder.Services.AddHttpClient<ILocationApiClient, LocationApiClient>(client =>
{
    client.BaseAddress = new Uri("https://countriesnow.space");
});

var app = builder.Build();

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