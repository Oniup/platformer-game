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
        private Callbacks _levelLoadCallbacks;

        public struct Callbacks
        {
            public delegate List<Actor> BeforeLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate List<Actor> BeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);
            public delegate List<Actor> AfterLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate List<Actor> AfterSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);

            public BeforeLevelLoadedCallback? BeforeLevelLoaded;
            public BeforeSceneLoadedCallback? BeforeSceneLoaded;
            public AfterLevelLoadedCallback? AfterLevelLoaded;
            public AfterSceneLoadedCallback? AfterSceneLoaded;
        }

        public World(Project project, CreateActorRegistry createInfos, string name, Callbacks levelLoadCallbacks)
        {
            _name = name;
            _globalActors = new List<Actor>();
            _createInfos = createInfos;
            _levelLoadCallbacks = levelLoadCallbacks;
            _backgroundColor = ColorConverter.Convert(project.Header.BgColor);

            LoadData(project, name);
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

        public void LoadNew(Project project, string name)
        {
            _globalActors.Clear();
            _scenes.Clear();
            LoadData(project, name);
        }

        private void ConstructScenes(List<(LDtkLevel, LDtkLevelInfo)> sceneData)
        {
            _scenes = new List<Scene>(sceneData.Count);
            foreach ((LDtkLevel data, LDtkLevelInfo info) in sceneData)
            {
                Scene scene = new Scene(info);

                if (_levelLoadCallbacks.BeforeSceneLoaded != null)
                    scene.AddActors(_levelLoadCallbacks.BeforeSceneLoaded(scene, _createInfos));

                List<Actor> globallyDefined = scene.Load(_createInfos, data);

                if (_levelLoadCallbacks.AfterSceneLoaded != null)
                    scene.AddActors(_levelLoadCallbacks.AfterSceneLoaded(scene, _createInfos));

                _scenes.Add(scene);
                _globalActors.AddRange(globallyDefined);
                if (scene.Identifier == _name)
                    _currentScene = scene;
            }
        }

        private void LoadData(Project project, string name)
        {
            if (_levelLoadCallbacks.BeforeLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.BeforeLevelLoaded(_createInfos));

            LDtkLevelInfo info = project.GetLevelInfoByIdentifier(name) ?? throw new NullReferenceException($"Cannot level {name}, definition doesn't exist");
            List<(LDtkLevel, LDtkLevelInfo)> sceneData = project.LoadLevel(info);
            ConstructScenes(sceneData);

            if (_levelLoadCallbacks.AfterLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.AfterLevelLoaded(_createInfos));
        }
    }
}