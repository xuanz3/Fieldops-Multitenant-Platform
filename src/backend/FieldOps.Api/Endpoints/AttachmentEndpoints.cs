using System.Security.Claims;
using FieldOps.Api.Contracts.Evidence;
using FieldOps.Application.Auditing;
using FieldOps.Domain.Attachments;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class AttachmentEndpoints
{
    private static readonly
        IReadOnlyDictionary<string, string[]>
        AllowedContentTypes =
            new Dictionary<string, string[]>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["application/pdf"] = [".pdf"],
                ["image/png"] = [".png"],
                ["image/jpeg"] =
                    [".jpg", ".jpeg"],
                ["text/plain"] = [".txt"]
            };

    public static IEndpointRouteBuilder
        MapAttachmentEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/work-orders/{workOrderId:guid}/attachments")
            .WithTags("Work Order Evidence")
            .RequireAuthorization();

        group.MapGet("", ListAsync);
        group.MapPost("", UploadAsync);
        group.MapGet(
            "/{attachmentId:guid}",
            DownloadAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        Guid workOrderId,
        ClaimsPrincipal principal,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var workOrder =
            await dbContext.WorkOrders
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        workOrderId,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        if (!await WorkOrderAccess
                .CanReadAsync(
                    principal,
                    workOrder,
                    dbContext,
                    cancellationToken))
        {
            return Results.Forbid();
        }

        var attachments =
            await dbContext
                .WorkOrderAttachments
                .AsNoTracking()
                .Where(attachment =>
                    attachment.WorkOrderId ==
                    workOrderId)
                .OrderByDescending(attachment =>
                    attachment.UploadedAt)
                .Select(attachment =>
                    ToResponse(attachment))
                .ToListAsync(
                    cancellationToken);

        return Results.Ok(attachments);
    }

    private static async Task<IResult> UploadAsync(
        Guid workOrderId,
        HttpRequest request,
        ClaimsPrincipal principal,
        IAuditActorContext actor,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var workOrder =
            await dbContext.WorkOrders
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        workOrderId,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        if (!WorkOrderAccess.CanUpload(
                principal,
                workOrder))
        {
            return Results.Forbid();
        }

        if (!request.HasFormContentType)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["file"] =
                        ["A multipart file is required."]
                });
        }

        var form =
            await request.ReadFormAsync(
                cancellationToken);

        var file =
            form.Files.GetFile("file");

        if (file is null)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["file"] =
                        ["Select a file to upload."]
                });
        }

        var validation =
            ValidateFile(file);

        if (validation is not null)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["file"] = [validation]
                });
        }

        if (!actor.UserId.HasValue)
        {
            return Results.Unauthorized();
        }

        await using var stream =
            file.OpenReadStream();

        await using var buffer =
            new MemoryStream(
                checked((int)file.Length));

        await stream.CopyToAsync(
            buffer,
            cancellationToken);

        WorkOrderAttachment attachment;

        try
        {
            attachment =
                new WorkOrderAttachment(
                    workOrder.TenantId,
                    workOrder.Id,
                    file.FileName,
                    file.ContentType,
                    buffer.ToArray(),
                    actor.UserId.Value,
                    actor.DisplayName);
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [exception.ParamName ??
                        "file"] =
                        [exception.Message]
                });
        }

        dbContext.WorkOrderAttachments.Add(
            attachment);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Results.Created(
            $"/api/work-orders/{workOrderId}/attachments/{attachment.Id}",
            ToResponse(attachment));
    }

    private static async Task<IResult>
        DownloadAsync(
            Guid workOrderId,
            Guid attachmentId,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var workOrder =
            await dbContext.WorkOrders
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        workOrderId,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        if (!await WorkOrderAccess
                .CanReadAsync(
                    principal,
                    workOrder,
                    dbContext,
                    cancellationToken))
        {
            return Results.Forbid();
        }

        var attachment =
            await dbContext
                .WorkOrderAttachments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                            attachmentId &&
                        item.WorkOrderId ==
                            workOrderId,
                    cancellationToken);

        if (attachment is null)
        {
            return Results.NotFound();
        }

        return Results.File(
            attachment.Content,
            attachment.ContentType,
            attachment.FileName,
            enableRangeProcessing: false);
    }

    private static string? ValidateFile(
        IFormFile file)
    {
        if (file.Length == 0)
        {
            return "The selected file is empty.";
        }

        if (file.Length >
            WorkOrderAttachment
                .MaximumFileSizeBytes)
        {
            return
                "The selected file exceeds the 5 MB limit.";
        }

        if (!AllowedContentTypes.TryGetValue(
                file.ContentType,
                out var extensions))
        {
            return
                "Only PDF, PNG, JPEG and TXT files are allowed.";
        }

        var extension =
            Path.GetExtension(
                Path.GetFileName(
                    file.FileName));

        if (!extensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            return
                "The file extension does not match its allowed content type.";
        }

        return null;
    }

    private static AttachmentResponse ToResponse(
        WorkOrderAttachment attachment) =>
        new(
            attachment.Id,
            attachment.WorkOrderId,
            attachment.FileName,
            attachment.ContentType,
            attachment.SizeBytes,
            attachment.Sha256,
            attachment.UploadedByUserId,
            attachment.UploadedByDisplayName,
            attachment.UploadedAt);
}
