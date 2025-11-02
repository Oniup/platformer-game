using System.Numerics;
using PlatformerGame.Engine.Events;

namespace PlatformerGame
{
    public partial class Enemy
    {
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
                Self.DisabledCollision = true;

                Self.PlayAnimation("Hit");
                Self._hitSound.Play();

                Self.ApplyImpulse -= Vector2.UnitY * Self.DeathSelfImpulseForce;
                EventDispatcher.FireEvent(new AddScoreEvent(Self.DeathScore), this);
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
        
        protected abstract class IdleState : State<Enemy>
        {
            private float _waitTimer;
            private bool _switchDirectionWhenStateChange;

            public IdleState(Enemy self, bool shouldSwitchDirection = false)
                : base(self)
            {
                _switchDirectionWhenStateChange = !shouldSwitchDirection;
                if (shouldSwitchDirection)
                    Self.MoveDirection = -Self.MoveDirection;
                Self.PlayAnimation("Idle");
            }

            public override void OnUpdate(float deltaTime)
            {
                if (Self.NoWalkState)
                {
                    if (Self.IsWallInFront)
                        Self.MoveDirection = -Self.MoveDirection;
                }
                else
                    _waitTimer += deltaTime;

                // Slow down to a stop
                if (Self.Velocity.X != 0)
                {
                    if (MathF.Abs(Self.Velocity.X) > 10f)
                        Self.Velocity -= new Vector2(Self.Velocity.X * Self.IdleStopDrag * deltaTime, 0.0f);
                    else
                        Self.Velocity = new Vector2(0.0f, Self.Velocity.Y);
                }
            }

            public bool SwitchToWalkState()
            {
                if (!Self.NoWalkState && _waitTimer >= Self.IdleWaitTime)
                {
                    if (_switchDirectionWhenStateChange)
                        Self.MoveDirection = -Self.MoveDirection;
                    return true;
                }

                return false;
            }
        }

        protected abstract class WalkState : State<Enemy>
        {
            public WalkState(Enemy self)
                : base(self)
            {
                Self.MaxVelocityCap = new Vector2(Self.WalkingSpeed, Self.MaxVelocityCap.Y);
                Self.PlayAnimation("Walk");
            }

            public override void OnUpdate(float deltaTime)
            {
                Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self.WalkingSpeed * deltaTime);
            }

            public bool SwitchToIdleState(out bool shouldSwitchDirection)
            {
                shouldSwitchDirection = false;
                if (Self.IsWallInFront)
                {
                    shouldSwitchDirection = true;
                    return true;
                }
                if (!Self.IsGroundInFront)
                    return true;
                return false;
            }
        }
    }
}