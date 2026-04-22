using Frontend.Services;
using Hms.Web.Services;

namespace Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IReceptionApiService, ReceptionApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000/");
});

            builder.Services.AddHttpClient<AuthGatewayService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7000/");
            });

            builder.Services.AddHttpClient<PatientGatewayService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7000/");
            });

            builder.Services.AddHttpClient<IAppointmentApiService, AppointmentApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7000/");
            });
            builder.Services.AddHttpClient<IBillingApiService, BillingApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7000/");
            });

            builder.Services.AddDistributedMemoryCache();

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


