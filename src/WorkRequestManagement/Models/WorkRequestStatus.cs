namespace WorkRequestManagement.Models;

/// <summary>Defines the lifecycle states a work request can occupy.</summary>
public enum WorkRequestStatus
{
    Submitted,
    Validated,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}
