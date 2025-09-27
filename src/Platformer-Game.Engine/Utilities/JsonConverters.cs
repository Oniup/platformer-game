using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformerGame.Engine.Utilities
{
    public class PointJsonConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int x, y;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"invalid json syntax for Point, should be: [x, y], {reader.TokenType} is invalid type");

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException($"invalid json syntax for Point, should be: [x, y], {reader.TokenType} is invalid type");
            x = reader.GetInt32();

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException($"invalid json syntax for Point, should be: [x, y], {reader.TokenType} is invalid type");
            y = reader.GetInt32();

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException($"invalid json syntax for Point, should be: [x, y], {reader.TokenType} is invalid type");

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

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");
            x = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException("invalid json syntax for Vector2, should be: [x, y]");
            y = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
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