using System.Diagnostics;

namespace PlatformerGame.Engine.Event
{
    public abstract class IEvent
    {
        public bool Handled { get; set; } = false;
    }

    public partial class EventDispatcher : IDisposable
    {
        public delegate void ListenerCallback(IEvent eventData, object? sender);

        private static EventDispatcher? _instance;
        private Dictionary<int, List<EventListener>> _events;
        private List<FiredEvent> _requested;

        public EventDispatcher()
        {
            if (_instance != null)
                throw new InvalidOperationException("Cannot initialize more than 1 event dispatcher");
            _instance = this;

            _events = new Dictionary<int, List<EventListener>>();
            _requested = new List<FiredEvent>();
        }

        public void CallDeferedEvents()
        {
            while (_requested.Count > 0)
            {
                List<FiredEvent> requested = _requested;
                _requested = new List<FiredEvent>();

                foreach (FiredEvent data in requested)
                {
                    List<EventListener>? listeners;
                    if (!_events.TryGetValue(data.Id, out listeners))
                        continue;

                    foreach (EventListener lstnr in listeners)
                    {
                        lstnr.Callback(data.Event, data.Sender);
                        if (data.Event.Handled)
                            break;
                    }
                }
            }
        }

        public static void AddListener<T>(object listener, ListenerCallback callback) where T : IEvent
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.AddListenerImpl<T>(new EventListener(callback, listener));
        }

        public static void RemoveListener<T>(object listener) where T : IEvent
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.RemoveListenerImpl<T>(listener);
        }

        public static void FireEvent<T>(T eventData, object? sender = null) where T : IEvent
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.FireEventImpl<T>(eventData, sender);
        }

        public void Dispose()
        {
            _instance = null;
        }

        private void AddListenerImpl<T>(EventListener listener) where T : IEvent
        {
            int key = GetEventTypeId<T>();
            List<EventListener>? listeners;
            if (!_events.TryGetValue(key, out listeners))
            {
                listeners = new List<EventListener>();
                _events.Add(key, listeners);
            }
            listeners.Add(listener);
        }

        private void FireEventImpl<T>(T eventData, object? sender) where T : IEvent
        {
            _requested.Add(new FiredEvent(GetEventTypeId<T>(), eventData, sender));
        }

        private void RemoveListenerImpl<T>(object listener) where T : IEvent
        {
            int key = GetEventTypeId<T>();
            List<EventListener>? listeners;
            if (!_events.TryGetValue(key, out listeners))
                return;

            for (int i = 0; i < listeners.Count(); ++i)
            {
                if (listeners[i] == listener)
                {
                    listeners.RemoveAt(i);
                    return;
                }
            }
        }

        private int GetEventTypeId<T>()
        {
            Type type = typeof(T);
            return type.GetHashCode();
        }
    }
}