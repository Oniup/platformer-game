using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class MainMenuCanvas : Canvas
    {
        public MainMenuCanvas(SpriteAtlas uiPanels, Vector2 position)
            : base(position)
        {
            AddElement("Level", new ElementGroup
            {
                Position = PanelOffset(0),
                Elements = CreateElements(uiPanels, "Play"),
                Next = [
                    (NextElementDirection.North, "Exit"),
                    (NextElementDirection.South, "Character")
                ],
                OnPress = OpenLevelMenu
            });
            AddElement("Character", new ElementGroup
            {
                Position = PanelOffset(1),
                Elements = CreateElements(uiPanels, "Select Character"),
                Next = [
                    (NextElementDirection.North, "Level"),
                    (NextElementDirection.South, "Exit")
                ],
                OnPress = OpenCharacterMenu
            });
            AddElement("Exit", new ElementGroup
            {
                Position = PanelOffset(2),
                Elements = CreateElements(uiPanels, "Quit"),
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

        public static List<Element> CreateElements(SpriteAtlas uiPanels, string text)
        {
            var basePanelOffset = new Vector2(0.0f, 48.0f);
            var hoveredPanelOffset = Vector2.Zero;

            int panelWidth = 304;
            int panelHeight = 48;

            int fontSize = 30;
            var fontOffset = new Vector2(panelWidth / 2, (panelHeight / 2) - fontSize / 2);

            return [
                new BasicElement(Vector2.Zero, uiPanels, basePanelOffset, hoveredPanelOffset, panelWidth, panelHeight),
                new TextElement(fontOffset, text, fontSize),
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
            Console.WriteLine("Exit application");
        }

        public class CreateInfo : CreateInfo<MainMenuCanvas>
        {
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                // Load all required menu UI
                resources.Load("UI Panels", new SpriteAtlas(0, resources.AssetDirectory + "/Graphics/UI/Panels.png"));
                (string, Resource)[] atlases = [
                    ("UI Star", new SpriteAtlas(32, resources.AssetDirectory + "/Graphics/UI/Star.png")),
                ];
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>("UI Panels");
                return new MainMenuCanvas(panels, info.Position);
            }
        }
    }
}