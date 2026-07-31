namespace FieldOps.Api.Contracts.Customers;

public sealed record UpdateCustomerRequest(
    string Name,
    string? Email);
