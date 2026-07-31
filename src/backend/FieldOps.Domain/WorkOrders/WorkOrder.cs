using FieldOps.Domain.Common;

namespace FieldOps.Domain.WorkOrders;

public sealed class WorkOrder
{
    private WorkOrder()
    {
    }

    public WorkOrder(
        Guid tenantId,
        Guid customerId,
        string reference,
        string title,
        string? description = null,
        WorkOrderPriority priority = WorkOrderPriority.Normal,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(customerId));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority));
        }

        Id = id ?? Guid.NewGuid();
        if (Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Work order ID cannot be empty.",
                nameof(id));
        }

        TenantId = tenantId;
        CustomerId = customerId;
        Reference =
            DomainText.Reference(
                reference,
                nameof(reference));
        Title =
            DomainText.Required(
                title,
                nameof(title),
                200);
        Description =
            DomainText.Optional(
                description,
                nameof(description),
                4000);
        Priority = priority;
        CreatedAt =
            createdAt ??
            DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Reference { get; private set; } =
        string.Empty;

    public string Title { get; private set; } =
        string.Empty;

    public string? Description { get; private set; }

    public WorkOrderPriority Priority { get; private set; }

    public WorkOrderStatus Status { get; private set; } =
        WorkOrderStatus.Submitted;

    public long Version { get; private set; } = 1;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool CanTransitionTo(
        WorkOrderStatus next) =>
        (Status, next) switch
        {
            (
                WorkOrderStatus.Submitted,
                WorkOrderStatus.Assigned
            ) => true,
            (
                WorkOrderStatus.Submitted,
                WorkOrderStatus.Cancelled
            ) => true,
            (
                WorkOrderStatus.Assigned,
                WorkOrderStatus.InProgress
            ) => true,
            (
                WorkOrderStatus.Assigned,
                WorkOrderStatus.Cancelled
            ) => true,
            (
                WorkOrderStatus.InProgress,
                WorkOrderStatus.AwaitingClientApproval
            ) => true,
            (
                WorkOrderStatus.AwaitingClientApproval,
                WorkOrderStatus.Completed
            ) => true,
            (
                WorkOrderStatus.AwaitingClientApproval,
                WorkOrderStatus.Reopened
            ) => true,
            (
                WorkOrderStatus.Reopened,
                WorkOrderStatus.Assigned
            ) => true,
            _ => false
        };

    public void UpdateDetails(
        Guid customerId,
        string title,
        string? description,
        WorkOrderPriority priority,
        long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Work order version conflict. Expected {expectedVersion}, current {Version}.");
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(customerId));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority));
        }

        CustomerId = customerId;
        Title =
            DomainText.Required(
                title,
                nameof(title),
                200);
        Description =
            DomainText.Optional(
                description,
                nameof(description),
                4000);
        Priority = priority;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void TransitionTo(
        WorkOrderStatus next)
    {
        if (!CanTransitionTo(next))
        {
            throw new InvalidOperationException(
                $"Cannot transition work order from {Status} to {next}.");
        }

        Status = next;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
