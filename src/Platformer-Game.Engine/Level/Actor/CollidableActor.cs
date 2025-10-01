using System.Numerics;

namespace PlatformerGame.Engine.Level.Collision
{
    [Flags]
    public enum CollisionLayer
    {
        Player,
        Ground,
        EffectGround,
        DamageGround,
        Fruit,
        Enemy,
        Trap,
    }

    public abstract class CollidableActor : Actor
    {
        private int _mask;

        protected CollidableActor(int id, Vector2 position, bool active)
            : base(id, position, active)
        {
            _mask = 0;
        }

        public int CollisionMask
        {
            get { return _mask; }
        }

        public abstract bool IsColliding(CollidableActor actor);
    }

    public abstract partial class CollisionShapeActor : CollidableActor
    {
        protected CollisionShapeActor(int id,  Vector2 position, bool active)
            : base(id, position, active)
        {
        }

        public override bool IsColliding(CollidableActor actor)
        {
            throw new NotImplementedException();
        }
    }
}