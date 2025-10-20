using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class MainMenuCanvas : Canvas
    {
        public MainMenuCanvas(SpriteAtlas uiPanels, FontInstance font, Vector2 position)
            : base(position)
        {
            AddElement("Level", new ElementGroup
            {
                Position = PanelOffset(0),
                Elements = CreateElements(uiPanels, font, "Play"),
                Next = [
                    (NextElementDirection.North, "Exit"),
                    (NextElementDirection.South, "Character")
                ],
                OnPress = OpenLevelMenu
            });
            AddElement("Character", new ElementGroup
            {
                Position = PanelOffset(1),
                Elements = CreateElements(uiPanels, font, "Select Character"),
                Next = [
                    (NextElementDirection.North, "Level"),
                    (NextElementDirection.South, "Exit")
                ],
                OnPress = OpenCharacterMenu
            });
            AddElement("Exit", new ElementGroup
            {
                Position = PanelOffset(2),
                Elements = CreateElements(uiPanels, font, "Quit"),
                Next = [
                    (NextElementDirection.North, "Character"),
                    (NextElementDirection.South, "Level"),
                ],
                OnPress = ExitApplication
            });

            Showing = true;
        }

        public static Vector2 PanelOffset(int i)
        {
            return new Vector2(10, 4 + i * 4) * 16.0f;
        }

        public static List<Element> CreateElements(SpriteAtlas uiPanels, FontInstance font, string text)
        {
            var basePanelOffset = new Vector2(0.0f, 48.0f);
            var hoveredPanelOffset = Vector2.Zero;

            int fontSize = 30;

            var panelSize = new Vector2(19, 3) * 16;
            Vector2 fontOffset = panelSize / 2 - Vector2.UnitY * (fontSize / 2);

            return [
                new BasicElement(Vector2.Zero, uiPanels, basePanelOffset, hoveredPanelOffset, (int)panelSize.X, (int)panelSize.Y),
                new TextElement(font, fontOffset, text, fontSize),
            ];
        }

        private void OpenLevelMenu()
        {
            World.Load("Testing");
        }

        private void OpenCharacterMenu()
        {
            var selectPlayer = World.Find<SelectPlayerCanvas>().First();
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
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>("UI Panels");
                var panelFont = resources.Get<FontInstance>("UI Panels Button Font");
                return new MainMenuCanvas(panels, panelFont, info.Position);
            }
        }
    }
}