using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class SelectPlayerCanvas : Canvas
    {
        public SelectPlayerCanvas(SpriteAtlas panels, SpriteAtlas skins, AnimationSet skinAnims, Vector2 position)
            : base(position)
        {
            AddBackPanel(panels);
            AddCharacterGroup("Ninja Frog", 0, "Mask Dude", "Pink Man", panels, skins, skinAnims, SelectNinjaFrog);
            AddCharacterGroup("Pink Man", 1, "Ninja Frog", "Virtual Guy", panels, skins, skinAnims, SelectPinkMan);
            AddCharacterGroup("Virtual Guy", 2, "Pink Man", "Mask Dude", panels, skins, skinAnims, SelectVirtualGuy);
            AddCharacterGroup("Mask Dude", 3, "Virtual Guy", "Ninja Frog", panels, skins, skinAnims, SelectMaskDude);
        }

        private void BackToMainMenu()
        {
            MainMenuCanvas mainMenu = World.Find<MainMenuCanvas>().First();
            mainMenu.Showing = true;
            Showing = false;
        }

        private void SelectNinjaFrog()
        {
            SaveData data = SaveData.Read();
            data.SelectedSkin = "Player1";
            SaveData.Write(data);
            BackToMainMenu();
        }

        private void SelectPinkMan() => SelectCharacter("Player2");
        private void SelectVirtualGuy() => SelectCharacter("Player3");
        private void SelectMaskDude() => SelectCharacter("Player4");

        private void SelectCharacter(string name)
        {
            SaveData data = SaveData.Read();
            data.SelectedSkin = name;
            SaveData.Write(data);
            BackToMainMenu();
        }

        private void AddBackPanel(SpriteAtlas panels)
        {
            var basePanelOffset = new Vector2(0.0f, 48.0f);
            var hoveredPanelOffset = Vector2.Zero;
            int panelWidth = 304;
            int panelHeight = 48;
            int fontSize = 30;
            var baseFontOffset = new Vector2(panelWidth / 2, (panelHeight / 2) - fontSize / 2);

            AddElement("BackToMainMenu", new ElementGroup
            {
                Position = new Vector2(10, 14) * 16,
                Elements = [
                    new BasicElement(Vector2.Zero, panels, basePanelOffset, hoveredPanelOffset, panelWidth, panelHeight),
                    new TextElement(baseFontOffset, "Back", fontSize),
                ],
                Next = [
                    (NextElementDirection.North, "Ninja Frog"),
                ],
                OnPress = BackToMainMenu
            });
        }

        private void AddCharacterGroup(string name, int i, string? prev, string? next, SpriteAtlas panels, SpriteAtlas skins, AnimationSet skinAnims, ElementGroup.OnPressCallback onPress)
        {
            var baseOffset = new Vector2(6, 6) * 16;
            var hoveredOffset = new Vector2(0.0f, 6) * 16;
            int panelSize = 6 * 16;

            int fontSize = 10;
            var fontOffset = new Vector2(panelSize / 2, (panelSize - panelSize / 4) - fontSize / 2);

            List<(NextElementDirection, string)> nextHover = new List<(NextElementDirection, string)>();
            if (prev != null)
                nextHover.Add((NextElementDirection.West, prev));
            if (next != null)
                nextHover.Add((NextElementDirection.East, next));
            nextHover.Add((NextElementDirection.South, "BackToMainMenu"));

            AddElement(name, new ElementGroup
            {
                Position = new Vector2(5 + (i * 8), 7) * 16,
                Elements = [
                    new BasicElement(Vector2.Zero, panels, baseOffset, hoveredOffset, panelSize, panelSize),
                    new AnimatedElement(new Vector2(panelSize / 2, panelSize / 3), skins, skinAnims, name),
                    new TextElement(fontOffset, name, fontSize),
                ],
                Next = nextHover,
                OnPress = onPress
            });
        }

        public class CreateInfo : CreateInfo<SelectPlayerCanvas>
        {
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                SpriteAtlas skins = new SpriteAtlas(64, resources.AssetDirectory + "/Graphics/UI/PlayerSelect (64x64).png");
                AnimationSet anims = new AnimationSet();

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
                return new SelectPlayerCanvas(panels, skins, skinAnims, info.Position);
            }
        }
    }
}