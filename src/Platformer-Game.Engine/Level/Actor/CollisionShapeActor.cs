using System.Numerics;
using Raylib_cs;

namespace PlatformerGame.Engine.Level.Collision
{
    public abstract class CollisionShapeActor : CollidableActor
    {
        private List<ShapeCollider> _colliders;

        protected CollisionShapeActor(CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(layer, mask, CollisionActorType.Shapes, position)
        {
            _colliders = new List<ShapeCollider>();
        }

        public List<ShapeCollider> Colliders
        {
            get { return _colliders; }
        }

        public void AddBoxCollider(Vector2 offset, int width, int height, ShapeCollider.TriggerCallback? trigger = null)
        {
            _colliders.Add(new BoxCollider
            {
                Type = ShapeColliderType.Box,
                Offset = offset,
                Trigger = trigger,
                Width = width,
                Height = height,
            });
        }

        public void AddCircleCollider(Vector2 offset, float radius, ShapeCollider.TriggerCallback? trigger = null)
        {
            _colliders.Add(new CircleCollider
            {
                Type = ShapeColliderType.Circle,
                Offset = offset,
                Trigger = trigger,
                Radius = radius,
            });
        }

#if DEBUG
        public override void OnDraw()
        {
            base.OnDraw();
            if (World.ShowCollisionOutlines)
            {
                foreach (ShapeCollider collider in _colliders)
                    collider.DrawOutline(Position);
            }
        }
#endif

        protected override bool IsColliding(CollidableActor actor, out Vector2 displacement)
        {
            displacement = Vector2.Zero;
            if (actor.CollisionType == CollisionActorType.Shapes)
                return CollidingWithShapes((CollisionShapeActor)actor, ref displacement);
            return false;
        }

        private bool CollidingWithShapes(CollisionShapeActor actor, ref Vector2 displacement)
        {
            bool collisionDetected = false;
            foreach (ShapeCollider collider in _colliders)
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
    }

    public enum ShapeColliderType
    {
        Box,
        Circle,
    }

    public abstract class ShapeCollider
    {
        public delegate void TriggerCallback(CollidableActor other, ShapeCollider collider);

        public Vector2 Offset { get; set; }
        public TriggerCallback? Trigger { get; set; }
        public ShapeColliderType Type { get; init; }

        public bool IsTrigger
        {
            get { return Trigger != null; }
        }

        protected abstract bool Calculate(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement);
        protected abstract bool Calculate(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement);

        public bool IsColliding(CollidableActor self, CollidableActor other, ShapeCollider collider, ref Vector2 displacement)
        {
            bool isColliding;
            switch (collider.Type)
            {
                case ShapeColliderType.Box:
                    isColliding = Calculate(self.Position, other.Position, (BoxCollider)collider, ref displacement);
                    break;
                case ShapeColliderType.Circle:
                    isColliding = Calculate(self.Position, other.Position, (CircleCollider)collider, ref displacement);
                    break;
                default:
                    return false;
            }
            if (isColliding)
            {
                if (HandleTriggers(self, other, collider))
                    displacement = Vector2.Zero;
            }
            return isColliding;
        }

        protected bool HandleTriggers(CollidableActor self, CollidableActor other, ShapeCollider otherCollider)
        {
            if (!IsTrigger && !otherCollider.IsTrigger)
                return false;
            Trigger?.Invoke(other, otherCollider);
            otherCollider.Trigger?.Invoke(self, this);
            return true;
        }

#if DEBUG
        public abstract void DrawOutline(Vector2 actorPosition);
#endif
    }

    public class BoxCollider : ShapeCollider
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public virtual Vector2 CornerOffset
        {
            get { return new Vector2(Width, Height) * 0.5f; }
        }

        protected override bool Calculate(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement)
        {
            Vector2 circleCenter = otherPosition + collider.Offset;
            Vector2 boxTopLeft = position + Offset - CornerOffset;
            Vector2 boxBottomRight = position + Offset + CornerOffset;

            Vector2 projection = new Vector2
            {
                X = Math.Clamp(circleCenter.X, boxTopLeft.X, boxBottomRight.X),
                Y = Math.Clamp(circleCenter.Y, boxTopLeft.Y, boxBottomRight.Y)
            };
            Vector2 direction = circleCenter - projection;

            float length2 = direction.LengthSquared();
            if (length2 < collider.Radius * collider.Radius)
            {
                displacement = -Vector2.Normalize(direction) * (collider.Radius - MathF.Sqrt(length2));
                return true;
            }
            return false;
        }

        protected override bool Calculate(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement)
        {
            Vector2 topLeft = position + Offset - CornerOffset;
            Vector2 botRight = position + Offset + CornerOffset;
            Vector2 otherTopLeft = otherPosition + collider.Offset - collider.CornerOffset;
            Vector2 otherBotRight = otherPosition + collider.Offset + collider.CornerOffset;

            bool overlapX = topLeft.X < otherBotRight.X && botRight.X > otherTopLeft.X;
            bool overlapY = botRight.Y > otherTopLeft.Y && topLeft.Y < otherBotRight.Y;
            if (overlapX && overlapY)
            {
                float penetrationX = MathF.Min(botRight.X, otherBotRight.X) - MathF.Max(topLeft.X, otherTopLeft.X);
                float penetrationY = MathF.Min(botRight.Y, otherBotRight.Y) - MathF.Max(topLeft.Y, otherTopLeft.Y);
                if (penetrationX < penetrationY)
                    displacement.X = topLeft.X < otherTopLeft.X ? -penetrationX : penetrationX;
                else
                    displacement.Y = topLeft.Y < otherTopLeft.Y ? -penetrationY : penetrationY;
                return true;
            }
            return false;
        }

#if DEBUG
        public override void DrawOutline(Vector2 actorPosition)
        {
            Vector2 topLeft = actorPosition + Offset - CornerOffset;
            Vector2 botRight = actorPosition + Offset + CornerOffset;
            Rectangle rect = new Rectangle
            {
                Position = topLeft,
                Width = Width,
                Height = Height,
            };
            Raylib.DrawRectangleLinesEx(rect, 1, Color.Green);
            Raylib.DrawCircleV(topLeft, 1, Color.Red);
            Raylib.DrawCircleV(botRight, 1, Color.Blue);
        }
#endif
    }

    public class CircleCollider : ShapeCollider
    {
        public float Radius { get; set; }

        protected override bool Calculate(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement)
        {
            Vector2 direction = otherPosition + collider.Offset - (position + Offset);
            float length2 = direction.LengthSquared();
            float combinedRadius = Radius + collider.Radius;

            if (length2 < combinedRadius * combinedRadius)
            {
                displacement = -Vector2.Normalize(direction) * (combinedRadius - MathF.Sqrt(length2));
                return true;
            }
            return false;
        }

        protected override bool Calculate(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement)
        {
            Vector2 circleCenter = position + Offset;
            Vector2 boxTopLeft = otherPosition + collider.Offset - collider.CornerOffset;
            Vector2 boxBottomRight = otherPosition + collider.Offset + collider.CornerOffset;

            Vector2 projection = new Vector2
            {
                X = Math.Clamp(circleCenter.X, boxTopLeft.X, boxBottomRight.X),
                Y = Math.Clamp(circleCenter.Y, boxTopLeft.Y, boxBottomRight.Y)
            };
            Vector2 direction = circleCenter - projection;

            float length2 = direction.LengthSquared();
            if (length2 < Radius * Radius)
            {
                displacement = Vector2.Normalize(direction) * (Radius - MathF.Sqrt(length2));
                return true;
            }
            return false;
        }

#if DEBUG
        public override void DrawOutline(Vector2 actorPosition)
        {
            Vector2 center = actorPosition + Offset;
            Raylib.DrawCircleLinesV(center, Radius, Color.Green);
        }
#endif
    }
}