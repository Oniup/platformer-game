using PlatformerGame.Engine.Level;

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

    public abstract class SceneEvents : IEvent
    {
        public SceneEvents(Scene scene)
        {
            Scene = scene;
        }

        public Scene Scene { get; init; }
    }

    public class NewCurrentSceneEvent : SceneEvents
    {
        public NewCurrentSceneEvent(Scene scene)
            : base(scene)
        {
        }
    }

    public class SetNewCurrentSceneEvent : IEvent
    {
        public enum IdentifierType
        {
            NeighbouringDirection,
            Iid,
        }

        private string _identifier;
        private IdentifierType _type;

        public SetNewCurrentSceneEvent(string identifier, IdentifierType type)
        {
            _identifier = identifier;
            _type = type;
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public IdentifierType SelectionType
        {
            get { return _type; }
        }
    }
}