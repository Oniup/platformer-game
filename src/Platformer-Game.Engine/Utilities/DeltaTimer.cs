namespace PlatformerGame.Engine.Utilities
{
    public struct DeltaTimer
    {
        private float _duration;
        private float _timer;

        public DeltaTimer(float duration, bool startNow = false)
        {
            _duration = duration;
            if (!startNow)
                _timer = duration;
        }

        public float Duration => _duration;
        public float Time => _timer;
        public bool Finished => _timer >= _duration;

        public void Start()
        {
            _timer = 0.0f;
        }

        public void SetDuration(float duration)
        {
            _duration = duration;
            _timer = duration;
        }

        public bool Update(float deltaTime)
        {
            if (_timer < _duration)
            {
                _timer += deltaTime;
                return true;
            }
            return false;
        }
    }
}