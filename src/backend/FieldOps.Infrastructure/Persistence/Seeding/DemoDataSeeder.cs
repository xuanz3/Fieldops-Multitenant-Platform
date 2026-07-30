using FieldOps.Application.Identity;
using FieldOps.Domain.Customers;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Infrastructure.Persistence.Seeding;

public sealed class DemoDataSeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly FieldOpsDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public DemoDataSeeder(
        FieldOpsDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await AddTenantIfMissingAsync(
            new Tenant(
                "Northside Property Services",
                "northside-property-services",
                DemoDataIds.NorthsideTenant,
                SeedTimestamp),
            cancellationToken);

        await AddTenantIfMissingAsync(
            new Tenant(
                "Bayside Facility Group",
                "bayside-facility-group",
                DemoDataIds.BaysideTenant,
                SeedTimestamp),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AddUserIfMissingAsync(
            new UserAccount(
                DemoDataIds.NorthsideTenant,
                "admin@northside.example.test",
                "Northside Tenant Admin",
                _passwordHasher.Hash(DemoCredentials.Password),
                UserRole.TenantAdmin,
                DemoDataIds.NorthsideAdminUser,
                SeedTimestamp),
            cancellationToken);

        await AddUserIfMissingAsync(
            new UserAccount(
                DemoDataIds.NorthsideTenant,
                "dispatcher@northside.example.test",
                "Northside Dispatcher",
                _passwordHasher.Hash(DemoCredentials.Password),
                UserRole.Dispatcher,
                DemoDataIds.NorthsideDispatcherUser,
                SeedTimestamp),
            cancellationToken);

        await AddUserIfMissingAsync(
            new UserAccount(
                DemoDataIds.NorthsideTenant,
                "technician@northside.example.test",
                "Northside Technician",
                _passwordHasher.Hash(DemoCredentials.Password),
                UserRole.Technician,
                DemoDataIds.NorthsideTechnicianUser,
                SeedTimestamp),
            cancellationToken);

        await AddUserIfMissingAsync(
            new UserAccount(
                DemoDataIds.NorthsideTenant,
                "client@northside.example.test",
                "Northside Client",
                _passwordHasher.Hash(DemoCredentials.Password),
                UserRole.Client,
                DemoDataIds.NorthsideClientUser,
                SeedTimestamp),
            cancellationToken);

        await AddUserIfMissingAsync(
            new UserAccount(
                DemoDataIds.BaysideTenant,
                "admin@bayside.example.test",
                "Bayside Tenant Admin",
                _passwordHasher.Hash(DemoCredentials.Password),
                UserRole.TenantAdmin,
                DemoDataIds.BaysideAdminUser,
                SeedTimestamp),
            cancellationToken);

        await AddCustomerIfMissingAsync(
            new Customer(
                DemoDataIds.NorthsideTenant,
                "CLIENT-001",
                "Northside Demo Client",
                "northside-client@example.test",
                DemoDataIds.NorthsideCustomerOne,
                SeedTimestamp),
            cancellationToken);

        await AddCustomerIfMissingAsync(
            new Customer(
                DemoDataIds.NorthsideTenant,
                "CLIENT-002",
                "Northside Retail Demo",
                "northside-retail@example.test",
                DemoDataIds.NorthsideCustomerTwo,
                SeedTimestamp),
            cancellationToken);

        await AddCustomerIfMissingAsync(
            new Customer(
                DemoDataIds.BaysideTenant,
                "CLIENT-001",
                "Bayside Demo Client",
                "bayside-client@example.test",
                DemoDataIds.BaysideCustomerOne,
                SeedTimestamp),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AddWorkOrderIfMissingAsync(
            new WorkOrder(
                DemoDataIds.NorthsideTenant,
                DemoDataIds.NorthsideCustomerOne,
                "WO-1001",
                "Inspect leaking kitchen tap",
                "Fictional demonstration work order.",
                WorkOrderPriority.Normal,
                DemoDataIds.NorthsideWorkOrderOne,
                SeedTimestamp),
            cancellationToken);

        await AddWorkOrderIfMissingAsync(
            new WorkOrder(
                DemoDataIds.NorthsideTenant,
                DemoDataIds.NorthsideCustomerTwo,
                "WO-1002",
                "Replace damaged access panel",
                "Fictional demonstration work order.",
                WorkOrderPriority.High,
                DemoDataIds.NorthsideWorkOrderTwo,
                SeedTimestamp),
            cancellationToken);

        await AddWorkOrderIfMissingAsync(
            new WorkOrder(
                DemoDataIds.BaysideTenant,
                DemoDataIds.BaysideCustomerOne,
                "WO-1001",
                "Test emergency lighting",
                "Fictional demonstration work order.",
                WorkOrderPriority.Urgent,
                DemoDataIds.BaysideWorkOrderOne,
                SeedTimestamp),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddTenantIfMissingAsync(
        Tenant tenant,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Tenants.AnyAsync(
                existing => existing.Id == tenant.Id,
                cancellationToken))
        {
            _dbContext.Tenants.Add(tenant);
        }
    }

    private async Task AddUserIfMissingAsync(
        UserAccount user,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.UserAccounts
                .IgnoreQueryFilters()
                .AnyAsync(
                    existing => existing.Id == user.Id,
                    cancellationToken))
        {
            _dbContext.UserAccounts.Add(user);
        }
    }

    private async Task AddCustomerIfMissingAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Customers
                .IgnoreQueryFilters()
                .AnyAsync(
                    existing => existing.Id == customer.Id,
                    cancellationToken))
        {
            _dbContext.Customers.Add(customer);
        }
    }

    private async Task AddWorkOrderIfMissingAsync(
        WorkOrder workOrder,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.WorkOrders
                .IgnoreQueryFilters()
                .AnyAsync(
                    existing => existing.Id == workOrder.Id,
                    cancellationToken))
        {
            _dbContext.WorkOrders.Add(workOrder);
        }
    }
}
