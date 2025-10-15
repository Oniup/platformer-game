using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class PauseCanvas : Canvas
    {
        public PauseCanvas(SpriteAtlas uiPanels, Vector2 position)
            : base(position)
        {
            AddElement("Main Menu", new ElementGroup
            {
                Position = MainMenuCanvas.PanelOffset(0),
                Elements = MainMenuCanvas.CreateElements(uiPanels, "Main Menu"),
                Next = [],
                OnPress = GoBackToMainMenu
            });
        }

        private void GoBackToMainMenu()
        {
            World.Load("Main Menu");
        }

        public class CreateInfo : CreateInfo<PauseCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                SpriteAtlas uiPanels = resources.Get<SpriteAtlas>("UI Panels");
                return new PauseCanvas(uiPanels, info.Position);
            }
        }
    }
}