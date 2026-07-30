using FieldOps.Application.Tenancy;
using FieldOps.Domain.Customers;
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

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    private Guid? ActiveTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FieldOpsDbContext).Assembly);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(customer =>
                ActiveTenantId.HasValue &&
                customer.TenantId == ActiveTenantId.Value);

        modelBuilder.Entity<WorkOrder>()
            .HasQueryFilter(workOrder =>
                ActiveTenantId.HasValue &&
                workOrder.TenantId == ActiveTenantId.Value);
    }
}
