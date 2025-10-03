namespace Point_v1.Models;

public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string City { get; set; }
    public string About { get; set; }
    public string PhotoUrl { get; set; }
    public List<string> InterestIds { get; set; } = new List<string>();
    public string Role { get; set; } = "user";
}