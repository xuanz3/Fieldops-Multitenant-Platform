using FieldOps.Api.Authorization;
using FieldOps.Api.Contracts.Auditing;
using FieldOps.Api.Contracts.Common;
using FieldOps.Domain.Auditing;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder
        MapAuditEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audit-events")
            .WithTags("Audit")
            .RequireAuthorization(
                FieldOpsPolicies.DispatchAccess);

        group.MapGet("", ListAsync);
        group.MapGet(
            "/verify",
            VerifyAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        string? search,
        string? action,
        Guid? workOrderId,
        int? page,
        int? pageSize,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var pagination =
            EndpointValidation.NormalisePage(
                page,
                pageSize);

        var query =
            dbContext.AuditEvents
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(
                search))
        {
            var pattern =
                $"%{search.Trim()}%";

            query = query.Where(item =>
                EF.Functions.ILike(
                    item.Summary,
                    pattern) ||
                EF.Functions.ILike(
                    item.ActorDisplayName,
                    pattern) ||
                EF.Functions.ILike(
                    item.EntityType,
                    pattern));
        }

        if (!string.IsNullOrWhiteSpace(
                action))
        {
            query = query.Where(item =>
                item.Action ==
                action.Trim());
        }

        if (workOrderId.HasValue)
        {
            query = query.Where(item =>
                item.WorkOrderId ==
                workOrderId.Value);
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var items = await query
            .OrderByDescending(item =>
                item.Sequence)
            .Skip(
                (pagination.Page - 1) *
                pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(item =>
                new AuditEventResponse(
                    item.Id,
                    item.Sequence,
                    item.Action,
                    item.EntityType,
                    item.EntityId,
                    item.WorkOrderId,
                    item.Summary,
                    item.ActorUserId,
                    item.ActorDisplayName,
                    item.ActorRole,
                    item.OccurredAt,
                    item.PreviousHash,
                    item.EventHash))
            .ToListAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pagination.PageSize);

        return Results.Ok(
            new PagedResponse<AuditEventResponse>(
                items,
                pagination.Page,
                pagination.PageSize,
                totalCount,
                totalPages));
    }

    private static async Task<IResult> VerifyAsync(
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var events =
            await dbContext.AuditEvents
                .AsNoTracking()
                .OrderBy(item =>
                    item.Sequence)
                .ToListAsync(
                    cancellationToken);

        string expectedPrevious =
            AuditEvent.GenesisHash;
        long expectedSequence = 1;

        foreach (var auditEvent in events)
        {
            if (auditEvent.Sequence !=
                expectedSequence)
            {
                return Results.Ok(
                    new AuditVerificationResponse(
                        false,
                        events.Count,
                        events.FirstOrDefault()?
                            .Sequence,
                        events.LastOrDefault()?
                            .Sequence,
                        $"Expected sequence {expectedSequence} but found {auditEvent.Sequence}."));
            }

            if (!string.Equals(
                    auditEvent.PreviousHash,
                    expectedPrevious,
                    StringComparison.Ordinal))
            {
                return Results.Ok(
                    new AuditVerificationResponse(
                        false,
                        events.Count,
                        events.FirstOrDefault()?
                            .Sequence,
                        events.LastOrDefault()?
                            .Sequence,
                        $"Previous hash mismatch at sequence {auditEvent.Sequence}."));
            }

            if (!auditEvent.HasValidHash())
            {
                return Results.Ok(
                    new AuditVerificationResponse(
                        false,
                        events.Count,
                        events.FirstOrDefault()?
                            .Sequence,
                        events.LastOrDefault()?
                            .Sequence,
                        $"Event hash mismatch at sequence {auditEvent.Sequence}."));
            }

            expectedPrevious =
                auditEvent.EventHash;
            expectedSequence++;
        }

        return Results.Ok(
            new AuditVerificationResponse(
                true,
                events.Count,
                events.FirstOrDefault()?
                    .Sequence,
                events.LastOrDefault()?
                    .Sequence,
                null));
    }
}
