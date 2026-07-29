namespace FieldOps.Domain.WorkOrders;

public enum WorkOrderStatus
{
    Submitted,
    Assigned,
    InProgress,
    AwaitingClientApproval,
    Completed,
    Reopened,
    Cancelled
}
