using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public abstract class Enemy : CharacterActor
    {
        protected IState CurrentState { get; set; } = null!;
        protected int ScoreAfterDeath { get; init; } = 2;
        protected float DeathPlayerImpulseForce { get; init; } = 4000.0f;

        public Enemy(SpriteAtlas atlas, AnimationSet animations, Vector2 position)
            : base(atlas, animations, CollisionLayer.Enemy & CollisionLayer.Ground, CollisionLayer.None, position)
        {
            DisabledCollisionDisplacement = false;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            base.OnUpdate(deltaTime);

            HandleState(deltaTime);
            HandleForces(deltaTime);
        }

        public void HandleForces(float deltaTime)
        {
            ApplyGravityForce();
            ApplyForcesToBody(deltaTime);
        }

        protected void HandleState(float deltaTime)
        {
            CurrentState.OnUpdate(deltaTime);

            IState? newState = CurrentState.SwitchState();
            if (newState != null)
                CurrentState = newState;
        }

        public void OnHeadHitTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
            {
                DisabledCollision = true;

                var player = (Player)actor;
                player.Velocity = new Vector2(player.Velocity.X, 0.0f);
                player.ApplyImpulse -= Vector2.UnitY * DeathPlayerImpulseForce;
                player.ResetDoubleJump();

                PlayAnimation("Hit");
                CurrentState = new DeathState(this);

                EventDispatcher.FireEvent(new AddScoreEvent(2));
            }
        }

        protected interface IState
        {
            public void OnUpdate(float deltaTime);
            public void OnEnter();
            public void OnExit();

            IState? SwitchState();
        }

        protected abstract class State<T> : IState where T : Enemy
        {
            protected T Self { get; }

            public State(T self)
            {
                Self = self;
            }

            public abstract void OnUpdate(float deltaTime);
            public virtual void OnEnter() { }
            public virtual void OnExit() { }
            public abstract IState? SwitchState();
        }

        protected class DeathState : State<Enemy>
        {
            public DeathState(Enemy self) 
                : base(self)
            {
            }

            public override void OnUpdate(float deltaTime)
            {
                if (Self.AnimationPaused)
                    Self.Destroy = true;
            }

            public override IState? SwitchState()
            {
                return null;
            }
        }
    }
}