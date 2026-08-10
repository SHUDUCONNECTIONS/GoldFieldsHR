namespace GoldFieldsHR.Application.Sites;

public record SiteDto(Guid Id, string Name, string Location, bool IsActive, int EmployeeCount);

public record CreateSiteRequest(string Name, string Location);

public record UpdateSiteRequest(string Name, string Location);

public record SetSiteActiveStatusRequest(bool IsActive);
