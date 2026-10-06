using WorkRequestManagement.Models;
using WorkRequestManagement.Repositories;
using WorkRequestManagement.Validators;

namespace WorkRequestManagement.Services;

/// <summary>
/// Coordinates work-request creation and lifecycle operations across validation,
/// domain behavior, and repository abstractions.
/// </summary>
/// <remarks>
/// Uses repository abstractions to keep workflow coordination separate from persistence.
/// Project: https://github.com/mounikachalla0706/work-request-management
/// </remarks>
public sealed class WorkRequestService
{
    private readonly IWorkRequestRepository requestRepository;
    private readonly IUserRepository userRepository;
    private readonly WorkRequestValidator validator;

    public WorkRequestService(
        IWorkRequestRepository requestRepository,
        IUserRepository userRepository,
        WorkRequestValidator validator)
    {
        this.requestRepository = requestRepository;
        this.userRepository = userRepository;
        this.validator = validator;
    }

    public WorkRequest CreateRequest(
        int id,
        string title,
        string description,
        Priority priority,
        RequestType requestType,
        DateTime createdDate,
        DateTime? dueDate = null)
    {
        validator.ValidateForCreation(
            id,
            title,
            description,
            priority,
            requestType,
            createdDate,
            dueDate);

        var request = new WorkRequest(
            id,
            title,
            description,
            priority,
            requestType,
            createdDate,
            dueDate);

        requestRepository.Add(request);
        return request;
    }

    public void ValidateRequest(int requestId)
    {
        var request = GetRequiredRequest(requestId);
        request.Validate();
        requestRepository.Update(request);
    }

    public void AssignRequest(int requestId, int userId)
    {
        var request = GetRequiredRequest(requestId);
        var user = userRepository.GetById(userId)
                   ?? throw new WorkRequestException($"User {userId} was not found.");

        request.Assign(user);
        requestRepository.Update(request);
    }

    public void StartWork(int requestId)
    {
        var request = GetRequiredRequest(requestId);
        request.Start();
        requestRepository.Update(request);
    }

    public void CompleteRequest(int requestId)
    {
        var request = GetRequiredRequest(requestId);
        request.Complete();
        requestRepository.Update(request);
    }

    public void CancelRequest(int requestId)
    {
        var request = GetRequiredRequest(requestId);
        request.Cancel();
        requestRepository.Update(request);
    }

    public WorkRequest GetRequest(int requestId) => GetRequiredRequest(requestId);

    private WorkRequest GetRequiredRequest(int requestId)
    {
        return requestRepository.GetById(requestId)
            ?? throw new WorkRequestException($"Work request {requestId} was not found.");
    }
}
