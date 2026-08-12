using FluentValidation;
using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Api.HealthChecks;
using GoldFieldsHR.Application.Auth;
using GoldFieldsHR.Infrastructure;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Single-stage Serilog configuration (no pre-Build bootstrap logger): a bootstrap
// ReloadableLogger conflicts with WebApplicationFactory, which invokes this entry point
// twice (once to discover the host, once for real) — the second call fails with
// "The logger is already frozen." against a bootstrap logger.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequest>();
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Partitioned per client IP so one caller exhausting the limit can't lock everyone
    // else out of auth endpoints (AddFixedWindowLimiter alone would share a single global bucket).
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        }));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

// Skipped only for the "Testing" environment (WebApplicationFactory integration tests), which
// swaps in an EF InMemory provider that doesn't support relational migrations. Runs in both
// Development and Production so a deployed instance gets its schema and bootstrap HR/Executive
// accounts without a separate manual migration step.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new { name = entry.Key, status = entry.Value.Status.ToString() }),
        });
    },
});

if (app.Environment.IsEnvironment("Testing"))
{
    // Lets integration tests prove the global exception handler suppresses stack traces;
    // unreachable outside the "Testing" environment used by GoldFieldsHR.Api.Tests.
    app.MapGet("/__test/throw", () =>
    {
        throw new InvalidOperationException("Deliberate test exception.");
#pragma warning disable CS0162 // Unreachable code — needed so the lambda has an inferable Ok() return type.
        return Results.Ok();
#pragma warning restore CS0162
    });
}

app.Run();

// Exposes the top-level-statements entry point so WebApplicationFactory<Program> can host it in tests.
public partial class Program;
