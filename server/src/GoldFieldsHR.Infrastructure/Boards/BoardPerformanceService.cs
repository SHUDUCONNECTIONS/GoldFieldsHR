using System.Globalization;
using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Common;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace GoldFieldsHR.Infrastructure.Boards;

public class BoardPerformanceService(ApplicationDbContext dbContext) : IBoardPerformanceService
{
    private const int MaxAllTimeMonths = 24;
    private static DateTime WeekStartUtc => GetIsoWeekStart(DateOnly.FromDateTime(DateTime.UtcNow))
        .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    public async Task<MyPerformanceDto> GetMyPerformanceAsync(
        Guid employeeId, PerformanceRange range, CancellationToken cancellationToken = default)
    {
        // An unassigned task is credited to whoever created it — a common workflow is creating
        // and completing a task for yourself without bothering to pick yourself as the assignee.
        var tasks = await dbContext.BoardTasks
            .Where(t => (t.AssigneeEmployeeId ?? t.CreatedByEmployeeId) == employeeId)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var chart = BuildChart(tasks, range, today);
        var completedTotal = range == PerformanceRange.All
            ? tasks.Count(t => t.CompletedAtUtc.HasValue)
            : chart.Sum(p => p.TasksCompleted);

        return new MyPerformanceDto(
            completedTotal,
            tasks.Count(t => t.Status == BoardTaskStatus.InProgress),
            tasks.Count(t => t.Status != BoardTaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < today),
            chart);
    }

    public async Task<IReadOnlyList<EmployeePerformanceDto>> GetOrgPerformanceAsync(
        Guid? siteId, PerformanceRange range, CancellationToken cancellationToken = default)
    {
        var employeesQuery = dbContext.Employees.Include(e => e.Site).Where(e => e.IsActive);
        if (siteId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.SiteId == siteId.Value);
        }

        var employees = await employeesQuery
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);

        // Same "unassigned falls back to creator" attribution as GetMyPerformanceAsync.
        var tasks = await dbContext.BoardTasks.ToListAsync(cancellationToken);
        var completedBoardMemberIds = await GetCompletedBoardMemberCountsAsync(siteId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = WeekStartUtc;

        return employees
            .Select(employee =>
            {
                var employeeTasks = tasks.Where(t => (t.AssigneeEmployeeId ?? t.CreatedByEmployeeId) == employee.Id).ToList();
                var completedTotal = employeeTasks.Count(t => t.CompletedAtUtc.HasValue);
                var completionRate = employeeTasks.Count == 0 ? 0 : (int)Math.Round(completedTotal * 100.0 / employeeTasks.Count);

                return new EmployeePerformanceDto(
                    employee.Id,
                    employee.FullName,
                    employee.Site?.Name ?? string.Empty,
                    CountCompletedForRange(employeeTasks, range, today),
                    employeeTasks.Count(t => t.Status == BoardTaskStatus.InProgress),
                    employeeTasks.Count(t => t.Status != BoardTaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < today),
                    employeeTasks.Count(t => t.CompletedAtUtc.HasValue && t.CompletedAtUtc.Value >= weekStart),
                    employeeTasks.Count,
                    completedBoardMemberIds.GetValueOrDefault(employee.Id),
                    completionRate);
            })
            .ToList();
    }

    public async Task<OrgPerformanceSummaryDto> GetOrgSummaryAsync(Guid? siteId, CancellationToken cancellationToken = default)
    {
        var performance = await GetOrgPerformanceAsync(siteId, PerformanceRange.All, cancellationToken);
        var completedBoardsCount = (await GetCompletedBoardsAsync(siteId, cancellationToken)).Count;
        var topPerformer = performance.OrderByDescending(p => p.TasksDoneThisWeek).FirstOrDefault();

        return new OrgPerformanceSummaryDto(
            performance.Count,
            performance.Sum(p => p.TasksDoneThisWeek),
            performance.Sum(p => p.TasksInProgress),
            completedBoardsCount,
            topPerformer is { TasksDoneThisWeek: > 0 } ? topPerformer.EmployeeName : null,
            topPerformer?.TasksDoneThisWeek ?? 0);
    }

    public async Task<IReadOnlyList<CompletedBoardDto>> GetCompletedBoardsAsync(Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Boards
            .Include(b => b.OwnerEmployee)
            .Include(b => b.Members).ThenInclude(m => m.Employee)
            .Where(b => b.Status == BoardStatus.Completed);

        if (siteId.HasValue)
        {
            query = query.Where(b => b.SiteId == siteId.Value);
        }

        var boards = await query.OrderByDescending(b => b.CreatedAtUtc).ToListAsync(cancellationToken);

        return boards
            .Select(b => new CompletedBoardDto(
                b.Id,
                b.Name,
                b.Description,
                b.OwnerEmployee!.FullName,
                b.Deadline,
                b.Priority,
                b.CreatedAtUtc,
                b.Members.Select(m => m.Employee!.FullName).ToList()))
            .ToList();
    }

    public async Task<Result<byte[]>> GenerateEmployeePerformancePdfAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.Include(e => e.Site).FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee is null)
        {
            return Result<byte[]>.Failure("Employee not found.");
        }

        var boards = await dbContext.Boards
            .Include(b => b.Tasks)
            .Where(b => b.OwnerEmployeeId == employeeId || b.Members.Any(m => m.EmployeeId == employeeId))
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var rows = boards.Select(b =>
        {
            var completion = b.Tasks.Count == 0
                ? 0
                : (int)Math.Round(b.Tasks.Count(t => t.Status == BoardTaskStatus.Done) * 100.0 / b.Tasks.Count);
            return (Board: b, CompletionPercentage: completion);
        }).ToList();

        return Result<byte[]>.Success(BuildEmployeePerformancePdf(employee, rows));
    }

    private async Task<Dictionary<Guid, int>> GetCompletedBoardMemberCountsAsync(Guid? siteId, CancellationToken cancellationToken)
    {
        var query = dbContext.Boards
            .Include(b => b.Members)
            .Where(b => b.Status == BoardStatus.Completed);

        if (siteId.HasValue)
        {
            query = query.Where(b => b.SiteId == siteId.Value);
        }

        var completedBoards = await query.ToListAsync(cancellationToken);

        var counts = new Dictionary<Guid, int>();
        foreach (var board in completedBoards)
        {
            foreach (var member in board.Members)
            {
                counts[member.EmployeeId] = counts.GetValueOrDefault(member.EmployeeId) + 1;
            }
        }

        return counts;
    }

    private static byte[] BuildEmployeePerformancePdf(
        Domain.Entities.Employee employee, List<(Board Board, int CompletionPercentage)> rows)
    {
        var totalBoards = rows.Count;
        var completed = rows.Count(r => r.Board.Status == BoardStatus.Completed);
        var inProgress = rows.Count(r => r.Board.Status == BoardStatus.InProgress);
        var onHold = rows.Count(r => r.Board.Status == BoardStatus.OnHold);
        var avgCompletion = totalBoards == 0 ? 0 : (int)Math.Round(rows.Average(r => r.CompletionPercentage));

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Element(header => PdfBranding.RenderLetterhead(
                        header, "Employee Performance Report", $"Generated {DateTime.UtcNow:d MMM yyyy}"));
                    column.Item().PaddingTop(8).Text($"Name: {employee.FullName}").FontSize(11);
                    column.Item().Text($"Job title: {employee.JobTitle}").FontSize(11);
                    column.Item().Text($"Site: {employee.Site?.Name ?? "—"}").FontSize(11);
                    column.Item().PaddingTop(6).Text(
                        $"Total Boards: {totalBoards}    Completed: {completed}    In Progress: {inProgress}    On Hold: {onHold}    Avg. Completion: {avgCompletion}%")
                        .FontSize(11).Bold();
                });

                page.Content().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.4f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Board Name").Bold();
                        header.Cell().Text("Priority").Bold();
                        header.Cell().Text("Status").Bold();
                        header.Cell().Text("Completion").Bold();
                        header.Cell().Text("Deadline").Bold();
                    });

                    if (rows.Count == 0)
                    {
                        table.Cell().ColumnSpan(5).Text("No boards found for this employee.");
                    }

                    foreach (var (board, completionPercentage) in rows)
                    {
                        table.Cell().Text(board.Name);
                        table.Cell().Text(board.Priority.ToString());
                        table.Cell().Text(board.Status.ToString());
                        table.Cell().Text($"{completionPercentage}%");
                        table.Cell().Text(board.Deadline.HasValue ? board.Deadline.Value.ToString("d MMM yyyy") : "No deadline");
                    }
                });

                page.Footer().AlignCenter().Text(
                    $"Generated {DateTime.UtcNow:d MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static List<PerformanceChartPointDto> BuildChart(List<BoardTask> tasks, PerformanceRange range, DateOnly today)
    {
        if (range == PerformanceRange.Week)
        {
            var weekStart = GetIsoWeekStart(today);
            return Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = weekStart.AddDays(i);
                    return new PerformanceChartPointDto(
                        day.ToString("ddd", CultureInfo.InvariantCulture), day, CountCompletedInRange(tasks, day, day.AddDays(1)));
                })
                .ToList();
        }

        if (range == PerformanceRange.Month)
        {
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            return Enumerable.Range(0, daysInMonth)
                .Select(i =>
                {
                    var day = monthStart.AddDays(i);
                    return new PerformanceChartPointDto(day.Day.ToString(), day, CountCompletedInRange(tasks, day, day.AddDays(1)));
                })
                .ToList();
        }

        // All time: bucket by month, from the earliest completed task's month to the current
        // month (capped at MaxAllTimeMonths so a very long-lived account doesn't render an
        // unbounded number of bars).
        var completedDates = tasks
            .Where(t => t.CompletedAtUtc.HasValue)
            .Select(t => DateOnly.FromDateTime(t.CompletedAtUtc!.Value))
            .ToList();

        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        if (completedDates.Count == 0)
        {
            return [new PerformanceChartPointDto(currentMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture), currentMonth, 0)];
        }

        var earliest = completedDates.Min();
        var earliestMonth = new DateOnly(earliest.Year, earliest.Month, 1);
        var monthsSpan = ((currentMonth.Year - earliestMonth.Year) * 12) + currentMonth.Month - earliestMonth.Month + 1;
        var cappedSpan = Math.Min(monthsSpan, MaxAllTimeMonths);
        var startMonth = currentMonth.AddMonths(-(cappedSpan - 1));

        return Enumerable.Range(0, cappedSpan)
            .Select(i =>
            {
                var monthStart = startMonth.AddMonths(i);
                return new PerformanceChartPointDto(
                    monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    monthStart,
                    CountCompletedInRange(tasks, monthStart, monthStart.AddMonths(1)));
            })
            .ToList();
    }

    private static int CountCompletedForRange(List<BoardTask> tasks, PerformanceRange range, DateOnly today)
    {
        if (range == PerformanceRange.All)
        {
            return tasks.Count(t => t.CompletedAtUtc.HasValue);
        }

        var (start, endExclusive) = range == PerformanceRange.Week
            ? (GetIsoWeekStart(today), GetIsoWeekStart(today).AddDays(7))
            : (new DateOnly(today.Year, today.Month, 1), new DateOnly(today.Year, today.Month, 1).AddMonths(1));

        return CountCompletedInRange(tasks, start, endExclusive);
    }

    private static int CountCompletedInRange(List<BoardTask> tasks, DateOnly startInclusive, DateOnly endExclusive)
    {
        var startUtc = startInclusive.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endUtc = endExclusive.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return tasks.Count(t => t.CompletedAtUtc.HasValue && t.CompletedAtUtc.Value >= startUtc && t.CompletedAtUtc.Value < endUtc);
    }

    private static DateOnly GetIsoWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
