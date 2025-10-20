using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class PauseCanvas : Canvas
    {
        public PauseCanvas(SpriteAtlas uiPanels, FontInstance font, Vector2 position)
            : base(position)
        {
            AddElement("Restart", new ElementGroup
            {
                Position = MainMenuCanvas.PanelOffset(0),
                Elements = MainMenuCanvas.CreateElements(uiPanels, font, "Restart Level"),
                Next = [
                    (NextElementDirection.North, "Main Menu"),
                    (NextElementDirection.South, "Main Menu"),
                ],
                OnPress = RestartLevel
            });
            AddElement("Main Menu", new ElementGroup
            {
                Position = MainMenuCanvas.PanelOffset(1),
                Elements = MainMenuCanvas.CreateElements(uiPanels, font, "Main Menu"),
                Next = [
                    (NextElementDirection.North, "Restart"),
                    (NextElementDirection.South, "Restart"),
                ],
                OnPress = GoBackToMainMenu
            });
        }

        private void RestartLevel()
        {
            World.Load(World.LevelName);
        }

        private void GoBackToMainMenu()
        {
            World.Load("Main Menu");
        }

        public class CreateInfo : CreateInfo<PauseCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>("UI Panels");
                var font = resources.Get<FontInstance>("UI Panels Button Font");
                return new PauseCanvas(uiPanels, font, info.Position);
            }
        }
    }
}