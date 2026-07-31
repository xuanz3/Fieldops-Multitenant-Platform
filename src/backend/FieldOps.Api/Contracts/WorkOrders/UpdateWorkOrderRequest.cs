using FieldOps.Domain.WorkOrders;

namespace FieldOps.Api.Contracts.WorkOrders;

public sealed record UpdateWorkOrderRequest(
    Guid CustomerId,
    string Title,
    string? Description,
    WorkOrderPriority Priority,
    long Version);
