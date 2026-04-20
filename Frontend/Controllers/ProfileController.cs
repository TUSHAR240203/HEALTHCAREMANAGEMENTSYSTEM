using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

public class ProfileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    public ProfileController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> Index()
    {
        var token = User.FindFirst("access_token")?.Value;
        var client = _httpClientFactory.CreateClient("AuthApi");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var resp = await client.GetAsync("api/protected/profile");
        if (!resp.IsSuccessStatusCode) return Challenge(); // or handle errors
        var profile = await resp.Content.ReadFromJsonAsync<object>();
        return View(profile);
    }
}