using System.Numerics;
using System.Text.Json.Serialization;

namespace PlatformerGame.Engine.Serialization
{
    public class LDtkIdentifier
    {
        public required string Identifier { get; init; }
        public required int UId { get; init; }
    }

    public class LDtkDefinition
    {
        public required List<Entity> Entities { get; init; }
        public required List<Tileset> Tilesets { get; init; }
        public required List<Layer> Layers { get; init; }
        public required List<string> Enums { get; init; }

        public class Entity : LDtkIdentifier
        {
            public required List<string> Tags { get; init; }
            public required string RenderMode { get; init; }
            public int? TilesetId { get; init; }
            public required int Width { get; init; }
            public required int Height { get; init; }
            public required float PivotX { get; init; }
            public required float PivotY { get; init; }
        }

        public class Tileset : LDtkIdentifier
        {
            public required string RelPath { get; init; }
            public required List<string> EnumTags { get; init; }
            [JsonPropertyName("pxWid")]
            public required int PxWidth { get; init; }
            [JsonPropertyName("pxHei")]
            public required int PxHeight { get; init; }
            public required int TileGridSize { get; init; }
        }

        public class Layer : LDtkIdentifier
        {
            public int? TilesetDefUId { get; init; }
        }
    }

    public class LDtkLevelInfo : LDtkIdentifier
    {
        public struct Neighbour
        {
            public required string LevelIId { get; init; }
            public required string Dir { get; init; }
        }

        public required string IId { get; init; }
        public required string ExternalRelPath { get; init; }
        public int WorldX { get; init; }
        public int WorldY { get; init; }
        [JsonPropertyName("PxWid")]
        public int Width { get; init; }
        [JsonPropertyName("PxHei")]
        public int Height { get; init; }
        [JsonPropertyName("__neighbours")]
        public required List<Neighbour> Neighbours { get; init; }
    }

    public class LDtkHeader
    {
        public required int DefaultEntityWidth { get; init; }
        public required int DefaultEntityHeight { get; init; }
        public required string BgColor { get; init; }
        public required string DefaultLevelBgColor { get; init; }
        public required LDtkDefinition Defs { get; init; }
        public required List<LDtkLevelInfo> Levels { get; init; }
    }

    public class LDtkLevel : LDtkIdentifier
    {
        public class Layer
        {
            public required int LayerDefUId { get; init; }
            public required int PxOffsetX { get; init; }
            public required int PxOffsetY { get; init; }
            public required List<Entity> EntityInstances { get; init; }
            public required List<int> IntGridCsv { get; init; }
            public required List<Tile> AutoLayerTiles { get; init; }
        }

        public struct Entity
        {
            public required int DefUId { get; init; }
            [JsonPropertyName("px")]
            public required Vector2 Position { get; init; }
            public required List<EntityField> FieldInstances { get; init; }
        }

        public struct Tile
        {
            [JsonPropertyName("px")]
            public required Vector2 ScenePosition { get; init; }
            [JsonPropertyName("src")]
            public required Vector2 AtlasPosition { get; init; }
        }

        public required string IId { get; init; }
        public required int WorldX { get; init; }
        public required int WorldY { get; init; }
        [JsonPropertyName("pxWid")]
        public required int PxWidth { get; init; }
        [JsonPropertyName("pxHei")]
        public required int PxHeight { get; init; }
        public required string? BgColor { get; init; }
        public required List<Layer> LayerInstances { get; init; }
    }

    public enum FieldType
    {
        None,
        Int,
        Float,
        Bool,
        String,
        Vector2,
    }

    public class EntityField
    {
        public interface IData
        {
            public FieldType Type { get; init; }
        }

        public class Data<T> : IData
        {
            public FieldType Type { get; init; }
            public T Value { get; init; }

            public Data(FieldType type, T value)
            {
                Type = type;
                Value = value;
            }
        }

        public required string Identifier;
        public required FieldType Type;
        public required IData Value;
    }
}