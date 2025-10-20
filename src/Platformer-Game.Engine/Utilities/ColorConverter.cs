using System.Globalization;
using Raylib_cs;

namespace PlatformerGame.Engine.Utilities
{
    public static class ColorConverter
    {
        public static Color Convert(string rgbSource)
        {
            int offset = 0;
            if (rgbSource.StartsWith('#'))
                offset++;
            int sourceLength = rgbSource.Length - offset;

            if (string.IsNullOrWhiteSpace(rgbSource) || sourceLength < 6)
                throw new FormatException($"Invalid rgb color format: {rgbSource}, needs to have at least 6 digits");

            byte r, g, b, a = 255;
            bool successR = byte.TryParse(rgbSource.AsSpan(offset + 0, 2), NumberStyles.HexNumber, null, out r);
            bool successG = byte.TryParse(rgbSource.AsSpan(offset + 2, 2), NumberStyles.HexNumber, null, out g);
            bool successB = byte.TryParse(rgbSource.AsSpan(offset + 4, 2), NumberStyles.HexNumber, null, out b);

            bool successA = true;
            if (sourceLength == 8)
                successA = byte.TryParse(rgbSource.AsSpan(offset + 6, 2), NumberStyles.HexNumber, null, out a);

            if (!successR || !successG || !successB || !successA)
                throw new FormatException($"Invalid rgb color format: {rgbSource}, failed to parse");

            return new Color(r, g, b, a);
        }
    }
}