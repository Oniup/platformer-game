using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public abstract class CharacterActor : CollidableActor, IAnimatable
    {
        public Vector2 Velocity { get; set; }
        public Vector2 ApplyForce { get; set; }
        public Vector2 ApplyImpulse { get; set; }
        public Vector2 MaxVelocityCap { get; set; }
        public float Mass { get; set; } = 15.0f;
        public float DefaultGravityFallMultiplier { get; set; } = 1.5f;

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        private SpriteAtlas _atlas;
        private AnimationController _animationController;

        protected CharacterActor(SpriteAtlas atlas, AnimationSet animations, CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(layer, mask, position)
        {
            _atlas = atlas;
            FlipX = false;
            FlipY = false;
            _animationController = new AnimationController(animations);
        }

        public bool AnimationPaused => _animationController.Paused;
        public string CurrentAnimation => _animationController.CurrentAnimation.Name;

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            CalculateCollisions();
            UpdateAnimation(deltaTime);
        }

        protected override void ApplyCollisionDisplacement(CollidableActor collidable, Vector2 displacement)
        {
            base.ApplyCollisionDisplacement(collidable, displacement);

            Vector2 surfaceNormal = Vector2.Normalize(displacement);
            float intoSurface = Vector2.Dot(Velocity, surfaceNormal);
            if (intoSurface < 0.0f)
                Velocity -= surfaceNormal * intoSurface;
        }

        public override void OnDraw()
        {
            _animationController.DrawFrame(_atlas, FlipX, FlipY, Position);
#if DEBUG
            // If drawing collision shapes is required
            base.OnDraw();
#endif
        }

        public void UpdateAnimation(float deltaTime)
        {
            _animationController.Update(deltaTime);
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

        public void ApplyGravityForce()
        {
            ApplyGravityForce(DefaultGravityFallMultiplier);
        }

        public void ApplyGravityForce(float gravityMultiplierWhenFalling)
        {
            float gravityAmplifier = Velocity.Y > 0.0f ? gravityMultiplierWhenFalling : 1.0f;
            ApplyForce += Vector2.UnitY * (World.GravityScale * gravityAmplifier * Mass);
        }

        public void ResetAllForces()
        {
            Velocity = Vector2.Zero;
            ResetForces();
        }

        public void ResetForces()
        {
            ApplyForce = Vector2.Zero;
            ApplyImpulse = Vector2.Zero;
        }

        public void ApplyForcesToBody(float deltaTime)
        {
            Velocity = Vector2.Clamp(Velocity, -MaxVelocityCap, MaxVelocityCap);

            Position += Velocity * deltaTime;
            Velocity += ApplyForce / Mass * deltaTime; // acceleration
            Velocity += ApplyImpulse / Mass;

            ApplyForce = Vector2.Zero;
            ApplyImpulse = Vector2.Zero;
        }
    }
}