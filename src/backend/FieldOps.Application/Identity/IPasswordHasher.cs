namespace FieldOps.Application.Identity;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string encodedHash);
}
