using Point_v1.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Point_v1.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged, IMessageSubscriber
{
    private readonly Dictionary<string, object> _callbacks = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public void SetCallback<TMessage>(string message, Action<TMessage> callback)
    {
        _callbacks[message] = callback;
    }

    public Action<TMessage> GetCallback<TMessage>(string message)
    {
        if (_callbacks.ContainsKey(message))
        {
            return _callbacks[message] as Action<TMessage>;
        }
        return null;
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnDisappearing()
    {
        // Очистка ресурсов при необходимости
    }
}