using Point_v1.Models;
using Point_v1.Services;

namespace Point_v1.Services;

public class ReportService : IReportService
{
    private readonly FirebaseRestService _firebaseRest;
    private readonly IAuthorizationService _authService;
    private readonly IDataService _dataService; 

    public ReportService(IAuthorizationService authService, IDataService dataService) 
    {
        _authService = authService;
        _dataService = dataService; 
        _firebaseRest = new FirebaseRestService();
    }

    public async Task<bool> CreateReportAsync(string eventId, string reporterId, ReportType type, string reason)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 Создание жалобы на событие: {eventId}");

            var eventItem = await _dataService.GetEventAsync(eventId);
            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие {eventId} не найдено в базе!");
                return false;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✅ Событие найдено: {eventItem.Title}");
            }

            var report = new Report
            {
                Id = Guid.NewGuid().ToString(),
                TargetEventId = eventId, 
                ReporterUserId = reporterId,
                Type = type,
                Reason = reason,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            System.Diagnostics.Debug.WriteLine($"💾 Сохраняем жалобу с TargetEventId: {eventId}");

            return await _firebaseRest.AddReportAsync(report);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания жалобы: {ex.Message}");
            return false;
        }
    }


    public async Task<List<Report>> GetPendingReportsAsync()
    {
        try
        {
            var reports = await _firebaseRest.GetReportsAsync();
            return reports.Where(r => r.Status == ReportStatus.Pending)
                         .OrderByDescending(r => r.CreatedAt)
                         .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки жалоб: {ex.Message}");
            return new List<Report>();
        }
    }

    public async Task<List<Report>> GetResolvedReportsAsync()
    {
        try
        {
            var reports = await _firebaseRest.GetReportsAsync();
            return reports.Where(r => r.Status != ReportStatus.Pending)
                         .OrderByDescending(r => r.ResolvedAt)
                         .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки решенных жалоб: {ex.Message}");
            return new List<Report>();
        }
    }

    public async Task<Report> GetReportAsync(string reportId)
    {
        try
        {
            var reports = await _firebaseRest.GetReportsAsync();
            return reports.FirstOrDefault(r => r.Id == reportId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки жалобы: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ResolveReportAsync(string reportId, string moderatorId, bool approveReport, string moderatorNotes)
    {
        try
        {
            if (!await _authService.CanModerateEventsAsync())
            {
                System.Diagnostics.Debug.WriteLine("❌ Недостаточно прав для модерации");
                return false;
            }

            var report = await GetReportAsync(reportId);
            if (report == null) return false;

            report.Status = approveReport ? ReportStatus.Approved : ReportStatus.Rejected;
            report.ResolvedBy = moderatorId;
            report.ResolvedAt = DateTime.UtcNow;
            report.ModeratorNotes = moderatorNotes;
            var success = await _firebaseRest.UpdateReportAsync(report);

            if (success && approveReport)
            {
                await HideReportedEventAsync(report.TargetEventId, moderatorId);
            }
            await LogModerationActionAsync(moderatorId, report);

            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка решения жалобы: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetPendingReportsCountAsync()
    {
        var reports = await GetPendingReportsAsync();
        return reports.Count;
    }

    private async Task HideReportedEventAsync(string eventId, string moderatorId)
    {
        try
        {
            var events = await _firebaseRest.GetEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null)
            {
                eventItem.IsActive = false;
                eventItem.ModerationNotes = $"Событие скрыто по жалобе модератором {moderatorId}";
                await _firebaseRest.UpdateEventAsync(eventItem);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка скрытия события: {ex.Message}");
        }
    }

    private async Task LogModerationActionAsync(string moderatorId, Report report)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                AdminUserId = moderatorId,
                Action = AuditAction.ReportResolved,
                TargetType = "Report",
                TargetId = report.Id,
                Changes = new Dictionary<string, object>
                {
                    { "Status", report.Status.ToString() },
                    { "TargetEventId", report.TargetEventId }
                },
                Timestamp = DateTime.UtcNow
            };

            await _firebaseRest.AddAuditLogAsync(auditLog);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка логирования: {ex.Message}");
        }
    }
}