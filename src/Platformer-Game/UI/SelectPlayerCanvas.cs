using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class SelectPlayerCanvas : ButtonCanvas
    {
        SpriteAtlas _skinAtlas;
        AnimationSet _skinAnimations;
        FontInstance _skinFont;

        public SelectPlayerCanvas(SpriteAtlas panels, SpriteAtlas skins, AnimationSet skinAnims, FontInstance buttonFont, FontInstance skinFont, Vector2 position)
            : base(panels, buttonFont, position)
        {
            UpdateOnlyHovered = true;

            _skinAtlas = skins;
            _skinAnimations = skinAnims;
            _skinFont = skinFont;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            var startListPosition = new Vector2
            {
                X = (winWidth - SkinButtonSize.X * 4) / 2,
                Y = (winHeight - SkinButtonSize.Y) / 3,
            };

            AddCharacterButton(startListPosition, "Ninja Frog", 0, SelectNinjaFrog, [
                (NextElementDirection.West, "Mask Dude"),
                (NextElementDirection.East, "Pink Man"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Pink Man", 1, SelectPinkMan, [
                (NextElementDirection.West, "Ninja Frog"),
                (NextElementDirection.East, "Virtual Guy"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Virtual Guy", 2, SelectVirtualGuy, [
                (NextElementDirection.West, "Pink Man"),
                (NextElementDirection.East, "Mask Dude"),
                (NextElementDirection.South, "Back"),
            ]);
            AddCharacterButton(startListPosition, "Mask Dude", 3, SelectMaskDude, [
                (NextElementDirection.West, "Virtual Guy"),
                (NextElementDirection.East, "Ninja Frog"),
                (NextElementDirection.South, "Back"),
            ]);

            var buttonPosition = new Vector2
            {
                X = (winWidth - ButtonSize.X) / 2,
                Y = startListPosition.Y + SkinButtonSize.Y
            };
            HoveringElement = AddButtonVertical(buttonPosition, 0, "Back", BackToMainMenu, [
                (NextElementDirection.North, "Ninja Frog"),
            ]);
        }

        private Vector2 SkinButtonSpriteOffset => new Vector2(6, 6) * 16;
        private Vector2 SkinHoverButtonSpriteOffset => new Vector2(0, 6) * 16;
        private Vector2 SkinButtonSize => new Vector2(6, 6) * 16;
        private Vector2 SkinAnimatedCharPosition => new Vector2(SkinButtonSize.X / 2, SkinButtonSize.Y / 3);
        private Vector2 SkinFontOffset => new Vector2(SkinButtonSize.X / 2, SkinButtonSize.Y / 4 * 3);
        private int SkinFontSize => 8;

        private ElementGroup AddCharacterButton(Vector2 startListPosition, string name, int i, ElementGroup.OnPressCallback callback, List<(NextElementDirection, string)> next)
        {
            return AddElement(name, new ElementGroup
            {
                Position = startListPosition + (Vector2.UnitX * SkinButtonSize.X * i),
                Elements = [
                    new BasicElement(Vector2.Zero, UiPanels, SkinButtonSpriteOffset, SkinHoverButtonSpriteOffset, (int)SkinButtonSize.X, (int)SkinButtonSize.Y),
                    new AnimatedElement(SkinAnimatedCharPosition, _skinAtlas, _skinAnimations, name),
                    new TextElement(_skinFont, SkinFontOffset, name, SkinFontSize),
                ],
                Next = next,
                OnPress = callback
            });
        }

        private void BackToMainMenu()
        {
            var mainMenu = World.Find<MainMenuCanvas>().First();
            mainMenu.Showing = true;
            Showing = false;
        }

        private void SelectNinjaFrog() => SelectCharacter("Player1");
        private void SelectPinkMan() => SelectCharacter("Player2");
        private void SelectVirtualGuy() => SelectCharacter("Player3");
        private void SelectMaskDude() => SelectCharacter("Player4");

        private void SelectCharacter(string name)
        {
            var data = SaveData.Read();
            data.SelectedSkin = name;
            SaveData.Write(data);
            BackToMainMenu();
        }

        public class CreateInfo : CreateInfo<SelectPlayerCanvas>
        {
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
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

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>("UI Panels");
                var skins = resources.Get<SpriteAtlas>("UI Player Select");
                var skinAnims = resources.Get<AnimationSet>("UI Player Select Animations");
                var buttonFont = resources.Get<FontInstance>("UI Panels Button Font");
                var nameFont = resources.Get<FontInstance>("UI Panels Info Font");
                return new SelectPlayerCanvas(panels, skins, skinAnims, buttonFont, nameFont, info.Position);
            }
        }
    }
}