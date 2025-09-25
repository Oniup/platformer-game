using Raylib_cs;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Event;

namespace PlatformerGame
{
    internal class Program : Application
    {
        public Program(ApplicationCreateInfo createInfo)
            : base(createInfo)
        {
            EventDispatcher.AddListener<LevelEvent>(this, LoadingNewLevelCallback);
        }

        public override void PreUpdate()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.A))
                EventDispatcher.FireEvent(new LevelEvent("Level A"));
            if (Raylib.IsKeyPressed(KeyboardKey.B))
                EventDispatcher.FireEvent(new LevelEvent("Level B"));
            if (Raylib.IsKeyPressed(KeyboardKey.C))
                EventDispatcher.FireEvent(new LevelEvent("Level C"));
        }

        private void LoadingNewLevelCallback(IEvent evnt, object? sender)
        {
            LevelEvent data = evnt as LevelEvent ?? throw new NullReferenceException("Invalid event type, needs to be LeveEvent");
            Console.WriteLine("Loading event: " + data.Name);
            data.Handled = true;
        }

        public static void Main(string[] args)
        {
            ApplicationCreateInfo createInfo = new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultConfigFlags | ConfigFlags.ResizableWindow,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectDirectory = "/LevelData/Testing.ldtk"
            };

            Program program = new Program(createInfo);
            program.Run();
            program.Dispose();
        }

        private static string GetAssetDirectory()
        {
            DirectoryInfo? dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                {
                    if (subDir.Name == "Assets")
                        return subDir.FullName;
                }
                dir = dir.Parent;
            }
            throw new NullReferenceException("Failed to find asset directory");
        }
    }
}