namespace FieldOps.Api.Contracts.Workflow;

public sealed record TechnicianOptionResponse(
    Guid Id,
    string DisplayName,
    string Email);

public sealed record ClientOptionResponse(
    Guid Id,
    string DisplayName,
    string Email);

public sealed record CustomerOwnershipResponse(
    Guid CustomerId,
    string CustomerReference,
    string CustomerName,
    Guid? ClientUserId,
    string? ClientDisplayName);

public sealed record AssignWorkOrderRequest(
    Guid TechnicianUserId,
    long Version);

public sealed record SubmitWorkOrderRequest(
    string CompletionSummary,
    long Version);

public sealed record WorkflowVersionRequest(
    long Version);

public sealed record ReopenWorkOrderRequest(
    string Reason,
    long Version);

public sealed record LinkCustomerClientRequest(
    Guid? ClientUserId);
