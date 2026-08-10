using System.Net;
using System.Net.Http.Json;

namespace GoldFieldsHR.Api.Tests;

public class ValidationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_WithMalformedBody_Returns400WithFieldErrors()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "short",
            firstName = "",
            lastName = "",
            employeeNumber = "",
            jobTitle = "",
            role = 0,
            siteId = Guid.Empty,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email", body);
        Assert.Contains("Password", body);
    }

    [Fact]
    public async Task Login_WithWellFormedBody_IsNotRejectedByValidation()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "someone@example.com",
            password = "whatever-password",
        });

        // No account exists against the InMemory test DB, so this should fail authentication (401),
        // never a validation (400) — proving well-formed input passes the validation filter through.
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
