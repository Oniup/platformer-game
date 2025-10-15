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
            // Sprite atlas offsets
            var basePanelOffset = new Vector2(0.0f, 48.0f);
            var hoveredPanelOffset = Vector2.Zero;

            int panelWidth = 304;
            int panelHeight = 48;

            int fontSize = 30;
            var fontOffset = new Vector2(panelWidth / 2, (panelHeight / 2) - fontSize / 2);

            AddElement("Level", new ElementGroup
            {
                Position = new Vector2(10, 4) * 16.0f,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, basePanelOffset, hoveredPanelOffset, panelWidth, panelHeight),
                    new TextElement(fontOffset, "Play", fontSize),
                ],
                Next = [
                    (NextElementDirection.North, "Exit"),
                    (NextElementDirection.South, "Character")
                ],
                OnPress = OpenLevelMenu
            });
            AddElement("Character", new ElementGroup
            {
                Position = new Vector2(10, 8) * 16.0f,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, basePanelOffset, hoveredPanelOffset, panelWidth, panelHeight),
                    new TextElement(fontOffset, "Select Character", fontSize),
                ],
                Next = [
                    (NextElementDirection.North, "Level"),
                    (NextElementDirection.South, "Exit")
                ],
                OnPress = OpenCharacterMenu
            });
            AddElement("Exit", new ElementGroup
            {
                Position = new Vector2(10, 13) * 16.0f,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, basePanelOffset, hoveredPanelOffset, panelWidth, panelHeight),
                    new TextElement(fontOffset, "Quit", fontSize),
                ],
                Next = [
                    (NextElementDirection.North, "Character"),
                    (NextElementDirection.South, "Level"),
                ],
                OnPress = ExitApplication
            });

            Showing = true;
        }

        private void OpenLevelMenu()
        {
            World.Load("Testing");
        }

        private void OpenCharacterMenu()
        {
            SelectPlayerCanvas selectPlayer = World.Find<SelectPlayerCanvas>().First();
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
                SpriteAtlas panels = resources.Get<SpriteAtlas>("UI Panels");
                return new MainMenuCanvas(panels, info.Position);
            }
        }
    }
}