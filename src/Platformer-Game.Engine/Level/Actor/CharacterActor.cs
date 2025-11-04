using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public abstract class CharacterActor : PawnActor
    {
        public Vector2 Velocity { get; set; }
        public Vector2 ApplyForce { get; set; }
        public Vector2 ApplyImpulse { get; set; }
        public Vector2 MaxVelocityCap { get; set; } = new Vector2(200.0f, 500.0f);
        public float Mass { get; set; } = 15.0f;
        public float DefaultGravityFallMultiplier { get; set; } = 1.5f;

        protected CharacterActor(SpriteAtlas atlas, AnimationSet animations, CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(atlas, animations, layer, mask, position)
        {
        }

        protected override void ApplyCollisionDisplacement(CollidableActor collidable, Vector2 displacement)
        {
            base.ApplyCollisionDisplacement(collidable, displacement);

            Vector2 surfaceNormal = Vector2.Normalize(displacement);
            float intoSurface = Vector2.Dot(Velocity, surfaceNormal);
            if (intoSurface < 0.0f)
                Velocity -= surfaceNormal * intoSurface;
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