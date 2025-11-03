using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class PauseCanvas : ButtonCanvas
    {
        public PauseCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, SoundEffect buttonSound, SoundEffect selectButtonSound, Vector2 position)
            : base(uiPanels, buttonFont, buttonSound, selectButtonSound, position)
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

            EventDispatcher.AddListener<WindowMovePositionEvent>(this, OnWindowMoveEvent);
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<WindowMovePositionEvent>(this);
        }

        private void OnWindowMoveEvent(Event eventData, object sender)
        {
            Showing = true;
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
            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>(PanelResourceName);
                var font = resources.Get<FontInstance>(ButtonFontResourceName);
                var buttonSound = resources.Get<SoundEffect>(ButtonNextSoundResourceName);
                var selectButtonSound = resources.Get<SoundEffect>(ButtonSelectSoundResourceName);
                return new PauseCanvas(uiPanels, font, buttonSound, selectButtonSound, info.Position);
            }
        }
    }
}