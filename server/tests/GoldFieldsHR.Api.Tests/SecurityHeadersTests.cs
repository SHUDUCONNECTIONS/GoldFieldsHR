namespace GoldFieldsHR.Api.Tests;

public class SecurityHeadersTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Response_IncludesBaselineSecurityHeaders()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
    }
}
