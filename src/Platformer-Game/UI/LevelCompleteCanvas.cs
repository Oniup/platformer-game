using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class LevelCompleteCanvas : ButtonCanvas
    {
        private ElementGroup _display;

        public LevelCompleteCanvas(SpriteAtlas uiPanels, SpriteAtlas stars, AnimationSet anims, FontInstance buttonFont, SoundEffect nextButtonSound, SoundEffect selectButtonSound, Vector2 position)
            : base(uiPanels, buttonFont, nextButtonSound, selectButtonSound, position)
        {
            Showing = false;
            UpdateOnlyHovered = false;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);

            var spriteOffset = new Vector2(0, 12) * 16;
            var panelSize = new Vector2(19, 6) * 16;
            Vector2 displayPosition = Center;
            displayPosition.Y /= 2;
            _display = AddElement("Score Display", new ElementGroup
            {
                Position = displayPosition,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, spriteOffset, (int)panelSize.X, (int)panelSize.Y),

                    // Display
                    new TextElement(buttonFont, new Vector2(panelSize.X / 2, panelSize.Y / 3), "Time: 123.00"),
                    new TextElement(buttonFont, new Vector2(panelSize.X / 4 * 1, panelSize.Y / 3 * 2), "Score: 123"),
                    new TextElement(buttonFont, new Vector2(panelSize.X / 4 * 3, panelSize.Y / 3 * 2), "Hits: 10"),

                    // Star score
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 1, 0), stars, anims, "Inactive"),
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 2, 0), stars, anims, "Inactive"),
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 3, 0), stars, anims, "Inactive"),
                ],
            });

            Vector2 buttonStartPosition = displayPosition + Vector2.UnitY * panelSize.Y;
            HoveringElement = AddButtonVertical(buttonStartPosition, 0, "Restart", RestartLevel, [
                (NextElementDirection.North, "Main Menu"),
                (NextElementDirection.South, "Main Menu"),
            ]);
            AddButtonVertical(buttonStartPosition, 1, "Main Menu", GoBackToMainMenu, [
                (NextElementDirection.North, "Restart"),
                (NextElementDirection.South, "Restart"),
            ]);
        }

        public void RegisterRun(int score, float time, int hit, float scoreRatio)
        {
            var timeElement = (TextElement)_display.Elements[1];
            var scoreElement = (TextElement)_display.Elements[2];
            var hitElement = (TextElement)_display.Elements[3];

            timeElement.Text = $"Time: {MathF.Round(time, 2)}";
            scoreElement.Text = $"Score: {score}";
            hitElement.Text = $"Hits: {hit}";

            if (scoreRatio == SaveData.ScoreRatio3Star)
                ((AnimatedElement)_display.Elements[6]).PlayAnimation("Active");
            if (scoreRatio >= SaveData.ScoreRatio2Star)
                ((AnimatedElement)_display.Elements[5]).PlayAnimation("Active");
            if (scoreRatio >= SaveData.ScoreRatio1Star)
                ((AnimatedElement)_display.Elements[4]).PlayAnimation("Active");
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
            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>(PanelResourceName);
                var buttonFont = resources.Get<FontInstance>(ButtonFontResourceName);
                var buttonSound = resources.Get<SoundEffect>(ButtonNextSoundResourceName);
                var selectButtonSound = resources.Get<SoundEffect>(ButtonSelectSoundResourceName);

                var stars = resources.Get<SpriteAtlas>("UI Star 64");
                var anims = resources.Get<AnimationSet>("UI Star 64 Animations");

                return new LevelCompleteCanvas(uiPanels, stars, anims, buttonFont, buttonSound, selectButtonSound, info.Position);
            }
        }
    }
}
