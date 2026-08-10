using System.Net;

namespace GoldFieldsHR.Api.Tests;

public class ExceptionHandlerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task UnhandledException_ReturnsGenericProblemDetailsWithoutLeakingDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/__test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("An unexpected error occurred.", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("Deliberate test exception", body);
        Assert.DoesNotContain("at GoldFieldsHR", body); // no stack trace frames
    }
}
