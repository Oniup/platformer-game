using System.ComponentModel.Design;
using System.Diagnostics;

namespace PlatformerGame.Engine.Events
{
    public abstract class Event
    {
        public bool Handled { get; set; } = false;
    }

    public partial class EventDispatcher
    {
        public delegate void ListenerCallback(Event eventData, object sender);

        private static EventDispatcher _instance = new EventDispatcher();

        private Dictionary<int, List<EventListener>> _listeners;
        private List<FiredEvent> _requested;

        private EventDispatcher()
        {
            if (_instance != null)
                throw new InvalidOperationException("Cannot initialize more than 1 event dispatcher");
            _instance = this;

            _listeners = new Dictionary<int, List<EventListener>>();
            _requested = new List<FiredEvent>();
        }

        public static void CallDeferredEvents()
        {
            _instance.CallDeferredEventsImpl();
        }

        public static void AddListener<T>(object listener, ListenerCallback callback) where T : Event
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.AddListenerImpl<T>(new EventListener(callback, listener));
        }

        public static void RemoveListener<T>(object listener) where T : Event
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.RemoveListenerImpl<T>(listener);
        }

        public static void FireEvent<T>(T eventData, object sender) where T : Event
        {
            Debug.Assert(_instance != null, "Event Dispatcher not initialized");
            _instance.FireEventImpl(eventData, sender);
        }

        private void CallDeferredEventsImpl()
        {
            for (int i = 0; i < _requested.Count; i++)
            {
                FiredEvent data = _requested[i];
                if (!_listeners.TryGetValue(data.Id, out List<EventListener>? listeners))
                    continue;

                foreach (EventListener listener in listeners)
                {
                    listener.Callback(data.Event, data.Sender);
                    if (data.Event.Handled)
                        break;
                }
            }

            _requested.Clear();
        }

        private void AddListenerImpl<T>(EventListener listener) where T : Event
        {
            int key = GetEventTypeId<T>();
            List<EventListener>? listeners;
            if (!_listeners.TryGetValue(key, out listeners))
            {
                listeners = new List<EventListener>();
                _listeners.Add(key, listeners);
            }
            listeners.Add(listener);
        }

        private void FireEventImpl<T>(T eventData, object sender) where T : Event
        {
            foreach (FiredEvent requested in _instance._requested)
            {
                // Don't allow duplicates of this event with the same sender
                if (requested.Sender == sender && requested.Event is T)
                    return;
            }
            _requested.Add(new FiredEvent(GetEventTypeId<T>(), eventData, sender));
        }

        private void RemoveListenerImpl<T>(object listener) where T : Event
        {
            int key = GetEventTypeId<T>();
            List<EventListener>? listeners;
            if (!_listeners.TryGetValue(key, out listeners))
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

        private static int GetEventTypeId<T>()
        {
            Type type = typeof(T);
            return type.GetHashCode();
        }
    }
}