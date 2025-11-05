using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public class PawnActor : CollidableActor, IAnimatable
    {
        private SpriteAtlas _atlas;
        private AnimationController _animationController;

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public PawnActor(SpriteAtlas atlas, AnimationSet animations, CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(layer, mask, position)
        {
            _atlas = atlas;
            _animationController = new AnimationController(animations);
        }

        public bool AnimationPaused => _animationController.Paused;
        public string CurrentAnimation => _animationController.CurrentAnimation.Name;

        public override void OnUpdate(float deltaTime)
        {
            if (!World.Paused)
                UpdateAnimation(deltaTime);
        }

        public override void OnDraw()
        {
            _animationController.DrawFrame(_atlas, FlipX, FlipY, Position);
#if DEBUG
            // If drawing collision shapes is required
            base.OnDraw();
#endif
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

        public void UpdateAnimation(float deltaTime)
        {
            _animationController.Update(deltaTime);
        }
    }
}
