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
        CreateActorRegistry _createInfos;
        private List<Actor> _globalActors;
        private List<Scene> _scenes;
        private Scene? _currentScene;
        private Color _backgroundColor;
        private Callbacks _levelLoadCallbacks;
        public string _levelName;

        public string? LoadingNewLevel { get; private set; }
        public float GravityScale { get; set; } = DefaultGravityScale;
        public bool Paused { get; set; }

#if DEBUG
        public bool ShowCollisionOutlines { get; set; }
#endif

        public readonly struct Callbacks
        {
            public delegate Actor[] SetupCustomLevelCallback(CreateActorRegistry createInfos);

            public delegate Actor[] BeforeLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate Actor[] BeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);
            public delegate Actor[] AfterLevelLoadedCallback(CreateActorRegistry createInfos);
            public delegate Actor[] AfterSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos);

            public List<(string, SetupCustomLevelCallback)> LoadCustomLevel { get; init; }
            public BeforeLevelLoadedCallback? BeforeLevelLoaded { get; init; }
            public BeforeSceneLoadedCallback? BeforeSceneLoaded { get; init; }
            public AfterLevelLoadedCallback? AfterLevelLoaded { get; init; }
            public AfterSceneLoadedCallback? AfterSceneLoaded { get; init; }
        }

        public World(Project project, CreateActorRegistry createInfos, string name, Callbacks levelLoadCallbacks)
        {
            _levelName = name;
            _globalActors = new List<Actor>();
            _scenes = new List<Scene>();
            _createInfos = createInfos;
            _levelLoadCallbacks = levelLoadCallbacks;
            _backgroundColor = ColorConverter.Convert(project.Header.BgColor);

            EventDispatcher.AddListener<SetNewCurrentSceneEvent>(this, OnSetNewSceneEvent);

            LoadData(project);
        }

        public Color BackgroundColor => _backgroundColor;
        public Scene CurrentScene => _currentScene!;
        public List<Actor> GlobalActors => _globalActors;
        public string LevelName => _levelName;

        public T Instantiate<T>(Vector2 position, Scene? scene = null)
            where T : Actor
        {
            T actor = _createInfos.Instantiate<T>(position, scene);
            actor.World = this;
            if (scene != null)
                scene.Actors.Add(actor);
            else
                _globalActors.Add(actor);

            actor.World = this;
            actor.OnAwake();
            return actor;
        }

        public T InstantiateBehind<T>(Vector2 position, Actor spawnBehind, Scene? scene = null)
            where T : Actor
        {
            T actor = _createInfos.Instantiate<T>(position, scene);
            bool foundPosition = false;
            if (scene != null)
            {
                for (int i = 0; i < scene.Actors.Count; i++)
                {
                    if (scene.Actors[i] == spawnBehind)
                    {
                        scene.Actors.Insert(i, actor);
                        foundPosition = true;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _globalActors.Count; i++)
                {
                    if (_globalActors[i] == spawnBehind)
                    {
                        _globalActors.Insert(i, actor);
                        foundPosition = true;
                        break;
                    }
                }
            }
            if (!foundPosition)
                throw new NullReferenceException($"Could not find the actor of to spawn of type {typeof(T).Name} behind");

            actor.World = this;
            actor.OnAwake();
            return actor;
        }

        public List<T> Find<T>(Scene? scene = null, int limit = int.MaxValue)
            where T : Actor
        {
            var found = new List<T>();
            List<Actor> actors = scene != null ? scene.Actors : _globalActors;
            foreach (Actor actor in actors)
            {
                if (found.Count >= limit)
                    break;

                if (actor is T inst)
                    found.Add(inst);
            }
            return found;
        }

        public List<T> FindAll<T>(int limit = int.MaxValue)
            where T : Actor
        {
            List<T> found = Find<T>(null, limit);
            limit -= found.Count;
            foreach (Scene scene in _scenes)
            {
                if (limit < 0)
                    break;

                List<T> actors = Find<T>(scene, limit);
                if (actors.Count > 0)
                {
                    limit -= actors.Count;
                    found.AddRange(actors);
                }
            }
            return found;
        }

        public int FindCount<T>(Scene? scene = null)
            where T : Actor
        {
            int count = 0;
            List<Actor> actors = scene != null ? scene.Actors : _globalActors;
            foreach (Actor actor in actors)
            {
                if (actor is T)
                    count++;
            }
            return count;
        }

        public int FindAllCount<T>()
            where T : Actor
        {
            int count = FindCount<T>();
            foreach (Scene scene in _scenes)
                count += FindCount<T>(scene);
            return count;
        }

        public void Load(string name)
        {
            LoadingNewLevel = name;
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

        public void OnBeforeUpdate(float deltaTime)
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
                actor.OnBeforeUpdate(deltaTime);
            }
            _currentScene?.OnBeforeUpdate(deltaTime);
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
            _levelName = LoadingNewLevel!;
            LoadData(project);

            Paused = false;
            LoadingNewLevel = null;
        }

        private void ConstructScenesFromLevelData(List<(LDtkLevel, LDtkLevelInfo)> sceneData)
        {
            foreach ((LDtkLevel data, LDtkLevelInfo info) in sceneData)
            {
                var scene = new Scene(info);

                if (_levelLoadCallbacks.BeforeSceneLoaded != null)
                    scene.AddActors(_levelLoadCallbacks.BeforeSceneLoaded(scene, _createInfos));

                List<Actor> globallyDefined = scene.Load(_createInfos, data);

                if (_levelLoadCallbacks.AfterSceneLoaded != null)
                    scene.AddActors(_levelLoadCallbacks.AfterSceneLoaded(scene, _createInfos));

                _scenes.Add(scene);
                _globalActors.AddRange(globallyDefined);

                if (scene.Identifier == _levelName)
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
                if (customName == _levelName)
                {
                    _globalActors = new List<Actor>(loadLevelCallback(_createInfos));
                    ActorAwakeSetup();
                    return;
                }
            }

            if (_levelLoadCallbacks.BeforeLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.BeforeLevelLoaded(_createInfos));

            LDtkLevelInfo info = project.GetLevelInfoByIdentifier(_levelName) ?? throw new NullReferenceException($"Cannot level {_levelName}, definition doesn't exist");
            List<(LDtkLevel, LDtkLevelInfo)> sceneData = project.LoadLevel(info);
            ConstructScenesFromLevelData(sceneData);

            if (_levelLoadCallbacks.AfterLevelLoaded != null)
                _globalActors.AddRange(_levelLoadCallbacks.AfterLevelLoaded(_createInfos));

            ActorAwakeSetup();

            if (_currentScene != null)
                EventDispatcher.FireEvent(new NewCurrentSceneEvent(_currentScene));
        }

        private void ActorAwakeSetup()
        {
            foreach (Scene scene in _scenes)
            {
                foreach (Actor actor in scene.Actors)
                {
                    actor.World = this;
                    actor.OnAwake();
                }
            }
            foreach (Actor actor in _globalActors)
            {
                actor.World = this;
                actor.OnAwake();
            }
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

            foreach (LDtkLevelInfo.Neighbor neighbor in _currentScene.Neighbors)
            {
                if (neighbor.Dir == dir)
                {
                    if (SetCurrentSceneByIid(neighbor.LevelIId))
                        return;
                }
            }
            Console.Error.WriteLine($"Could not load scene using direction {dir}");
        }

        private void OnSetNewSceneEvent(Event evt, object? sender)
        {
            var data = (SetNewCurrentSceneEvent)evt;
            switch (data.SelectionType)
            {
                case SetNewCurrentSceneEvent.IdentifierType.Iid:
                    SetCurrentSceneByIid(data.Identifier);
                    break;
                case SetNewCurrentSceneEvent.IdentifierType.NeighboringDirection:
                    SetCurrentSceneByDirection(data.Identifier);
                    break;
            }
            data.Handled = true;

            EventDispatcher.FireEvent(new NewCurrentSceneEvent(_currentScene!));
        }
    }
}