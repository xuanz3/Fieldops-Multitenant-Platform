using FieldOps.Domain.WorkOrders;

namespace FieldOps.UnitTests;

public sealed class WorkOrderTests
{
    [Fact]
    public void New_work_order_starts_as_submitted()
    {
        var workOrder = new WorkOrder { TenantId = Guid.NewGuid() };
        Assert.Equal(WorkOrderStatus.Submitted, workOrder.Status);
    }

    [Fact]
    public void Submitted_work_order_can_be_assigned()
    {
        var workOrder = new WorkOrder { TenantId = Guid.NewGuid() };
        workOrder.TransitionTo(WorkOrderStatus.Assigned);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);
    }

    [Fact]
    public void Submitted_work_order_cannot_skip_to_completed()
    {
        var workOrder = new WorkOrder { TenantId = Guid.NewGuid() };
        var exception = Assert.Throws<InvalidOperationException>(
            () => workOrder.TransitionTo(WorkOrderStatus.Completed));
        Assert.Contains("Submitted", exception.Message);
        Assert.Equal(WorkOrderStatus.Submitted, workOrder.Status);
    }
}
