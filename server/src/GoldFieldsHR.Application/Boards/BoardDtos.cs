using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Boards;

public record CreateBoardRequest(
    string Name,
    string? Description,
    Guid? SiteId,
    BoardPriority Priority,
    DateOnly? Deadline,
    List<Guid> InitialMemberEmployeeIds);

public record UpdateBoardRequest(
    string Name,
    string? Description,
    bool IsArchived,
    BoardPriority Priority,
    BoardStatus Status,
    DateOnly? Deadline);

public record AddBoardMemberRequest(Guid EmployeeId);

public record BoardMemberDto(Guid EmployeeId, string EmployeeName, string JobTitle, DateTime AddedAtUtc);

public record BoardDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerEmployeeId,
    string OwnerEmployeeName,
    Guid? SiteId,
    string? SiteName,
    bool IsArchived,
    BoardPriority Priority,
    BoardStatus Status,
    DateOnly? Deadline,
    int CompletionPercentage,
    DateTime CreatedAtUtc,
    IReadOnlyList<BoardMemberDto> Members);

public record CreateBoardTaskRequest(
    string Title,
    string? Description,
    Guid? AssigneeEmployeeId,
    DateOnly? DueDate);

public record UpdateBoardTaskRequest(
    string Title,
    string? Description,
    Guid? AssigneeEmployeeId,
    DateOnly? DueDate);

public record ChangeTaskStatusRequest(BoardTaskStatus Status);

public record BoardTaskDto(
    Guid Id,
    Guid BoardId,
    string Title,
    string? Description,
    Guid? AssigneeEmployeeId,
    string? AssigneeEmployeeName,
    Guid CreatedByEmployeeId,
    string CreatedByEmployeeName,
    BoardTaskStatus Status,
    DateOnly? DueDate,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public record WeeklyTaskCompletionDto(
    Guid EmployeeId,
    string EmployeeName,
    int TasksCompleted,
    int TasksInProgress,
    int TasksOverdue);
