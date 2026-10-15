namespace API.Errors.Exceptions;

public class ProviderException : Exception
{
    public ProviderException(string message) : base(message) { }
}