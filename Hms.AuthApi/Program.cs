<<<<<<< HEAD
using System.Text;
=======
using FluentValidation;
using FluentValidation.AspNetCore;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.AuthApi.Clients;
using Hms.AuthApi.Data;
using Hms.AuthApi.Interfaces.Clients;
using Hms.AuthApi.Interfaces.Repository;
using Hms.AuthApi.Interfaces.Services;
using Hms.AuthApi.Middleware;
using Hms.AuthApi.Repositories;
using Hms.AuthApi.Services;
<<<<<<< HEAD
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
=======
using Hms.AuthApi.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
<<<<<<< HEAD
=======

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VerifyOtpRequestValidator>();

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
<<<<<<< HEAD
=======
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IPatientUserLinkRepository, PatientUserLinkRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddHttpClient<IPatientsApiClient, PatientsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PatientsApi"]!);
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
<<<<<<< HEAD
            ClockSkew = TimeSpan.Zero
=======
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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