using FieldOps.Domain.WorkOrders;

namespace FieldOps.UnitTests;

public sealed class WorkOrderWorkflowTests
{
    [Fact]
    public void Assignment_moves_submitted_work_to_assigned()
    {
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();

        workOrder.AssignTo(
            technicianId,
            expectedVersion: 1);

        Assert.Equal(
            WorkOrderStatus.Assigned,
            workOrder.Status);
        Assert.Equal(
            technicianId,
            workOrder.AssignedTechnicianId);
        Assert.NotNull(
            workOrder.AssignedAt);
        Assert.Equal(2, workOrder.Version);
    }

    [Fact]
    public void Assigned_technician_can_start_work()
    {
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();

        workOrder.AssignTo(
            technicianId,
            expectedVersion: 1);
        workOrder.Start(
            technicianId,
            expectedVersion: 2);

        Assert.Equal(
            WorkOrderStatus.InProgress,
            workOrder.Status);
        Assert.NotNull(
            workOrder.StartedAt);
        Assert.Equal(3, workOrder.Version);
    }

    [Fact]
    public void Another_technician_cannot_start_work()
    {
        var workOrder = CreateWorkOrder();

        workOrder.AssignTo(
            Guid.NewGuid(),
            expectedVersion: 1);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    workOrder.Start(
                        Guid.NewGuid(),
                        expectedVersion: 2));

        Assert.Contains(
            "assigned technician",
            exception.Message.ToLowerInvariant());
    }

    [Fact]
    public void Technician_can_submit_completion_summary()
    {
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();

        workOrder.AssignTo(
            technicianId,
            expectedVersion: 1);
        workOrder.Start(
            technicianId,
            expectedVersion: 2);
        workOrder.SubmitForClientApproval(
            technicianId,
            "Repair completed and tested.",
            expectedVersion: 3);

        Assert.Equal(
            WorkOrderStatus.AwaitingClientApproval,
            workOrder.Status);
        Assert.Equal(
            "Repair completed and tested.",
            workOrder.CompletionSummary);
        Assert.NotNull(
            workOrder.SubmittedForApprovalAt);
        Assert.Equal(4, workOrder.Version);
    }

    [Fact]
    public void Client_can_approve_completion()
    {
        var workOrder =
            CreateAwaitingApproval();

        workOrder.ApproveCompletion(
            expectedVersion: 4);

        Assert.Equal(
            WorkOrderStatus.Completed,
            workOrder.Status);
        Assert.NotNull(
            workOrder.CompletedAt);
        Assert.Equal(5, workOrder.Version);
    }

    [Fact]
    public void Client_reopen_clears_assignment_for_dispatch()
    {
        var workOrder =
            CreateAwaitingApproval();

        workOrder.Reopen(
            "The issue remains visible.",
            expectedVersion: 4);

        Assert.Equal(
            WorkOrderStatus.Reopened,
            workOrder.Status);
        Assert.Null(
            workOrder.AssignedTechnicianId);
        Assert.Equal(
            "The issue remains visible.",
            workOrder.ClientReopenReason);
        Assert.Equal(5, workOrder.Version);
    }

    [Fact]
    public void Stale_assignment_version_is_rejected()
    {
        var workOrder = CreateWorkOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    workOrder.AssignTo(
                        Guid.NewGuid(),
                        expectedVersion: 2));

        Assert.Contains(
            "version conflict",
            exception.Message.ToLowerInvariant());
    }

    private static WorkOrder
        CreateAwaitingApproval()
    {
        var workOrder = CreateWorkOrder();
        var technicianId = Guid.NewGuid();

        workOrder.AssignTo(
            technicianId,
            expectedVersion: 1);
        workOrder.Start(
            technicianId,
            expectedVersion: 2);
        workOrder.SubmitForClientApproval(
            technicianId,
            "Completed.",
            expectedVersion: 3);

        return workOrder;
    }

    private static WorkOrder CreateWorkOrder() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WO-WORKFLOW",
            "Workflow test");
}
