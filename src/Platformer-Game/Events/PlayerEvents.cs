using PlatformerGame.Engine.Events;

namespace PlatformerGame
{
    public class PlayerHitEvent : Event
    {
    }

    public class LevelComplete : Event
    {
    }

    public class AddScoreEvent : Event
    {
        public int Score { get; init; }

        public AddScoreEvent(int score)
        {
            Score = score;
        }
    }
}