using System.Text.Json;

namespace PlatformerGame
{
    public class SaveData
    {
        public class LevelScore
        {
            public required string Name { get; set; }
        }

        public string SelectedSkin { get; set; } = "Ninja Frog";
        public List<LevelScore> Scores { get; set; } = [];

        public static string FileName => "SaveData.json";

        public static void CreateDefaultIfDoesntExist()
        {
            var file = new FileInfo(FileName);
            if (!file.Exists)
                Write(new SaveData { });
        }

        public static SaveData Read()
        {
            string jsonSource = string.Join(Environment.NewLine, File.ReadAllLines(FileName));
            return JsonSerializer.Deserialize<SaveData>(jsonSource) ?? throw new JsonException("Failed to deserialize save data");
        }

        public static void Write(SaveData saveData)
        {
            string jsonStr = JsonSerializer.Serialize(saveData);
            var writer = new StreamWriter(FileName);
            writer.Write(jsonStr);
            writer.Close();
        }
    }
}