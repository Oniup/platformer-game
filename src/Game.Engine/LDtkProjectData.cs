/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Date Created:   9-21-2025
/// Date Created:   9-21-2025
/// </summary>

using System.Text.Json;

namespace Game.Engine
{
    public class LDtkHeader
    {
        // TODO...
    }

    public class LDtkProjectData
    {
        public string RootDirectory { get; init; }
        public LDtkHeader Header { get; init; }

        public LDtkProjectData(string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
                throw new NullReferenceException("Cannot find LDtk project file at \"" + path + "\"");

            string source = string.Join(Environment.NewLine, File.ReadAllLines(path));
            Header = JsonSerializer.Deserialize<LDtkHeader>(source, RequiredJsonOptions) ?? throw new NullReferenceException("Failed to parse LDtk project json file");
            RootDirectory = path;
        }

        private JsonSerializerOptions RequiredJsonOptions
        {
            get
            {
                JsonSerializerOptions opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new PointJsonConverter(), new VectorJsonConverter() }
                };
                return opts;
            }
        }
    }
}