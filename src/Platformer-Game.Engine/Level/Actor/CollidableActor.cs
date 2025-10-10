using System.Numerics;

namespace PlatformerGame.Engine.Level.Collision
{
    [Flags]
    public enum CollisionLayer : int
    {
        None            = 0,
        Ground          = 1 << 1,
        Platform        = 1 << 2,
        EffectGround    = 1 << 3,
        Collectable     = 1 << 4,
        Player          = 1 << 5,
        Trap            = 1 << 6,
        Enemey          = 1 << 7,
        Damage          = 1 << 8,
        All             = int.MaxValue
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

        public bool DisabledCollision { get; set; }
        public bool DisabledCollisionDisplacement { get; init; }

        protected CollidableActor(CollisionLayer layer, CollisionLayer mask, CollisionActorType collisionType, Vector2 position)
            : base(position)
        {
            _layer = layer;
            _mask = mask;
            _type = collisionType;

            DisabledCollision = false;
            DisabledCollisionDisplacement = true;
        }

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

        protected abstract bool IsColliding(CollidableActor actor, out Vector2 displacement);

        protected virtual void ApplyDisplacement(Vector2 displacement)
        {
            Position += displacement;
        }

        public override void OnUpdate(float deltaTime)
        {
            CalculateCollisions();
        }

        public bool CalculateCollisions()
        {
            if (!DisabledCollision)
            {
                bool global = CalculateCollisions(World.GlobalActors);
                bool scene = CalculateCollisions(World.CurrentScene.Actors);
                return global || scene;
            }
            return false;
        }

        public bool CalculateCollisions(List<Actor> actors)
        {
            bool collisionDetected = false;
            foreach (Actor actor in actors)
            {
                CollidableActor? collidable = GetCollidableIfCollisionApplicable(actor);
                if (collidable == null)
                    continue;

                if (IsColliding(collidable, out Vector2 displacement))
                {
                    ApplyDisplacements(collidable, displacement);
                    collisionDetected = true;
                }
            }
            return collisionDetected;
        }

        protected void CalculateCollisions(List<Actor> actors, ref List<CollidableActor> colliding)
        {
            foreach (Actor actor in actors)
            {
                CollidableActor? collidable = GetCollidableIfCollisionApplicable(actor);
                if (collidable == null)
                    continue;

                if (IsColliding(collidable, out Vector2 displacement))
                {
                    ApplyDisplacements(collidable, displacement);
                    colliding.Add(collidable);
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

            if (collidable.DisabledCollision)
                return null;

            // Skip if their collision layer has been masked out by either collidableActors
            if ((CollisionMask & collidable.CollisionLayer) != 0 || (collidable.CollisionMask & CollisionLayer) != 0)
                return null;
            return collidable;
        }

        private void ApplyDisplacements(CollidableActor collidable, Vector2 displacement)
        {
            if (displacement == Vector2.Zero)
                return;

            if (!DisabledCollisionDisplacement && !collidable.DisabledCollisionDisplacement)
            {
                displacement *= 0.5f;
                ApplyDisplacement(displacement);
                collidable.ApplyDisplacement(-displacement);
            }
            else if (!DisabledCollisionDisplacement && collidable.DisabledCollisionDisplacement)
                ApplyDisplacement(displacement);
            else if (DisabledCollisionDisplacement && !collidable.DisabledCollisionDisplacement)
                collidable.ApplyDisplacement(-displacement);
        }
    }
}