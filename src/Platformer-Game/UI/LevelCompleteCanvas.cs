using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.UI
{
    public class LevelCompleteCanvas : Canvas
    {
        private ElementGroup _display;

        public LevelCompleteCanvas(SpriteAtlas uiPanels, SpriteAtlas stars, AnimationSet anims, FontInstance font, Vector2 position) 
            : base(position)
        {
            Showing = false;
            UpdateOnlyHovered = false;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);

            int fontSize = 15;
            var spriteOffset = new Vector2(0, 12) * 16;
            var panelSize = new Vector2(19, 6) * 16;
            var displayPosition = new Vector2
            {
                X = (winWidth - panelSize.X) / 2,
                Y = (winHeight - panelSize.Y) / 4
            };
            _display = AddElement("Score Display", new ElementGroup
            {
                Position = displayPosition,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, spriteOffset, (int)panelSize.X, (int)panelSize.Y),

                    // Display
                    new TextElement(font, new Vector2(panelSize.X / 2, panelSize.Y / 3), "Time: 123.00", fontSize),
                    new TextElement(font, new Vector2(panelSize.X / 4 * 1, panelSize.Y / 3 * 2), "Score: 123", fontSize),
                    new TextElement(font, new Vector2(panelSize.X / 4 * 3, panelSize.Y / 3 * 2), "Hits: 10", fontSize),

                    // Star score
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 1, 0), stars, anims, "Active"),
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 2, 0), stars, anims, "Active"),
                    new AnimatedElement(new Vector2(panelSize.X / 4 * 3, 0), stars, anims, "Inactive"),
                ],
            });

            fontSize = 30;
            var panelHoveredOffset = Vector2.Zero;
            var panelBaseOffset = new Vector2(0, 3) * 16;
            var selectablePanelSize = new Vector2(19, 3) * 16;
            Vector2 fontOffset = selectablePanelSize / 2 - Vector2.UnitY * (fontSize / 2);
            HoveringElement = AddElement("Restart", new ElementGroup
            {
                Position = displayPosition + Vector2.UnitY * panelSize.Y,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, panelBaseOffset, panelHoveredOffset, (int)selectablePanelSize.X, (int)selectablePanelSize.Y),
                    new TextElement(font, fontOffset, "Restart", fontSize),
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
                    new TextElement(font, fontOffset, "Main Menu", fontSize),
                ],
                Next = [
                    (NextElementDirection.North, "Restart"),
                    (NextElementDirection.South, "Restart"),
                ],
                OnPress = GoBackToMainMenu
            });
        }

        public void SetTime(float time)
        {
            var textElement = (TextElement)_display.Elements[1];
            textElement.Text = $"Time: {MathF.Round(time, 2)}";
        }

        public void SetScore(int score)
        {
            var textElement = (TextElement)_display.Elements[2];
            textElement.Text = $"Score: {score}";
        }

        public void SetHit(int hit)
        {
            var textElement = (TextElement)_display.Elements[3];
            textElement.Text = $"Hits: {hit}";
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
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                var atlas = new SpriteAtlas(64, resources.AssetDirectory + "/Graphics/UI/Star.png");
                var anims = new AnimationSet();
                anims.Add(atlas, "Active", 0, 7);
                anims.Add(atlas, "Inactive", 1, 1);

                resources.Load("UI Star", atlas);
                resources.Load("UI Star Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>("UI Panels");
                var stars = resources.Get<SpriteAtlas>("UI Star");
                var anims = resources.Get<AnimationSet>("UI Star Animations");
                var buttonFont = resources.Get<FontInstance>("UI Panels Button Font");
                return new LevelCompleteCanvas(uiPanels, stars, anims, buttonFont, info.Position);
            }
        }
    }
}