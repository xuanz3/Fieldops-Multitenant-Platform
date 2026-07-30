using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FieldOps.Infrastructure.Persistence.Design;

public sealed class FieldOpsDbContextFactory
    : IDesignTimeDbContextFactory<FieldOpsDbContext>
{
    public FieldOpsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FieldOps")
            ?? "Host=localhost;Port=5432;Database=fieldops;Username=fieldops;Password=fieldops_dev_password";

        var options = new DbContextOptionsBuilder<FieldOpsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FieldOpsDbContext(
            options,
            new DesignTimeTenantContext());
    }
}
