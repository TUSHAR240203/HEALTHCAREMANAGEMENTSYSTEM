using Hms.DoctorsApi.Clients;
using Hms.DoctorsApi.Data;
using Hms.DoctorsApi.Interfaces.Clients;
using Hms.DoctorsApi.Interfaces.Repository;
using Hms.DoctorsApi.Interfaces.Services;
using Hms.DoctorsApi.Middleware;
using Hms.DoctorsApi.Repositories;
using Hms.DoctorsApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DoctorsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
