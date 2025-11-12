public class User
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BlockedUntil { get; set; }
    public List<string> InterestIds { get; set; } = new();
    public string City { get; set; }
    public string About { get; set; }

    // ДОБАВЬТЕ ЭТИ СВОЙСТВА:
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedEventsCount { get; set; } = 0;
    public int ParticipatedEventsCount { get; set; } = 0;
}
public enum UserRole
{
    User = 0,       // Обычный пользователь
    Organizer = 1,  // Может создавать события
    Moderator = 2,  // Может модерировать
    Admin = 3       // Полный доступ
}