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

        public override void Dispose()
        {
            Raylib.UnloadFont(_font);
        }

        public float MeasureText(string text, int size)
        {
            return Raylib.MeasureTextEx(_font, text, size, 1).X;
        }

        public void Draw(Vector2 position, string text, int size)
        {
            Draw(position, text, size, Color.Black);
        }

        public void Draw(Vector2 position, string text, int size, Color color)
        {
            Vector2 roundedPosition = Vector2.Round(position);
            Raylib.DrawTextEx(_font, text, roundedPosition, size, 1, color);
        }
    }
}