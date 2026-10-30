using System.Net.Mail;
using System.Text.RegularExpressions;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Common;
internal static class DomainRules
{
    public static void RequiredId(Guid id, string field)
    {
        if (id == Guid.Empty) throw new DomainException($"{field} cannot be empty.");
    }
    public static string Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{field} is required.");

        var result = value.Trim();
        if (result.Length > maxLength) throw new DomainException($"{field} must be at most {maxLength} characters.");

        return result;
    }
    public static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Required(value, field, maxLength);
    }
    public static string? Email(string? value)
    {
        var email = Optional(value, "Email", 254);
        if (email is null) return null;

        if (!MailAddress.TryCreate(email, out var parsed) ||
            !string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("A valid email address is required.");

        return email.ToLowerInvariant();
    }
    public static string Slug(string value)
    {
        var slug = Required(value, "Slug", 63).ToLowerInvariant();
        if (slug.Length < 3 || !Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new DomainException("Slug must have 3-63 lowercase letters, digits or single hyphens between groups.");
        return slug;
    }
    public static string Currency(string value)
    {
        var code = Required(value, "Currency", 3).ToUpperInvariant();
        if (!Regex.IsMatch(code, "^[A-Z]{3}$")) throw new DomainException("Currency must be a three-letter ISO code.");
        return code;
    }
}
