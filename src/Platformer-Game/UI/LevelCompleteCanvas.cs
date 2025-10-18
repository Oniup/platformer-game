using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using Raylib_cs;

namespace PlatformerGame.UI
{
    public class LevelCompleteCanvas : Canvas
    {
        public LevelCompleteCanvas(SpriteAtlas uiPanels, SpriteAtlas stars, Vector2 position) 
            : base(position)
        {
            Showing = false;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            var spriteOffset = new Vector2(0, 12) * 16;
            var panelSize = new Vector2(19, 6) * 16;
            var displayPosition = new Vector2
            {
                X = (winWidth - panelSize.X) / 2,
                Y = (winHeight - panelSize.Y) / 4
            };
            AddElement("Score Display", new ElementGroup
            {
                Position = displayPosition,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, spriteOffset, (int)panelSize.X, (int)panelSize.Y),
                ],
            });

            var panelHoveredOffset = Vector2.Zero;
            var panelBaseOffset = new Vector2(0, 3) * 16;
            var selectablePanelSize = new Vector2(19, 3) * 16;
            HoveringElement = AddElement("Restart", new ElementGroup
            {
                Position = displayPosition + Vector2.UnitY * panelSize.Y,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, panelBaseOffset, panelHoveredOffset, (int)selectablePanelSize.X, (int)selectablePanelSize.Y),
                    new TextElement(selectablePanelSize / 2 - Vector2.UnitY * 15, "Restart", 30),
                ],
                Next = [
                    (NextElementDirection.North, "Main Menu"),
                    (NextElementDirection.South, "Main Menu"),
                ],
                OnPress = RestartLevel
            });
            AddElement("Main Menu", new ElementGroup
            {
                Position = displayPosition + Vector2.UnitY * (panelSize.Y + selectablePanelSize.Y),
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, panelBaseOffset, panelHoveredOffset, (int)selectablePanelSize.X, (int)selectablePanelSize.Y),
                    new TextElement(selectablePanelSize / 2 - Vector2.UnitY * 15, "Main Menu", 30),
                ],
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

        public class CreateInfo : CreateInfo<LevelCompleteCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>("UI Panels");
                var stars = resources.Get<SpriteAtlas>("UI Star");
                return new LevelCompleteCanvas(uiPanels, stars, info.Position);
            }
        }
    }
}