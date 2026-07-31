namespace FieldOps.Api.Contracts.Reports;

public sealed record NamedCountResponse(
    string Name,
    int Count);

public sealed record TechnicianReportResponse(
    Guid TechnicianId,
    string TechnicianName,
    int Assigned,
    int InProgress,
    int AwaitingClientApproval,
    int Completed);

public sealed record CustomerReportResponse(
    Guid CustomerId,
    string CustomerReference,
    string CustomerName,
    int Total,
    int Open,
    int Completed);

public sealed record OperationsReportResponse(
    int TotalWorkOrders,
    int OpenWorkOrders,
    int CompletedWorkOrders,
    decimal CompletionRate,
    double? AverageCompletionHours,
    int AttachmentCount,
    int AuditEventCount,
    IReadOnlyList<NamedCountResponse> StatusCounts,
    IReadOnlyList<NamedCountResponse> PriorityCounts,
    IReadOnlyList<TechnicianReportResponse> Technicians,
    IReadOnlyList<CustomerReportResponse> Customers,
    DateTimeOffset GeneratedAt);
