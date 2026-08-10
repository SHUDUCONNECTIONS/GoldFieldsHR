using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GoldFieldsHR.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing", not "Development" — Program.cs only runs EF migrations/seeding when
        // IsDevelopment() is true, and migrations aren't supported against the InMemory provider.
        builder.UseEnvironment("Testing");

        // UseSetting (not ConfigureAppConfiguration) — Program.cs reads builder.Configuration
        // synchronously before Build(), so settings must be in place before that point.
        builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused;Username=unused;Password=unused");
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-at-least-32-characters-long!");
        builder.UseSetting("Jwt:Issuer", "GoldFieldsHR.Tests");
        builder.UseSetting("Jwt:Audience", "GoldFieldsHR.Tests.Client");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://allowed.test");

        builder.ConfigureServices(services =>
        {
            // AddDbContext registers more than just DbContextOptions<T> — Npgsql's own provider
            // services (added by AddInfrastructure's UseNpgsql call) must also go, or EF Core sees
            // two providers registered and throws at first DbContext use.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        });
    }
}
