using System.Runtime.CompilerServices;
using System.Text;
using GoldFieldsHR.Application.Account;
using GoldFieldsHR.Application.Acknowledgments;
using GoldFieldsHR.Application.Announcements;
using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Application.Auth;
using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Application.Certificates;
using GoldFieldsHR.Application.Common.Interfaces;
using GoldFieldsHR.Application.Dashboard;
using GoldFieldsHR.Application.Documents;
using GoldFieldsHR.Application.Emergency;
using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Application.Incidents;
using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Application.LegalAppointments;
using GoldFieldsHR.Application.Medical;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Performance;
using GoldFieldsHR.Application.Policies;
using GoldFieldsHR.Application.Ppe;
using GoldFieldsHR.Application.Reports;
using GoldFieldsHR.Application.Safety;
using GoldFieldsHR.Application.Sites;
using GoldFieldsHR.Application.Timesheet;
using GoldFieldsHR.Application.WorkShift;
using GoldFieldsHR.Infrastructure.Auth;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

namespace GoldFieldsHR.Infrastructure;

internal static class QuestPdfLicenseInitializer
{
    // Runs once when this assembly loads — covers both the API host (via AddInfrastructure)
    // and unit tests that construct services like BoardTaskService directly without DI.
    //
    // Community license is free only for organizations under $1M USD annual revenue.
    // Confirm Goldfields qualifies, or purchase a commercial QuestPDF license, before
    // shipping this to production — see https://www.questpdf.com/pricing.html.
#pragma warning disable CA2255 // Deliberate: guarantees the license is set for every host (API, tests), not just DI-based startup.
    [ModuleInitializer]
    internal static void SetLicense() => QuestPDF.Settings.License = LicenseType.Community;
#pragma warning restore CA2255
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<Attachments.FileStorageSettings>(configuration.GetSection(Attachments.FileStorageSettings.SectionName));
        services.Configure<Timesheet.ClockingParserSettings>(configuration.GetSection(Timesheet.ClockingParserSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };

                // WebSocket upgrade requests can't carry a custom Authorization header, so the
                // SignalR client sends the token as an access_token query param instead. Only
                // trust that fallback for hub paths — everywhere else still requires a bearer header.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAnnouncementService, Announcements.AnnouncementService>();
        services.AddScoped<IAccountService, Account.AccountService>();
        services.AddScoped<ICertificateService, Certificates.CertificateService>();
        services.AddScoped<IPerformanceService, Performance.PerformanceService>();
        services.AddScoped<ITimesheetService, Timesheet.TimesheetService>();
        services.AddScoped<IWorkShiftService, WorkShift.WorkShiftService>();
        services.AddScoped<IPostedScheduleDocumentService, WorkShift.PostedScheduleDocumentService>();
        services.AddScoped<ILeaveService, Leave.LeaveService>();
        services.AddScoped<IDashboardService, Dashboard.DashboardService>();
        services.AddScoped<ISafetyService, Safety.SafetyService>();
        services.AddScoped<IIncidentService, Incidents.IncidentService>();
        services.AddScoped<IPolicyService, Policies.PolicyService>();
        services.AddScoped<IMedicalService, Medical.MedicalService>();
        services.AddScoped<IPpeService, Ppe.PpeService>();
        services.AddScoped<ILegalAppointmentService, LegalAppointments.LegalAppointmentService>();
        services.AddScoped<IReportsService, Reports.ReportsService>();
        services.AddScoped<IEmergencyService, Emergency.EmergencyService>();
        services.AddScoped<IEmployeeDirectoryService, Employees.EmployeeDirectoryService>();
        services.AddScoped<IAttachmentService, Attachments.AttachmentService>();
        services.AddScoped<IAcknowledgmentService, Acknowledgments.AcknowledgmentService>();
        services.AddScoped<INotificationService, Notifications.NotificationService>();
        services.AddScoped<ISiteService, Sites.SiteService>();
        services.AddScoped<IBoardService, Boards.BoardService>();
        services.AddScoped<IBoardTaskService, Boards.BoardTaskService>();
        services.AddScoped<IDocumentSigningService, Documents.DocumentSigningService>();

        services.AddHttpClient<IClockingReportParserService, Timesheet.ClockingReportParserService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<Timesheet.ClockingParserSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            // Multi-page clocking reports can take a while to parse (pdfplumber's word
            // extraction is the slow part) - give it much more room than a typical API call.
            client.Timeout = TimeSpan.FromMinutes(3);
        });

        services.AddHostedService<Auth.RefreshTokenCleanupService>();

        return services;
    }
}
