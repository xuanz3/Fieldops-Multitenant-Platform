using System.Text;
using FieldOps.Domain.Attachments;
using FieldOps.Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Infrastructure.Persistence.Seeding;

public sealed class DemoEvidenceSeeder
{
    private readonly FieldOpsDbContext
        _dbContext;

    public DemoEvidenceSeeder(
        FieldOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken =
            default)
    {
        var attachmentExists =
            await _dbContext
                .WorkOrderAttachments
                .IgnoreQueryFilters()
                .AnyAsync(
                    attachment =>
                        attachment.Id ==
                        DemoEvidenceIds
                            .NorthsideCompletionNote,
                    cancellationToken);

        if (!attachmentExists)
        {
            var attachment =
                new WorkOrderAttachment(
                    DemoDataIds.NorthsideTenant,
                    DemoDataIds.NorthsideWorkOrderTwo,
                    "WO-1002-completion-note.txt",
                    "text/plain",
                    Encoding.UTF8.GetBytes(
                        "Fictional completion record: access panel replaced, safety checks completed, and the work order submitted for client approval."),
                    DemoDataIds
                        .NorthsideTechnicianUser,
                    "Northside Technician",
                    DemoEvidenceIds
                        .NorthsideCompletionNote,
                    new DateTimeOffset(
                        2026,
                        7,
                        31,
                        5,
                        0,
                        0,
                        TimeSpan.Zero));

            _dbContext
                .WorkOrderAttachments
                .Add(attachment);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var northsideCount =
            await _dbContext.AuditEvents
                .IgnoreQueryFilters()
                .CountAsync(
                    auditEvent =>
                        auditEvent.TenantId ==
                        DemoDataIds
                            .NorthsideTenant,
                    cancellationToken);

        if (northsideCount < 3)
        {
            await AppendSystemEventsAsync(
                DemoDataIds.NorthsideTenant,
                [
                    (
                        "AuditChainEnabled",
                        "Tenant",
                        DemoDataIds.NorthsideTenant,
                        null,
                        "Tenant-scoped append-only audit chain enabled."
                    ),
                    (
                        "ReportingEnabled",
                        "Tenant",
                        DemoDataIds.NorthsideTenant,
                        null,
                        "Operational reporting and CSV export enabled."
                    )
                ],
                cancellationToken);
        }
    }

    private async Task AppendSystemEventsAsync(
        Guid tenantId,
        IReadOnlyList<(
            string Action,
            string EntityType,
            Guid EntityId,
            Guid? WorkOrderId,
            string Summary)> descriptors,
        CancellationToken cancellationToken)
    {
        var latest =
            await _dbContext.AuditEvents
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(auditEvent =>
                    auditEvent.TenantId ==
                    tenantId)
                .OrderByDescending(auditEvent =>
                    auditEvent.Sequence)
                .Select(auditEvent => new
                {
                    auditEvent.Sequence,
                    auditEvent.EventHash
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        var sequence =
            (latest?.Sequence ?? 0) + 1;
        var previousHash =
            latest?.EventHash ??
            AuditEvent.GenesisHash;

        foreach (var descriptor in descriptors)
        {
            var auditEvent =
                new AuditEvent(
                    tenantId,
                    sequence,
                    descriptor.Action,
                    descriptor.EntityType,
                    descriptor.EntityId,
                    descriptor.Summary,
                    "System",
                    "System",
                    previousHash,
                    workOrderId:
                        descriptor.WorkOrderId);

            _dbContext.AuditEvents.Add(
                auditEvent);

            previousHash =
                auditEvent.EventHash;
            sequence++;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}

public static class DemoEvidenceIds
{
    public static readonly Guid
        NorthsideCompletionNote =
            Guid.Parse(
                "11111111-1111-1111-1111-111111140001");
}
