/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-24-2025
/// </summary>

using Raylib_cs;

namespace Game.Engine
{
    public class ApplicationCreateInfo
    {
        public string Title { get; init; } = "No Name";
        public WindowResolution Resolution { get; init; } = WindowResolution.Auto;
        public ConfigFlags WindowOptions { get; init; } = Window.DefaultConfigFlags;

        public required string LDtkProjectDirectory { get; init; }
        public required string AssetDirectory { get; init; }
    }

    public abstract class Application
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

            _eventDispatcher = new EventDispatcher();
            _window = new Window(createInfo.Title, createInfo.Resolution, createInfo.WindowOptions);

            _resourceManager = new ResourceManager(createInfo.AssetDirectory);
            _projectData = new Project(createInfo.AssetDirectory + createInfo.LDtkProjectDirectory);
            _resourceManager.LoadProject(_projectData);
        }

        ~Application()
        {
            _instance = null;
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
    }
}