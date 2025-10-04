using System.Numerics;

namespace PlatformerGame.Engine.Level.Collision
{
    [Flags]
    public enum CollisionLayer : int
    {
        None = 0,
        Ground = 1 << 1,
        EffectGround = 1 << 2,
        Collectable = 1 << 3,
        Player = 1 << 4,
        Trap = 1 << 5,
        Enemey = 1 << 6,
        Damage = 1 << 7,
        All = int.MaxValue
    }

    public enum CollisionActorType
    {
        Shapes,
        Tilemap,
    }

    public abstract class CollidableActor : Actor
    {
        private CollisionLayer _layer;
        private CollisionLayer _mask;
        private CollisionActorType _type;
        private List<CollidableActor> _hitInfos;

        protected CollidableActor(CollisionLayer layer, CollisionLayer mask, CollisionActorType collisionType, Vector2 position)
            : base(position)
        {
            _layer = layer;
            _mask = mask;
            _type = collisionType;
            _hitInfos = new List<CollidableActor>();
            DisabledCollisionDisplacement = true;
        }

        public bool DisabledCollisionDisplacement { get; init; }

        public CollisionLayer CollisionLayer
        {
            get { return _layer; }
        }

        public CollisionLayer CollisionMask
        {
            get { return _mask; }
        }

        public CollisionActorType CollisionType
        {
            get { return _type; }
        }

        public IReadOnlyList<CollidableActor> CollisionHits
        {
            get { return _hitInfos; }
        }

        public override void OnUpdate(float deltaTime)
        {
            CalculateCollision(World.GlobalActors);
            CalculateCollision(World.CurrentScene.Actors);
        }

        public override void OnLateUpdate(float deltaTime)
        {
            _hitInfos.Clear();
        }

        protected abstract bool IsColliding(CollidableActor actor, out Vector2 displacement);

        private void CalculateCollision(List<Actor> actors)
        {
            foreach (Actor actor in actors)
            {
                CollidableActor? collidable = GetCollidableIfCollisionApplicable(actor);
                if (collidable == null)
                    continue;

                if (IsColliding(collidable, out Vector2 displacement))
                {
                    ApplyDisplacements(collidable, displacement);

                    // Register hits so not calling the same function again
                    _hitInfos.Add(collidable);
                    collidable._hitInfos.Add(this);
                }
            }
        }

        /// <summary>
        /// Validates whether to continue to calculate collision detection and displacement
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        private CollidableActor? GetCollidableIfCollisionApplicable(Actor? actor)
        {
            CollidableActor? collidable = actor as CollidableActor;
            if (collidable == null || this == actor)
                return null;

            // Skip if their collision layer has been masked out by either collidableActors
            if ((CollisionMask & collidable.CollisionLayer) != 0 || (collidable.CollisionMask & CollisionLayer) != 0)
                return null;

            // Skip if collision calculation has already happened
            foreach (CollidableActor hit in _hitInfos)
            {
                if (hit == collidable)
                    return null;
            }
            return collidable;
        }

        private void ApplyDisplacements(CollidableActor collidable, Vector2 displacement)
        {
            if (!DisabledCollisionDisplacement && !collidable.DisabledCollisionDisplacement)
            {
                displacement *= 0.5f;
                Position += displacement;
                collidable.Position -= displacement;
            }
            else if (!DisabledCollisionDisplacement && collidable.DisabledCollisionDisplacement)
                Position += displacement;
            else if (DisabledCollisionDisplacement && !collidable.DisabledCollisionDisplacement)
                collidable.Position -= displacement;
        }
    }
}