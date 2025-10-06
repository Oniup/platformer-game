using System.Numerics;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public abstract class CharacterActor : CollisionShapeActor, IAnimatable
    {
        private SpriteAtlas _atlas;
        private AnimationController _animationController;

        protected CharacterActor(SpriteAtlas atlas, AnimationSet animations, CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(layer, mask, position)
        {
            _atlas = atlas;
            _animationController = new AnimationController(animations);
        }

        public Vector2 Velocity { get; set; }
        public Vector2 ApplyForce { get; set; }
        public float Mass { get; set; } = 15.0f;

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
            // Handle collision detection
            base.OnUpdate(deltaTime);

            _animationController.UpdateAnimation(deltaTime);
        }

        public void ApplyMovementForces(float deltaTime)
        {
            Vector2 acceleration = ApplyForce / Mass;
            Position += Velocity * deltaTime;
            Velocity += acceleration * deltaTime;
            ApplyForce = Vector2.Zero;
        }

        public override void OnDraw()
        {
            _animationController.DrawFrame(_atlas, Position);

#if DEBUG
            // If drawing collision shapes is required
            base.OnDraw();
#endif
        }
    }
}