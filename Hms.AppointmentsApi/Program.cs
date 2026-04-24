<<<<<<< HEAD
using Hms.AppointmentsApi.Clients;
using Hms.AppointmentsApi.Data;
using Hms.AppointmentsApi.Interfaces.Clients;
=======
using Hms.AppointmentsApi.Data;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.AppointmentsApi.Interfaces.Repository;
using Hms.AppointmentsApi.Interfaces.Services;
using Hms.AppointmentsApi.Middleware;
using Hms.AppointmentsApi.Repositories;
using Hms.AppointmentsApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
<<<<<<< HEAD
builder.Services.AddHttpClient<IDoctorsApiClient, DoctorsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorsApi"]!);
});
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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