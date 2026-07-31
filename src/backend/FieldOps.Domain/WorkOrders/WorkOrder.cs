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

    public Guid? AssignedTechnicianId { get; private set; }

    public DateTimeOffset? AssignedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? SubmittedForApprovalAt { get; private set; }

    public string? CompletionSummary { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? ClientReopenReason { get; private set; }

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
        EnsureVersion(expectedVersion);

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
        Touch();
    }

    public void AssignTo(
        Guid technicianUserId,
        long expectedVersion)
    {
        EnsureVersion(expectedVersion);

        if (technicianUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Technician user ID cannot be empty.",
                nameof(technicianUserId));
        }

        if (Status is not (
            WorkOrderStatus.Submitted or
            WorkOrderStatus.Assigned or
            WorkOrderStatus.Reopened))
        {
            throw new InvalidOperationException(
                $"Cannot assign a work order while it is {Status}.");
        }

        AssignedTechnicianId = technicianUserId;
        AssignedAt = DateTimeOffset.UtcNow;
        StartedAt = null;
        SubmittedForApprovalAt = null;
        CompletionSummary = null;
        CompletedAt = null;
        ClientReopenReason = null;
        Status = WorkOrderStatus.Assigned;
        Touch();
    }

    public void Start(
        Guid technicianUserId,
        long expectedVersion)
    {
        EnsureVersion(expectedVersion);
        EnsureAssignedTechnician(
            technicianUserId);

        if (Status != WorkOrderStatus.Assigned)
        {
            throw new InvalidOperationException(
                $"Cannot start a work order while it is {Status}.");
        }

        Status = WorkOrderStatus.InProgress;
        StartedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void SubmitForClientApproval(
        Guid technicianUserId,
        string completionSummary,
        long expectedVersion)
    {
        EnsureVersion(expectedVersion);
        EnsureAssignedTechnician(
            technicianUserId);

        if (Status != WorkOrderStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot submit a work order while it is {Status}.");
        }

        CompletionSummary =
            DomainText.Required(
                completionSummary,
                nameof(completionSummary),
                2000);
        SubmittedForApprovalAt =
            DateTimeOffset.UtcNow;
        Status =
            WorkOrderStatus.AwaitingClientApproval;
        Touch();
    }

    public void ApproveCompletion(
        long expectedVersion)
    {
        EnsureVersion(expectedVersion);

        if (Status !=
            WorkOrderStatus.AwaitingClientApproval)
        {
            throw new InvalidOperationException(
                $"Cannot approve a work order while it is {Status}.");
        }

        ClientReopenReason = null;
        CompletedAt = DateTimeOffset.UtcNow;
        Status = WorkOrderStatus.Completed;
        Touch();
    }

    public void Reopen(
        string reason,
        long expectedVersion)
    {
        EnsureVersion(expectedVersion);

        if (Status !=
            WorkOrderStatus.AwaitingClientApproval)
        {
            throw new InvalidOperationException(
                $"Cannot reopen a work order while it is {Status}.");
        }

        ClientReopenReason =
            DomainText.Required(
                reason,
                nameof(reason),
                1000);
        AssignedTechnicianId = null;
        AssignedAt = null;
        StartedAt = null;
        SubmittedForApprovalAt = null;
        CompletedAt = null;
        Status = WorkOrderStatus.Reopened;
        Touch();
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
        Touch();
    }

    private void EnsureAssignedTechnician(
        Guid technicianUserId)
    {
        if (AssignedTechnicianId !=
            technicianUserId)
        {
            throw new InvalidOperationException(
                "Only the assigned technician can perform this action.");
        }
    }

    private void EnsureVersion(
        long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Work order version conflict. Expected {expectedVersion}, current {Version}.");
        }
    }

    private void Touch()
    {
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
