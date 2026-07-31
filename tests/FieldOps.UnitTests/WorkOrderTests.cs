using FieldOps.Domain.WorkOrders;

namespace FieldOps.UnitTests;

public sealed class WorkOrderTests
{
    [Fact]
    public void New_work_order_starts_as_submitted()
    {
        var workOrder = CreateWorkOrder();

        Assert.Equal(
            WorkOrderStatus.Submitted,
            workOrder.Status);
        Assert.Equal(1, workOrder.Version);
    }

    [Fact]
    public void Submitted_work_order_can_be_assigned()
    {
        var workOrder = CreateWorkOrder();

        workOrder.TransitionTo(
            WorkOrderStatus.Assigned);

        Assert.Equal(
            WorkOrderStatus.Assigned,
            workOrder.Status);
        Assert.Equal(2, workOrder.Version);
    }

    [Fact]
    public void Submitted_work_order_cannot_skip_to_completed()
    {
        var workOrder = CreateWorkOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    workOrder.TransitionTo(
                        WorkOrderStatus.Completed));

        Assert.Contains(
            "Submitted",
            exception.Message);
        Assert.Equal(
            WorkOrderStatus.Submitted,
            workOrder.Status);
        Assert.Equal(1, workOrder.Version);
    }

    [Fact]
    public void Work_order_preserves_tenant_and_customer_ownership()
    {
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var workOrder = new WorkOrder(
            tenantId,
            customerId,
            "wo-1001",
            "Repair leaking tap");

        Assert.Equal(
            tenantId,
            workOrder.TenantId);
        Assert.Equal(
            customerId,
            workOrder.CustomerId);
        Assert.Equal(
            "WO-1001",
            workOrder.Reference);
    }

    [Fact]
    public void Update_details_changes_fields_and_increments_version()
    {
        var workOrder = CreateWorkOrder();
        var replacementCustomerId =
            Guid.NewGuid();

        workOrder.UpdateDetails(
            replacementCustomerId,
            "Replace access panel",
            "Updated details",
            WorkOrderPriority.High,
            expectedVersion: 1);

        Assert.Equal(
            replacementCustomerId,
            workOrder.CustomerId);
        Assert.Equal(
            "Replace access panel",
            workOrder.Title);
        Assert.Equal(
            "Updated details",
            workOrder.Description);
        Assert.Equal(
            WorkOrderPriority.High,
            workOrder.Priority);
        Assert.Equal(2, workOrder.Version);
    }

    [Fact]
    public void Update_details_rejects_stale_version()
    {
        var workOrder = CreateWorkOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    workOrder.UpdateDetails(
                        Guid.NewGuid(),
                        "New title",
                        null,
                        WorkOrderPriority.Normal,
                        expectedVersion: 2));

        Assert.Contains(
            "version conflict",
            exception.Message.ToLowerInvariant());
        Assert.Equal(1, workOrder.Version);
    }

    [Fact]
    public void Update_details_rejects_empty_customer()
    {
        var workOrder = CreateWorkOrder();

        Assert.Throws<ArgumentException>(
            () =>
                workOrder.UpdateDetails(
                    Guid.Empty,
                    "New title",
                    null,
                    WorkOrderPriority.Normal,
                    expectedVersion: 1));
    }

    private static WorkOrder CreateWorkOrder() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WO-1001",
            "Repair leaking tap");
}
