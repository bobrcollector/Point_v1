using System;
using System.Collections.Generic;

namespace Point_v1.Models;

public class AuditLog
{
    public string Id { get; set; }
    public string AdminUserId { get; set; }
    public AuditAction Action { get; set; }
    public string TargetType { get; set; }
    public string TargetId { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; }
}

public enum AuditAction
{
    UserBlocked = 0,
    UserUnblocked = 1,
    UserRoleChanged = 2,
    EventApproved = 3,
    EventRejected = 4,
    EventDeleted = 5,
    ReportResolved = 6
}