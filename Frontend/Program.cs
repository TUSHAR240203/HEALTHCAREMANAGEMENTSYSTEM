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

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthHeaderHandler>();

            builder.Services.Configure<ApiSettings>(
                builder.Configuration.GetSection("ApiSettings"));

            var gatewayBaseUrl =
                builder.Configuration.GetSection("ApiSettings")["BaseUrl"]
                ?? "https://localhost:7000/";

            if (!gatewayBaseUrl.EndsWith('/'))
                gatewayBaseUrl += "/";

            var gatewayUri = new Uri(gatewayBaseUrl);

            void ConfigureGatewayClient(HttpClient client)
            {
                client.BaseAddress = gatewayUri;
            }

            builder.Services.AddHttpClient("AuthApi", ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<AuthGatewayService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<PatientGatewayService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<StaffUserGatewayService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<DoctorGatewayService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IReceptionApiService, ReceptionApiService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IAppointmentApiService, AppointmentApiService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IBillingApiService, BillingApiService>(ConfigureGatewayClient)
                .AddHttpMessageHandler<AuthHeaderHandler>();

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