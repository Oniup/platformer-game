using System.Numerics;

namespace PlatformerGame.Engine.Level.Collision
{
    [Flags]
    public enum CollisionLayer : int
    {
        None            = 0,
        Ground          = 1 << 1,
        EffectGround    = 1 << 2,
        Collectable     = 1 << 3,
        Player          = 1 << 4,
        Trap            = 1 << 5,
        Enemey          = 1 << 6,
        Damage          = 1 << 7,
        All             = int.MaxValue
    }

    public enum CollisionActorType
    {
        Shapes,
        Tilemap,
    }

    public struct CollisionHitInfo
    {
        public CollidableActor? Hit;
        public Vector2 Displacement;

        static readonly CollisionHitInfo None = new CollisionHitInfo
        {
            Hit = null,
            Displacement = Vector2.Zero
        };
    }

    public abstract class CollidableActor : Actor
    {
        private CollisionLayer _layer;
        private CollisionLayer _mask;
        private CollisionActorType _type;
        private List<CollisionHitInfo> _hitInfos;

        protected CollidableActor(CollisionLayer layer, CollisionLayer mask, CollisionActorType collisionType, Vector2 position)
            : base(position)
        {
            _layer = layer;
            _mask = mask;
            _type = collisionType;
            _hitInfos = new List<CollisionHitInfo>();
            DisableDisplacement = true;
        }

        public bool DisableDisplacement { get; init; }

        public CollisionLayer CollisionLayer
        {
            get { return _layer; }
        }

        public CollisionLayer CollisionMask
        {
            get { return _mask; }
        }

        public CollisionActorType Type
        {
            get { return _type; }
        }

        public IReadOnlyList<CollisionHitInfo> CollisionHitInfos
        {
            get { return _hitInfos; }
        }

        public override void OnUpdate(float deltaTime)
        {
            _hitInfos.Clear();
            CalculateCollision(World.GlobalActors);
            CalculateCollision(World.CurrentScene.Actors);
        }

        protected abstract bool IsColliding(CollidableActor actor, out Vector2 displacement);

        private void CalculateCollision(List<Actor> actors)
        {
            foreach (Actor actor in actors)
            {
                if (this == actor)
                    continue;

                CollidableActor? collidable = actor as CollidableActor;
                if (collidable == null || (CollisionMask & collidable.CollisionLayer) != 0)
                    continue;

                if (IsColliding(collidable, out Vector2 displacement))
                {
                    _hitInfos.Add(new CollisionHitInfo
                    {
                        Hit = collidable,
                        Displacement = displacement,
                    });

                    if (!DisableDisplacement)
                        Position += displacement;
                }
            }
        }
    }
}