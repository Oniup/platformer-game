using Raylib_cs;
using PlatformerGame.Engine.Event;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Level;

namespace PlatformerGame.Engine
{
    public class ApplicationCreateInfo
    {
        public string Title { get; init; } = "No Name";
        public WindowResolution Resolution { get; init; } = WindowResolution.Auto;
        public WindowOptions WindowOptions { get; init; } = Window.DefaultOptions;

        public required string LDtkProjectDirectory { get; init; }
        public required string AssetDirectory { get; init; }

        public required string InitialLevelName { get; init; }
        public float FixedUpdateTimeInterval = 1.0f / 60.0f;

        public string RenderTargetResourceName = "Main Render Target";
    }

    public abstract class Application : IDisposable
    {
        private EventDispatcher _eventDispatcher;
        private Window _window;
        private ResourceManager _resources;
        private Project _project;
        private World _world;

        private RenderTarget _renderTarget;
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
            _renderTarget = new RenderTarget(_window);
            _resources.Load(createInfo.RenderTargetResourceName, _renderTarget);

            // Creating the world/level
            CreateActorRegistry registry = new CreateActorRegistry(_project, DefineActorCreateInfos());
            _world = new World(_resources, _project, registry, createInfo.InitialLevelName);

            _fixedUpdateTimeInterval = createInfo.FixedUpdateTimeInterval;
        }

        public Window Window
        {
            get { return _window; }
        }

        public abstract Actor.ICreateInfo[] DefineActorCreateInfos();
        public abstract Actor[] ConstructTestScene(ResourceManager resources, Project project, CreateActorRegistry createInfos);

        public void Run()
        {
            // FIXME: Remove later
            foreach (Actor actor in ConstructTestScene(_resources, _project, _world.CreateInfos))
                _world.Actors.Add(actor);

            float lastTime = 0.0f;
            float lastFixedTime = 0.0f;
            while (_window.IsRunning)
            {
                float time = (float)Raylib.GetTime();
                float deltaTime = CalculateDeltaTime(time, ref lastTime);
                float fixedDeltaTime = CalculateFixedDeltaTime(time, ref lastFixedTime);

                _eventDispatcher.CallDeferedEvents();

                _world.Update(deltaTime);
                _world.LateUpdate(deltaTime);
                if (fixedDeltaTime != -1.0f)
                    _world.FixedUpdate(fixedDeltaTime);

                Draw();
            }
        }

        public void Draw()
        {
            Camera2D worldCamera = _renderTarget.GetWorldCamera();
            Camera2D smoothCamera = _renderTarget.GetSmoothCamera(worldCamera);

            // Draw to render target framebuffer
            _renderTarget.Draw(worldCamera, Color.White, _world.Draw);

            // Draw render targets texture to window framebuffer
            Raylib.BeginDrawing();
            Raylib.BeginMode2D(smoothCamera);
            {
                _renderTarget.DrawFramebufferTexture();
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