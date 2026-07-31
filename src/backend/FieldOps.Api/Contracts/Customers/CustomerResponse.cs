namespace FieldOps.Api.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    string Reference,
    string Name,
    string? Email,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
