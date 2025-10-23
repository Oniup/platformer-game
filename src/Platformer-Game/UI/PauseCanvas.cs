using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class PauseCanvas : ButtonCanvas
    {
        public PauseCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, Vector2 position)
            : base(uiPanels, buttonFont, position)
        {
            Vector2 startListPosition = Center;
            startListPosition.Y -= ButtonSize.Y / 2;

            HoveringElement = AddButtonVertical(startListPosition, 0, "Restart", RestartLevel, [
                (NextElementDirection.North, "Main Menu"),
                (NextElementDirection.South, "Main Menu"),
            ]);
            AddButtonVertical(startListPosition, 1, "Main Menu", GoBackToMainMenu, [
                (NextElementDirection.North, "Restart"),
                (NextElementDirection.South, "Restart"),
            ]);
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