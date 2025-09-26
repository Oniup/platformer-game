using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class Scene
    {
        private List<Actor> _actors;
        private string _iid;

        public Scene(ResourceManager resources, CreateActorRegistry createaInfos, Project project, string iid)
        {
            _actors = new List<Actor>();
            _iid = iid;

            foreach (LDtkLevelInfo info in project.Header.Levels)
            {
            }
        }

        public string IId
        {
            get { return _iid; }
        }

        public void Draw()
        {
        }

        public void Update(float deltaTime)
        {
        }

        public void LateUpdate(float deltaTime)
        {
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
        }

        public void Load(Project project, LDtkLevel level, ResourceManager resources, CreateActorRegistry createInfos)
        {
        }
    }
}