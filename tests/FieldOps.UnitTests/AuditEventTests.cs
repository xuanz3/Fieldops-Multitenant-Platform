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
}
