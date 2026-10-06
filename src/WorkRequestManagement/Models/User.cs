namespace WorkRequestManagement.Models;

/// <summary>Represents a user who may be assigned to a work request.</summary>
public sealed class User
{
    public int Id { get; }
    public string Name { get; }
    public bool IsActive { get; private set; }

    public User(int id, string name, bool isActive = true)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "User ID must be positive.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("User name is required.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
