using System.Text;
using GoldFieldsHR.Application.Account;
using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Application.Auth;
using GoldFieldsHR.Application.Certificates;
using GoldFieldsHR.Application.Common.Interfaces;
using GoldFieldsHR.Application.Dashboard;
using GoldFieldsHR.Application.Emergency;
using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Application.Incidents;
using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Application.Medical;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Performance;
using GoldFieldsHR.Application.Permits;
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
using Microsoft.IdentityModel.Tokens;

namespace GoldFieldsHR.Infrastructure;

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
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, Account.AccountService>();
        services.AddScoped<ICertificateService, Certificates.CertificateService>();
        services.AddScoped<IPerformanceService, Performance.PerformanceService>();
        services.AddScoped<ITimesheetService, Timesheet.TimesheetService>();
        services.AddScoped<IWorkShiftService, WorkShift.WorkShiftService>();
        services.AddScoped<ILeaveService, Leave.LeaveService>();
        services.AddScoped<IDashboardService, Dashboard.DashboardService>();
        services.AddScoped<ISafetyService, Safety.SafetyService>();
        services.AddScoped<IIncidentService, Incidents.IncidentService>();
        services.AddScoped<IPolicyService, Policies.PolicyService>();
        services.AddScoped<IMedicalService, Medical.MedicalService>();
        services.AddScoped<IPpeService, Ppe.PpeService>();
        services.AddScoped<IPermitService, Permits.PermitService>();
        services.AddScoped<IReportsService, Reports.ReportsService>();
        services.AddScoped<IEmergencyService, Emergency.EmergencyService>();
        services.AddScoped<IEmployeeDirectoryService, Employees.EmployeeDirectoryService>();
        services.AddScoped<IAttachmentService, Attachments.AttachmentService>();
        services.AddScoped<INotificationService, Notifications.NotificationService>();
        services.AddScoped<ISiteService, Sites.SiteService>();

        services.AddHostedService<Auth.RefreshTokenCleanupService>();

        return services;
    }
}
