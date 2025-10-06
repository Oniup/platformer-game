namespace PlatformerGame.Engine.Events
{
    public partial class EventDispatcher
    {
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
            private Event _data;
            private object? _sender;
            private int id;

            public FiredEvent(int eventId, Event eventData, object? sender)
            {
                id = eventId;
                _data = eventData;
                _sender = sender;
            }

            public Event Event
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