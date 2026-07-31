using FieldOps.Api.Authorization;
using FieldOps.Api.Contracts.Common;
using FieldOps.Api.Contracts.WorkOrders;
using FieldOps.Application.Tenancy;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class WorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/work-orders")
            .WithTags("Work Orders")
            .RequireAuthorization(
                FieldOpsPolicies.DispatchAccess);

        group.MapGet("", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        string? search,
        WorkOrderStatus? status,
        WorkOrderPriority? priority,
        Guid? customerId,
        int? page,
        int? pageSize,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var pagination =
            EndpointValidation.NormalisePage(
                page,
                pageSize);

        var query =
            dbContext.WorkOrders
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern =
                $"%{search.Trim()}%";

            query = query.Where(workOrder =>
                EF.Functions.ILike(
                    workOrder.Reference,
                    pattern) ||
                EF.Functions.ILike(
                    workOrder.Title,
                    pattern) ||
                dbContext.Customers.Any(customer =>
                    customer.Id ==
                        workOrder.CustomerId &&
                    EF.Functions.ILike(
                        customer.Name,
                        pattern)));
        }

        if (status.HasValue)
        {
            query = query.Where(workOrder =>
                workOrder.Status ==
                status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(workOrder =>
                workOrder.Priority ==
                priority.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(workOrder =>
                workOrder.CustomerId ==
                customerId.Value);
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        query = query
            .OrderByDescending(workOrder =>
                workOrder.CreatedAt)
            .ThenBy(workOrder =>
                workOrder.Reference)
            .Skip(
                (pagination.Page - 1) *
                pagination.PageSize)
            .Take(pagination.PageSize);

        var items =
            await WorkOrderResponseQueries
                .Create(
                    dbContext,
                    query)
                .ToListAsync(
                    cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pagination.PageSize);

        return Results.Ok(
            new PagedResponse<WorkOrderResponse>(
                items,
                pagination.Page,
                pagination.PageSize,
                totalCount,
                totalPages));
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var workOrders =
            dbContext.WorkOrders
                .AsNoTracking()
                .Where(workOrder =>
                    workOrder.Id == id);

        var response =
            await WorkOrderResponseQueries
                .Create(
                    dbContext,
                    workOrders)
                .SingleOrDefaultAsync(
                    cancellationToken);

        return response is null
            ? Results.NotFound()
            : Results.Ok(response);
    }

    private static async Task<IResult> CreateAsync(
        CreateWorkOrderRequest request,
        ITenantContext tenantContext,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors =
            EndpointValidation.Validate(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        if (!tenantContext.TenantId.HasValue)
        {
            return Results.Unauthorized();
        }

        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.Id ==
                    request.CustomerId,
                cancellationToken);

        if (customer is null)
        {
            return Results.NotFound(new
            {
                error =
                    "The customer does not exist in the authenticated tenant."
            });
        }

        WorkOrder workOrder;

        try
        {
            workOrder = new WorkOrder(
                tenantContext.TenantId.Value,
                customer.Id,
                request.Reference,
                request.Title,
                request.Description,
                request.Priority);
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [exception.ParamName ?? "request"] =
                        [exception.Message]
                });
        }

        var duplicate = await dbContext.WorkOrders
            .AnyAsync(
                existing =>
                    existing.Reference ==
                    workOrder.Reference,
                cancellationToken);

        if (duplicate)
        {
            return Results.Conflict(new
            {
                error =
                    "A work order with this reference already exists in the tenant."
            });
        }

        dbContext.WorkOrders.Add(workOrder);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new
            {
                error =
                    "The work order could not be created because a conflicting record exists."
            });
        }

        var createdWorkOrder =
            dbContext.WorkOrders
                .AsNoTracking()
                .Where(item =>
                    item.Id ==
                    workOrder.Id);

        var response =
            await WorkOrderResponseQueries
                .Create(
                    dbContext,
                    createdWorkOrder)
                .SingleAsync(
                    cancellationToken);

        return Results.Created(
            $"/api/work-orders/{workOrder.Id}",
            response);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateWorkOrderRequest request,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors =
            EndpointValidation.Validate(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var workOrder = await dbContext.WorkOrders
            .SingleOrDefaultAsync(
                existing => existing.Id == id,
                cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.Id ==
                    request.CustomerId,
                cancellationToken);

        if (customer is null)
        {
            return Results.NotFound(new
            {
                error =
                    "The customer does not exist in the authenticated tenant."
            });
        }

        try
        {
            workOrder.UpdateDetails(
                customer.Id,
                request.Title,
                request.Description,
                request.Priority,
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new
            {
                error = exception.Message,
                currentVersion =
                    workOrder.Version
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new
            {
                error =
                    "The work order was changed by another request. Reload it and retry."
            });
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [exception.ParamName ?? "request"] =
                        [exception.Message]
                });
        }

        var updatedWorkOrder =
            dbContext.WorkOrders
                .AsNoTracking()
                .Where(item =>
                    item.Id == id);

        var response =
            await WorkOrderResponseQueries
                .Create(
                    dbContext,
                    updatedWorkOrder)
                .SingleAsync(
                    cancellationToken);

        return Results.Ok(response);
    }
}
