using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<TimesheetCorrectionRequest> TimesheetCorrectionRequests => Set<TimesheetCorrectionRequest>();
    public DbSet<ShiftChangeRequest> ShiftChangeRequests => Set<ShiftChangeRequest>();
    public DbSet<PostedScheduleDocument> PostedScheduleDocuments => Set<PostedScheduleDocument>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PreShiftSafetyCheck> PreShiftSafetyChecks => Set<PreShiftSafetyCheck>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyAcknowledgment> PolicyAcknowledgments => Set<PolicyAcknowledgment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<MedicalExamination> MedicalExaminations => Set<MedicalExamination>();
    public DbSet<PpeRequest> PpeRequests => Set<PpeRequest>();
    public DbSet<LegalAppointment> LegalAppointments => Set<LegalAppointment>();
    public DbSet<EmergencyAlert> EmergencyAlerts => Set<EmergencyAlert>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Acknowledgment> Acknowledgments => Set<Acknowledgment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<BoardTask> BoardTasks => Set<BoardTask>();
    public DbSet<KpiTemplate> KpiTemplates => Set<KpiTemplate>();
    public DbSet<KpiTemplateCategory> KpiTemplateCategories => Set<KpiTemplateCategory>();
    public DbSet<KpiTemplateItem> KpiTemplateItems => Set<KpiTemplateItem>();
    public DbSet<KpiAppraisal> KpiAppraisals => Set<KpiAppraisal>();
    public DbSet<KpiAppraisalItem> KpiAppraisalItems => Set<KpiAppraisalItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
