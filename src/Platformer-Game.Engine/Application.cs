using Raylib_cs;
using PlatformerGame.Engine.Event;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine
{
    public class ApplicationCreateInfo
    {
        public string Title { get; init; } = "No Name";
        public WindowResolution Resolution { get; init; } = WindowResolution.Auto;
        public ConfigFlags WindowOptions { get; init; } = Window.DefaultConfigFlags;

        public required string LDtkProjectDirectory { get; init; }
        public required string AssetDirectory { get; init; }
    }

    public abstract class Application : IDisposable
    {
        private static Application? _instance = null;
        private EventDispatcher _eventDispatcher;
        private Window _window;
        private ResourceManager _resourceManager;
        private Project _projectData;

        public Application(ApplicationCreateInfo createInfo)
        {
            if (_instance != null)
                throw new NullReferenceException("Cannot initialize more than 1 Application");
            _instance = this;

            Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

            _eventDispatcher = new EventDispatcher();
            _window = new Window(createInfo.Title, createInfo.Resolution, createInfo.WindowOptions);

            _resourceManager = new ResourceManager(createInfo.AssetDirectory);
            _projectData = new Project(createInfo.AssetDirectory + createInfo.LDtkProjectDirectory);
            _resourceManager.LoadProject(_projectData);
        }

        public Application Instance
        {
            get
            {
                if (_instance == null)
                    throw new NullReferenceException("Application instance not been created yet");
                return _instance;
            }
        }

        public Window Window
        {
            get { return _window; }
        }

        public ResourceManager Resources
        {
            get { return _resourceManager; }
        }

        public abstract void PreUpdate();

        public void Run()
        {
            float lastTime = 0;
            while (_window.IsRunning)
            {
                float time = (float)Raylib.GetTime();
                float deltaTime = time - lastTime;
                lastTime = time;

                // Update actors
                PreUpdate();
                _eventDispatcher.CallDeferedEvents();

                Draw();
            }
        }

        public void Draw()
        {
            // Render to camera's framebuffer
            // Set clear background to the cameras framebuffer
            // then render camera's framebuffer to the main framebuffer 

            string message = "Test message";
            int fontSize = 30;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            {
                int length = Raylib.MeasureText(message, fontSize);
                Raylib.DrawText(message, Raylib.GetScreenWidth() / 2 - length / 2, Raylib.GetScreenHeight() / 2, fontSize, Color.Black);
            }
            Raylib.EndDrawing();
        }

        public virtual void Dispose()
        {
            // Properly cleaning up resources
            // Cannot rely on gc to release in this specific order
            _resourceManager.Dispose();
            _window.Dispose();
            _eventDispatcher.Dispose();

            _instance = null;
        }
    }
}