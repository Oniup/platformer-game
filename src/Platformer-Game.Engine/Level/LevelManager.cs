using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class LevelManager
    {
        private string _name;
        private List<Actor> _globalActors;
        private CreateActorRegistry _createInfos;

        public LevelManager(ResourceManager resources, Project project, CreateActorRegistry createInfos, string name)
        {
            _name = name;
            _globalActors = new List<Actor>();
            _createInfos = createInfos;
        }

        public List<Actor> Actors
        {
            get { return _globalActors; }
        }

        public CreateActorRegistry CreateInfos
        {
            get { return _createInfos; }
        }

        public void Update(float deltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnLateUpdate(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnFixedUpdate(fixedDeltaTime);
        }

        public void Draw()
        {
            foreach (Actor actor in _globalActors)
                actor.OnDraw();
        }
    }
}