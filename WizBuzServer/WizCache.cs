using System.Collections.Concurrent;

using WizBuzModels;

public class WizCache
{
    public WizCache()
    {
        _mapQueueName = new ConcurrentDictionary<string, List<MessageDto>>();
        _mapQueueListeners = new ConcurrentDictionary<string, List<SetListenerDto>>();
        _queueNames = new();
    }


    private ConcurrentDictionary<string, List<MessageDto>> _mapQueueName;
    private List<string> _queueNames;
    private ConcurrentDictionary<string, List<SetListenerDto>> _mapQueueListeners;

    public void PushToQueue(string queueName, MessageDto message, bool notifyListeners = false)
    {
        if (_mapQueueName.ContainsKey(queueName))
        {
            _mapQueueName[queueName].Add(message);
        }
        else
        {
            _mapQueueName[queueName] = new List<MessageDto> { message };
        }

        if (notifyListeners)
        {
            if (_mapQueueListeners.TryGetValue(queueName, out var listeners))
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    //TODO: Build notify mechanism
                }
            }

        }
    }

    public MessageDto? PopFromQueue(string queueName)
    {
        bool queueExists = _mapQueueName.TryGetValue(queueName, out var messages);
        if (queueExists)
        {
            var message = messages[0];
            messages.RemoveAt(0);
            if (messages.Count == 0)
            {
                _mapQueueName.Remove(queueName, out _);
            }
            else
            {
                _mapQueueName[queueName] = messages;
            }
        }

        return null;
    }

    public void AddListener(SetListenerDto listener)
    {
        if (_mapQueueListeners.TryGetValue(listener.QueueName, out var listenersForQueue))
        {
            listenersForQueue.Add(listener);
        }
        else
        {
            _mapQueueListeners[listener.QueueName] = new List<SetListenerDto> { listener };
        }
        
    }
}