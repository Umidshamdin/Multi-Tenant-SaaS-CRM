namespace SalesFlow.Application.Common.Errors;
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
