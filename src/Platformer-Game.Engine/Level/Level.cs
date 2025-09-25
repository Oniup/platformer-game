using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class Level
    {
        private string _name;
        private List<Actor> _globalActors;
        private CreateActorRegistry _createInfos;

        public Level(ResourceManager resources, CreateActorRegistry createInfos, string name, Project project)
        {
            _name = name;
            _globalActors = new List<Actor>();
            _createInfos = createInfos;
        }
    }
}