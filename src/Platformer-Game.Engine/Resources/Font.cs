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

        public void Draw(Vector2 position, string text, int size)
        {
            Draw(position, text, size, Color.Black);
        }

        public void Draw(Vector2 position, string text, int size, Color color)
        {
            Raylib.DrawTextEx(_font, text, position, size, 1, color);
        }
    }
}