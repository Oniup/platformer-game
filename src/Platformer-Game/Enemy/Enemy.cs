using System.ComponentModel;
using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public abstract partial class Enemy : CharacterActor
    {
        private SoundEffect _hitSound;

        // Conditions
        private (CollidableActor, ShapeCollider)? _visibleActor;
        private bool _isGroundInFront;
        private bool _isWallInFront;

        protected float MoveDirection { get; set; }

        // State
        protected IState CurrentState { get; set; } = null!;

        // Colliders for conditions
        protected BoxCollider IsOnGroundCollider { get; init; } = null!;
        protected BoxCollider IsWallInFrontCollider { get; init; } = null!;
        protected BoxCollider? VisionCollider { get; init; } = null!;
        protected float CheckColliderOffset { get; init; }

        protected int DeathScore { get; init; } = 1;
        protected float DeathPlayerImpulseForce { get; init; } = 4000.0f;
        protected float DeathSelfImpulseForce { get; init; } = 1000.0f;

        public Enemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, Vector2 position)
            : base(atlas, animations, CollisionLayer.Enemy, CollisionLayer.Collectable, position)
        {
            DisabledCollisionDisplacement = false;

            _hitSound = hitSound;
        }

        protected bool IsGroundInFront => _isGroundInFront;
        protected bool IsWallInFront => _isWallInFront;
        protected CollidableActor? VisibleActor => _visibleActor != null ? _visibleActor.Value.Item1 : null;

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            base.OnUpdate(deltaTime);

            HandleState(deltaTime);
            HandleForces(deltaTime);
            ResetConditions();
        }

        public void SetToDeathState()
        {
            CurrentState = new DeathState(this);
        }

        protected bool IsSeeingPlayer()
        {
            if (_visibleActor != null)
            {
                (CollidableActor actor, _) = _visibleActor.Value;
                if (actor is Player)
                    return true;
            }
            return false;
        }

        protected void OnHeadHitTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
            {
                var player = (Player)actor;
                player.Velocity = new Vector2(player.Velocity.X, 0.0f);
                player.ApplyImpulse -= Vector2.UnitY * DeathPlayerImpulseForce;
                player.ResetDoubleJump();

                SetToDeathState();
            }
        }

        protected void OnPlayerHit(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
                EventDispatcher.FireEvent(new PlayerHitEvent(), this);
        }

        protected void OnIsGroundInFrontTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Ground))
                _isGroundInFront = true;
        }

        protected void OnIsWallRightInFrontTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Ground))
                _isWallInFront = true;
        }

        protected void OnVisionEnterTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (_visibleActor == null)
            {
                _visibleActor = (actor, collider);
                return;
            }

            (CollidableActor currActor, ShapeCollider currCol) = _visibleActor.Value;
            float currDist = GetDistance(currActor, currCol);
            float dist = GetDistance(actor, collider);
            if (dist < currDist)
                _visibleActor = (actor, collider);
        }

        private void HandleForces(float deltaTime)
        {
            ApplyGravityForce();
            ApplyForcesToBody(deltaTime);
        }

        private void HandleState(float deltaTime)
        {
            CurrentState.OnUpdate(deltaTime);

            IState? newState = CurrentState.SwitchState();
            if (newState != null)
                CurrentState = newState;

            // Make sure to face the correct way when moving
            if (MoveDirection != 0.0f)
                FlipX = MoveDirection > 0.0f;

            // Move detection colliders to correct positions based on move direction
            float xColliderOffset = MoveDirection * CheckColliderOffset;
            IsOnGroundCollider.Offset = new Vector2(xColliderOffset, IsOnGroundCollider.Offset.Y);
            IsWallInFrontCollider.Offset = new Vector2(xColliderOffset, IsWallInFrontCollider.Offset.Y);
            if (VisionCollider != null && xColliderOffset != 0)
                VisionCollider.Offset = new Vector2(MoveDirection * VisionCollider.Size.X * 0.5f, VisionCollider.Offset.Y);
        }

        private void ResetConditions()
        {
            _visibleActor = null;
            _isGroundInFront = false;
            _isWallInFront = false;
        }

        private float GetDistance(CollidableActor actor, ShapeCollider collider)
        {
            if (collider.Type == ShapeColliderType.Box)
            {
                var boxCollider = (BoxCollider)collider;
                float topX = actor.Position.X + boxCollider.TopLeftOffset.X;
                float botX = actor.Position.X + boxCollider.BottomRightOffset.X;

                // If in between the two end points 
                if (Position.X >= topX && Position.X <= botX)
                {
                    if (MoveDirection > 0.0f)
                        return MathF.Max(0.0f, botX - Position.X);
                    else
                        return MathF.MaxMagnitude(0.0f, Position.X - topX);
                }
                // Otherwise choose the shortest distance between to two points
                return MathF.Min(
                    MathF.Abs(topX - Position.X),
                    MathF.Abs(botX - Position.X)
                );
            }
            else if (collider.Type == ShapeColliderType.Circle)
            {
                var circleCollider = (CircleCollider)collider;
                float circleCenter = actor.Position.X + circleCollider.Offset.X;
                float distance = MathF.Abs(Position.X - circleCenter) - circleCollider.Radius;
                return MathF.Max(0.0f, distance);
            }
            throw new InvalidEnumArgumentException("This should never be called");
        }
    }
}