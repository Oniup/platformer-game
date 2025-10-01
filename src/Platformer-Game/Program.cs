using System.Numerics;
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
                new Background.CreateInfo(),
            ];
        }

        public override TilemapLayer.ICreateInfo[] DefineTilemapLayerCreateInfos()
        {
            return [
                // Custom tilemap layers
            ];
        }

        public static void Main(string[] args)
        {
            Program program = new Program(new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectDirectory = "/LevelData/Testing.ldtk",

                InitialLevelName = "Level_0",
                WorldCallbacks = new World.Callbacks
                {
                    AfterSceneLoaded = BeforeSceneLoadedCallback,
                },
            });
            program.Run();
            program.Dispose();
        }

        private static List<Actor> BeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Background>(Vector2.Zero, scene),
            ];
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