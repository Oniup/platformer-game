namespace PlatformerGame.Engine.Utilities
{
    public struct BasicTimer
    {
        public float Duration { get; set; }
        public float Time { get; private set; }

        public BasicTimer(float duration)
        {
            Duration = duration;
        }

        public bool Finished => Time > Duration;

        public void Restart()
        {
            Time = 0.0f;
        }

        public void Tick(float deltaTime)
        {
            Time += deltaTime;
        }
    }
}