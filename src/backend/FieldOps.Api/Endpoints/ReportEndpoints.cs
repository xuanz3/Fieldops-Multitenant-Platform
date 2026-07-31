using System.Globalization;
using System.Text;
using FieldOps.Api.Authorization;
using FieldOps.Api.Contracts.Reports;
using FieldOps.Domain.Identity;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder
        MapReportEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization(
                FieldOpsPolicies.DispatchAccess);

        group.MapGet(
            "/operations",
            GetOperationsAsync);
        group.MapGet(
            "/operations.csv",
            DownloadOperationsCsvAsync);

        return endpoints;
    }

    private static async Task<IResult>
        GetOperationsAsync(
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        return Results.Ok(
            await BuildReportAsync(
                dbContext,
                cancellationToken));
    }

    private static async Task<IResult>
        DownloadOperationsCsvAsync(
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var report =
            await BuildReportAsync(
                dbContext,
                cancellationToken);

        var csv = BuildCsv(report);

        return Results.File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv; charset=utf-8",
            "fieldops-operations-report.csv");
    }

    private static async Task<
        OperationsReportResponse>
        BuildReportAsync(
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var workOrders =
            await dbContext.WorkOrders
                .AsNoTracking()
                .Select(workOrder => new
                {
                    workOrder.Id,
                    workOrder.CustomerId,
                    workOrder.AssignedTechnicianId,
                    workOrder.Status,
                    workOrder.Priority,
                    workOrder.CreatedAt,
                    workOrder.CompletedAt
                })
                .ToListAsync(
                    cancellationToken);

        var customers =
            await dbContext.Customers
                .AsNoTracking()
                .OrderBy(customer =>
                    customer.Reference)
                .Select(customer => new
                {
                    customer.Id,
                    customer.Reference,
                    customer.Name
                })
                .ToListAsync(
                    cancellationToken);

        var technicians =
            await dbContext.UserAccounts
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    user.Role ==
                    UserRole.Technician)
                .OrderBy(user =>
                    user.DisplayName)
                .Select(user => new
                {
                    user.Id,
                    user.DisplayName
                })
                .ToListAsync(
                    cancellationToken);

        var total =
            workOrders.Count;

        var completed =
            workOrders.Count(item =>
                item.Status ==
                WorkOrderStatus.Completed);

        var open =
            workOrders.Count(item =>
                item.Status is not (
                    WorkOrderStatus.Completed or
                    WorkOrderStatus.Cancelled));

        var completionHours =
            workOrders
                .Where(item =>
                    item.CompletedAt.HasValue)
                .Select(item =>
                    (item.CompletedAt!.Value -
                     item.CreatedAt)
                    .TotalHours)
                .ToList();

        var statusCounts =
            Enum.GetValues<WorkOrderStatus>()
                .Select(status =>
                    new NamedCountResponse(
                        status.ToString(),
                        workOrders.Count(item =>
                            item.Status ==
                            status)))
                .ToList();

        var priorityCounts =
            Enum.GetValues<
                    WorkOrderPriority>()
                .Select(priority =>
                    new NamedCountResponse(
                        priority.ToString(),
                        workOrders.Count(item =>
                            item.Priority ==
                            priority)))
                .ToList();

        var technicianReports =
            technicians
                .Select(technician =>
                {
                    var assigned =
                        workOrders.Where(item =>
                            item.AssignedTechnicianId ==
                            technician.Id);

                    return new
                        TechnicianReportResponse(
                            technician.Id,
                            technician.DisplayName,
                            assigned.Count(item =>
                                item.Status ==
                                WorkOrderStatus.Assigned),
                            assigned.Count(item =>
                                item.Status ==
                                WorkOrderStatus.InProgress),
                            assigned.Count(item =>
                                item.Status ==
                                WorkOrderStatus.AwaitingClientApproval),
                            assigned.Count(item =>
                                item.Status ==
                                WorkOrderStatus.Completed));
                })
                .ToList();

        var customerReports =
            customers
                .Select(customer =>
                {
                    var records =
                        workOrders.Where(item =>
                            item.CustomerId ==
                            customer.Id)
                        .ToList();

                    return new
                        CustomerReportResponse(
                            customer.Id,
                            customer.Reference,
                            customer.Name,
                            records.Count,
                            records.Count(item =>
                                item.Status is not (
                                    WorkOrderStatus.Completed or
                                    WorkOrderStatus.Cancelled)),
                            records.Count(item =>
                                item.Status ==
                                WorkOrderStatus.Completed));
                })
                .ToList();

        var attachmentCount =
            await dbContext
                .WorkOrderAttachments
                .CountAsync(
                    cancellationToken);

        var auditEventCount =
            await dbContext.AuditEvents
                .CountAsync(
                    cancellationToken);

        return new OperationsReportResponse(
            total,
            open,
            completed,
            total == 0
                ? 0
                : Math.Round(
                    completed * 100m / total,
                    1),
            completionHours.Count == 0
                ? null
                : Math.Round(
                    completionHours.Average(),
                    1),
            attachmentCount,
            auditEventCount,
            statusCounts,
            priorityCounts,
            technicianReports,
            customerReports,
            DateTimeOffset.UtcNow);
    }

    private static string BuildCsv(
        OperationsReportResponse report)
    {
        var lines =
            new List<string>
            {
                "Section,Name,Count",
                $"Summary,Total work orders,{report.TotalWorkOrders}",
                $"Summary,Open work orders,{report.OpenWorkOrders}",
                $"Summary,Completed work orders,{report.CompletedWorkOrders}",
                $"Summary,Completion rate,{report.CompletionRate.ToString(CultureInfo.InvariantCulture)}%",
                $"Summary,Average completion hours,{report.AverageCompletionHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty}",
                $"Summary,Attachments,{report.AttachmentCount}",
                $"Summary,Audit events,{report.AuditEventCount}"
            };

        lines.AddRange(
            report.StatusCounts.Select(item =>
                $"Status,{Escape(item.Name)},{item.Count}"));

        lines.AddRange(
            report.PriorityCounts.Select(item =>
                $"Priority,{Escape(item.Name)},{item.Count}"));

        lines.Add("");
        lines.Add(
            "Technician,Assigned,In progress,Awaiting client,Completed");

        lines.AddRange(
            report.Technicians.Select(item =>
                $"{Escape(item.TechnicianName)},{item.Assigned},{item.InProgress},{item.AwaitingClientApproval},{item.Completed}"));

        lines.Add("");
        lines.Add(
            "Customer reference,Customer,Total,Open,Completed");

        lines.AddRange(
            report.Customers.Select(item =>
                $"{Escape(item.CustomerReference)},{Escape(item.CustomerName)},{item.Total},{item.Open},{item.Completed}"));

        return string.Join(
            Environment.NewLine,
            lines);
    }

    private static string Escape(
        string value)
    {
        if (value.IndexOfAny(
                new[]
                {
                    ',',
                    '"',
                    '\n',
                    '\r'
                }) < 0)
        {
            return value;
        }

        return
            $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
