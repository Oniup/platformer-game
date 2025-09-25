namespace PlatformerGame.Engine.Event
{
    public class LevelEvent : IEvent
    {
        private string _name;

        public LevelEvent(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}