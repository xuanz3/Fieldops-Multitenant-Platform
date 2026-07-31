using FieldOps.Api.Authorization;
using FieldOps.Api.Contracts.Common;
using FieldOps.Api.Contracts.Customers;
using FieldOps.Application.Tenancy;
using FieldOps.Domain.Customers;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/customers")
            .WithTags("Customers")
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
        int? page,
        int? pageSize,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var pagination =
            EndpointValidation.NormalisePage(
                page,
                pageSize);

        var query = dbContext.Customers
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern =
                $"%{search.Trim()}%";

            query = query.Where(customer =>
                EF.Functions.ILike(
                    customer.Reference,
                    pattern) ||
                EF.Functions.ILike(
                    customer.Name,
                    pattern) ||
                (customer.Email != null &&
                 EF.Functions.ILike(
                     customer.Email,
                     pattern)));
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var items = await query
            .OrderBy(customer =>
                customer.Reference)
            .Skip(
                (pagination.Page - 1) *
                pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(customer =>
                new CustomerResponse(
                    customer.Id,
                    customer.Reference,
                    customer.Name,
                    customer.Email,
                    customer.CreatedAt,
                    customer.UpdatedAt))
            .ToListAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pagination.PageSize);

        return Results.Ok(
            new PagedResponse<CustomerResponse>(
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
        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing => existing.Id == id,
                cancellationToken);

        return customer is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(customer));
    }

    private static async Task<IResult> CreateAsync(
        CreateCustomerRequest request,
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

        Customer customer;

        try
        {
            customer = new Customer(
                tenantContext.TenantId.Value,
                request.Reference,
                request.Name,
                request.Email);
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

        var duplicate = await dbContext.Customers
            .AnyAsync(
                existing =>
                    existing.Reference ==
                    customer.Reference,
                cancellationToken);

        if (duplicate)
        {
            return Results.Conflict(new
            {
                error =
                    "A customer with this reference already exists in the tenant."
            });
        }

        dbContext.Customers.Add(customer);

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
                    "The customer could not be created because a conflicting record exists."
            });
        }

        return Results.Created(
            $"/api/customers/{customer.Id}",
            ToResponse(customer));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors =
            EndpointValidation.Validate(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(
                existing => existing.Id == id,
                cancellationToken);

        if (customer is null)
        {
            return Results.NotFound();
        }

        try
        {
            customer.UpdateContact(
                request.Name,
                request.Email);
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

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Results.Ok(ToResponse(customer));
    }

    private static CustomerResponse ToResponse(
        Customer customer) =>
        new(
            customer.Id,
            customer.Reference,
            customer.Name,
            customer.Email,
            customer.CreatedAt,
            customer.UpdatedAt);
}
