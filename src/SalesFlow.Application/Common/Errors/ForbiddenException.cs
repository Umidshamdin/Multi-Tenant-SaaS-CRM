namespace SalesFlow.Application.Common.Errors;
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
