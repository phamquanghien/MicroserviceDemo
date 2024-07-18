using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DemoMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace DemoMVC.Services
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() != null && context.User.Identity.IsAuthenticated)
            {
                var accessToken = context.Request.Cookies["AccessToken"];
                var refreshToken = context.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(accessToken);
                    var expiresIn = token.ValidTo;

                    if (expiresIn < DateTime.UtcNow)
                    {
                        var newToken = await RefreshTokenAsync(refreshToken, accessToken);
                        if (newToken != null)
                        {
                            context.Response.Cookies.Append("accessToken", newToken.Token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddMinutes(30)
                            });
                            context.Request.Headers["Authorization"] = $"Bearer {newToken.Token}";
                        }
                        else
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
        private async Task<TokenResponseVM> RefreshTokenAsync(string refreshToken, string accessToken)
        {
            var refreshTokenRequest = new { refreshToken = refreshToken };
            var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequest), Encoding.UTF8, "application/json");
            var request = CreateRequest(_configuration["ApiSettings:RefreshUrl"], HttpMethod.Post, content, accessToken);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenResponseVM>(responseData);
            }
            return null;
        }

        private HttpRequestMessage CreateRequest(string url, HttpMethod method, HttpContent content, string accessToken)
        {
            var request = new HttpRequestMessage(method, url);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }

    }
}
