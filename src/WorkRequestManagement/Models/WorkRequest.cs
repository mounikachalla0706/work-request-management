namespace WorkRequestManagement.Models;

/// <summary>Represents a work request and enforces its lifecycle rules.</summary>
public sealed class WorkRequest
{
    public int Id { get; }
    public string Title { get; }
    public string Description { get; }
    public Priority Priority { get; }
    public RequestType RequestType { get; }
    public WorkRequestStatus Status { get; private set; }
    public int? AssignedUserId { get; private set; }
    public DateTime CreatedDate { get; }
    public DateTime? DueDate { get; }

    internal WorkRequest(
        int id,
        string title,
        string description,
        Priority priority,
        RequestType requestType,
        DateTime createdDate,
        DateTime? dueDate)
    {
        Id = id;
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        RequestType = requestType;
        CreatedDate = createdDate;
        DueDate = dueDate;
        Status = WorkRequestStatus.Submitted;
    }

    public void Validate()
    {
        EnsureStatus(WorkRequestStatus.Submitted, WorkRequestStatus.Validated);
        Status = WorkRequestStatus.Validated;
    }

    public void Assign(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        EnsureStatus(WorkRequestStatus.Validated, WorkRequestStatus.Assigned);

        if (!user.IsActive)
        {
            throw new WorkRequestException(
                $"User {user.Id} is inactive and cannot be assigned a work request.");
        }

        AssignedUserId = user.Id;
        Status = WorkRequestStatus.Assigned;
    }

    public void Start()
    {
        EnsureStatus(WorkRequestStatus.Assigned, WorkRequestStatus.InProgress);
        Status = WorkRequestStatus.InProgress;
    }

    public void Complete()
    {
        EnsureStatus(WorkRequestStatus.InProgress, WorkRequestStatus.Completed);
        Status = WorkRequestStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is not (
            WorkRequestStatus.Submitted or
            WorkRequestStatus.Validated or
            WorkRequestStatus.Assigned or
            WorkRequestStatus.InProgress))
        {
            throw new WorkRequestException(
                $"A {Status} work request cannot be cancelled.");
        }

        Status = WorkRequestStatus.Cancelled;
    }

    private void EnsureStatus(WorkRequestStatus expected, WorkRequestStatus target)
    {
        if (Status != expected)
        {
            throw new WorkRequestException(
                $"Invalid status transition: {Status} -> {target}. Expected status: {expected}.");
        }
    }
}
