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
        public MainMenuCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, SoundEffect nextButtonSound, SoundEffect selectButtonSound, Vector2 position)
            : base(uiPanels, buttonFont, nextButtonSound, selectButtonSound, position)
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
            EventDispatcher.FireEvent(new WindowShouldClose(), this);
        }

        public class CreateInfo : CreateInfo<MainMenuCanvas>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                // Panel and buttons
                resources.Load(PanelResourceName, new SpriteAtlas(0, $"{resources.AssetDirectory}/Graphics/UI/Panels.png"));

                // Fonts
                resources.Load(ButtonFontResourceName, new FontInstance($"{resources.AssetDirectory}/Graphics/UI/Fonts/UI/Font.ttf", 15));
                resources.Load(InfoFontResourceName, new FontInstance($"{resources.AssetDirectory}/Graphics/UI/Fonts/UndeadPixel/Font.ttf", 8));

                // Sound effects
                var nextButtonSound = new SoundEffect([$"{resources.AssetDirectory}/Sounds/UI/menu_129.wav"], 4);
                var selectButtonSound = new SoundEffect([$"{resources.AssetDirectory}/Sounds/UI/menu_056.wav"]);
                nextButtonSound.SetPitchVariation(0.8f);
                selectButtonSound.SetPitchVariation(0.8f);
                resources.Load(ButtonNextSoundResourceName, nextButtonSound);
                resources.Load(ButtonSelectSoundResourceName, selectButtonSound);

                // Star score sprite and animations
                SetupStarAnimations(32, resources);
                SetupStarAnimations(64, resources);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>(PanelResourceName);
                var buttonFont = resources.Get<FontInstance>(ButtonFontResourceName);

                var nextButtonSound = resources.Get<SoundEffect>(ButtonNextSoundResourceName);
                var selectButtonSound = resources.Get<SoundEffect>(ButtonSelectSoundResourceName);

                return new MainMenuCanvas(panels, buttonFont, nextButtonSound, selectButtonSound, info.Position);
            }

            private static void SetupStarAnimations(int gridSize, ResourceRegistry resources)
            {
                var atlas = new SpriteAtlas(gridSize, $"{resources.AssetDirectory}/Graphics/UI/Star ({gridSize}x{gridSize}).png");
                var anims = new AnimationSet();
                anims.Add(atlas, "Active", 0, 7);
                anims.Add(atlas, "Inactive", 1, 1);
                resources.Load($"UI Star {gridSize}", atlas);
                resources.Load($"UI Star {gridSize} Animations", anims);
            }
        }
    }
}