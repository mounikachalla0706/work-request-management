using WorkRequestManagement.Models;

namespace WorkRequestManagement.Repositories;

/// <summary>Stores work requests in memory and returns open requests in priority order.</summary>
public sealed class InMemoryWorkRequestRepository : IWorkRequestRepository
{
    private readonly Dictionary<int, WorkRequest> requests = new();

    public WorkRequest? GetById(int id)
    {
        return requests.TryGetValue(id, out var request) ? request : null;
    }

    public void Add(WorkRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!requests.TryAdd(request.Id, request))
        {
            throw new InvalidOperationException(
                $"A work request with ID {request.Id} already exists.");
        }
    }

    public void Update(WorkRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!requests.ContainsKey(request.Id))
        {
            throw new InvalidOperationException(
                $"Work request {request.Id} does not exist.");
        }

        requests[request.Id] = request;
    }

    public IReadOnlyCollection<WorkRequest> GetOpenRequests()
    {
        return requests.Values
            .Where(request =>
                request.Status != WorkRequestStatus.Completed &&
                request.Status != WorkRequestStatus.Cancelled)
            .OrderBy(request => request.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(request => request.Priority)
            .ToList();
    }
}
