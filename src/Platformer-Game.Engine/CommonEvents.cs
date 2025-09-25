/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-24-2025
/// Last Edited:    9-25-2025
/// </summary>

namespace PlatformerGame.Engine
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