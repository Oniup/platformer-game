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
        public string[] RejectTilemapLayerIdentifiers { get; init; } = [];

        public string RenderTargetResourceName { get; init; } = "Main Render Target";

        public required string InitialLevelName { get; init; }
        public World.Callbacks WorldCallbacks { get; init; }
    }

    public abstract class Application : IDisposable
    {
        private Window _window;
        private ResourceRegistry _resources;
        private Project _project;
        private World _world;
        private MainFramebuffer _mainFramebuffer;

        protected Application(ApplicationCreateInfo createInfo)
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

            _window = new Window(createInfo.Title, createInfo.LimitFps, createInfo.Resolution, createInfo.WindowOptions);

            Raylib.InitAudioDevice();

            // Load Resources from project
            _resources = new ResourceRegistry(createInfo.AssetDirectory);
            _project = new Project(createInfo.AssetDirectory + createInfo.LDtkProjectRelativeDirectory);

            _mainFramebuffer = new MainFramebuffer(_window);
            _resources.Load(createInfo.RenderTargetResourceName, _mainFramebuffer);
            _resources.LoadProjectRequired(_project, createInfo.RejectTilemapLayerIdentifiers);

            // Creating the world/level
            var registry = new CreateActorRegistry(_resources, _project, DefineActorCreateInfos(), DefineTilemapLayerCreateInfos());
            _world = new World(_project, registry, createInfo.InitialLevelName, createInfo.WorldCallbacks);
        }

        /// <summary>
        /// Define what Actor.CreateInfos the game uses.
        /// </summary>
        /// <returns>List of Create Infos that the game requires</returns>
        protected abstract Actor.ICreateInfo[] DefineActorCreateInfos();

        /// <summary>
        /// Define what Tile map Layers Create Infos the game uses.
        /// </summary>
        /// <returns>List of Create Infos that the game requires</returns>
        protected abstract TilemapLayer.ICreateInfo[] DefineTilemapLayerCreateInfos();

        public void Run()
        {
            float lastTime = 0.0f;
            while (_window.IsOpen)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.F1))
                    _world.DebugShowMode = !_world.DebugShowMode;

                if (_world.LoadingNewLevel != null)
                { 
                    _world.LoadNew(_project);
                    _mainFramebuffer.CameraPosition = Vector2.Zero;
                }

                float time = (float)Raylib.GetTime();
                float deltaTime = CalculateDeltaTime(time, ref lastTime);

                EventDispatcher.CallDeferredEvents();
                _world.OnBeforeUpdate(deltaTime);
                _world.Update(deltaTime);

                Draw();
            }
        }

        public virtual void Dispose()
        {
            // Properly cleaning up resources, cannot rely on gc to release in this specific order
            _resources.Dispose();

            Raylib.CloseAudioDevice();
            _window.Dispose();
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

            // Debug Show FPS Mode
            if (_world.DebugShowMode)
                Raylib.DrawFPS(0, 0);

            Raylib.EndDrawing();
        }

        private static float CalculateDeltaTime(float time, ref float lastTime)
        {
            float deltaTime = time - lastTime;
            lastTime = time;
            return deltaTime;
        }
    }
}