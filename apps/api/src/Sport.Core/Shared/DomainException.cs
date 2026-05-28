namespace Sport.Core.Shared;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
