namespace WorkRequestManagement.Models;

/// <summary>Represents a business-rule or workflow failure for a work request.</summary>
public sealed class WorkRequestException : Exception
{
    public WorkRequestException(string message) : base(message)
    {
    }
}
