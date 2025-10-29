using System.Numerics;
using PlatformerGame.Engine.Level;
using Raylib_cs;

namespace PlatformerGame.Engine.Utilities
{
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

        public bool IsTrigger => Trigger != null;

        protected abstract bool CollideWithCircle(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement);
        protected abstract bool CollideWithBox(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement);

        public bool IsColliding(CollidableActor self, CollidableActor other, ShapeCollider collider, ref Vector2 displacement)
        {
            bool isColliding;
            switch (collider.Type)
            {
                case ShapeColliderType.Box:
                    isColliding = CollideWithBox(self.Position, other.Position, (BoxCollider)collider, ref displacement);
                    break;
                case ShapeColliderType.Circle:
                    isColliding = CollideWithCircle(self.Position, other.Position, (CircleCollider)collider, ref displacement);
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

        protected static bool CircleVsCircle(Vector2 circle1, Vector2 circle2, float radius1, float radius2, ref Vector2 displacement)
        {
            Vector2 direction = circle2 - circle1;
            float length2 = direction.LengthSquared();
            float combinedRadius = radius1 + radius2;

            if (length2 < combinedRadius * combinedRadius)
            {
                displacement = -Vector2.Normalize(direction) * (combinedRadius - MathF.Sqrt(length2));
                return true;
            }
            return false;
        }

        protected static bool CircleVsBox(Vector2 circleCenter, float circleRadius, Vector2 boxTopLeft, Vector2 boxBottomRight, ref Vector2 displacement)
        {
            var projection = new Vector2
            {
                X = Math.Clamp(circleCenter.X, boxTopLeft.X, boxBottomRight.X),
                Y = Math.Clamp(circleCenter.Y, boxTopLeft.Y, boxBottomRight.Y)
            };
            Vector2 direction = circleCenter - projection;

            float length2 = direction.LengthSquared();
            if (length2 < circleRadius * circleRadius)
            {
                displacement = Vector2.Normalize(direction) * (circleRadius - MathF.Sqrt(length2));
                return true;
            }
            return false;
        }

        protected static bool BoxVsBox(Vector2 topLeft1, Vector2 bottomRight1, Vector2 topLeft2, Vector2 bottomRight2, ref Vector2 displacement)
        {
            bool overlapX = topLeft1.X < bottomRight2.X && bottomRight1.X > topLeft2.X;
            bool overlapY = bottomRight1.Y > topLeft2.Y && topLeft1.Y < bottomRight2.Y;
            if (overlapX && overlapY)
            {
                float penetrationX = MathF.Min(bottomRight1.X, bottomRight2.X) - MathF.Max(topLeft1.X, topLeft2.X);
                float penetrationY = MathF.Min(bottomRight1.Y, bottomRight2.Y) - MathF.Max(topLeft1.Y, topLeft2.Y);
                if (penetrationX < penetrationY)
                    displacement.X = topLeft1.X < topLeft2.X ? -penetrationX : penetrationX;
                else
                    displacement.Y = topLeft1.Y < topLeft2.Y ? -penetrationY : penetrationY;
                return true;
            }
            return false;
        }

#if DEBUG
        public abstract void DrawOutline(Vector2 actorPosition);
#endif
    }

    public class BoxCollider : ShapeCollider
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public virtual Vector2 CornerOffset => new Vector2(Width, Height) * 0.5f;

        protected override bool CollideWithCircle(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement)
        {
            Vector2 circleCenter = otherPosition + collider.Offset;
            Vector2 boxTopLeft = position + Offset - CornerOffset;
            Vector2 boxBottomRight = position + Offset + CornerOffset;
            if (CircleVsBox(circleCenter, collider.Radius, boxTopLeft, boxBottomRight, ref displacement))
            {
                displacement = -displacement;
                return true;
            }
            return false;
        }

        protected override bool CollideWithBox(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement)
        {
            Vector2 topLeft1 = position + Offset - CornerOffset;
            Vector2 bottomRight1 = position + Offset + CornerOffset;
            Vector2 topLeft2 = otherPosition + collider.Offset - collider.CornerOffset;
            Vector2 bottomRight2 = otherPosition + collider.Offset + collider.CornerOffset;
            return BoxVsBox(topLeft1, bottomRight1, topLeft2, bottomRight2, ref displacement);
        }

#if DEBUG
        public override void DrawOutline(Vector2 actorPosition)
        {
            Vector2 topLeft = actorPosition + Offset - CornerOffset;
            Vector2 botRight = actorPosition + Offset + CornerOffset;
            var rect = new Rectangle
            {
                Position = topLeft,
                Width = Width,
                Height = Height,
            };
            Raylib.DrawRectangleLinesEx(rect, 1, Color.Green);
        }
#endif
    }

    public class CircleCollider : ShapeCollider
    {
        public float Radius { get; set; }

        protected override bool CollideWithCircle(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement)
        {
            Vector2 circle1 = position + Offset;
            Vector2 circle2 = otherPosition + collider.Offset;
            return CircleVsCircle(circle1, circle2, Radius, collider.Radius, ref displacement);
        }

        protected override bool CollideWithBox(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement)
        {
            Vector2 circleCenter = position + Offset;
            Vector2 boxTopLeft = otherPosition + collider.Offset - collider.CornerOffset;
            Vector2 boxBottomRight = otherPosition + collider.Offset + collider.CornerOffset;
            return CircleVsBox(circleCenter, Radius, boxTopLeft, boxBottomRight, ref displacement);
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