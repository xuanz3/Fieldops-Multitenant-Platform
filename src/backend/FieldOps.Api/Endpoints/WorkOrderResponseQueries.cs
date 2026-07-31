using FieldOps.Api.Contracts.WorkOrders;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

internal static class WorkOrderResponseQueries
{
    public static IQueryable<WorkOrderResponse> Create(
        FieldOpsDbContext dbContext,
        IQueryable<WorkOrder> workOrders)
    {
        return
            from workOrder in workOrders
            join customer in
                dbContext.Customers.AsNoTracking()
                on workOrder.CustomerId
                equals customer.Id
            join technician in
                dbContext.UserAccounts.AsNoTracking()
                on workOrder.AssignedTechnicianId
                equals (Guid?)technician.Id
                into technicians
            from technician in
                technicians.DefaultIfEmpty()
            select new WorkOrderResponse(
                workOrder.Id,
                workOrder.CustomerId,
                customer.Name,
                workOrder.Reference,
                workOrder.Title,
                workOrder.Description,
                workOrder.Priority,
                workOrder.Status,
                workOrder.AssignedTechnicianId,
                technician == null
                    ? null
                    : technician.DisplayName,
                workOrder.AssignedAt,
                workOrder.StartedAt,
                workOrder.SubmittedForApprovalAt,
                workOrder.CompletionSummary,
                workOrder.CompletedAt,
                workOrder.ClientReopenReason,
                workOrder.Version,
                workOrder.CreatedAt,
                workOrder.UpdatedAt);
    }
}
