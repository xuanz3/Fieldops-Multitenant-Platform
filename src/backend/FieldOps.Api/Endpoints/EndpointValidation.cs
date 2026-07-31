using System.Net.Mail;
using FieldOps.Api.Contracts.Customers;
using FieldOps.Api.Contracts.WorkOrders;

namespace FieldOps.Api.Endpoints;

internal static class EndpointValidation
{
    public static Dictionary<string, string[]> Validate(
        CreateCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateReference(request.Reference, errors);
        ValidateRequired(request.Name, "name", 160, errors);
        ValidateEmail(request.Email, errors);

        return errors;
    }

    public static Dictionary<string, string[]> Validate(
        UpdateCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateRequired(request.Name, "name", 160, errors);
        ValidateEmail(request.Email, errors);

        return errors;
    }

    public static Dictionary<string, string[]> Validate(
        CreateWorkOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId == Guid.Empty)
        {
            errors["customerId"] =
                ["Customer ID is required."];
        }

        ValidateReference(request.Reference, errors);
        ValidateRequired(request.Title, "title", 200, errors);
        ValidateOptional(
            request.Description,
            "description",
            4000,
            errors);

        if (!Enum.IsDefined(request.Priority))
        {
            errors["priority"] =
                ["Priority is invalid."];
        }

        return errors;
    }

    public static Dictionary<string, string[]> Validate(
        UpdateWorkOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId == Guid.Empty)
        {
            errors["customerId"] =
                ["Customer ID is required."];
        }

        ValidateRequired(request.Title, "title", 200, errors);
        ValidateOptional(
            request.Description,
            "description",
            4000,
            errors);

        if (!Enum.IsDefined(request.Priority))
        {
            errors["priority"] =
                ["Priority is invalid."];
        }

        if (request.Version < 1)
        {
            errors["version"] =
                ["Version must be at least 1."];
        }

        return errors;
    }

    public static (int Page, int PageSize) NormalisePage(
        int? page,
        int? pageSize)
    {
        return (
            Math.Max(page ?? 1, 1),
            Math.Clamp(pageSize ?? 20, 1, 100));
    }

    private static void ValidateReference(
        string? value,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors["reference"] =
                ["Reference is required."];
            return;
        }

        var normalised = value.Trim();

        if (normalised.Length > 40)
        {
            errors["reference"] =
                ["Reference cannot exceed 40 characters."];
            return;
        }

        if (normalised.Any(character =>
                !char.IsLetterOrDigit(character) &&
                character is not '-' and not '_'))
        {
            errors["reference"] =
                ["Reference may contain only letters, numbers, hyphens and underscores."];
        }
    }

    private static void ValidateEmail(
        string? value,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var normalised = value.Trim();

        if (normalised.Length > 254)
        {
            errors["email"] =
                ["Email cannot exceed 254 characters."];
            return;
        }

        try
        {
            _ = new MailAddress(normalised);
        }
        catch (FormatException)
        {
            errors["email"] =
                ["Email format is invalid."];
        }
    }

    private static void ValidateRequired(
        string? value,
        string key,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] =
                [$"{UppercaseFirst(key)} is required."];
            return;
        }

        if (value.Trim().Length > maxLength)
        {
            errors[key] =
                [$"{UppercaseFirst(key)} cannot exceed {maxLength} characters."];
        }
    }

    private static void ValidateOptional(
        string? value,
        string key,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            value.Trim().Length > maxLength)
        {
            errors[key] =
                [$"{UppercaseFirst(key)} cannot exceed {maxLength} characters."];
        }
    }

    private static string UppercaseFirst(string value) =>
        char.ToUpperInvariant(value[0]) + value[1..];
}
