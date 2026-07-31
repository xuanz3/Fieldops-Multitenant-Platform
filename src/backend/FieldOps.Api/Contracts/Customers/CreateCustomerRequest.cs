namespace FieldOps.Api.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string Reference,
    string Name,
    string? Email);
