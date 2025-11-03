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
                // Core
                new Player.CreateInfo(),
                new GameManager.CreateInfo(),
                new CameraController.CreateInfo(),
                new RespawnPosition.CreateInfo(),
                new RespawnEffect.CreateInfo(),
                new Background.CreateInfo(),
                new EndLevel.CreateInfo(),

                // Collectables
                new Fruit.CreateInfo(),
                // Effects
                new FruitCollected.CreateInfo(),

                // Traps
                new Trampoline.CreateInfo(),
                new Fan.CreateInfo(),
                // Damage Traps
                new SpikedBall.CreateInfo(),

                // Enemies
                new PigEnemy.CreateInfo(),
                new MushroomEnemy.CreateInfo(),
                new PlantShooterEnemy.CreateInfo(),
                new TrunkEnemy.CreateInfo(),
                // Enemy Projectiles
                new PlantShooterProjectile.CreateInfo(),

                // Main Menu UI
                new MainMenuCanvas.CreateInfo(),
                new SelectLevelCanvas.CreateInfo(),
                new SelectCharacterCanvas.CreateInfo(),
                new AppInfoCanvas.CreateInfo(),
                // Runtime UI
                new PauseCanvas.CreateInfo(),
                new RuntimeCanvas.CreateInfo(),
                new LevelCompleteCanvas.CreateInfo(),
            ];
        }

        protected override TilemapLayer.ICreateInfo[] DefineTilemapLayerCreateInfos()
        {
            return [
                new SpikeTilemapLayer.CreateInfo(),
                new PlatformTilemapLayer.CreateInfo(),
            ];
        }

        private static Actor[] OnBeforeSceneLoadedCallback(Scene scene, CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Background>(scene),
            ];
        }

        private static Actor[] OnAfterLevelLoadedCallback(CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Player>(),
                createInfos.Instantiate<CameraController>(),
                createInfos.Instantiate<GameManager>(),
                // UI
                createInfos.Instantiate<PauseCanvas>(),
                createInfos.Instantiate<RuntimeCanvas>(),
                createInfos.Instantiate<LevelCompleteCanvas>(),
            ];
        }

        private static Actor[] LoadMainMenu(CreateActorRegistry createInfos)
        {
            return [
                createInfos.Instantiate<Background>(),
                createInfos.Instantiate<MainMenuCanvas>(),
                createInfos.Instantiate<SelectLevelCanvas>(),
                createInfos.Instantiate<SelectCharacterCanvas>(),
                createInfos.Instantiate<AppInfoCanvas>(),
            ];
        }

        public static void Main(string[] args)
        {
            SaveData.CreateDefaultIfNotValid();

            var program = new Program(new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultOptions | WindowOptions.ManualResizable,
                AssetDirectory = GetAssetDirectory(),
                LDtkProjectRelativeDirectory = "/LevelData.ldtk",
#if !DEBUG
                RejectTilemapLayerIdentifiers = ["Prototype"],
#endif

                InitialLevelName = "Testing",
                // InitialLevelName = "Main Menu",
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