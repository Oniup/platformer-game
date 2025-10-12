using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformerGame.Engine.Serialization
{

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

    public class EntityFieldJsonConverter : JsonConverter<EntityField>
    {
        public override EntityField? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            string identifier = root.GetProperty("__identifier").ToString();
            string typeStr = root.GetProperty("__type").ToString();
            JsonElement value = root.GetProperty("__value");

            FieldType type = typeStr switch
            {
                "Int" => FieldType.Int,
                "Float" => FieldType.Float,
                "Bool" => FieldType.Bool,
                "String" => FieldType.String,
                "Point" => FieldType.Vector2,
                _ => throw new InvalidDataException(),
            };

            EntityField.IData data = type switch
            {
                FieldType.Int => new EntityField.Data<int>(type, value.GetInt32()),
                FieldType.Float => new EntityField.Data<float>(type, value.GetSingle()),
                FieldType.Bool => new EntityField.Data<bool>(type, value.GetBoolean()),
                FieldType.String => new EntityField.Data<string>(type, value.GetString()!),
                FieldType.Vector2 => new EntityField.Data<Vector2>(type, new Vector2(value.GetProperty("cx").GetSingle(), value.GetProperty("cy").GetSingle()) * 16),
                _ => throw new InvalidDataException(),
            };

            return new EntityField
            {
                Identifier = identifier,
                Type = type,
                Value = data,
            };
        }

        public override void Write(Utf8JsonWriter writer, EntityField value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("__identifier", value.Identifier);
            writer.WriteString("__type", value.Type.ToString());
            writer.WritePropertyName("__value");
            JsonSerializer.Serialize(writer, ((dynamic)value.Value).Value, options);
            writer.WriteEndObject();
        }
    }
}