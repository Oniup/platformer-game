using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;

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

        public static void Main(string[] args)
        {
            ApplicationCreateInfo createInfo = new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectDirectory = "/LevelData/Testing.ldtk",
                InitialLevelName = "Level_0",
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