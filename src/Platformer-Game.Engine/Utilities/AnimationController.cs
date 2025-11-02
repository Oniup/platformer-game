using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Utilities
{
    /// <summary>
    /// Interface that actors use to interact with the AnimationController
    /// </summary>
    public interface IAnimatable
    {
        public bool AnimationPaused { get; }
        public string CurrentAnimation { get; }

        public void UpdateAnimation(float deltaTime);

        public void PlayAnimation(string name, int startingFrame = 0);
        public void PauseAnimation();
        public void ResumeAnimation();
    }

    /// <summary>
    /// Facade for controlling the actor specific animations based from the AnimationSet
    /// </summary>
    internal class AnimationController
    {
        private AnimationSet _animationSet;
        private int _frameIndex;
        private float _frameTimer;

        internal AnimationSet.Animation CurrentAnimation { get; private set; }
        public bool Paused { get; private set; }

        public AnimationController(AnimationSet animationSet)
        {
            _animationSet = animationSet;
            _frameTimer = 0.0f;
            _frameIndex = 0;

            CurrentAnimation = animationSet.Default;
            Paused = false;
        }

        public void Play(string name, int startingFrame)
        {
            Paused = false;

            if (CurrentAnimation.Name == name)
                return;

            AnimationSet.Animation anim = _animationSet.Get(name);
            if (CurrentAnimation.Options.HasFlag(AnimationOption.UninterruptibleUntilComplete) && !anim.Options.HasFlag(AnimationOption.ForceInterruptOnStart))
            {
                if (_frameIndex != CurrentAnimation.FrameCount && !Paused)
                    return;
            }
            BeginPlaying(anim, startingFrame);
        }

        public void Pause()
        {
            Paused = true;
            _frameTimer = 0.0f;
        }

        public void Resume()
        {
            Paused = false;
        }

        public void Update(float deltaTime)
        {
            if (Paused)
                return;

            while (_frameTimer > CurrentAnimation.FrameDuration)
            {
                _frameTimer -= CurrentAnimation.FrameDuration;
                int previousFrame = _frameIndex;

                CurrentAnimation.NextFrame(ref _frameIndex);
                if (_frameIndex == 0 && previousFrame != 0)
                {
                    if (CurrentAnimation.Options.HasFlag(AnimationOption.PauseOnComplete))
                        Pause();

                    if (CurrentAnimation.PlayAfter != null)
                        BeginPlaying(_animationSet.Get(CurrentAnimation.PlayAfter), 0);
                }
            }
            _frameTimer += deltaTime;
        }

        public void DrawFrame(SpriteAtlas atlas, bool flipX, bool flipY, Vector2 position)
        {
            atlas.GridPosition = CurrentAnimation.GetFramePoint(_frameIndex);
            atlas.Draw(position - new Vector2(atlas.GridWidth * 0.5f, atlas.GridHeight * 0.5f), flipX, flipY);
        }

        private void BeginPlaying(AnimationSet.Animation animation, int startingFrame)
        {
            CurrentAnimation = animation;
            _frameIndex = startingFrame;
        }
    }
}