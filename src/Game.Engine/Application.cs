/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Date Created:   9-19-2025
/// Date Created:   9-21-2025
/// </summary>

using Raylib_cs;

namespace Game.Engine
{
    public struct ApplicationCreateInfo
    {
        public string Title = "No Name";
        public WindowResolution Resolution = WindowResolution.Auto;
        public ConfigFlags WindowOptions = Window.DefaultConfigFlags;

        // Default initialization
        public ApplicationCreateInfo()
        {
        }
    }

    public abstract class Application
    {
        private static Application? _instance = null;
        private Window _window;
        private ResourceManager _resourceManager;
        private int _resId;

        public Application(ApplicationCreateInfo createInfo)
        {
            _instance = this;

            _window = new Window(createInfo.Title, createInfo.Resolution, createInfo.WindowOptions);
            _resourceManager = new ResourceManager();
        }

        public virtual void LoadExtraResources()
        {
            _resourceManager.Load(new SpriteAtlas("Terrain", 16, _resourceManager.GetAssetPath("Graphics/Terrain/Terrain (16x16).png")));
            _resId = _resourceManager.Get<SpriteAtlas>("Terrain").Id;
        }

        public void Run()
        {
            float lastTime = 0;
            while (_window.IsRunning)
            {
                float time = (float)Raylib.GetTime();
                float deltaTime = time - lastTime;
                lastTime = time;

                // Update actors

                Draw();
            }
        }

        public void Draw()
        {
            // Render to camera's framebuffer
            // Set clear background to the cameras framebuffer
            // then render camera's framebuffer to the main framebuffer 

            SpriteAtlas res = _resourceManager.Get<SpriteAtlas>(_resId);
            string message = string.Format("Name: {0}, Id: {1}, Type: {2}", res.Name, res.Id, res.Type);
            int fontSize = 30;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            {
                int length = Raylib.MeasureText(message, fontSize);
                Raylib.DrawText(message, Raylib.GetScreenWidth() / 2 - length / 2, Raylib.GetScreenHeight() / 2, fontSize, Color.Black);
            }
            Raylib.EndDrawing();
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
    }
}