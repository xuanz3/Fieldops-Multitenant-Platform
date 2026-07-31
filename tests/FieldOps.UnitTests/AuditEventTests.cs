using FieldOps.Domain.Auditing;

namespace FieldOps.UnitTests;

public sealed class AuditEventTests
{
    [Fact]
    public void Audit_event_hash_is_valid()
    {
        var auditEvent =
            new AuditEvent(
                Guid.NewGuid(),
                1,
                "WorkOrderCreated",
                "WorkOrder",
                Guid.NewGuid(),
                "Work order was created.",
                "Test Dispatcher",
                "Dispatcher",
                AuditEvent.GenesisHash,
                Guid.NewGuid());

        Assert.True(
            auditEvent.HasValidHash());
        Assert.Equal(
            64,
            auditEvent.EventHash.Length);
    }

    [Fact]
    public void Second_event_links_to_first_event_hash()
    {
        var tenantId =
            Guid.NewGuid();

        var first =
            new AuditEvent(
                tenantId,
                1,
                "WorkOrderCreated",
                "WorkOrder",
                Guid.NewGuid(),
                "Created.",
                "Test Dispatcher",
                "Dispatcher",
                AuditEvent.GenesisHash);

        var second =
            new AuditEvent(
                tenantId,
                2,
                "WorkOrderAssigned",
                "WorkOrder",
                Guid.NewGuid(),
                "Assigned.",
                "Test Dispatcher",
                "Dispatcher",
                first.EventHash);

        Assert.Equal(
            first.EventHash,
            second.PreviousHash);
        Assert.True(
            second.HasValidHash());
    }
    [Fact]
    public void Timestamp_is_normalised_to_PostgreSql_microsecond_precision()
    {
        var baseTimestamp =
            new DateTimeOffset(
                2026,
                7,
                31,
                9,
                44,
                39,
                TimeSpan.Zero);

        var subMicrosecondTimestamp =
            new DateTimeOffset(
                baseTimestamp.Ticks + 7,
                TimeSpan.Zero);

        var auditEvent =
            new AuditEvent(
                Guid.NewGuid(),
                1,
                "AttachmentUploaded",
                "WorkOrderAttachment",
                Guid.NewGuid(),
                "Evidence was uploaded.",
                "Test Dispatcher",
                "Dispatcher",
                AuditEvent.GenesisHash,
                Guid.NewGuid(),
                Guid.NewGuid(),
                occurredAt:
                    subMicrosecondTimestamp);

        Assert.Equal(
            0,
            auditEvent.OccurredAt.Ticks %
            10);

        Assert.True(
            auditEvent.HasValidHash());
    }

}
