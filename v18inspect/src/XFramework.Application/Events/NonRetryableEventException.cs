namespace XFramework.Application.Events;

public class NonRetryableEventException : Exception
{
    public NonRetryableEventException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}