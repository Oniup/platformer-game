/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-25-2025
/// </summary>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformerGame.Engine
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
            public required int? TilesetDefUId { get; init; }
        }
    }

    public class LDtkLevelInfo : LDtkIdentifier
    {
        public class Neighbour
        {
            public required string LevelIId { get; init; }
            public required char Dir { get; init; }
        }

        public required string IId { get; init; }
        public required string ExternalRelPath { get; init; }
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
            public required List<Tile> AutoLayerTiles { get; init; }
        }

        public class Entity
        {
            public required int DefUId { get; init; }
            [JsonPropertyName("px")]
            public required Point Position { get; init; }
        }

        public class Tile
        {
            [JsonPropertyName("px")]
            public required Point ScenePosition { get; init; }
            [JsonPropertyName("src")]
            public required Point AtlasPosition { get; init; }
        }

        public required string IId { get; init; }
        public required int WorldX { get; init; }
        public required int WorldY { get; init; }
        public required int PxWidth { get; init; }
        public required int PxHeight { get; init; }
        public required string? BgColor { get; init; }
        public required List<Layer> Layers { get; init; }
    }

    public class Project
    {
        public string RootDirectory { get; init; }
        public LDtkHeader Header { get; init; }

        public Project(string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
                throw new NullReferenceException("Cannot find LDtk project file at \"" + path + "\"");

            string source = string.Join(Environment.NewLine, File.ReadAllLines(path));
            Header = JsonSerializer.Deserialize<LDtkHeader>(source, RequiredJsonOptions)?? throw new NullReferenceException("Failed to load LDtk Project");
            RootDirectory = file.Directory!.FullName + "/";
        }

        private JsonSerializerOptions RequiredJsonOptions
        {
            get
            {
                JsonSerializerOptions opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new PointJsonConverter() }
                };
                return opts;
            }
        }

        public LDtkDefinition.Entity? GetEntityDefinition(int uid)
        {
            foreach (LDtkDefinition.Entity ent in Header.Defs.Entities)
            {
                if (ent.UId == uid)
                    return ent;
            }
            return null;
        }

        public LDtkDefinition.Tileset? GetTilesetDefinition(int uid)
        {
            foreach (LDtkDefinition.Tileset set in Header.Defs.Tilesets)
            {
                if (set.UId == uid)
                    return set;
            }
            return null;
        }

        public LDtkDefinition.Layer? GetLayerDefinition(int uid)
        {
            foreach (LDtkDefinition.Layer layer in Header.Defs.Layers)
            {
                if (layer.UId == uid)
                    return layer;
            }
            return null;
        }

        /// <summary>
        /// Reads the data for the specififed level info and all the neighbouring levels
        /// </summary>
        /// <param name="levelInfo">Starting level to load and all the neighbours and their neighbours</param>
        /// <returns>
        /// A List of Tuples defining the level data and their neighbouring levels in which is also defined within the
        /// list
        /// </returns>
        public List<(LDtkLevel, LDtkLevelInfo)> GetLevelData(LDtkLevelInfo levelInfo)
        {
            List<(LDtkLevel, LDtkLevelInfo)> levels = new();

            // Read level info
            // Iterate through neighbouring level Ids
            //      Check if level has already been loaded
            //          If doesn't exist Read LDtkLevel json

            return levels;
        }
    }
}