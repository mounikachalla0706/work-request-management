using WorkRequestManagement.Models;

namespace WorkRequestManagement.Repositories;

/// <summary>Defines persistence operations for work requests.</summary>
public interface IWorkRequestRepository
{
    WorkRequest? GetById(int id);

    void Add(WorkRequest request);

    void Update(WorkRequest request);

    IReadOnlyCollection<WorkRequest> GetOpenRequests();
}
