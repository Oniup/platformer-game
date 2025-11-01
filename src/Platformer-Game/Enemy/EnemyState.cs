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