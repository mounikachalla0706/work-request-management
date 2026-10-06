-- SQL Server schema for the Work Request Management System.

CREATE TABLE Users
(
    Id INT NOT NULL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE WorkRequests
(
    Id INT NOT NULL PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    Priority VARCHAR(20) NOT NULL,
    RequestType VARCHAR(30) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    AssignedUserId INT NULL,
    CreatedDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NULL,

    CONSTRAINT FK_WorkRequests_Users
        FOREIGN KEY (AssignedUserId)
        REFERENCES Users(Id),

    CONSTRAINT CK_WorkRequests_DueDate
        CHECK (DueDate IS NULL OR DueDate >= CreatedDate),

    CONSTRAINT CK_WorkRequests_Priority
        CHECK (Priority IN ('Low', 'Medium', 'High', 'Critical')),

    CONSTRAINT CK_WorkRequests_Status
        CHECK (Status IN
        (
            'Submitted',
            'Validated',
            'Assigned',
            'InProgress',
            'Completed',
            'Cancelled'
        )),

    CONSTRAINT CK_WorkRequests_RequestType
        CHECK (RequestType IN
        (
            'General',
            'Incident',
            'Maintenance',
            'ConfigurationChange'
        ))
);

CREATE INDEX IX_WorkRequests_Status_Priority
    ON WorkRequests(Status, Priority);

CREATE INDEX IX_WorkRequests_AssignedUserId
    ON WorkRequests(AssignedUserId);
