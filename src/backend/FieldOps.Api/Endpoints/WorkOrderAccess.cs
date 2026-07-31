using System.Security.Claims;
using FieldOps.Api.Authentication;
using FieldOps.Domain.Identity;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

internal static class WorkOrderAccess
{
    public static async Task<bool>
        CanReadAsync(
            ClaimsPrincipal principal,
            WorkOrder workOrder,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        if (principal.IsInRole(
                UserRole.TenantAdmin.ToString()) ||
            principal.IsInRole(
                UserRole.Dispatcher.ToString()))
        {
            return true;
        }

        var userId =
            CurrentUser.RequireUserId(
                principal);

        if (principal.IsInRole(
                UserRole.Technician.ToString()))
        {
            return
                workOrder.AssignedTechnicianId ==
                userId;
        }

        if (principal.IsInRole(
                UserRole.Client.ToString()))
        {
            return await dbContext.Customers
                .AsNoTracking()
                .AnyAsync(
                    customer =>
                        customer.Id ==
                        workOrder.CustomerId &&
                        customer.ClientUserId ==
                        userId,
                    cancellationToken);
        }

        return false;
    }

    public static bool CanUpload(
        ClaimsPrincipal principal,
        WorkOrder workOrder)
    {
        if (principal.IsInRole(
                UserRole.TenantAdmin.ToString()) ||
            principal.IsInRole(
                UserRole.Dispatcher.ToString()))
        {
            return true;
        }

        return
            principal.IsInRole(
                UserRole.Technician.ToString()) &&
            workOrder.AssignedTechnicianId ==
                CurrentUser.RequireUserId(
                    principal);
    }
}
