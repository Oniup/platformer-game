namespace PlatformerGame.Engine.Events
{
    public partial class EventDispatcher
    {
        private class EventListener
        {
            public ListenerCallback Callback { get; }
            public object Listener { get; }

            public EventListener(ListenerCallback callback, object listener)
            {
                Callback = callback;
                Listener = listener;
            }

            public static bool operator ==(EventListener self, object compare)
            {
                return self.Listener == compare;
            }

            public static bool operator !=(EventListener self, object compare)
            {
                return self.Listener != compare;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                    return false;
                return this == obj;
            }

            public override int GetHashCode()
            {
                return Listener.GetHashCode();
            }
        }

        private class FiredEvent
        {
            public Event Event { get; }
            public object Sender { get; }
            public int Id { get; }

            public FiredEvent(int eventId, Event eventData, object sender)
            {
                Id = eventId;
                Event = eventData;
                Sender = sender;
            }
        }
    }
}