using System.Numerics;
using PlatformerGame.Engine.Events;

namespace PlatformerGame
{
    public partial class Enemy
    {
        protected interface IState
        {
            public void OnUpdate(float deltaTime);
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
            public virtual void OnExit() { }
            public abstract IState? SwitchState();
        }

        protected class NoState : State<Enemy>
        {
            public NoState(Enemy self) 
                : base(self)
            {
            }

            public override void OnUpdate(float deltaTime)
            {
            }

            public override IState? SwitchState()
            {
                return null;
            }
        }

        protected class DeathState : State<Enemy>
        {
            private const float TimeDuration = 2.0f;
            private float _timer;

            public DeathState(Enemy self) 
                : base(self)
            {
                Self.DisabledCollision = true;

                Self.PlayAnimation("Hit");
                Self._hitSound.Play();

                EventDispatcher.FireEvent(new AddScoreEvent(Self.DeathScore), this);
            }

            public override void OnUpdate(float deltaTime)
            {
                _timer += deltaTime;
                if (_timer >= TimeDuration)
                    Self.Destroy = true;
            }

            public override IState? SwitchState()
            {
                return null;
            }
        }
        
        protected abstract class IdleState<T> : State<T> where T : Enemy
        {
            private float _waitTimer;
            private bool _switchDirectionWhenStateChange;

            public IdleState(T self, bool shouldSwitchDirection = false)
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

        protected abstract class WalkState<T> : State<T> where T : Enemy
        {
            public WalkState(T self)
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