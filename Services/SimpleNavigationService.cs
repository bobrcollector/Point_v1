namespace Point_v1.Services;

public class SimpleNavigationService
{
    private string _previousPage;

    public void SetPreviousPage(string pageName)
    {
        _previousPage = pageName;
        System.Diagnostics.Debug.WriteLine($"📝 Установлена предыдущая страница: {pageName}");
    }

    public async Task GoBackAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_previousPage))
            {
                System.Diagnostics.Debug.WriteLine($"🔙 Возврат на: {_previousPage}");
                await Shell.Current.GoToAsync($"///{_previousPage}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔙 Возврат назад (по стеку)");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка возврата: {ex.Message}");
            await Shell.Current.GoToAsync("///HomePage");
        }
    }
}