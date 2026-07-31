using System.Security.Claims;
using FieldOps.Api.Authentication;
using FieldOps.Api.Authorization;
using FieldOps.Api.Contracts.Workflow;
using FieldOps.Domain.Identity;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class WorkflowEndpoints
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var dispatch = endpoints
            .MapGroup("/api/workflow")
            .WithTags("Workflow")
            .RequireAuthorization(
                FieldOpsPolicies.DispatchAccess);

        dispatch.MapGet(
            "/technicians",
            ListTechniciansAsync);
        dispatch.MapGet(
            "/clients",
            ListClientsAsync);
        dispatch.MapGet(
            "/customer-ownership",
            ListCustomerOwnershipAsync);
        dispatch.MapPut(
            "/customers/{customerId:guid}/client",
            LinkCustomerClientAsync);
        dispatch.MapPost(
            "/work-orders/{id:guid}/assign",
            AssignWorkOrderAsync);

        var technician = endpoints
            .MapGroup("/api/technician")
            .WithTags("Technician Workflow")
            .RequireAuthorization(
                FieldOpsPolicies.TechnicianAccess);

        technician.MapGet(
            "/work-orders",
            ListTechnicianWorkOrdersAsync);
        technician.MapPost(
            "/work-orders/{id:guid}/start",
            StartWorkOrderAsync);
        technician.MapPost(
            "/work-orders/{id:guid}/submit",
            SubmitWorkOrderAsync);

        var client = endpoints
            .MapGroup("/api/client")
            .WithTags("Client Workflow")
            .RequireAuthorization(
                FieldOpsPolicies.ClientAccess);

        client.MapGet(
            "/work-orders",
            ListClientWorkOrdersAsync);
        client.MapPost(
            "/work-orders/{id:guid}/approve",
            ApproveWorkOrderAsync);
        client.MapPost(
            "/work-orders/{id:guid}/reopen",
            ReopenWorkOrderAsync);

        return endpoints;
    }

    private static async Task<IResult> ListTechniciansAsync(
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var technicians =
            await dbContext.UserAccounts
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    user.Role ==
                    UserRole.Technician)
                .OrderBy(user =>
                    user.DisplayName)
                .Select(user =>
                    new TechnicianOptionResponse(
                        user.Id,
                        user.DisplayName,
                        user.Email))
                .ToListAsync(cancellationToken);

        return Results.Ok(technicians);
    }

    private static async Task<IResult> ListClientsAsync(
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var clients =
            await dbContext.UserAccounts
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    user.Role ==
                    UserRole.Client)
                .OrderBy(user =>
                    user.DisplayName)
                .Select(user =>
                    new ClientOptionResponse(
                        user.Id,
                        user.DisplayName,
                        user.Email))
                .ToListAsync(cancellationToken);

        return Results.Ok(clients);
    }

    private static async Task<IResult>
        ListCustomerOwnershipAsync(
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var ownership =
            await (
                from customer in
                    dbContext.Customers.AsNoTracking()
                join client in
                    dbContext.UserAccounts.AsNoTracking()
                    on customer.ClientUserId
                    equals (Guid?)client.Id
                    into clients
                from client in
                    clients.DefaultIfEmpty()
                orderby customer.Reference
                select new CustomerOwnershipResponse(
                    customer.Id,
                    customer.Reference,
                    customer.Name,
                    customer.ClientUserId,
                    client == null
                        ? null
                        : client.DisplayName))
            .ToListAsync(cancellationToken);

        return Results.Ok(ownership);
    }

    private static async Task<IResult>
        LinkCustomerClientAsync(
            Guid customerId,
            LinkCustomerClientRequest request,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var customer =
            await dbContext.Customers
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        customerId,
                    cancellationToken);

        if (customer is null)
        {
            return Results.NotFound();
        }

        if (request.ClientUserId.HasValue)
        {
            var client =
                await dbContext.UserAccounts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        user =>
                            user.Id ==
                            request.ClientUserId.Value &&
                            user.IsActive &&
                            user.Role ==
                            UserRole.Client,
                        cancellationToken);

            if (client is null)
            {
                return Results.NotFound(new
                {
                    error =
                        "The Client user does not exist in the authenticated tenant."
                });
            }
        }

        customer.LinkClient(
            request.ClientUserId);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var result =
            await (
                from linkedCustomer in
                    dbContext.Customers.AsNoTracking()
                join client in
                    dbContext.UserAccounts.AsNoTracking()
                    on linkedCustomer.ClientUserId
                    equals (Guid?)client.Id
                    into clients
                from client in
                    clients.DefaultIfEmpty()
                where linkedCustomer.Id ==
                    customerId
                select new CustomerOwnershipResponse(
                    linkedCustomer.Id,
                    linkedCustomer.Reference,
                    linkedCustomer.Name,
                    linkedCustomer.ClientUserId,
                    client == null
                        ? null
                        : client.DisplayName))
            .SingleAsync(cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult>
        AssignWorkOrderAsync(
            Guid id,
            AssignWorkOrderRequest request,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        if (request.TechnicianUserId ==
            Guid.Empty)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["technicianUserId"] =
                        ["Technician user ID is required."]
                });
        }

        if (request.Version < 1)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["version"] =
                        ["Version must be at least 1."]
                });
        }

        var technician =
            await dbContext.UserAccounts
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.Id ==
                        request.TechnicianUserId &&
                        user.IsActive &&
                        user.Role ==
                        UserRole.Technician,
                    cancellationToken);

        if (technician is null)
        {
            return Results.NotFound(new
            {
                error =
                    "The Technician user does not exist in the authenticated tenant."
            });
        }

        var workOrder =
            await dbContext.WorkOrders
                .SingleOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        try
        {
            workOrder.AssignTo(
                technician.Id,
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                exception.Message,
                workOrder.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The work order changed while it was being assigned.",
                workOrder.Version);
        }

        return Results.Ok(
            await GetResponseAsync(
                dbContext,
                id,
                cancellationToken));
    }

    private static async Task<IResult>
        ListTechnicianWorkOrdersAsync(
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var query =
            WorkOrderResponseQueries.Create(
                dbContext);

        if (!principal.IsInRole(
                UserRole.TenantAdmin.ToString()))
        {
            var userId =
                CurrentUser.RequireUserId(
                    principal);

            query = query.Where(item =>
                item.AssignedTechnicianId ==
                userId);
        }

        var items = await query
            .Where(item =>
                item.Status !=
                    WorkOrderStatus.Cancelled)
            .OrderBy(item =>
                item.Status)
            .ThenByDescending(item =>
                item.UpdatedAt)
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult>
        StartWorkOrderAsync(
            Guid id,
            WorkflowVersionRequest request,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var workOrder =
            await dbContext.WorkOrders
                .SingleOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        var actorId =
            ResolveTechnicianActor(
                principal,
                workOrder);

        if (!actorId.HasValue)
        {
            return Results.Forbid();
        }

        try
        {
            workOrder.Start(
                actorId.Value,
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                exception.Message,
                workOrder.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The work order changed before it could be started.",
                workOrder.Version);
        }

        return Results.Ok(
            await GetResponseAsync(
                dbContext,
                id,
                cancellationToken));
    }

    private static async Task<IResult>
        SubmitWorkOrderAsync(
            Guid id,
            SubmitWorkOrderRequest request,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.CompletionSummary))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["completionSummary"] =
                        ["Completion summary is required."]
                });
        }

        var workOrder =
            await dbContext.WorkOrders
                .SingleOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        var actorId =
            ResolveTechnicianActor(
                principal,
                workOrder);

        if (!actorId.HasValue)
        {
            return Results.Forbid();
        }

        try
        {
            workOrder.SubmitForClientApproval(
                actorId.Value,
                request.CompletionSummary,
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [exception.ParamName ??
                        "completionSummary"] =
                        [exception.Message]
                });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                exception.Message,
                workOrder.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The work order changed before it could be submitted.",
                workOrder.Version);
        }

        return Results.Ok(
            await GetResponseAsync(
                dbContext,
                id,
                cancellationToken));
    }

    private static async Task<IResult>
        ListClientWorkOrdersAsync(
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var query =
            WorkOrderResponseQueries.Create(
                dbContext);

        if (!principal.IsInRole(
                UserRole.TenantAdmin.ToString()))
        {
            var userId =
                CurrentUser.RequireUserId(
                    principal);

            query = query.Where(item =>
                dbContext.Customers.Any(
                    customer =>
                        customer.Id ==
                        item.CustomerId &&
                        customer.ClientUserId ==
                        userId));
        }

        var items = await query
            .OrderBy(item =>
                item.Status ==
                WorkOrderStatus.AwaitingClientApproval
                    ? 0
                    : 1)
            .ThenByDescending(item =>
                item.UpdatedAt)
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult>
        ApproveWorkOrderAsync(
            Guid id,
            WorkflowVersionRequest request,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var workOrder =
            await FindClientWorkOrderAsync(
                id,
                principal,
                dbContext,
                cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        try
        {
            workOrder.ApproveCompletion(
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                exception.Message,
                workOrder.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The work order changed before approval could be recorded.",
                workOrder.Version);
        }

        return Results.Ok(
            await GetResponseAsync(
                dbContext,
                id,
                cancellationToken));
    }

    private static async Task<IResult>
        ReopenWorkOrderAsync(
            Guid id,
            ReopenWorkOrderRequest request,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.Reason))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["reason"] =
                        ["A reopen reason is required."]
                });
        }

        var workOrder =
            await FindClientWorkOrderAsync(
                id,
                principal,
                dbContext,
                cancellationToken);

        if (workOrder is null)
        {
            return Results.NotFound();
        }

        try
        {
            workOrder.Reopen(
                request.Reason,
                request.Version);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [exception.ParamName ??
                        "reason"] =
                        [exception.Message]
                });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                exception.Message,
                workOrder.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The work order changed before it could be reopened.",
                workOrder.Version);
        }

        return Results.Ok(
            await GetResponseAsync(
                dbContext,
                id,
                cancellationToken));
    }

    private static Guid?
        ResolveTechnicianActor(
            ClaimsPrincipal principal,
            WorkOrder workOrder)
    {
        if (principal.IsInRole(
                UserRole.TenantAdmin.ToString()))
        {
            return workOrder.AssignedTechnicianId;
        }

        return CurrentUser.RequireUserId(
            principal);
    }

    private static async Task<WorkOrder?>
        FindClientWorkOrderAsync(
            Guid id,
            ClaimsPrincipal principal,
            FieldOpsDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var query =
            dbContext.WorkOrders
                .AsQueryable();

        if (!principal.IsInRole(
                UserRole.TenantAdmin.ToString()))
        {
            var userId =
                CurrentUser.RequireUserId(
                    principal);

            query = query.Where(workOrder =>
                dbContext.Customers.Any(
                    customer =>
                        customer.Id ==
                        workOrder.CustomerId &&
                        customer.ClientUserId ==
                        userId));
        }

        return await query
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);
    }

    private static async Task<
        FieldOps.Api.Contracts.WorkOrders.WorkOrderResponse>
        GetResponseAsync(
            FieldOpsDbContext dbContext,
            Guid id,
            CancellationToken cancellationToken)
    {
        return await WorkOrderResponseQueries
            .Create(dbContext)
            .SingleAsync(
                item => item.Id == id,
                cancellationToken);
    }

    private static IResult Conflict(
        string message,
        long currentVersion)
    {
        return Results.Conflict(new
        {
            error = message,
            currentVersion
        });
    }
}
