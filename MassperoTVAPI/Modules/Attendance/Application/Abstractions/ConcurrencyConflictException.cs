namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>
/// Raised when a save fails because the data changed since it was read (optimistic concurrency).
/// Lets the Application layer react without depending on EF Core.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}