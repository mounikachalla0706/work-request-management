using WorkRequestManagement.Models;

namespace WorkRequestManagement.Repositories;

/// <summary>Defines persistence operations for users available to the application.</summary>
public interface IUserRepository
{
    User? GetById(int id);

    IReadOnlyCollection<User> GetActiveUsers();
}
