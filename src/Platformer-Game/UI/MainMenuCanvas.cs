using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class MainMenuCanvas : ButtonCanvas
    {
        public MainMenuCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, Vector2 position)
            : base(uiPanels, buttonFont, position)
        {
            Showing = true;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            var startListPosition = new Vector2
            {
                X = (winWidth - ButtonSize.X) / 2,
                Y = (winHeight - ButtonSize.Y) / 3
            };

            HoveringElement = AddButtonVertical(startListPosition, 0, "Play", OpenLevelMenu, [
                (NextElementDirection.North, "Quit"),
                (NextElementDirection.South, "Character")
            ]);
            AddButtonVertical(startListPosition, 1, "Character", OpenCharacterMenu, [
                (NextElementDirection.North, "Play"),
                (NextElementDirection.South, "Quit")
            ]);
            AddButtonVertical(startListPosition, 2, "Quit", ExitApplication, [
                (NextElementDirection.North, "Character"),
                (NextElementDirection.South, "Play"),
            ]);
        }

        private void OpenLevelMenu()
        {
            var selectLevel = World.Find<SelectLevelCanvas>().First();
            selectLevel.Showing = true;
            Showing = false;
        }

        private void OpenCharacterMenu()
        {
            var selectPlayer = World.Find<SelectCharacterCanvas>().First();
            selectPlayer.Showing = true;
            Showing = false;
        }

        private void ExitApplication()
        {
            EventDispatcher.FireEvent(new WindowShouldClose());
        }

        public class CreateInfo : CreateInfo<MainMenuCanvas>
        {
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                resources.Load("UI Panels Button Font", new FontInstance(resources.AssetDirectory + "/Graphics/UI/Fonts/UI/Font.ttf", 15));
                resources.Load("UI Panels Info Font", new FontInstance(resources.AssetDirectory + "/Graphics/UI/Fonts/UndeadPixel/Font.ttf", 8));
                resources.Load("UI Panels", new SpriteAtlas(0, resources.AssetDirectory + "/Graphics/UI/Panels.png"));

                SetupStarAnimations(32, resources);
                SetupStarAnimations(64, resources);
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>("UI Panels");
                var buttonFont = resources.Get<FontInstance>("UI Panels Button Font");
                return new MainMenuCanvas(panels, buttonFont, info.Position);
            }

            private static void SetupStarAnimations(int gridSize, ResourceManager resources)
            {
                var atlas = new SpriteAtlas(gridSize, resources.AssetDirectory + $"/Graphics/UI/Star ({gridSize}x{gridSize}).png");
                var anims = new AnimationSet();
                anims.Add(atlas, "Active", 0, 7);
                anims.Add(atlas, "Inactive", 1, 1);
                resources.Load($"UI Star {gridSize}", atlas);
                resources.Load($"UI Star {gridSize} Animations", anims);
            }
        }
    }
}