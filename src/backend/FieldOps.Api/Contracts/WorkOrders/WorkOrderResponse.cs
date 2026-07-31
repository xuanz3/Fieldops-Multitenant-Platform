using FieldOps.Domain.WorkOrders;

namespace FieldOps.Api.Contracts.WorkOrders;

public sealed record WorkOrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Reference,
    string Title,
    string? Description,
    WorkOrderPriority Priority,
    WorkOrderStatus Status,
    Guid? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? SubmittedForApprovalAt,
    string? CompletionSummary,
    DateTimeOffset? CompletedAt,
    string? ClientReopenReason,
    long Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
