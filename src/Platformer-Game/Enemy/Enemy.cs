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

        // Movement and direction
        protected float MoveDirection { get; set; }
        protected float WalkingSpeed { get; init; } = 50.0f;
        protected float IdleStopDrag { get; init; } = 12.0f;
        protected float IdleWaitTime { get; init; } = 1.0f;

        // Conditions
        protected bool NoWalkState { get; init; }
        private (CollidableActor, ShapeCollider)? _visibleActor;
        private bool _isGroundInFront;
        private bool _isWallInFront;


        // State
        protected IState CurrentState { get; set; } = null!;

        // Colliders for conditions
        protected BoxCollider? IsOnGroundCollider { get; set; } = null!;
        protected BoxCollider? IsWallInFrontCollider { get; set; } = null!;
        protected BoxCollider? VisionCollider { get; set; } = null!;
        protected float CheckColliderOffset { get; set; }

        protected int DeathScore { get; init; } = 1;
        protected float DeathPlayerImpulseForce { get; init; } = 4000.0f;
        protected float DeathSelfImpulseForce { get; init; } = 1000.0f;

        public Enemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, Vector2 position)
            : base(atlas, animations, CollisionLayer.Enemy, CollisionLayer.All & ~(CollisionLayer.Player | CollisionLayer.Ground | CollisionLayer.Trap | CollisionLayer.Damage), position)
        {
            DisabledCollisionDisplacement = false;

            _hitSound = hitSound;
        }

        public bool IsGroundInFront => IsOnGroundCollider != null ? _isGroundInFront : true;
        public bool IsWallInFront => IsWallInFrontCollider != null ? _isWallInFront : false;
        public CollidableActor? VisibleActor => _visibleActor != null ? _visibleActor.Value.Item1 : null;

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

        public bool IsSeeingPlayer()
        {
            if (_visibleActor != null)
            {
                (CollidableActor actor, _) = _visibleActor.Value;
                if (actor is Player)
                    return true;
            }
            return false;
        }

        protected void SetupRequiredColliders(Vector2 baseOffset, Vector2 headOffset, Vector2 baseSize, bool groundCollider = true, bool wallCollider = true)
        {
            AddBoxCollider(baseOffset, baseSize.X, baseSize.Y, OnPlayerHit, true);
            AddBoxCollider(headOffset, baseSize.X - 8, 6, OnHeadHitTrigger);

            if (groundCollider)
                IsOnGroundCollider = AddBoxCollider(new Vector2(0.0f, baseOffset.Y + baseSize.Y / 2), 5, 5, OnIsGroundInFrontTrigger);
            if (wallCollider)
                IsWallInFrontCollider = AddBoxCollider(new Vector2(0.0f, baseOffset.Y + baseSize.Y / 4), 5, 5, OnIsWallRightInFrontTrigger);

            CheckColliderOffset = groundCollider || wallCollider ? baseSize.X * 0.7f : 0.0f;
        }

        protected void SetupVisionCollider(float detectRange, float height, float yOffset)
        {
            var offset = new Vector2(0.0f, yOffset);
            if (MoveDirection != 0.0f)
                offset += Vector2.UnitX * (MoveDirection * detectRange * 0.5f);
            VisionCollider = AddBoxCollider(offset, detectRange, height, OnVisionEnterTrigger);
        }

        protected void OnHeadHitTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
            {
                var player = (Player)actor;
                player.Velocity = new Vector2(player.Velocity.X, 0.0f);
                player.ApplyImpulse -= Vector2.UnitY * DeathPlayerImpulseForce;
                player.ResetDoubleJump();

                ApplyImpulse += new Vector2(-MathF.Sign(player.Velocity.X), 1.0f) * DeathSelfImpulseForce;

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
            if (!collider.IsTrigger && actor.CollisionLayer.HasFlag(CollisionLayer.Ground))
                _isGroundInFront = true;
        }

        protected void OnIsWallRightInFrontTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger && actor.CollisionLayer.HasFlag(CollisionLayer.Ground) || actor.CollisionLayer.HasFlag(CollisionLayer.Trap))
                _isWallInFront = true;
        }

        protected void OnVisionEnterTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger)
                return;

            if (_visibleActor == null)
            {
                _visibleActor = (actor, collider);
                return;
            }

            // FIXME: Can still detect player through the wall
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
            {
                CurrentState.OnExit();
                CurrentState = newState;
            }

            // Make sure to face the correct way when moving
            if (MoveDirection != 0.0f)
                FlipX = MoveDirection > 0.0f;

            // Move detection colliders to correct positions based on move direction
            float xColliderOffset = MoveDirection * CheckColliderOffset;
            if (IsOnGroundCollider != null)
                IsOnGroundCollider.Offset = new Vector2(xColliderOffset, IsOnGroundCollider.Offset.Y);
            if (IsWallInFrontCollider != null)
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
                        return MathF.Max(0.0f, Position.X - topX);
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