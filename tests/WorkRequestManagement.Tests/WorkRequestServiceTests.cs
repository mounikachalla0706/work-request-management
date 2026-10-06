using WorkRequestManagement.Models;
using WorkRequestManagement.Repositories;
using WorkRequestManagement.Services;
using WorkRequestManagement.Validators;
using Xunit;

namespace WorkRequestManagement.Tests;

/// <summary>Verifies work-request creation, workflow operations, and failure cases.</summary>
public sealed class WorkRequestServiceTests
{
    private readonly InMemoryWorkRequestRepository requestRepository = new();
    private readonly InMemoryUserRepository userRepository = new();
    private readonly WorkRequestService service;

    public WorkRequestServiceTests()
    {
        service = new WorkRequestService(
            requestRepository,
            userRepository,
            new WorkRequestValidator());
    }

    [Fact]
    public void CreateRequest_WithValidData_Succeeds()
    {
        var request = service.CreateRequest(
            1,
            "Investigate processing delay",
            "Investigate a delayed scheduled processing operation.",
            Priority.High,
            RequestType.Incident,
            new DateTime(2026, 10, 1),
            new DateTime(2026, 10, 3));

        Assert.Equal(WorkRequestStatus.Submitted, request.Status);
        Assert.Equal(Priority.High, request.Priority);
        Assert.Equal(RequestType.Incident, request.RequestType);
    }

    [Fact]
    public void CreateRequest_WithoutTitle_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                " ",
                "A valid description.",
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithNullTitle_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                null!,
                "A valid description.",
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithNonPositiveId_Fails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.CreateRequest(
                0,
                "Valid title",
                "A valid description.",
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithNullDescription_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                "Valid title",
                null!,
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithInvalidPriority_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                "Valid title",
                "A valid description.",
                (Priority)999,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithInvalidRequestType_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                "Valid title",
                "A valid description.",
                Priority.Medium,
                (RequestType)999,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithTitleAtMaximumLength_Succeeds()
    {
        var title = new string('T', WorkRequestValidator.MaxTitleLength);

        var request = service.CreateRequest(
            1,
            title,
            "A valid description.",
            Priority.Medium,
            RequestType.General,
            DateTime.UtcNow);

        Assert.Equal(title, request.Title);
    }

    [Fact]
    public void CreateRequest_WithTitleOverMaximumLength_Fails()
    {
        var title = new string('T', WorkRequestValidator.MaxTitleLength + 1);

        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                title,
                "A valid description.",
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithDescriptionAtMaximumLength_Succeeds()
    {
        var description = new string('D', WorkRequestValidator.MaxDescriptionLength);

        var request = service.CreateRequest(
            1,
            "Valid title",
            description,
            Priority.Medium,
            RequestType.General,
            DateTime.UtcNow);

        Assert.Equal(description, request.Description);
    }

    [Fact]
    public void CreateRequest_WithDescriptionOverMaximumLength_Fails()
    {
        var description = new string('D', WorkRequestValidator.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                "Valid title",
                description,
                Priority.Medium,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateRequest_WithEqualCreationAndDueDate_Succeeds()
    {
        var createdDate = new DateTime(2026, 10, 1);

        var request = service.CreateRequest(
            1,
            "Same-day request",
            "The due date matches the creation date.",
            Priority.Medium,
            RequestType.General,
            createdDate,
            createdDate);

        Assert.Equal(createdDate, request.DueDate);
    }

    [Fact]
    public void CreateRequest_WithDueDateBeforeCreationDate_Fails()
    {
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest(
                1,
                "Invalid date request",
                "A valid description.",
                Priority.Medium,
                RequestType.General,
                new DateTime(2026, 10, 10),
                new DateTime(2026, 10, 9)));
    }

    [Fact]
    public void CreateRequest_WithDuplicateId_Fails()
    {
        CreateDefaultRequest();

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateRequest(
                1,
                "Another request",
                "A valid description.",
                Priority.Low,
                RequestType.General,
                DateTime.UtcNow));
    }

    [Fact]
    public void ValidateRequest_FromSubmitted_Succeeds()
    {
        CreateDefaultRequest();

        service.ValidateRequest(1);

        Assert.Equal(WorkRequestStatus.Validated, service.GetRequest(1).Status);
    }

    [Fact]
    public void AssignRequest_ToActiveUser_Succeeds()
    {
        CreateDefaultRequest();
        service.ValidateRequest(1);
        userRepository.Add(new User(10, "Alice"));

        service.AssignRequest(1, 10);

        var request = service.GetRequest(1);
        Assert.Equal(WorkRequestStatus.Assigned, request.Status);
        Assert.Equal(10, request.AssignedUserId);
    }

    [Fact]
    public void AssignRequest_ToInactiveUser_FailsAndPreservesState()
    {
        CreateDefaultRequest();
        service.ValidateRequest(1);

        var user = new User(10, "Alice");
        user.Deactivate();
        userRepository.Add(user);

        var exception = Assert.Throws<WorkRequestException>(() =>
            service.AssignRequest(1, 10));

        Assert.Contains("inactive", exception.Message);
        Assert.Equal(WorkRequestStatus.Validated, service.GetRequest(1).Status);
        Assert.Null(service.GetRequest(1).AssignedUserId);
    }

    [Fact]
    public void AssignRequest_ToMissingUser_FailsAndPreservesState()
    {
        CreateDefaultRequest();
        service.ValidateRequest(1);

        var exception = Assert.Throws<WorkRequestException>(() =>
            service.AssignRequest(1, 999));

        Assert.Contains("not found", exception.Message);
        Assert.Equal(WorkRequestStatus.Validated, service.GetRequest(1).Status);
    }

    [Fact]
    public void StartWork_WithoutAssignment_Fails()
    {
        CreateDefaultRequest();
        service.ValidateRequest(1);

        var exception = Assert.Throws<WorkRequestException>(() =>
            service.StartWork(1));

        Assert.Contains("Expected status: Assigned", exception.Message);
        Assert.Equal(WorkRequestStatus.Validated, service.GetRequest(1).Status);
    }

    [Fact]
    public void CompleteRequest_WhenInProgress_Succeeds()
    {
        CreateDefaultRequest();
        MoveRequestToInProgress(1);

        service.CompleteRequest(1);

        Assert.Equal(WorkRequestStatus.Completed, service.GetRequest(1).Status);
    }

    [Fact]
    public void CompleteRequest_FromSubmitted_Fails()
    {
        CreateDefaultRequest();

        var exception = Assert.Throws<WorkRequestException>(() =>
            service.CompleteRequest(1));

        Assert.Contains("Submitted -> Completed", exception.Message);
    }

    [Theory]
    [InlineData(WorkRequestStatus.Submitted)]
    [InlineData(WorkRequestStatus.Validated)]
    [InlineData(WorkRequestStatus.Assigned)]
    [InlineData(WorkRequestStatus.InProgress)]
    public void CancelRequest_FromActiveState_Succeeds(WorkRequestStatus state)
    {
        CreateRequestInState(state);

        service.CancelRequest(1);

        Assert.Equal(WorkRequestStatus.Cancelled, service.GetRequest(1).Status);
    }

    [Fact]
    public void CompletedRequest_CannotBeCancelled()
    {
        CreateDefaultRequest();
        MoveRequestToInProgress(1);
        service.CompleteRequest(1);

        var exception = Assert.Throws<WorkRequestException>(() =>
            service.CancelRequest(1));

        Assert.Contains("Completed", exception.Message);
    }

    [Fact]
    public void CancelledRequest_CannotBeModified()
    {
        CreateDefaultRequest();
        service.CancelRequest(1);

        Assert.Throws<WorkRequestException>(() => service.ValidateRequest(1));
        Assert.Equal(WorkRequestStatus.Cancelled, service.GetRequest(1).Status);
    }

    [Fact]
    public void GetOpenRequests_ExcludesTerminalRequestsAndOrdersByDueDateThenPriority()
    {
        service.CreateRequest(
            1, "Later", "Request one", Priority.Critical, RequestType.General,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 10));
        service.CreateRequest(
            2, "Soon", "Request two", Priority.Low, RequestType.General,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 5));
        service.CreateRequest(
            3, "Same day high", "Request three", Priority.High, RequestType.General,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 5));
        service.CreateRequest(
            4, "Completed", "Terminal request", Priority.Critical, RequestType.General,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 2));
        service.CreateRequest(
            5, "Cancelled", "Terminal request", Priority.Critical, RequestType.General,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 3));

        MoveRequestToInProgress(4);
        service.CompleteRequest(4);
        service.CancelRequest(5);

        var openRequests = requestRepository.GetOpenRequests().ToList();

        Assert.Equal(new[] { 3, 2, 1 }, openRequests.Select(r => r.Id));
    }

    private void CreateDefaultRequest()
    {
        service.CreateRequest(
            1,
            "Test work request",
            "A test request used by the unit tests.",
            Priority.Medium,
            RequestType.General,
            new DateTime(2026, 10, 1));
    }

    private void CreateRequestInState(WorkRequestStatus state)
    {
        CreateDefaultRequest();

        switch (state)
        {
            case WorkRequestStatus.Submitted:
                return;
            case WorkRequestStatus.Validated:
                service.ValidateRequest(1);
                return;
            case WorkRequestStatus.Assigned:
                service.ValidateRequest(1);
                userRepository.Add(new User(10, "Alice"));
                service.AssignRequest(1, 10);
                return;
            case WorkRequestStatus.InProgress:
                MoveRequestToInProgress(1);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Test state is not an active request state.");
        }
    }

    private void MoveRequestToInProgress(int requestId)
    {
        service.ValidateRequest(requestId);
        userRepository.Add(new User(10, "Alice"));
        service.AssignRequest(requestId, 10);
        service.StartWork(requestId);
    }
}
