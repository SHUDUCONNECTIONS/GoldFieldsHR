using GoldFieldsHR.Domain.Entities;

namespace GoldFieldsHR.Infrastructure.Kpi;

// Transcribed directly from docs/Copy of KPI - Mabapa v1 (003).xlsx (4 sheets, one per
// designation). Category names are shared across designations; item text and counts differ.
public static class KpiTemplateSeedData
{
    public static List<KpiTemplate> BuildTemplates() =>
    [
        BuildTemplate("Engineering Foreman", EngineeringForemanCategories()),
        BuildTemplate("Production Supervisor", ProductionSupervisorCategories()),
        BuildTemplate("Engineering co-ordinator", EngineeringCoordinatorCategories()),
        BuildTemplate("Explosive co-ordinator", ExplosiveCoordinatorCategories()),
    ];

    private static KpiTemplate BuildTemplate(
        string designation, List<(string Category, List<(string Description, string? SubGroup)> Items)> categories)
    {
        var template = new KpiTemplate { Id = Guid.NewGuid(), Designation = designation };

        var categoryOrder = 0;
        foreach (var (categoryName, items) in categories)
        {
            var category = new KpiTemplateCategory
            {
                Id = Guid.NewGuid(),
                KpiTemplateId = template.Id,
                Name = categoryName,
                DisplayOrder = categoryOrder++,
            };

            var itemOrder = 0;
            foreach (var (description, subGroup) in items)
            {
                category.Items.Add(new KpiTemplateItem
                {
                    Id = Guid.NewGuid(),
                    KpiTemplateCategoryId = category.Id,
                    Description = description,
                    SubGroupLabel = subGroup,
                    DisplayOrder = itemOrder++,
                });
            }

            template.Categories.Add(category);
        }

        return template;
    }

    private static List<(string, List<(string, string?)>)> EngineeringForemanCategories() =>
    [
        ("SAFETY & HEALTH", [
            ("Attend daily internal safety meeting (INVOCOM)", null),
            ("Attent sections INVOCOM", null),
            ("Submit completed Risk Assessments (SLAMM)", null),
            ("Submit VFL", null),
        ]),
        ("QUALITY & QUANTITY BLAST", [
            ("Work Attendance (timesheet)", "Daily"),
            ("Innovations ( Team improvements )", "Daily"),
            ("Charging Units Compliance Checklist", "Daily"),
            ("Complete incident report on bursting disc repture", "Daily"),
            ("Report deviations on the relevant SOP's ,COP's Compliance and updating", "Daily"),
            ("Team refresher and coaching", "Daily"),
            ("Communicate effectively with the crew members and colleagues", "Daily"),
            ("Conduct emergency drills", "Weekly"),
            ("Conduct charging units audits", "Weekly"),
            ("Service Compliance to be at 100%", "Weekly"),
            ("Conduct explosives audits/Units Audits", "Weekly"),
            ("Innovations ( Team improvements )", "Weekly"),
            ("Explosives compliance audit  on Units(Emulsion)", "Weekly"),
            ("Audit Explosives Supervisor File", "Weekly"),
            ("Charging units audits and Reports", "Monthly"),
            ("Ensure charging units calibration", "Monthly"),
            ("Ensure charging units trip test", "Monthly"),
            ("Team Reports and Compliance ( SK,Ronnie,Diale )", "Monthly"),
        ]),
        ("SITE OBJECTIVES", [
            ("No Accidents / Incidents recorded ( VFL,PTO, NEAR MISSES)", null),
            ("Availability and Utilization of the Charging unit target", null),
            ("Effective System for Explosive Efficiency Monitoring and Control", null),
        ]),
    ];

    private static List<(string, List<(string, string?)>)> ProductionSupervisorCategories() =>
    [
        ("SAFETY & HEALTH", [
            ("Attend daily internal safety meeting (INVOCOM)", null),
            ("Attent sections INVOCOM", null),
            ("Submit completed Risk Assessments (SLAMM)", null),
            ("Submit complete PTO'S (Hole inspection / Charging Units / Timing / Explosives management)", null),
            ("Submit VFL", null),
        ]),
        ("QUALITY & QUANTITY BLAST", [
            ("Work Attendance (timesheet)", "Daily"),
            ("Innovations ( Team improvements )", "Daily"),
            ("Hole Inspections, Timing Efficiency and Proficiency", "Daily"),
            ("Refilling and accessories bay activities House keeping", "Daily"),
            ("Ensure charge returns are recorded", "Daily"),
            ("Explosives Project efficiency ( 2 sections per month improvement )", "Daily"),
            ("Oversee explosives refilling processes", "Daily"),
            ("INVOCOM Attendance", "Daily"),
            ("Mining Engineering Project ( Advance, Long hole Complaince,Fragmentation)", "Daily"),
            ("Submit emergency drills", "Weekly"),
            ("Submit a progress report on the projects ( Due every Friday by 14:00)", "Weekly"),
            ("Conduct Pre and post blast analysis (Long hole stopes)", "Weekly"),
            ("Conduct Pre and post blast analysis (Developments ans destress)", "Weekly"),
            ("Innovations ( Team improvements )", "Weekly"),
            ("Explosives management compliance Miners Accessory boxes", "Weekly"),
            ("Audit reports", "Monthly"),
            ("Coaching report", "Monthly"),
            ("Participate in RCA meetings and compile internal report", "Monthly"),
            ("Charge returns analysis", "Monthly"),
            ("Innovations (Blast improvements)", "Monthly"),
        ]),
        ("SITE OBJECTIVES", [
            ("No Accidents / Incidents recorded ( VFL,PTO, NEAR MISSES)", null),
            ("Availability and Utilization of the Charging unit target", null),
            ("Reduce emulsion waste by continues coaching and assist on face with emulsion control and monitoring( Batch Controllers )", null),
            ("Effective System for Explosive Efficiency Monitoring and Control", null),
            ("Monthly Section Report on Blast Optimization communicated and signed off", null),
        ]),
    ];

    private static List<(string, List<(string, string?)>)> EngineeringCoordinatorCategories() =>
    [
        ("SAFETY & HEALTH", [
            ("Attend daily internal safety meeting (INVOCOM)", null),
            ("Attent sections INVOCOM", null),
            ("Submit completed Risk Assessments (SLAMM)", null),
            ("Submit complete PTO'S (Hole inspection / Charging Units / Timing / Explosives management)", null),
            ("Submit VFL", null),
        ]),
        ("QUALITY & QUANTITY BLAST", [
            ("Work Attendance (timesheet)", "Daily"),
            ("Innovations ( Team improvements )", "Daily"),
            ("Complete incident report on bursting disc repture", "Daily"),
            ("Ensure charging units are maintained as per schedule", "Daily"),
            ("Report deviations on the relevant SOP's ,COP's Compliance and updating", "Daily"),
            ("Batch Control reports / Working condition and Data retreive", "Daily"),
            ("Oversee chargings", "Daily"),
            ("Team refresher and coaching", "Daily"),
            ("Communicate effectively with the crew members and colleagues", "Daily"),
            ("Conduct emergency drills", "Weekly"),
            ("Follow-up on serviced units", "Weekly"),
            ("Ensure availability of spares", "Weekly"),
            ("Participate in drumbeat meetings meetings", "Weekly"),
            ("Innovations ( Team and machine improvements )", "Weekly"),
            ("Ensure charging unit safety compliance (Trips)", "Weekly"),
            ("Ensure charging unit quality compliance (Batching)", "Weekly"),
            ("Charging units maintenances compliance", "Weekly"),
            ("maintenance audit reports", "Monthly"),
            ("Coaching report", "Monthly"),
            ("Charge returns", "Monthly"),
            ("Innovations (Blast improvements)", "Monthly"),
        ]),
        ("SITE OBJECTIVES", [
            ("No Accidents / Incidents recorded ( VFL,PTO, NEAR MISSES)", null),
            ("Availability and Utilization of the Charging unit target", null),
            ("Reduce emulsion waste by continues coaching and assist on face with emulsion control and monitoring( Batch Controllers )", null),
            ("Effective System for Explosive Efficiency Monitoring and Control", null),
        ]),
    ];

    private static List<(string, List<(string, string?)>)> ExplosiveCoordinatorCategories() =>
    [
        ("SAFETY & HEALTH", [
            ("Attend daily internal safety meeting (INVOCOM)", null),
            ("Attent sections INVOCOM", null),
            ("Submit completed Risk Assessments (SLAMM)", null),
            ("Submit complete PTO'S (Hole inspection / Charging Units / Timing / Explosives management)", null),
            ("Submit VFL", null),
        ]),
        ("QUALITY & QUANTITY BLAST", [
            ("Work Attendance (timesheet)", "Daily"),
            ("Innovations ( Team improvements )", "Daily"),
            ("Explosive Transportation audit and compliance(Underground)", "Daily"),
            ("Complete incident report on bursting disc repture", "Daily"),
            ("Ensure charge returns are submitted", "Daily"),
            ("Report deviations on the relevant SOP's ,COP's Compliance and updating", "Daily"),
            ("Batch Control reports / Working condition and Data retreive", "Daily"),
            ("Oversee chargings", "Daily"),
            ("Team refresher and coaching", "Daily"),
            ("Communicate effectively with the crew members and colleagues", "Daily"),
            ("Conduct emergency drills", "Weekly"),
            ("Conduct explosives audits", "Weekly"),
            ("Conduct BCU audits", "Weekly"),
            ("Conduct Pre and post blast analysis (Long hole stopes)", "Weekly"),
            ("Conduct Pre and post blast analysis (Developments ans destress)", "Weekly"),
            ("Participate in RCA meetings", "Weekly"),
            ("Innovations ( Team improvements )", "Weekly"),
            ("Explosives management compliance audit", "Weekly"),
            ("Audit reports", "Monthly"),
            ("Coaching report", "Monthly"),
            ("Charge returns", "Monthly"),
            ("Innovations (Blast improvements)", "Monthly"),
        ]),
        ("SITE OBJECTIVES", [
            ("No Accidents / Incidents recorded ( VFL,PTO, NEAR MISSES)", null),
            ("Availability and Utilization of the Charging unit target", null),
            ("Reduce emulsion waste by continues coaching and assist on face with emulsion control and monitoring( Batch Controllers )", null),
            ("Effective System for Explosive Efficiency Monitoring and Control", null),
            ("Monthly Section Report on Blast Optimization communicated and signed off", null),
        ]),
    ];
}
