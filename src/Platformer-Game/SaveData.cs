using System.Text.Json;

namespace PlatformerGame
{
    public class SaveData
    {
        public class Level
        {
            public class Run
            {
                public required int Score { get; set; }
                public required float Time { get; set; }
                public required int Hits { get; set; }
            }

            public required string Name { get; init; }
            public required float Required3StarTime { get; init; }
            public required float Required2StarTime { get; init; }
            public required int TotalRequiredScore { get; init; }
            public required int MinHitsFor2Star { get; init; }
            public Run? BestEntry { get; set; }
        }

        public string SelectedSkin { get; set; } = "Player1";
        public List<Level> Scores { get; set; } = [
            new Level
            {
                Name = "TestLevel",
                Required3StarTime = 200,
                Required2StarTime = 400,
                TotalRequiredScore = 17,
                MinHitsFor2Star = 5,
            },
        ];

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