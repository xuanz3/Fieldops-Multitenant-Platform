namespace FieldOps.Api.Contracts.Evidence;

public sealed record AttachmentResponse(
    Guid Id,
    Guid WorkOrderId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    Guid UploadedByUserId,
    string UploadedByDisplayName,
    DateTimeOffset UploadedAt);
