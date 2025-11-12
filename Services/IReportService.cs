using Point_v1.Models;

namespace Point_v1.Services;

public interface IReportService
{
    Task<bool> CreateReportAsync(string eventId, string reporterId, ReportType type, string reason); // ИСПРАВЛЕНО: ReportType вместо Report
    Task<List<Report>> GetPendingReportsAsync();
    Task<List<Report>> GetResolvedReportsAsync();
    Task<Report> GetReportAsync(string reportId);
    Task<bool> ResolveReportAsync(string reportId, string moderatorId, bool approveReport, string moderatorNotes);
    Task<int> GetPendingReportsCountAsync();
}