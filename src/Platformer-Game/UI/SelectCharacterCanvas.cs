using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class SelectCharacterCanvas : ButtonCanvas
    {
        private readonly SpriteAtlas _skinAtlas;
        private readonly AnimationSet _skinAnimations;
        private readonly FontInstance _infoFont;

        public SelectCharacterCanvas(SpriteAtlas panels, SpriteAtlas skins, AnimationSet skinAnims, FontInstance buttonFont, FontInstance skinFont, SoundEffect buttonSound, Vector2 position)
            : base(panels, buttonFont, buttonSound, position)
        {
            UpdateOnlyHovered = true;

            _skinAtlas = skins;
            _skinAnimations = skinAnims;
            _infoFont = skinFont;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            var startListPosition = new Vector2
            {
                X = (winWidth - SqButtonSize.X * 4) / 2,
                Y = (winHeight - SqButtonSize.Y) / 3,
            };

            AddCharacterButton(startListPosition, "Ninja Frog", 0, [
                (NextElementDirection.West, "Mask Dude"),
                (NextElementDirection.East, "Pink Man"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Pink Man", 1, [
                (NextElementDirection.West, "Ninja Frog"),
                (NextElementDirection.East, "Virtual Guy"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Virtual Guy", 2, [
                (NextElementDirection.West, "Pink Man"),
                (NextElementDirection.East, "Mask Dude"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Mask Dude", 3, [
                (NextElementDirection.West, "Virtual Guy"),
                (NextElementDirection.East, "Ninja Frog"),
                (NextElementDirection.South, "Back"),
            ]);

            var buttonPosition = new Vector2
            {
                X = (winWidth - ButtonSize.X) / 2,
                Y = startListPosition.Y + SqButtonSize.Y
            };
            HoveringElement = AddButtonVertical(buttonPosition, 0, "Back", BackToMainMenu, [
                (NextElementDirection.North, "Ninja Frog"),
            ]);
        }

        private static Vector2 SkinAnimatedCharPosition => new Vector2(SqButtonSize.X / 2, SqButtonSize.Y / 3);
        private static Vector2 SkinFontOffset => new Vector2(SqButtonSize.X / 2, SqButtonSize.Y / 4 * 3);

        private ElementGroup AddCharacterButton(Vector2 startListPosition, string name, int i, List<(NextElementDirection, string)> next)
        {
            return AddElement(name, new ElementGroup
            {
                Position = startListPosition + (Vector2.UnitX * SqButtonSize.X * i),
                Elements = [
                    new BasicElement(Vector2.Zero, UiPanels, SqButtonBaseSpriteOffset, SqButtonHoverSpriteOffset, (int)SqButtonSize.X, (int)SqButtonSize.Y),
                    new AnimatedElement(SkinAnimatedCharPosition, _skinAtlas, _skinAnimations, name),
                    new TextElement(_infoFont, SkinFontOffset, name, InfoFontSize),
                ],
                Next = next,
                OnPress = () =>
                {
                    var data = SaveData.Read();
                    data.SelectedSkin = name;
                    SaveData.Write(data);
                    BackToMainMenu();
                }
            });
        }

        private void BackToMainMenu()
        {
            var mainMenu = World.Find<MainMenuCanvas>().First();
            mainMenu.Showing = true;
            Showing = false;
        }

        public class CreateInfo : CreateInfo<SelectCharacterCanvas>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var skins = new SpriteAtlas(64, resources.AssetDirectory + "/Graphics/UI/PlayerSelect (64x64).png");
                var anims = new AnimationSet();

                anims.Add(skins, "Ninja Frog", 0, 11);
                anims.Add(skins, "Pink Man", 1, 11);
                anims.Add(skins, "Virtual Guy", 2, 11);
                anims.Add(skins, "Mask Dude", 3, 11);

                resources.Load("UI Player Select Animations", anims);
                resources.Load("UI Player Select", skins);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>(PanelResourceName);
                var skins = resources.Get<SpriteAtlas>("UI Player Select");
                var skinAnims = resources.Get<AnimationSet>("UI Player Select Animations");
                var buttonFont = resources.Get<FontInstance>(ButtonFontResourceName);
                var infoFont = resources.Get<FontInstance>(InfoFontResourceName);
                var buttonSound = resources.Get<SoundEffect>(ButtonSoundResourceName);
                return new SelectCharacterCanvas(panels, skins, skinAnims, buttonFont, infoFont, buttonSound, info.Position);
            }
        }
    }
}