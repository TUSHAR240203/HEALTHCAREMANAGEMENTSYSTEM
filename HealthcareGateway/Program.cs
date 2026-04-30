using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvc", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7007",
                "http://localhost:5261",
                "https://localhost:44310")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowMvc");

app.MapGet("/", () => "Healthcare Gateway Running");
app.MapReverseProxy();

app.Run();
