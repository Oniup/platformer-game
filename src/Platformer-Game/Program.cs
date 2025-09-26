using Raylib_cs;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using System.Numerics;

namespace PlatformerGame
{
    internal class Program : Application
    {
        public Program(ApplicationCreateInfo createInfo)
            : base(createInfo)
        {
        }

        public override Actor.ICreateInfo[] DefineActorCreateInfos()
        {
            return [
                new Player.CreateInfo()
            ];
        }

        public override Actor[] ConstructTestScene(ResourceManager resources, Project project, CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Player>(resources, project, new Vector2(Window.Width / 2, Window.Height / 2)),
            ];
        }

        public static void Main(string[] args)
        {
            ApplicationCreateInfo createInfo = new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultConfigFlags | ConfigFlags.ResizableWindow,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectDirectory = "/LevelData/Testing.ldtk",
                InitialLevelName = "Main Menu",
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