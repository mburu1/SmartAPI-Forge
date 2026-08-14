using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartAPIForge.Api.Controllers;
using Xunit;

namespace SmartAPIForge.IntegrationTests;

public class SystemControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetStatus_ReturnsHealthyWithConfiguredProvider()
    {
        var response = await _client.GetAsync("/system/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var status = await response.Content.ReadFromJsonAsync<SystemStatusResponse>(JsonOptions);
        Assert.NotNull(status);
        Assert.Equal("Healthy", status!.Status);
        Assert.Equal("InMemory", status.DatabaseProvider);
        Assert.Equal("Development", status.Environment);
    }
}
