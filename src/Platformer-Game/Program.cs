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
                new Player.CreateInfo(),
                new Fruit.CreateInfo(),
            ];
        }

        public override Actor[] ConstructTestScene(ResourceManager resources, Project project, CreateActorRegistry createInfos)
        {
            (float width, float height) = Window.GetResolutionSize(WindowResolution.nHD);
            Vector2 worldOrigin = new Vector2(width / 2, height / 2);

            return [
                createInfos.Instantiate<Player>(resources, project, worldOrigin),
            ];
        }

        public static void Main(string[] args)
        {
            ApplicationCreateInfo createInfo = new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
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