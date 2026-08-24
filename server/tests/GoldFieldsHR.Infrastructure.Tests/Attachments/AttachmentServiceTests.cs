using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Attachments;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Attachments;

public class AttachmentServiceTests
{
    private static AttachmentService CreateService(ApplicationDbContext dbContext) =>
        new(dbContext, Options.Create(new FileStorageSettings
        {
            UploadsRootPath = Path.Combine(Path.GetTempPath(), "goldfields-attachment-tests", Guid.NewGuid().ToString()),
        }));

    private static UploadAttachmentRequest ValidPdf() =>
        new("evidence.pdf", "application/pdf", [1, 2, 3, 4]);

    [Fact]
    public async Task Upload_ToPolicy_NonHR_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var policy = new Policy { Id = Guid.NewGuid(), Title = "Safety Rules", Content = "...", PublishedByEmployeeId = hr.Id };
        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var result = await service.UploadAsync(AttachmentEntityType.Policy, policy.Id, employee.Id, ValidPdf());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Upload_ToPolicy_HR_SucceedsAndAnyoneCanView()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var policy = new Policy { Id = Guid.NewGuid(), Title = "Safety Rules", Content = "...", PublishedByEmployeeId = hr.Id };
        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var uploaded = await service.UploadAsync(AttachmentEntityType.Policy, policy.Id, hr.Id, ValidPdf());
        Assert.True(uploaded.Succeeded);

        var viewed = await service.GetForEntityAsync(AttachmentEntityType.Policy, policy.Id, employee.Id);
        Assert.True(viewed.Succeeded);
        Assert.Single(viewed.Value!);
    }

    [Fact]
    public async Task Upload_ToCertificate_OwnerCannotUploadButCanView()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var owner = dbContext.AddEmployee(EmployeeRole.Employee);
        var otherEmployee = dbContext.AddEmployee(EmployeeRole.Employee);
        var cert = new Certificate
        {
            Id = Guid.NewGuid(), EmployeeId = owner.Id, Title = "First Aid",
            IssuedDate = DateOnly.FromDateTime(DateTime.UtcNow), ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            IssuedByEmployeeId = hr.Id,
        };
        dbContext.Certificates.Add(cert);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var ownerUpload = await service.UploadAsync(AttachmentEntityType.Certificate, cert.Id, owner.Id, ValidPdf());
        Assert.False(ownerUpload.Succeeded);

        var hrUpload = await service.UploadAsync(AttachmentEntityType.Certificate, cert.Id, hr.Id, ValidPdf());
        Assert.True(hrUpload.Succeeded);

        var ownerView = await service.GetForEntityAsync(AttachmentEntityType.Certificate, cert.Id, owner.Id);
        Assert.True(ownerView.Succeeded);

        var strangerView = await service.GetForEntityAsync(AttachmentEntityType.Certificate, cert.Id, otherEmployee.Id);
        Assert.False(strangerView.Succeeded);
    }

    [Fact]
    public async Task Upload_DisallowedContentType_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var policy = new Policy { Id = Guid.NewGuid(), Title = "Safety Rules", Content = "...", PublishedByEmployeeId = hr.Id };
        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var result = await service.UploadAsync(
            AttachmentEntityType.Policy, policy.Id, hr.Id,
            new UploadAttachmentRequest("virus.exe", "application/x-msdownload", [1, 2, 3]));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Upload_EmptyFile_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var policy = new Policy { Id = Guid.NewGuid(), Title = "Safety Rules", Content = "...", PublishedByEmployeeId = hr.Id };
        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var result = await service.UploadAsync(
            AttachmentEntityType.Policy, policy.Id, hr.Id,
            new UploadAttachmentRequest("empty.pdf", "application/pdf", []));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Upload_ThenDownload_RoundTripsFileContentCorrectly()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var policy = new Policy { Id = Guid.NewGuid(), Title = "Safety Rules", Content = "...", PublishedByEmployeeId = hr.Id };
        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();
        var service = CreateService(dbContext);

        var uploaded = await service.UploadAsync(
            AttachmentEntityType.Policy, policy.Id, hr.Id,
            new UploadAttachmentRequest("doc.pdf", "application/pdf", [9, 8, 7, 6]));
        Assert.True(uploaded.Succeeded);

        var downloaded = await service.DownloadAsync(uploaded.Value!.Id, hr.Id);

        Assert.True(downloaded.Succeeded);
        Assert.Equal([9, 8, 7, 6], downloaded.Value!.Content);
        Assert.Equal("doc.pdf", downloaded.Value.FileName);
    }
}
