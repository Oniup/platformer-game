using System.Numerics;
using Raylib_cs;

namespace PlatformerGame.Engine.Resources
{
    public class FontInstance : Resource
    {
        private Font _font;

        public FontInstance(string sourcePath, int fontWidth)
            : base(ResourceType.Font)
        {
            _font = Raylib.LoadFontEx(sourcePath, fontWidth, null, 0);
        }

        public int Size => _font.BaseSize;

        public override void Dispose()
        {
            Raylib.UnloadFont(_font);
        }

        public float MeasureText(string text, int size = 1)
        {
            return Raylib.MeasureTextEx(_font, text, _font.BaseSize * size, 1).X;
        }

        public void Draw(Vector2 position, string text, int size = 1)
        {
            Draw(position, text, Color.Black, size);
        }

        public void Draw(Vector2 position, string text, Color color, int size = 1)
        {
            Vector2 roundedPosition = Vector2.Round(position);
            Raylib.DrawTextEx(_font, text, roundedPosition, _font.BaseSize * size, 1, color);
        }
    }
}