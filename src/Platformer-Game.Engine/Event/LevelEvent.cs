using PlatformerGame.Engine.Level;

namespace PlatformerGame.Engine.Events
{
    public class LevelEvent : Event
    {
        public string Name { get; }

        public LevelEvent(string name)
        {
            Name = name;
        }
    }

    public abstract class SceneEvents : Event
    {
        public Scene Scene { get; init; }

        public SceneEvents(Scene scene)
        {
            Scene = scene;
        }
    }

    public class NewCurrentSceneEvent : SceneEvents
    {
        public NewCurrentSceneEvent(Scene scene)
            : base(scene)
        {
        }
    }

    public class SetNewCurrentSceneEvent : Event
    {
        public string Identifier { get; }
        public IdentifierType SelectionType { get; }

        public SetNewCurrentSceneEvent(string identifier, IdentifierType type)
        {
            Identifier = identifier;
            SelectionType = type;
        }

        public enum IdentifierType
        {
            NeighbouringDirection,
            Iid,
        }
    }
}