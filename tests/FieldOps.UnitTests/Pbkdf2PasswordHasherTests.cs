using FieldOps.Infrastructure.Identity;

namespace FieldOps.UnitTests;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher =
        new();

    [Fact]
    public void Hash_and_verify_accepts_the_original_password()
    {
        const string password =
            "Correct-Horse-2026!";

        var encoded = _hasher.Hash(password);

        Assert.NotEqual(password, encoded);
        Assert.True(
            _hasher.Verify(
                password,
                encoded));
    }

    [Fact]
    public void Verify_rejects_a_different_password()
    {
        var encoded = _hasher.Hash(
            "Correct-Horse-2026!");

        Assert.False(
            _hasher.Verify(
                "Incorrect-Horse-2026!",
                encoded));
    }

    [Fact]
    public void Hash_uses_a_unique_random_salt()
    {
        const string password =
            "Correct-Horse-2026!";

        var first = _hasher.Hash(password);
        var second = _hasher.Hash(password);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_rejects_a_malformed_hash()
    {
        Assert.False(
            _hasher.Verify(
                "Correct-Horse-2026!",
                "malformed"));
    }
}
