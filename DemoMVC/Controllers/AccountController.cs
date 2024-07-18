using System.Text.Json;
using DemoMVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var loginUrl = _configuration["ApiSettings:LoginUrl"];
            var response = await client.PostAsJsonAsync(loginUrl, model);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<TokenResponseVM>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var token = responseData?.Token;
                var refreshToken = response.Headers.GetValues("Set-Cookie")
                             .FirstOrDefault(c => c.Contains("refreshToken"))
                             ?.Split(';')[0]
                             ?.Split('=')[1];
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve tokens.");
                    return View(model);
                }
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                Response.Cookies.Append("AccessToken", token, cookieOptions);
                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

                return RedirectToAction("Index", "Employee");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    }
}