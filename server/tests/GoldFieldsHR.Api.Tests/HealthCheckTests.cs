using System.Net;

namespace GoldFieldsHR.Api.Tests;

public class HealthCheckTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task HealthEndpoint_ReturnsOkWithJsonStatus()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\"", body);
    }
}
