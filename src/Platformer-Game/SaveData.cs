using System.Text.Json;

namespace PlatformerGame
{
    public class SaveData
    {
        public const float ScoreRatio3Star = 1.0f;
        public const float ScoreRatio2Star = 0.75f;
        public const float ScoreRatio1Star = 0.3f;

        public class LevelScore
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
            public Run? BestRun { get; set; }

            public bool SetBestRun(Run run)
            {
                if (BestRun == null)
                {
                    BestRun = run;
                    return true;
                }

                float prevScore = GetRunScoreRatio(BestRun);
                float newScore = GetRunScoreRatio(run);
                if (prevScore < newScore || (prevScore == newScore && run.Time < BestRun.Time))
                {
                    BestRun = run;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Will provide a number from 0-1, the stars can be broken up into thirds.
            /// </summary>
            /// <param name="run"></param>
            /// <returns></returns>
            public float GetRunScoreRatio(Run run)
            {
                float scoreRatio = (float)run.Score / TotalRequiredScore;
                float hitRatio = 1.0f - Math.Clamp((float)run.Hits / MinHitsFor2Star, 0.0f, 1.0f);

                float timeRatio;
                if (run.Time <= Required3StarTime)
                    timeRatio = 1.0f;
                else if (run.Time <= Required2StarTime)
                {
                    float linearFade = (Required2StarTime - run.Time) / (Required2StarTime - Required3StarTime);
                    timeRatio = 0.5f + 0.5f * linearFade;
                }
                else
                {
                    float linearFade = Math.Clamp(Required2StarTime / run.Time * 0.5f, 0.0f, 0.5f);
                    timeRatio = linearFade;
                }

                return (scoreRatio * 0.4f) + (timeRatio * 0.4f) + (hitRatio * 0.2f);
            }
        }

        public string SelectedSkin { get; set; } = "Ninja Frog";
        public List<LevelScore> Scores { get; set; } = [
            new LevelScore
            {
                Name = "Testing",
                Required3StarTime = 18,
                Required2StarTime = 25,
                TotalRequiredScore = 17,
                MinHitsFor2Star = 2,
            },
            new LevelScore
            {
                Name = "Level2",
                Required3StarTime = 200,
                Required2StarTime = 400,
                TotalRequiredScore = 17,
                MinHitsFor2Star = 5,
            },
            new LevelScore
            {
                Name = "Level3",
                Required3StarTime = 200,
                Required2StarTime = 400,
                TotalRequiredScore = 17,
                MinHitsFor2Star = 5,
            },
        ];

        public LevelScore GetLevelScore(string name)
        {
            foreach (LevelScore score in Scores)
            {
                if (score.Name == name)
                    return score;
            }
            throw new NullReferenceException($"There is no \"{name}\" Level, unable to obtain Score");
        }

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