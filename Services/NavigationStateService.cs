using System.Collections.Generic;

namespace Point_v1.Services;

public class NavigationStateService
{
    private readonly Stack<string> _navigationStack = new();

    public void PushPage(string pageName)
    {
        _navigationStack.Push(pageName);
        System.Diagnostics.Debug.WriteLine($"📚 Навигационный стек: {string.Join(" → ", _navigationStack)}");
    }

    public string PopPage()
    {
        if (_navigationStack.Count > 1)
        {
            _navigationStack.Pop(); // Убираем текущую страницу
            var previousPage = _navigationStack.Peek();
            System.Diagnostics.Debug.WriteLine($"🔙 Возврат к: {previousPage}");
            return previousPage;
        }
        return "///HomePage"; // По умолчанию на главную
    }

    public void Clear()
    {
        _navigationStack.Clear();
    }
}