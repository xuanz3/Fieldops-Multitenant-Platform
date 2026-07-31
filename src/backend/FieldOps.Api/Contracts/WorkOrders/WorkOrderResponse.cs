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
    long Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
