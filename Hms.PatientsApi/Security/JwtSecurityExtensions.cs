using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hms.PatientsApi.Security;

public static class JwtSecurityExtensions
{
    public static IServiceCollection AddHmsJwtSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? configuration["JwtSettings:Secret"]
            ?? configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JWT signing key is missing. Configure Jwt:Key or JwtSettings:Secret.");

        var issuer = configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"];
        var audience = configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ReceptionAccess", policy => policy.RequireRole("Admin", "Receptionist"));
            options.AddPolicy("DoctorAccess", policy => policy.RequireRole("Admin", "Doctor"));
            options.AddPolicy("PatientAccess", policy => policy.RequireRole("Admin", "Patient"));
            options.AddPolicy("BillingAccess", policy => policy.RequireRole("Admin", "Receptionist"));
        });

        return services;
    }

    public static void AddJwtSwaggerSecurity(this SwaggerGenOptions options, string title)
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter only the JWT token. Swagger sends it as Bearer <token>."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
