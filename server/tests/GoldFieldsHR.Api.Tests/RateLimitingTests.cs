using System.Net;
using System.Net.Http.Json;

namespace GoldFieldsHR.Api.Tests;

public class RateLimitingTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task AuthEndpoint_Returns429AfterExceedingLimit()
    {
        var client = factory.CreateClient();
        HttpStatusCode? lastStatus = null;

        for (var i = 0; i < 11; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "ratelimit@example.com",
                password = "whatever",
            });
            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
    }
}
