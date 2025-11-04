namespace PlatformerGame.Engine.Utilities
{
    public struct DeltaTimer
    {
        private float _duration;
        private float _timer;

        public DeltaTimer(float duration)
        {
            _duration = duration;
            _timer = duration;
        }

        public float Duration => _duration;
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

        public void Update(float deltaTime)
        {
            if (_timer < _duration)
                _timer += deltaTime;
        }
    }
}