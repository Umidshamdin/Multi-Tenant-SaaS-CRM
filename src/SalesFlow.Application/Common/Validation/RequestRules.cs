using SalesFlow.Application.Common.Errors;

namespace SalesFlow.Application.Common.Validation;
internal static class RequestRules
{
    public static void RequiredId(Guid id, string name)
    {
        if (id == Guid.Empty) throw new ValidationException($"{name} is required.");
    }
    public static string Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ValidationException($"{name} is required.");
        return value.Trim();
    }
    public static void ValidEnum<TEnum>(TEnum value, string name) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ValidationException($"Invalid {name}.");
    }
}
