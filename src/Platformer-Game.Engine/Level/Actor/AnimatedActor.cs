using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public abstract class AnimatedActor : Actor, IAnimatable
    {
        private SpriteAtlas _atlas;
        private AnimationController _animationController;

        protected AnimatedActor(SpriteAtlas atlas, AnimationSet animations, Vector2 position)
            : base(position)
        {
            _atlas = atlas;
            _animationController = new AnimationController(animations);
        }

        public bool AnimationPaused
        {
            get { return _animationController.Paused; }
        }

        public void PlayAnimation(string name, int startingFrame = 0)
        {
            _animationController.Play(name, startingFrame);
        }

        public void PauseAnimation()
        {
            _animationController.Pause();
        }

        public void ResumeAnimation()
        {
            _animationController.Resume();
        }

        public override void OnUpdate(float deltaTime)
        {
            _animationController.UpdateFrame(deltaTime);
        }

        public override void OnDraw()
        {
            _animationController.DrawFrame(_atlas, false, false, Position);
        }
    }
}