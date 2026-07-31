using System.Security.Cryptography;
using FieldOps.Domain.Common;

namespace FieldOps.Domain.Attachments;

public sealed class WorkOrderAttachment
{
    public const int MaximumFileSizeBytes =
        5 * 1024 * 1024;

    private WorkOrderAttachment()
    {
    }

    public WorkOrderAttachment(
        Guid tenantId,
        Guid workOrderId,
        string fileName,
        string contentType,
        byte[] content,
        Guid uploadedByUserId,
        string uploadedByDisplayName,
        Guid? id = null,
        DateTimeOffset? uploadedAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        if (workOrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Work order ID cannot be empty.",
                nameof(workOrderId));
        }

        if (uploadedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Uploader user ID cannot be empty.",
                nameof(uploadedByUserId));
        }

        ArgumentNullException.ThrowIfNull(content);

        if (content.Length == 0)
        {
            throw new ArgumentException(
                "Attachment content cannot be empty.",
                nameof(content));
        }

        if (content.Length > MaximumFileSizeBytes)
        {
            throw new ArgumentException(
                "Attachment exceeds the 5 MB limit.",
                nameof(content));
        }

        Id = id ?? Guid.NewGuid();

        if (Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Attachment ID cannot be empty.",
                nameof(id));
        }

        TenantId = tenantId;
        WorkOrderId = workOrderId;
        FileName = NormaliseFileName(fileName);
        ContentType =
            DomainText.Required(
                contentType,
                nameof(contentType),
                120);
        Content = content.ToArray();
        SizeBytes = content.LongLength;
        Sha256 = Convert.ToHexString(
            SHA256.HashData(content));
        UploadedByUserId = uploadedByUserId;
        UploadedByDisplayName =
            DomainText.Required(
                uploadedByDisplayName,
                nameof(uploadedByDisplayName),
                120);
        UploadedAt =
            uploadedAt ??
            DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid WorkOrderId { get; private set; }

    public string FileName { get; private set; } =
        string.Empty;

    public string ContentType { get; private set; } =
        string.Empty;

    public byte[] Content { get; private set; } =
        [];

    public long SizeBytes { get; private set; }

    public string Sha256 { get; private set; } =
        string.Empty;

    public Guid UploadedByUserId { get; private set; }

    public string UploadedByDisplayName { get; private set; } =
        string.Empty;

    public DateTimeOffset UploadedAt { get; private set; }

    private static string NormaliseFileName(
        string fileName)
    {
        var safeName =
            Path.GetFileName(fileName)
                .Trim();

        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new ArgumentException(
                "Attachment file name is required.",
                nameof(fileName));
        }

        if (safeName.Length > 180)
        {
            throw new ArgumentException(
                "Attachment file name cannot exceed 180 characters.",
                nameof(fileName));
        }

        return safeName;
    }
}
