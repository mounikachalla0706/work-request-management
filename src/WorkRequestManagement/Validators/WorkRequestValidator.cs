using WorkRequestManagement.Models;

namespace WorkRequestManagement.Validators;

/// <summary>Validates work-request data and business rules before creation.</summary>
public sealed class WorkRequestValidator
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 1000;

    public void ValidateForCreation(
        int id,
        string? title,
        string? description,
        Priority priority,
        RequestType requestType,
        DateTime createdDate,
        DateTime? dueDate)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "Request ID must be positive.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Title is required.",
                nameof(title));
        }

        if (title.Trim().Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Title cannot exceed {MaxTitleLength} characters.",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Description is required.",
                nameof(description));
        }

        if (description.Trim().Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentException(
                "Priority is not a valid value.",
                nameof(priority));
        }

        if (!Enum.IsDefined(requestType))
        {
            throw new ArgumentException(
                "Request type is not a valid value.",
                nameof(requestType));
        }

        if (dueDate.HasValue && dueDate.Value < createdDate)
        {
            throw new ArgumentException(
                "Due date cannot be before the created date.",
                nameof(dueDate));
        }
    }
}
