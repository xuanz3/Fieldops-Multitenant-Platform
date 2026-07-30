using FieldOps.Application.Tenancy;
using FieldOps.Domain.Customers;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Infrastructure.Persistence;

public sealed class FieldOpsDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public FieldOpsDbContext(
        DbContextOptions<FieldOpsDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    private Guid? ActiveTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FieldOpsDbContext).Assembly);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(customer =>
                ActiveTenantId == customer.TenantId);

        modelBuilder.Entity<WorkOrder>()
            .HasQueryFilter(workOrder =>
                ActiveTenantId == workOrder.TenantId);

        modelBuilder.Entity<UserAccount>()
            .HasQueryFilter(user =>
                ActiveTenantId == user.TenantId);
    }
}
