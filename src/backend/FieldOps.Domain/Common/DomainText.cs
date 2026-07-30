namespace FieldOps.Domain.Common;

internal static class DomainText
{
    public static string Required(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        var normalised = value.Trim();
        if (normalised.Length > maxLength)
        {
            throw new ArgumentException($"The value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalised;
    }

    public static string Reference(string? value, string parameterName)
    {
        var normalised = Required(value, parameterName, 40).ToUpperInvariant();

        if (normalised.Any(character =>
                !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new ArgumentException(
                "References may contain only letters, numbers, hyphens and underscores.",
                parameterName);
        }

        return normalised;
    }

    public static string? Optional(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Required(value, parameterName, maxLength);
    }
}
