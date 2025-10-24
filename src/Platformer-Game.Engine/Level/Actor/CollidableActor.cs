using System.Numerics;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
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

    public abstract class CollidableActor : Actor
    {
        public CollisionLayer CollisionLayer { get; }
        public CollisionLayer CollisionMask { get; }
        public List<ShapeCollider> Colliders { get; }

        public bool DisabledCollision { get; set; }
        public bool DisabledCollisionDisplacement { get; init; }

        protected CollidableActor(CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(position)
        {
            CollisionLayer = layer;
            CollisionMask = mask;
            Colliders = new List<ShapeCollider>();

            DisabledCollision = false;
            DisabledCollisionDisplacement = true;
        }

        public BoxCollider AddBoxCollider(Vector2 offset, float width, float height, ShapeCollider.TriggerCallback? trigger = null)
        {
            var collider = new BoxCollider
            {
                Type = ShapeColliderType.Box,
                Offset = offset,
                Trigger = trigger,
                Width = width,
                Height = height,
            };
            Colliders.Add(collider);
            return collider;
        }

        public CircleCollider AddCircleCollider(Vector2 offset, float radius, ShapeCollider.TriggerCallback? trigger = null)
        {
            var collider = new CircleCollider
            {
                Type = ShapeColliderType.Circle,
                Offset = offset,
                Trigger = trigger,
                Radius = radius,
            };
            Colliders.Add(collider);
            return collider;
        }

#if DEBUG
        public override void OnDraw()
        {
            if (World.ShowCollisionOutlines && !DisabledCollision)
            {
                foreach (ShapeCollider collider in Colliders)
                    collider.DrawOutline(Position);
            }
        }
#endif

        public override void OnUpdate(float deltaTime)
        {
            if (!World.Paused)
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

        protected virtual bool IsColliding(CollidableActor actor, out Vector2 displacement)
        {
            displacement = Vector2.Zero;
            bool collisionDetected = false;
            foreach (ShapeCollider collider in Colliders)
            {
                foreach (ShapeCollider otherCollider in actor.Colliders)
                {
                    Vector2 thisDisplacement = Vector2.Zero;
                    if (collider.IsColliding(this, actor, otherCollider, ref thisDisplacement))
                    {
                        collisionDetected = true;
                        displacement += thisDisplacement;
                    }
                }
            }
            return collisionDetected;
        }

        protected virtual void ApplyDisplacement(Vector2 displacement)
        {
            Position += displacement;
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

        protected virtual void ApplyDisplacements(CollidableActor collidable, Vector2 displacement)
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
    }
}