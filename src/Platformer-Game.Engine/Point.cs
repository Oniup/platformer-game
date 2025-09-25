/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-25-2025
/// </summary>

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformerGame.Engine
{
    public struct Point : IEquatable<Point>, IFormattable
    {
        public int X;
        public int Y;

        public Point()
        {
            X = 0;
            Y = 0;
        }

        public Point(int val)
        {
            X = val;
            Y = val;
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(Vector2 vec)
        {
            X = (int)vec.X;
            Y = (int)vec.Y;
        }

        public static explicit operator Vector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static explicit operator Point(Vector2 vec)
        {
            return new Point((int)vec.X, (int)vec.Y);
        }

        public static Point operator +(Point pt, Point pt2) => Add(pt, pt2);
        public static Point operator -(Point pt, Point pt2) => Subtract(pt, pt2);
        public static bool operator ==(Point pt, Point pt2) => pt.X == pt2.X && pt.Y == pt2.Y;
        public static bool operator !=(Point pt, Point pt2) => !(pt == pt2);

        public static Point operator +(Point pt, Vector2 vec) => Add(pt, vec);
        public static Point operator -(Point pt, Vector2 vec) => Subtract(pt, vec);
        public static bool operator ==(Point pt, Vector2 vec) => pt.X == vec.X && pt.Y == vec.Y;
        public static bool operator !=(Point pt, Vector2 vec) => !(pt == vec);

        public static Point Add(Point pt, Point pt2)
        {
            return new Point(pt.X + pt2.X, pt.Y + pt2.Y);
        }

        public static Point Subtract(Point pt, Point pt2)
        {
            return new Point(pt.X + pt2.X, pt.Y + pt2.Y);
        }

        public static Point Add(Point pt, Vector2 vec)
        {
            return new Point(pt.X + (int)vec.X, pt.Y + (int)vec.Y);
        }

        public static Point Subtract(Point pt, Vector2 vec)
        {
            return new Point(pt.X + (int)vec.X, pt.Y + (int)vec.Y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Point && Equals((Point)obj);
        }

        public readonly bool Equals(Point other)
        {
            return this == other;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}>";
        }
    }

    public class PointJsonConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int x, y;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("invalid json syntax for Point, should be: [x, y]");

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Point, should be: [x, y]");
            x = reader.GetInt32();

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Point, should be: [x, y]");
            y = reader.GetInt32();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("invalid json syntax for Point, should be: [x, y]");

            return new Point(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Point point, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(point.X);
            writer.WriteNumberValue(point.Y);
            writer.WriteEndArray();
        }
    }

    public class VectorJsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float x, y;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");
            x = reader.GetSingle();

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");
            y = reader.GetSingle();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 point, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(point.X);
            writer.WriteNumberValue(point.Y);
            writer.WriteEndArray();
        }
    }
}