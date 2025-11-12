using System;

namespace Point_v1.Models;

public class Report
{
    public string Id { get; set; }
    public string TargetEventId { get; set; }
    public string ReporterUserId { get; set; }
    public ReportType Type { get; set; }
    public string Reason { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string ModeratorNotes { get; set; }
}

public enum ReportType
{
    Spam = 0,
    Inappropriate = 1,
    Scam = 2,
    Illegal = 3,
    Other = 4
}

public enum ReportStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}