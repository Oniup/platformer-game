using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;
using Raylib_cs;

namespace PlatformerGame.UI
{
    public class RuntimeCanvas : Canvas
    {
        private ElementGroup _timeUI;
        private ElementGroup _scoreUI;
        private ElementGroup _hitUI;

        public RuntimeCanvas(SpriteAtlas uiPanels, FontInstance font, Vector2 position)
            : base(position)
        {
            Showing = true;

            _timeUI = CreateLabel("Time", uiPanels, font, 0);
            _scoreUI = CreateLabel("Score", uiPanels, font, 1);
            _hitUI = CreateLabel("Hit", uiPanels, font, 2);
        }

        public void SetTime(float time)
        {
            var textElement = (TextElement)_timeUI.Elements[2];
            textElement.Text = $"{time}";
        }

        public void SetScore(int score)
        {
            var textElement = (TextElement)_scoreUI.Elements[2];
            textElement.Text = $"{score}";
        }

        public void SetHit(int hit)
        {
            var textElement = (TextElement)_hitUI.Elements[2];
            textElement.Text = $"{hit}";
        }

        private ElementGroup CreateLabel(string name, SpriteAtlas uiPanels, FontInstance font, int column)
        {
            var panelOffset = new Vector2(12 * 16, 6 * 16);
            var panelSize = new Vector2(5 * 16, 2 * 16);

            int textLength = Raylib.MeasureText($"{name} ", font.Size);
            var labelOffset = new Vector2
            { 
                X = 15,
                Y = panelSize.Y / 2 - font.Size / 2,
            };
            Vector2 numOffset = labelOffset;
            numOffset.X += textLength;

            return AddElement(name, new ElementGroup
            {
                Position = Vector2.UnitX * panelSize.X * column,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, panelOffset, (int)panelSize.X, (int)panelSize.Y),
                    new TextElement(font, labelOffset, name, 1, false),
                    new TextElement(font, numOffset, "0", 1, false),
                ],
            });
        }

        public class CreateInfo : CreateInfo<RuntimeCanvas>
        {
            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>(ButtonCanvas.PanelResourceName);
                var font = resources.Get<FontInstance>(ButtonCanvas.InfoFontResourceName);
                return new RuntimeCanvas(uiPanels, font, info.Position);
            }
        }
    }
}