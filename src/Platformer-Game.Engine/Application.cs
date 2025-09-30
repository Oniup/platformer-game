using PlatformerGame.Engine.Event;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Level;
using Raylib_cs;

namespace PlatformerGame.Engine
{
    public class ApplicationCreateInfo
    {
        public string Title { get; init; } = "No Name";
        public WindowResolution Resolution { get; init; } = WindowResolution.Auto;
        public WindowOptions WindowOptions { get; init; } = Window.DefaultOptions;

        public required string LDtkProjectDirectory { get; init; }
        public required string AssetDirectory { get; init; }

        public string RenderTargetResourceName { get; init; } = "Main Render Target";
        public float FixedUpdateTimeInterval { get; init; } = 1.0f / 60.0f;

        public required string InitialLevelName { get; init; }
        public World.Callbacks WorldCallbacks { get; init; }
    }

    public abstract class Application : IDisposable
    {
        private EventDispatcher _eventDispatcher;
        private Window _window;
        private ResourceManager _resources;
        private Project _project;
        private World _world;

        private MainFramebuffer _mainFramebuffer;
        private float _fixedUpdateTimeInterval;

        public Application(ApplicationCreateInfo createInfo)
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

            _eventDispatcher = new EventDispatcher();
            _window = new Window(createInfo.Title, createInfo.Resolution, createInfo.WindowOptions);

            // Load Resources from project
            _resources = new ResourceManager(createInfo.AssetDirectory);
            _project = new Project(createInfo.AssetDirectory + createInfo.LDtkProjectDirectory);
            _resources.LoadProjectRequired(_project);
            _mainFramebuffer = new MainFramebuffer(_window);
            _resources.Load(createInfo.RenderTargetResourceName, _mainFramebuffer);

            // Creating the world/level
            CreateActorRegistry registry = new CreateActorRegistry(_resources, _project, DefineActorCreateInfos());
            _world = new World(_project, registry, createInfo.InitialLevelName, createInfo.WorldCallbacks);

            _fixedUpdateTimeInterval = createInfo.FixedUpdateTimeInterval;

            List<(LDtkLevel, LDtkLevelInfo)> loaded = _project.LoadLevel(_project.GetLevelInfoByIdentifier("Level_0"));
            foreach ((LDtkLevel data, LDtkLevelInfo info) in loaded)
            {
                int entityCount = 0;
                foreach (LDtkLevel.Layer layer in data.LayerInstances)
                {
                    if (layer.EntityInstances.Count > 0)
                    {
                        entityCount = layer.EntityInstances.Count;
                        break;
                    }
                }
                Console.WriteLine($"Loaded Level {info.Identifier}, with {entityCount} Entities in it");
            }
        }

        public Window Window
        {
            get { return _window; }
        }

        public World World
        {
            get { return _world; }
        }

        public abstract Actor.ICreateInfo[] DefineActorCreateInfos();

        public void Run()
        {
            float lastTime = 0.0f;
            float lastFixedTime = 0.0f;
            while (_window.IsRunning)
            {
                float time = (float)Raylib.GetTime();
                float deltaTime = CalculateDeltaTime(time, ref lastTime);
                float fixedDeltaTime = CalculateFixedDeltaTime(time, ref lastFixedTime);

                _eventDispatcher.CallDeferedEvents();

                _world.Update(deltaTime);
                if (fixedDeltaTime != -1.0f)
                    _world.FixedUpdate(fixedDeltaTime);
                _world.LateUpdate(deltaTime);

                Draw();
            }
        }

        public void Draw()
        {
            Camera2D worldCamera = _mainFramebuffer.GetWorldCamera();
            Camera2D smoothCamera = _mainFramebuffer.GetSmoothCamera(worldCamera);

            // Draw to render target framebuffer
            _mainFramebuffer.Draw(worldCamera, _world.BackgroundColor, _world.Draw);

            // Draw render targets texture to window framebuffer
            Raylib.BeginDrawing();
            Raylib.BeginMode2D(smoothCamera);
            {
                _mainFramebuffer.DrawFramebufferTexture();
            }
            Raylib.EndMode2D();
            Raylib.EndDrawing();
        }

        public virtual void Dispose()
        {
            // Properly cleaning up resources, cannot rely on gc to release in this specific order
            _resources.Dispose();
            _window.Dispose();
            _eventDispatcher.Dispose();
        }

        private float CalculateDeltaTime(float time, ref float lastTime)
        {
            float deltaTime = time - lastTime;
            lastTime = time;
            return deltaTime;
        }

        private float CalculateFixedDeltaTime(float time, ref float lastTime)
        {
            float fixedDeltaTime = time - lastTime;
            if (fixedDeltaTime > _fixedUpdateTimeInterval)
            {
                lastTime = time;
                return fixedDeltaTime;
            }
            return -1.0f;
        }
    }
}