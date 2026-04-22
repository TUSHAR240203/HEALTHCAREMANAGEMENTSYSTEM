using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient for your Auth API
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri("https://auth.example.com/"); // replace with your auth API base URL
    client.DefaultRequestHeaders.Accept.Add(new("application/json"));
});

// Cookie authentication for MVC app
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "MyAppAuth";
        options.Cookie.SameSite = SameSiteMode.Lax; // adjust for cross-site scenarios
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();