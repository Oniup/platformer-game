using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame.Engine.Level
{
    public class World
    {
        private string _name;
        private CreateActorRegistry _createInfos;
        private List<Actor> _globalActors;
        private List<Scene> _scenes = null!;
        private Scene _currentScene = null!;
        private Color _backgroundColor;

        public World(Project project, CreateActorRegistry createInfos, string name)
        {
            _name = name;
            _globalActors = new List<Actor>();
            _createInfos = createInfos;

            // Construct Scenes
            List<(LDtkLevel, LDtkLevelInfo)> sceneData = project.LoadLevel(project.GetLevelInfoByIdentifier(name));
            ConstructScenes(sceneData);
            _backgroundColor = ColorConverter.Convert(project.Header.BgColor);
            // _backgroundColor = Color.White;
        }

        public string LevelName
        {
            get { return _name; }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        public void Update(float deltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnUpdate(deltaTime);
            _currentScene.Update(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnLateUpdate(deltaTime);
            _currentScene.LateUpdate(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (Actor actor in _globalActors)
                actor.OnFixedUpdate(fixedDeltaTime);
            _currentScene.FixedUpdate(fixedDeltaTime);
        }

        public void Draw()
        {
            _currentScene.Draw();
            foreach (Actor actor in _globalActors)
                actor.OnDraw();
        }

        public void LoadNewLevel(Project project, string name)
        {
            _globalActors.Clear();
            _scenes.Clear();

            List<(LDtkLevel, LDtkLevelInfo)> sceneData = project.LoadLevel(project.GetLevelInfoByIdentifier(name));
            ConstructScenes(sceneData);
        }

        private void ConstructScenes(List<(LDtkLevel, LDtkLevelInfo)> sceneData)
        {
            _scenes = new List<Scene>(sceneData.Count);
            foreach ((LDtkLevel data, LDtkLevelInfo info) in sceneData)
            {
                Scene scene = new Scene(info);
                List<Actor> globallyDefined = scene.Load(_createInfos, data);

                _scenes.Add(scene);
                _globalActors.AddRange(globallyDefined);
                if (scene.Identifier == _name)
                    _currentScene = scene;
            }
        }
    }
}