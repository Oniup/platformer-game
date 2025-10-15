using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame.Engine.Level
{
    public class World
    {
        const float DefaultGravityScale = 1000f;
        private static World _instance = null!;

        private string _name;
        private bool _paused;
        private CreateActorRegistry _createInfos;
        private List<Actor> _globalActors;
        private List<Scene> _scenes;
        private Scene? _currentScene;
        private Color _backgroundColor;
        private Callbacks _levelLoadCallbacks;

#if DEBUG
        private bool _showCollisionOutlines = false;
#endif

        public readonly struct Callbacks
        {
            public delegate List<Actor> SetupCustomLevelCallback(CreateActorRegistry createInfos);

            public delegate List<Actor> BeforeLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate List<Actor> BeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);
            public delegate List<Actor> AfterLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate List<Actor> AfterSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);

            public List<(string, SetupCustomLevelCallback)> LoadCustomLevel { get; init; }
            public BeforeLevelLoadedCallback? BeforeLevelLoaded { get; init; }
            public BeforeSceneLoadedCallback? BeforeSceneLoaded { get; init; }
            public AfterLevelLoadedCallback? AfterLevelLoaded { get; init; }
            public AfterSceneLoadedCallback? AfterSceneLoaded { get; init; }
        }

        public World(Project project, CreateActorRegistry createInfos, string name, Callbacks levelLoadCallbacks)
        {
            _instance = this;

            _name = name;
            _globalActors = new List<Actor>();
            _scenes = new List<Scene>();
            _createInfos = createInfos;
            _levelLoadCallbacks = levelLoadCallbacks;
            _backgroundColor = ColorConverter.Convert(project.Header.BgColor);
            _backgroundColor = Color.DarkGray;

            EventDispatcher.AddListener<SetNewCurrentSceneEvent>(this, OnSetNewSceneEvent);

            LoadData(project);
        }

        public string? LoadingNewLevel { get; private set; }

        public string LevelName
        {
            get { return _name; }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        public static Scene CurrentScene
        {
            get { return _instance._currentScene!; }
        }

        public static List<Actor> GlobalActors
        {
            get { return _instance._globalActors; }
        }

        public static float GravityScale { get; set; } = DefaultGravityScale;

        public static bool Paused
        {
            get { return _instance._paused; }
            set { _instance._paused = value; }
        }

#if DEBUG
        public static bool ShowCollisionOutlines
        {
            get { return _instance._showCollisionOutlines; }
            set { _instance._showCollisionOutlines = value; }
        }
#endif

        public static T Instantiate<T>(Vector2 position, Scene? scene = null) where T : Actor
        {
            T actor = _instance._createInfos.Instantiate<T>(position, scene);
            if (scene != null)
                scene.Actors.Add(actor);
            else
                _instance._globalActors.Add(actor);

            actor.OnAwake();
            return actor;
        }

        public static List<T> Find<T>(Scene? scene = null, int limit = int.MaxValue) where T : Actor
        {
            List<T> found = new();
            List<Actor> actors = scene != null ? scene.Actors : _instance._globalActors;
            foreach (Actor actor in actors)
            {
                if (actor is T inst)
                {
                    found.Add(inst);
                    if (found.Count >= limit)
                        break;
                }
            }
            return found;
        }

        public static void Load(string name)
        {
            _instance.LoadingNewLevel = name;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < _globalActors.Count(); i++)
            {
                Actor actor = _globalActors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    actor.OnDispose();
                    _globalActors.RemoveAt(i);
                }
                actor.OnUpdate(deltaTime);
            }
            _currentScene?.Update(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < _globalActors.Count(); i++)
            {
                Actor actor = _globalActors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    actor.OnDispose();
                    _globalActors.RemoveAt(i);
                }
                actor.OnLateUpdate(deltaTime);
            }
            _currentScene?.LateUpdate(deltaTime);
        }

        public void Draw()
        {
            _currentScene?.Draw();
            foreach (Actor actor in _globalActors)
                actor.OnDraw();
        }

        public void LoadNew(Project project)
        {
            Clear();
            _name = LoadingNewLevel!;
            LoadData(project);

            Paused = false;
            LoadingNewLevel = null;
        }

        private void ConstructScenesFromLevelData(List<(LDtkLevel, LDtkLevelInfo)> sceneData)
        {
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

        private void Clear()
        {
            foreach (Actor actor in _globalActors)
                actor.OnDispose();
            foreach (Scene scene in _scenes)
            {
                foreach (Actor actor in scene.Actors)
                    actor.OnDispose();
            }
            _globalActors.Clear();
            _scenes.Clear();
        }

        private void LoadData(Project project)
        {
            foreach ((string customName, Callbacks.SetupCustomLevelCallback loadLevelCallback) in _levelLoadCallbacks.LoadCustomLevel)
            {
                if (customName == _name)
                {
                    _globalActors = loadLevelCallback(_createInfos);
                    CallActorsOnAwake();
                    return;
                }
            }

            if (_levelLoadCallbacks.BeforeLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.BeforeLevelLoaded(_createInfos));

            LDtkLevelInfo info = project.GetLevelInfoByIdentifier(_name) ?? throw new NullReferenceException($"Cannot level {_name}, definition doesn't exist");
            List<(LDtkLevel, LDtkLevelInfo)> sceneData = project.LoadLevel(info);
            ConstructScenesFromLevelData(sceneData);

            if (_levelLoadCallbacks.AfterLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.AfterLevelLoaded(_createInfos));

            CallActorsOnAwake();

            if (_currentScene != null)
                EventDispatcher.FireEvent(new NewCurrentSceneEvent(_currentScene));
        }

        private void CallActorsOnAwake()
        {
            foreach (Scene scene in _scenes)
            {
                foreach (Actor actor in scene.Actors)
                    actor.OnAwake();
            }
            foreach (Actor actor in _globalActors)
                actor.OnAwake();
        }

        private bool SetCurrentSceneByIid(string iid)
        {
            foreach (Scene scene in _scenes)
            {
                if (scene.IId == iid)
                {
                    _currentScene = scene;
                    return true;
                }
            }
            Console.Error.WriteLine($"Scene with IId {iid} doesn't exist");
            return false;
        }

        private void SetCurrentSceneByDirection(string dir)
        {
            if (_currentScene == null)
                throw new NullReferenceException("No current scene is set, thus can't set new current scene by direction");

            foreach (LDtkLevelInfo.Neighbour neighbour in _currentScene.Neighbours)
            {
                if (neighbour.Dir == dir)
                {
                    if (SetCurrentSceneByIid(neighbour.LevelIId))
                        return;
                }
            }
            Console.Error.WriteLine($"Could not load scene using direction {dir}");
        }

        private void OnSetNewSceneEvent(Event evt, object? sender)
        {
            SetNewCurrentSceneEvent data = (SetNewCurrentSceneEvent)evt;
            switch (data.SelectionType)
            {
                case SetNewCurrentSceneEvent.IdentifierType.Iid:
                    SetCurrentSceneByIid(data.Identifier);
                    break;
                case SetNewCurrentSceneEvent.IdentifierType.NeighbouringDirection:
                    SetCurrentSceneByDirection(data.Identifier);
                    break;
            }
            data.Handled = true;

            EventDispatcher.FireEvent(new NewCurrentSceneEvent(_currentScene!));
        }
    }
}