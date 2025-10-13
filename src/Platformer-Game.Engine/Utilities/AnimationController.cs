using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Utilities
{
    public interface IAnimatable
    {
        public bool AnimationPaused { get; }
        public string CurrentAnimation { get; }

        public void UpdateAnimation(float deltaTime);

        public void PlayAnimation(string name, int startingFrame = 0);
        public void PauseAnimation();
        public void ResumeAnimation();
    }

    public class AnimationController
    {
        private AnimationSet _animationSet;
        private AnimationSet.Animation _currentAnimation;
        private int _frameIndex;
        private bool _paused;
        private float _frameTimer;

        public AnimationController(AnimationSet animationSet)
        {
            _animationSet = animationSet;
            _currentAnimation = animationSet.Default;
            _paused = false;
            _frameTimer = 0.0f;
            _frameIndex = 0;
        }

        public bool Paused
        {
            get { return _paused; }
        }

        public AnimationSet.Animation CurrentAnimation
        {
            get { return _currentAnimation; }
        }

        public void Play(string name, int startingFrame)
        {
            if (_currentAnimation.Mode == AnimationMode.UninterruptableUntilComplete)
            {
                if (_frameIndex != _currentAnimation.FrameCount && !_paused)
                    return;
            }
            if (_currentAnimation.Name == name)
                return;

            BeginPlaying(name, startingFrame);
        }

        public void Pause()
        {
            _paused = true;
            _frameTimer = 0.0f;
        }

        public void Resume()
        {
            _paused = false;
        }

        public void Update(float deltaTime)
        {
            if (_paused)
                return;

            while (_frameTimer > _currentAnimation.FrameDuration)
            {
                _frameTimer -= _currentAnimation.FrameDuration;
                int previousFrame = _frameIndex;

                _currentAnimation.NextFrame(ref _frameIndex);
                if (_frameIndex == 0 && previousFrame != 0)
                {
                    if (_currentAnimation.Mode == AnimationMode.PauseOnComplete)
                        Pause();

                    if (_currentAnimation.PlayAfter != null)
                        BeginPlaying(_currentAnimation.PlayAfter, 0);
                }
            }
            _frameTimer += deltaTime;
        }

        public void DrawFrame(SpriteAtlas atlas, bool flipX, bool flipY, Vector2 position)
        {
            atlas.GridPosition = _currentAnimation.GetFramePoint(_frameIndex);
            atlas.Draw(position - new Vector2(atlas.GridWidth * 0.5f, atlas.GridHeight * 0.5f), flipX, flipY);
        }

        private void BeginPlaying(string name, int startingFrame)
        {
            _currentAnimation = _animationSet.Get(name);
            _frameIndex = startingFrame;
        }
    }
}