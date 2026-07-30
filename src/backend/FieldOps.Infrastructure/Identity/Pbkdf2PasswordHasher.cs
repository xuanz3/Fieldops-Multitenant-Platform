using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FieldOps.Application.Identity;

namespace FieldOps.Infrastructure.Identity;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "fieldops";
    private const string Algorithm = "pbkdf2-sha256";
    private const string Version = "v1";

    public string Hash(string password)
    {
        ValidatePassword(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        try
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return string.Join(
                '$',
                string.Empty,
                Prefix,
                Algorithm,
                Version,
                Iterations.ToString(CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public bool Verify(
        string password,
        string encodedHash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(encodedHash))
        {
            return false;
        }

        try
        {
            var parts = encodedHash.Split(
                '$',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 6 ||
                parts[0] != Prefix ||
                parts[1] != Algorithm ||
                parts[2] != Version ||
                !int.TryParse(
                    parts[3],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var iterations) ||
                iterations < 100_000 ||
                iterations > 1_000_000)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[4]);
            var expected = Convert.FromBase64String(parts[5]);

            if (salt.Length < SaltSize ||
                expected.Length < HashSize)
            {
                return false;
            }

            var passwordBytes = Encoding.UTF8.GetBytes(password);

            try
            {
                var actual = Rfc2898DeriveBytes.Pbkdf2(
                    passwordBytes,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expected.Length);

                return CryptographicOperations.FixedTimeEquals(
                    actual,
                    expected);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passwordBytes);
            }
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "A password is required.",
                nameof(password));
        }

        if (password.Length < 12)
        {
            throw new ArgumentException(
                "Passwords must contain at least 12 characters.",
                nameof(password));
        }

        if (password.Length > 256)
        {
            throw new ArgumentException(
                "Passwords cannot exceed 256 characters.",
                nameof(password));
        }
    }
}
