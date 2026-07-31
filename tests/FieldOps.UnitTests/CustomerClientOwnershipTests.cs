using FieldOps.Domain.Customers;

namespace FieldOps.UnitTests;

public sealed class CustomerClientOwnershipTests
{
    [Fact]
    public void Customer_can_be_linked_to_client_user()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "CLIENT-001",
            "Test Client");

        var clientUserId =
            Guid.NewGuid();

        customer.LinkClient(
            clientUserId);

        Assert.Equal(
            clientUserId,
            customer.ClientUserId);
    }

    [Fact]
    public void Customer_can_be_unlinked()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "CLIENT-001",
            "Test Client",
            clientUserId:
                Guid.NewGuid());

        customer.LinkClient(null);

        Assert.Null(
            customer.ClientUserId);
    }
}
