using System.Text.Json;

namespace PlatformerGame.Engine.Serialization
{
    public class EntityFields
    {
        private List<EntityField> _fields;

        public EntityFields(List<EntityField> fields)
        {
            _fields = fields;
        }

        public T GetValue<T>(string identifier)
        {
            foreach (EntityField field in _fields)
            {
                if (field.Identifier == identifier)
                {
                    var data = field.Value as EntityField.Data<T> ?? throw new InvalidCastException($"{identifier} is not of type {typeof(T).Name}");
                    return data.Value;
                }
            }
            throw new NullReferenceException($"Could not find entity field identifier {identifier}");
        }
    }

    public class Project
    {
        public string RootDirectory { get; init; }
        public LDtkHeader Header { get; init; }

        public Project(string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
                throw new FileNotFoundException("Cannot find LDtk project file at \"" + path + "\"");

            string source = string.Join(Environment.NewLine, File.ReadAllLines(path));
            Header = JsonSerializer.Deserialize<LDtkHeader>(source, RequiredJsonOptions) ?? throw new JsonException("Failed to load LDtk Project");
            RootDirectory = file.Directory!.FullName + "/";
        }

        private JsonSerializerOptions RequiredJsonOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = {
                        new VectorJsonConverter(),
                        new EntityFieldJsonConverter()
                    }
                };
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

        public LDtkDefinition.Entity? GetEntityDefinition(string identifier)
        {
            foreach (LDtkDefinition.Entity ent in Header.Defs.Entities)
            {
                if (ent.Identifier == identifier)
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

        public LDtkDefinition.Layer? GetLayerDefinition(string identifier)
        {
            foreach (LDtkDefinition.Layer layer in Header.Defs.Layers)
            {
                if (layer.Identifier == identifier)
                    return layer;
            }
            return null;
        }

        public LDtkLevelInfo? GetLevelInfo(string iid)
        {
            foreach (LDtkLevelInfo info in Header.Levels)
            {
                if (info.IId == iid)
                    return info;
            }
            return null;
        }

        public LDtkLevelInfo? GetLevelInfoByIdentifier(string identifier)
        {
            foreach (LDtkLevelInfo info in Header.Levels)
            {
                if (info.Identifier == identifier)
                    return info;
            }
            return null;
        }

        /// <summary>
        /// Reads the data for the specified level info and all the neighboring levels
        /// </summary>
        /// <param name="levelInfo">Starting level to load and all the neighbors and their neighbors</param>
        /// <returns>
        /// A List of Tuples defining the level data and their neighboring levels in which is also defined within the
        /// list
        /// </returns>
        public List<(LDtkLevel, LDtkLevelInfo)> LoadLevel(LDtkLevelInfo levelInfo)
        {
            List<(LDtkLevel, LDtkLevelInfo)> levels = [(LoadLevelData(levelInfo), levelInfo)];
            LoadNextLevelData(levels, levelInfo);
            return levels;
        }

        private void LoadNextLevelData(List<(LDtkLevel, LDtkLevelInfo)> loaded, LDtkLevelInfo currInfo)
        {
            List<LDtkLevelInfo.Neighbor> toLoad = currInfo.Neighbors;

            foreach (LDtkLevelInfo.Neighbor neighbor in toLoad)
            {
                if (!IsNeighborLoaded(loaded, neighbor))
                {
                    LDtkLevelInfo neighborInfo = GetLevelInfo(neighbor.LevelIId) ?? throw new NullReferenceException($"Failed to load neighboring scene {neighbor.LevelIId}, level info doesn't exist");
                    loaded.Add((LoadLevelData(neighborInfo), neighborInfo));
                    LoadNextLevelData(loaded, neighborInfo);
                }
            }
        }

        private bool IsNeighborLoaded(List<(LDtkLevel, LDtkLevelInfo)> loaded, LDtkLevelInfo.Neighbor neighbor)
        {
            for (int i = 0; i < loaded.Count; ++i)
            {
                if (neighbor.LevelIId == loaded[i].Item2.IId)
                    return true;
            }
            return false;
        }

        private LDtkLevel LoadLevelData(LDtkLevelInfo levelInfo)
        {
            string filePath = RootDirectory + levelInfo.ExternalRelPath;
            var info = new FileInfo(filePath);
            if (!info.Exists)
                throw new FileNotFoundException($"Level Info {levelInfo.Identifier} level data doesn't exist");

            string source = string.Join(Environment.NewLine, File.ReadAllLines(filePath));
            return JsonSerializer.Deserialize<LDtkLevel>(source, RequiredJsonOptions)
                ?? throw new JsonException($"Failed to load level data from Level Info {levelInfo.Identifier} at path {filePath}");
        }
    }
}