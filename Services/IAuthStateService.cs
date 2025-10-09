namespace Point_v1.Services;

public interface IAuthStateService
{
    bool IsAuthenticated { get; }
    string CurrentUserId { get; }
    event EventHandler AuthenticationStateChanged; // днаюбэ щрс ярпнйс
}