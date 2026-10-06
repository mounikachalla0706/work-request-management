# Work Request Management

A small C#/.NET sample application that models a business work-request workflow. It demonstrates object-oriented domain modeling, business-rule validation, layered architecture, repository abstractions, SQL relational modeling, and automated testing.

The focus is on the request lifecycle and the rules that govern it.

## Workflow

Requests move through the following lifecycle:

```text
Submitted -> Validated -> Assigned -> InProgress -> Completed
    |            |          |            |
    +------------+----------+------------+-> Cancelled
```

Cancellation is allowed from any active state. `Completed` and `Cancelled` are terminal states. Invalid transitions are rejected by the domain model.

## Business Rules

- A request ID must be positive, and title and description are required.
- Titles are limited to 200 characters; descriptions are limited to 1,000 characters.
- A due date cannot be earlier than the creation date.
- Requests must be validated before assignment, and only active users can be assigned.
- Requests must be assigned before work can start, and must be in progress before completion.
- Completed and cancelled requests cannot transition to another state.

## Architecture

```text
WorkRequestService
  ├── WorkRequestValidator
  ├── IWorkRequestRepository
  ├── IUserRepository
  └── WorkRequest (domain behavior)
```

- **`WorkRequestService`** coordinates application operations and repository access.
- **`WorkRequestValidator`** validates creation input, including required fields, length limits, enum values, and date rules.
- **`WorkRequest`** owns lifecycle transitions and protects its status and assigned-user ID behind domain methods such as `Validate`, `Assign`, `Start`, `Complete`, and `Cancel`.
- **Repository interfaces** separate application and domain behavior from persistence. The included implementations store data in memory.

Creation validation and lifecycle rules live in separate components so each has a focused responsibility. The `WorkRequest` constructor is internal; requests are created through the service's validation path. `WorkRequestException` represents business-rule and missing-request/user failures. Duplicate IDs are reported as `InvalidOperationException`, reflecting a repository invariant.

## Persistence and SQL

The application uses in-memory repositories, so the tests run without a database dependency. The `sql/` directory contains a SQL Server schema and example queries for the same data model.

The schema defines `Users` and `WorkRequests`, including a nullable foreign key from `WorkRequests.AssignedUserId` to `Users.Id`, constraints for dates and controlled values, and indexes.

## Project Layout

```text
src/WorkRequestManagement/
  Models/          Domain entities, enums, and business exception
  Repositories/    Repository interfaces and in-memory implementations
  Services/        WorkRequestService application workflows
  Validators/      Work-request creation validation
tests/
  WorkRequestManagement.Tests/  xUnit tests
sql/
  schema.sql       SQL Server table definitions and constraints
  queries.sql      Example SQL queries
```

## Build and Test

Requirements: .NET 8 SDK.

From the repository root:

```bash
dotnet build WorkRequestManagement.sln
dotnet test WorkRequestManagement.sln
```

The xUnit tests cover successful and invalid request creation, validation boundaries, duplicate IDs, permitted and rejected state transitions, user-assignment cases, failed-operation state preservation, cancellation, terminal states, and open-request filtering and ordering.

## Suggested Reading Order

1. `src/WorkRequestManagement/Services/WorkRequestService.cs` — how application operations are coordinated.
2. `src/WorkRequestManagement/Models/WorkRequest.cs` — how lifecycle rules are enforced.
3. `src/WorkRequestManagement/Validators/WorkRequestValidator.cs` — how creation input is checked.
4. `tests/WorkRequestManagement.Tests/WorkRequestServiceTests.cs` — how the behavior is verified.
5. `sql/schema.sql` — how the data model could be represented in SQL Server.

## Design Choices

The repository interfaces keep persistence separate from application workflows and domain behavior. In-memory implementations keep the sample easy to run, while the SQL schema and queries document a relational representation of the same model. Business requirements are expressed as explicit, testable rules, with lifecycle behavior owned by the domain entity.
