/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-23-2025
/// Last Edited:    9-25-2025
/// </summary>

using System.Diagnostics;

namespace PlatformerGame.Engine
{
    public abstract class IEvent
    {
        public bool Handled { get; set; } = false;
    }

    public class EventDispatcher
    {
        public delegate void ListenerCallback(IEvent eventData, object? sender);

        private static EventDispatcher? _instance;
        private Dictionary<int, List<EventListener>> _events;
        private List<FiredEvent> _requested;

        public EventDispatcher()
        {
            if (_instance != null)
                throw new NullReferenceException("Cannot initialize more than 1 event dispatcher");
            _instance = this;

            _events = new Dictionary<int, List<EventListener>>();
            _requested = new List<FiredEvent>();
        }

        ~EventDispatcher()
        {
            _instance = null;
        }

        public void CallDeferedEvents()
        {
            foreach (FiredEvent data in _requested)
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
            _requested.Clear();
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

        private class EventListener
        {
            private ListenerCallback _callback;
            private object _listener;

            public EventListener(ListenerCallback callback, object listener)
            {
                _callback = callback;
                _listener = listener;
            }

            public ListenerCallback Callback
            {
                get { return _callback; }
            }

            public object Listener
            {
                get { return _listener; }
            }

            public static bool operator ==(EventListener self, object compare)
            {
                return self._listener == compare;
            }

            public static bool operator !=(EventListener self, object compare)
            {
                return self._listener != compare;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                    return false;
                return this == obj;
            }

            public override int GetHashCode()
            {
                return _listener.GetHashCode();
            }
        }

        private class FiredEvent
        {
            private IEvent _data;
            private object? _sender;
            private int id;

            public FiredEvent(int eventId, IEvent eventData, object? sender)
            {
                id = eventId;
                _data = eventData;
                _sender = sender;
            }

            public IEvent Event
            {
                get { return _data; }
            }

            public object? Sender
            {
                get { return _sender; }
            }

            public int Id
            {
                get { return id; }
            }

        }
    }
}