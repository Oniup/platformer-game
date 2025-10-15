using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Level;
using Raylib_cs;
using System.Numerics;

namespace PlatformerGame.Engine
{
    public class ApplicationCreateInfo
    {
        public string Title { get; init; } = "No Name";
        public WindowResolution Resolution { get; init; } = WindowResolution.Auto;
        public WindowOptions WindowOptions { get; init; } = Window.DefaultOptions;
        public int LimitFps = 0;

        public required string LDtkProjectRelativeDirectory { get; init; }
        public required string AssetDirectory { get; init; }

        public string RenderTargetResourceName { get; init; } = "Main Render Target";

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

        protected Application(ApplicationCreateInfo createInfo)
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

            _eventDispatcher = new EventDispatcher();
            _window = new Window(createInfo.Title, createInfo.LimitFps, createInfo.Resolution, createInfo.WindowOptions);

            // Load Resources from project
            _resources = new ResourceManager(createInfo.AssetDirectory);
            _project = new Project(createInfo.AssetDirectory + createInfo.LDtkProjectRelativeDirectory);

            _mainFramebuffer = new MainFramebuffer(_window);
            _resources.Load(createInfo.RenderTargetResourceName, _mainFramebuffer);
            _resources.LoadProjectRequired(_project);

            // Creating the world/level
            var registry = new CreateActorRegistry(_resources, _project, DefineActorCreateInfos(), DefineTilemapLayerCreateInfos());
            _world = new World(_project, registry, createInfo.InitialLevelName, createInfo.WorldCallbacks);
        }

        protected abstract Actor.ICreateInfo[] DefineActorCreateInfos();

        /// <summary>
        /// Define custom tilemap layer create infos to add custom functionality to the tilemap based on the identifier
        /// </summary>
        /// <returns></returns>
        protected abstract TilemapLayer.ICreateInfo[] DefineTilemapLayerCreateInfos();

        public void Run()
        {
            float lastTime = 0.0f;
            while (_window.IsRunning)
            {
                if (_world.LoadingNewLevel != null)
                { 
                    _world.LoadNew(_project);
                    _mainFramebuffer.CameraPosition = Vector2.Zero;
                }

                float time = (float)Raylib.GetTime();
                float deltaTime = CalculateDeltaTime(time, ref lastTime);

                _eventDispatcher.CallDeferedEvents();

                _world.Update(deltaTime);
                _world.LateUpdate(deltaTime);

                Draw();
            }
        }

        public virtual void Dispose()
        {
            // Properly cleaning up resources, cannot rely on gc to release in this specific order
            _resources.Dispose();
            _window.Dispose();
            _eventDispatcher.Dispose();
        }

        private void Draw()
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

            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
        }

        private float CalculateDeltaTime(float time, ref float lastTime)
        {
            float deltaTime = time - lastTime;
            lastTime = time;
            return deltaTime;
        }
    }
}