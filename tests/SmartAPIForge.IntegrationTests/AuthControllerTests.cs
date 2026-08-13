using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SmartAPIForge.Application.Auth.DTOs;
using Xunit;

namespace SmartAPIForge.IntegrationTests;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Register_NewUser_ReturnsTokensAndUser()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            email = UniqueEmail(),
            password = "Password1!",
            displayName = "Test User"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result!.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal("Test User", result.User?.DisplayName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var email = UniqueEmail();
        var payload = new { email, password = "Password1!", displayName = (string?)null };

        var first = await _client.PostAsJsonAsync("/auth/register", payload);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/auth/register", payload);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/auth/register", new { email, password = "Password1!", displayName = (string?)null });

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password = "Password1!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        Assert.True(result!.Succeeded);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/auth/register", new { email, password = "Password1!", displayName = (string?)null });

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokenPair()
    {
        var email = UniqueEmail();
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new { email, password = "Password1!", displayName = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);

        var response = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = registered!.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        Assert.True(result!.Succeeded);
        Assert.NotEqual(registered.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_UnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = "not-a-real-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsAuthenticatedUser()
    {
        var email = UniqueEmail();
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new { email, password = "Password1!", displayName = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", registered!.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.Equal(email, user!.Email);
    }
}
