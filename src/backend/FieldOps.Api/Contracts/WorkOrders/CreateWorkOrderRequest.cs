using FieldOps.Domain.WorkOrders;

namespace FieldOps.Api.Contracts.WorkOrders;

public sealed record CreateWorkOrderRequest(
    Guid CustomerId,
    string Reference,
    string Title,
    string? Description,
    WorkOrderPriority Priority);
