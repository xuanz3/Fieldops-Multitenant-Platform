using FieldOps.Domain.Identity;

namespace FieldOps.UnitTests;

public sealed class UserAccountTests
{
    [Fact]
    public void User_account_normalises_email_and_preserves_role()
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            "  ADMIN@EXAMPLE.TEST ",
            "Tenant Admin",
            "encoded-password-hash",
            UserRole.TenantAdmin);

        Assert.Equal(
            "admin@example.test",
            user.Email);
        Assert.Equal(
            UserRole.TenantAdmin,
            user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_account_can_be_deactivated()
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            "technician@example.test",
            "Technician",
            "encoded-password-hash",
            UserRole.Technician);

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@example.test")]
    public void User_account_rejects_invalid_email(
        string email)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserAccount(
                Guid.NewGuid(),
                email,
                "User",
                "encoded-password-hash",
                UserRole.Client));
    }
}
