namespace GoldFieldsHR.Infrastructure.Attachments;

public class FileStorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Local disk directory attachments are written to. Defaults to App_Data/uploads under the
    /// app's base directory when unset. A production deployment should point this at durable,
    /// backed-up storage (or be replaced with a cloud blob store) rather than local disk.
    /// </summary>
    public string UploadsRootPath { get; set; } = string.Empty;
}
