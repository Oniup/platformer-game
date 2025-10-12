using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Traps;

namespace PlatformerGame
{
    internal class Program(ApplicationCreateInfo createInfo) : Application(createInfo)
    {
        protected override Actor.ICreateInfo[] DefineActorCreateInfos()
        {
            return [
                new Player.CreateInfo(),
                new Fruit.CreateInfo(),
                new FruitCollected.CreateInfo(),

                new Trampoline.CreateInfo(),
                new Fan.CreateInfo(),

                new RespawnPosition.CreateInfo(),
                new RespawnEffect.CreateInfo(),
                new Background.CreateInfo(),

                new CameraController.CreateInfo(),
                new GameManager.CreateInfo(),
            ];
        }

        protected override TilemapLayer.ICreateInfo[] DefineTilemapLayerCreateInfos()
        {
            return [
                new SpikeTilemapLayer.CreateInfo(),
                new PlatformTilemapLayer.CreateInfo(),
            ];
        }

        private static List<Actor> OnBeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Background>(Vector2.Zero, scene),
            ];
        }

        private static List<Actor> OnAfterLevelLoadedCallback(CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Player>(Vector2.Zero),
                createInfos.Instantiate<CameraController>(Vector2.Zero),
                createInfos.Instantiate<GameManager>(Vector2.Zero),
            ];
        }

        public static void Main(string[] args)
        {
            Program program = new Program(new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectRelativeDirectory = "/LevelData.ldtk",

                InitialLevelName = "Testing",
                WorldCallbacks = new World.Callbacks
                {
                    BeforeSceneLoaded = OnBeforeSceneLoadedCallback,
                    AfterLevelLoaded = OnAfterLevelLoadedCallback
                },
            });
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