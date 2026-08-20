using System.Globalization;
using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Boards;

public class BoardPerformanceService(ApplicationDbContext dbContext) : IBoardPerformanceService
{
    private const int MaxAllTimeMonths = 24;

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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return employees
            .Select(employee =>
            {
                var employeeTasks = tasks.Where(t => (t.AssigneeEmployeeId ?? t.CreatedByEmployeeId) == employee.Id).ToList();
                return new EmployeePerformanceDto(
                    employee.Id,
                    employee.FullName,
                    employee.Site?.Name ?? string.Empty,
                    CountCompletedForRange(employeeTasks, range, today),
                    employeeTasks.Count(t => t.Status == BoardTaskStatus.InProgress),
                    employeeTasks.Count(t => t.Status != BoardTaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < today));
            })
            .ToList();
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
