using WorkRequestManagement.Models;

namespace WorkRequestManagement.Repositories;

/// <summary>Stores users in memory and provides lookup and active-user queries.</summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> users = new();

    public void Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!users.TryAdd(user.Id, user))
        {
            throw new InvalidOperationException(
                $"A user with ID {user.Id} already exists.");
        }
    }

    public User? GetById(int id)
    {
        return users.TryGetValue(id, out var user) ? user : null;
    }

    public IReadOnlyCollection<User> GetActiveUsers()
    {
        return users.Values
            .Where(user => user.IsActive)
            .ToList();
    }
}
