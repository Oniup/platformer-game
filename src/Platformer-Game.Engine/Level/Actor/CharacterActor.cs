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
        public Vector2 ImpulseForce { get; set; }
        public Vector2 MaxVelocityCap { get; set; }
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

        public void ApplyGravityForce(float gravityAmplifierWhenFalling = 1.5f)
        {
            float gravityAmplifier = Velocity.Y > 0.0f ? gravityAmplifierWhenFalling : 1.0f;
            ApplyForce += Vector2.UnitY * (World.GravityScale * gravityAmplifier * Mass);
        }

        public void ApplyForcesBody(float deltaTime)
        {
            Velocity = Vector2.Clamp(Velocity, -MaxVelocityCap, MaxVelocityCap);

            Position += Velocity * deltaTime;
            Velocity += ApplyForce / Mass * deltaTime; // acceleration
            Velocity += ImpulseForce / Mass;

            ApplyForce = Vector2.Zero;
            ImpulseForce = Vector2.Zero;
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