namespace FieldOps.Domain.WorkOrders;

public sealed class WorkOrder
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Submitted;

    public bool CanTransitionTo(WorkOrderStatus next) => (Status, next) switch
    {
        (WorkOrderStatus.Submitted, WorkOrderStatus.Assigned) => true,
        (WorkOrderStatus.Submitted, WorkOrderStatus.Cancelled) => true,
        (WorkOrderStatus.Assigned, WorkOrderStatus.InProgress) => true,
        (WorkOrderStatus.Assigned, WorkOrderStatus.Cancelled) => true,
        (WorkOrderStatus.InProgress, WorkOrderStatus.AwaitingClientApproval) => true,
        (WorkOrderStatus.AwaitingClientApproval, WorkOrderStatus.Completed) => true,
        (WorkOrderStatus.AwaitingClientApproval, WorkOrderStatus.Reopened) => true,
        (WorkOrderStatus.Reopened, WorkOrderStatus.Assigned) => true,
        _ => false
    };

    public void TransitionTo(WorkOrderStatus next)
    {
        if (!CanTransitionTo(next))
        {
            throw new InvalidOperationException($"Cannot transition work order from {Status} to {next}.");
        }

        Status = next;
    }
}
