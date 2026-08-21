export const BoardTaskStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
} as const;

export type BoardTaskStatus = (typeof BoardTaskStatus)[keyof typeof BoardTaskStatus];

export const BoardTaskStatusLabels: Record<BoardTaskStatus, string> = {
  [BoardTaskStatus.Todo]: "To Do",
  [BoardTaskStatus.InProgress]: "In Progress",
  [BoardTaskStatus.Done]: "Completed",
};

export const BoardPriority = {
  Normal: 0,
  Important: 1,
  Critical: 2,
} as const;

export type BoardPriority = (typeof BoardPriority)[keyof typeof BoardPriority];

export const BoardPriorityLabels: Record<BoardPriority, string> = {
  [BoardPriority.Normal]: "Normal",
  [BoardPriority.Important]: "Important",
  [BoardPriority.Critical]: "Critical",
};

export const BoardPriorityColors: Record<BoardPriority, string> = {
  [BoardPriority.Normal]: "#4caf50",
  [BoardPriority.Important]: "#ff9800",
  [BoardPriority.Critical]: "#ef5350",
};

export const BoardStatus = {
  NotStarted: 0,
  InProgress: 1,
  OnHold: 2,
  Completed: 3,
} as const;

export type BoardStatus = (typeof BoardStatus)[keyof typeof BoardStatus];

export const BoardStatusLabels: Record<BoardStatus, string> = {
  [BoardStatus.NotStarted]: "Not Started",
  [BoardStatus.InProgress]: "In Progress",
  [BoardStatus.OnHold]: "On Hold",
  [BoardStatus.Completed]: "Completed",
};

export interface EmployeeLiteDto {
  id: string;
  fullName: string;
  jobTitle: string;
  siteName: string;
}

export interface BoardMemberDto {
  employeeId: string;
  employeeName: string;
  jobTitle: string;
  addedAtUtc: string;
}

export interface BoardDto {
  id: string;
  name: string;
  description: string | null;
  ownerEmployeeId: string;
  ownerEmployeeName: string;
  siteId: string | null;
  siteName: string | null;
  isArchived: boolean;
  priority: BoardPriority;
  status: BoardStatus;
  deadline: string | null;
  completionPercentage: number;
  createdAtUtc: string;
  members: BoardMemberDto[];
}

export interface CreateBoardRequest {
  name: string;
  description?: string;
  siteId?: string;
  priority: BoardPriority;
  deadline?: string;
  initialMemberEmployeeIds: string[];
}

export interface UpdateBoardRequest {
  name: string;
  description?: string;
  isArchived: boolean;
  priority: BoardPriority;
  status: BoardStatus;
  deadline?: string;
}

export interface AddBoardMemberRequest {
  employeeId: string;
}

export interface BoardTaskDto {
  id: string;
  boardId: string;
  title: string;
  description: string | null;
  assigneeEmployeeId: string | null;
  assigneeEmployeeName: string | null;
  createdByEmployeeId: string;
  createdByEmployeeName: string;
  status: BoardTaskStatus;
  dueDate: string | null;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

export interface CreateBoardTaskRequest {
  title: string;
  description?: string;
  assigneeEmployeeId?: string;
  dueDate?: string;
}

export interface UpdateBoardTaskRequest {
  title: string;
  description?: string;
  assigneeEmployeeId?: string;
  dueDate?: string;
}

export interface ChangeTaskStatusRequest {
  status: BoardTaskStatus;
}

export interface WeeklyTaskCompletionDto {
  employeeId: string;
  employeeName: string;
  tasksCompleted: number;
  tasksInProgress: number;
  tasksOverdue: number;
}
