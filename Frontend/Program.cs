using Frontend.Infrastructure;
using Frontend.Services;

namespace Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthHeaderHandler>();

            var gatewayBaseUrl = builder.Configuration.GetSection("ApiSettings")["BaseUrl"] ?? "https://localhost:7000/";

            builder.Services.AddHttpClient<AuthGatewayService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<PatientGatewayService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<StaffUserGatewayService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<DoctorGatewayService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<IReceptionApiService, ReceptionApiService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<IAppointmentApiService, AppointmentApiService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<IBillingApiService, BillingApiService>(client => client.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
