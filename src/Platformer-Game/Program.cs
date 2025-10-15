using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Traps;
using PlatformerGame.UI;

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

                new MainMenuCanvas.CreateInfo(),
                new SelectPlayerCanvas.CreateInfo(),
                new PauseCanvas.CreateInfo(),
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
                createInfos.Instantiate<Background>(scene),
            ];
        }

        private static List<Actor> OnAfterLevelLoadedCallback(CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Player>(),
                createInfos.Instantiate<CameraController>(),
                createInfos.Instantiate<PauseCanvas>(),
                createInfos.Instantiate<GameManager>(),
            ];
        }

        private static List<Actor> LoadMainMenu(CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Background>(),
                createInfos.Instantiate<MainMenuCanvas>(),
                createInfos.Instantiate<SelectPlayerCanvas>(),
            ];
        }

        public static void Main(string[] args)
        {
            SaveData.CreateDefaultIfDoesntExist();

            Program program = new Program(new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectRelativeDirectory = "/LevelData.ldtk",

                InitialLevelName = "Main Menu",
                WorldCallbacks = new World.Callbacks
                {
                    BeforeSceneLoaded = OnBeforeSceneLoadedCallback,
                    AfterLevelLoaded = OnAfterLevelLoadedCallback,
                    LoadCustomLevel = [
                        ("Main Menu", LoadMainMenu),
                    ]
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