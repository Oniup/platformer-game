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

        public RuntimeCanvas(SpriteAtlas uiPanels, Vector2 position)
            : base(position)
        {
            Showing = true;

            _timeUI = CreateLabel("Time", uiPanels, 0, 2, 6);
            _scoreUI = CreateLabel("Score", uiPanels, 1, 4, 2);
            _hitUI = CreateLabel("Hit", uiPanels, 2, 2, 3);
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

        private ElementGroup CreateLabel(string name, SpriteAtlas uiPanels, int column, int labelLengthDivide = 2, int numLengthDivide = 4)
        {
            var panelOffset = new Vector2(12 * 16, 6 * 16);
            var panelSize = new Vector2(5 * 16, 2 * 16);

            int size = 10;
            int textLength = Raylib.MeasureText($"{name} ", size);
            Vector2 labelOffset = new Vector2
            { 
                X = panelSize.X / 2 - textLength / labelLengthDivide,
                Y = panelSize.Y / 2 - size / 2
            };
            Vector2 numOffset = new Vector2
            {
                X = panelSize.X / 2 + textLength / numLengthDivide,
                Y = panelSize.Y / 2 - size / 2
            };

            return AddElement(name, new ElementGroup
            {
                Position = Vector2.UnitX * panelSize.X * column,
                Elements = [
                    new BasicElement(Vector2.Zero, uiPanels, panelOffset, (int)panelSize.X, (int)panelSize.Y),
                    new TextElement(labelOffset, name, size),
                    new TextElement(numOffset, "0", size, false),
                ],
            });
        }

        public class CreateInfo : CreateInfo<RuntimeCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var uiPanels = resources.Get<SpriteAtlas>("UI Panels");
                return new RuntimeCanvas(uiPanels, info.Position);
            }
        }
    }
}