-- Find open high-priority requests.
SELECT
    Id,
    Title,
    Priority,
    Status,
    AssignedUserId,
    DueDate
FROM WorkRequests
WHERE Status NOT IN ('Completed', 'Cancelled')
  AND Priority IN ('High', 'Critical')
ORDER BY
    CASE Priority
        WHEN 'Critical' THEN 1
        WHEN 'High' THEN 2
        ELSE 3
    END,
    DueDate;

-- Count open requests by active user.
SELECT
    u.Id,
    u.Name,
    COUNT(w.Id) AS OpenRequestCount
FROM Users AS u
LEFT JOIN WorkRequests AS w
    ON w.AssignedUserId = u.Id
    AND w.Status NOT IN ('Completed', 'Cancelled')
WHERE u.IsActive = 1
GROUP BY
    u.Id,
    u.Name
ORDER BY
    OpenRequestCount DESC,
    u.Name;

-- Find overdue requests.
SELECT
    Id,
    Title,
    Priority,
    Status,
    AssignedUserId,
    DueDate
FROM WorkRequests
WHERE DueDate < SYSUTCDATETIME()
  AND Status NOT IN ('Completed', 'Cancelled')
ORDER BY DueDate;
