namespace FieldOps.Infrastructure.Persistence.Seeding;

public static class DemoDataIds
{
    public static readonly Guid NorthsideTenant =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid BaysideTenant =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid NorthsideCustomerOne =
        Guid.Parse("11111111-1111-1111-1111-111111110001");

    public static readonly Guid NorthsideCustomerTwo =
        Guid.Parse("11111111-1111-1111-1111-111111110002");

    public static readonly Guid BaysideCustomerOne =
        Guid.Parse("22222222-2222-2222-2222-222222220001");

    public static readonly Guid NorthsideWorkOrderOne =
        Guid.Parse("11111111-1111-1111-1111-111111120001");

    public static readonly Guid NorthsideWorkOrderTwo =
        Guid.Parse("11111111-1111-1111-1111-111111120002");

    public static readonly Guid BaysideWorkOrderOne =
        Guid.Parse("22222222-2222-2222-2222-222222220101");

    public static readonly Guid NorthsideAdminUser =
        Guid.Parse("11111111-1111-1111-1111-111111130001");

    public static readonly Guid NorthsideDispatcherUser =
        Guid.Parse("11111111-1111-1111-1111-111111130002");

    public static readonly Guid NorthsideTechnicianUser =
        Guid.Parse("11111111-1111-1111-1111-111111130003");

    public static readonly Guid NorthsideClientUser =
        Guid.Parse("11111111-1111-1111-1111-111111130004");

    public static readonly Guid BaysideAdminUser =
        Guid.Parse("22222222-2222-2222-2222-222222230001");
}
