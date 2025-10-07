using System.Collections.ObjectModel;

namespace Point_v1.Services;

public interface IMessagingService
{
    void Send<TMessage>(string message, TMessage parameter);
    void Subscribe<TMessage>(object subscriber, string message, Action<TMessage> callback);
    void Unsubscribe<TMessage>(object subscriber, string message);
}

public class MessagingService : IMessagingService
{
    private readonly Dictionary<string, List<WeakReference>> _subscribers = new();

    public void Send<TMessage>(string message, TMessage parameter)
    {
        if (_subscribers.ContainsKey(message))
        {
            // Создаем копию списка для безопасной итерации
            var subscribersCopy = _subscribers[message].ToList();
            var deadSubscribers = new List<WeakReference>();

            foreach (var subscriberRef in subscribersCopy)
            {
                if (subscriberRef.IsAlive && subscriberRef.Target != null)
                {
                    var callback = GetCallbackForSubscriber<TMessage>(subscriberRef.Target, message);
                    callback?.Invoke(parameter);
                }
                else
                {
                    deadSubscribers.Add(subscriberRef);
                }
            }

            // Удаляем мертвые ссылки
            foreach (var dead in deadSubscribers)
            {
                _subscribers[message].Remove(dead);
            }

            // Очищаем пустой список
            if (_subscribers[message].Count == 0)
            {
                _subscribers.Remove(message);
            }
        }
    }

    public void Subscribe<TMessage>(object subscriber, string message, Action<TMessage> callback)
    {
        if (!_subscribers.ContainsKey(message))
        {
            _subscribers[message] = new List<WeakReference>();
        }

        // Сохраняем callback в подписчике
        if (subscriber is IMessageSubscriber messageSubscriber)
        {
            messageSubscriber.SetCallback(message, callback);
        }

        _subscribers[message].Add(new WeakReference(subscriber));
    }

    public void Unsubscribe<TMessage>(object subscriber, string message)
    {
        if (_subscribers.ContainsKey(message))
        {
            var subscribersToRemove = new List<WeakReference>();

            foreach (var reference in _subscribers[message])
            {
                if (reference.IsAlive && reference.Target == subscriber)
                {
                    subscribersToRemove.Add(reference);
                }
            }

            foreach (var toRemove in subscribersToRemove)
            {
                _subscribers[message].Remove(toRemove);
            }

            if (_subscribers[message].Count == 0)
            {
                _subscribers.Remove(message);
            }
        }
    }

    private Action<TMessage> GetCallbackForSubscriber<TMessage>(object subscriber, string message)
    {
        if (subscriber is IMessageSubscriber messageSubscriber)
        {
            return messageSubscriber.GetCallback<TMessage>(message);
        }
        return null;
    }
}

public interface IMessageSubscriber
{
    void SetCallback<TMessage>(string message, Action<TMessage> callback);
    Action<TMessage> GetCallback<TMessage>(string message);
}