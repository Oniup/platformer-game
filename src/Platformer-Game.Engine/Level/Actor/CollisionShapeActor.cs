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

        public void AddBoxCollider(Vector2 offset, int width, int height, bool isTrigger = false)
        {
            _colliders.Add(new BoxCollider
            {
                Type = ShapeColliderType.Box,
                Offset = offset,
                IsTrigger = isTrigger,
                Width = width,
                Height = height,
            });
        }

        public void AddCircleCollider(Vector2 offset, float radius, bool isTrigger = false)
        {
            _colliders.Add(new CircleCollider
            {
                Type = ShapeColliderType.Circle,
                Offset = offset,
                IsTrigger = isTrigger,
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
            switch (actor.Type)
            {
                case CollisionActorType.Shapes:
                    return CollidingWithShapes((CollisionShapeActor)actor, ref displacement);
                case CollisionActorType.Tilemap:
                    // return CollidingWithTilemapLayer((TilemapLayer)actor, out displacement);
                    // TODO: ...
                    break;
            }
            return false;
        }

        private bool CollidingWithShapes(CollisionShapeActor actor, ref Vector2 displacement)
        {
            foreach (ShapeCollider collider in _colliders)
            {
                foreach (ShapeCollider otherCollider in actor.Colliders)
                {
                    ShapeCollider.ActorsCollisionInfo info = new()
                    {
                        DisabledDisplacement = DisableCollisionDisplacement,
                        Position = Position,
                        OtherDisabledDisplacement = actor.DisableCollisionDisplacement,
                        OtherPosition = actor.Position,

                    };
                    return collider.IsColliding(ref info, otherCollider, out displacement);
                }
            }
            return false;
        }

        private bool CollidingWithTilemapLayer(TilemapLayer tilemap, ref Vector2 displacement)
        {
            displacement = Vector2.Zero;
            return false;
        }
    }

    public enum ShapeColliderType
    {
        Box,
        Circle,
    }

    public abstract class ShapeCollider
    {
        public Vector2 Offset { get; set; }
        public bool IsTrigger { get; set; }
        public ShapeColliderType Type { get; init; }

        public struct ActorsCollisionInfo
        {
            public Vector2 Position;
            public Vector2 OtherPosition;
            public bool DisabledDisplacement;
            public bool OtherDisabledDisplacement;
        }

        public bool IsColliding(ref ActorsCollisionInfo info, ShapeCollider collider, out Vector2 displacement)
        {
            switch (collider.Type)
            {
                case ShapeColliderType.Box:
                    return Calculate(ref info, (BoxCollider)collider, out displacement);
                case ShapeColliderType.Circle:
                    return Calculate(ref info, (CircleCollider)collider, out displacement);
                default:
                    displacement = Vector2.Zero;
                    return false;
            }
        }

        protected abstract bool Calculate(ref ActorsCollisionInfo info, CircleCollider collider, out Vector2 displacement);
        protected abstract bool Calculate(ref ActorsCollisionInfo info, BoxCollider collider, out Vector2 displacement);

#if DEBUG
        public abstract void DrawOutline(Vector2 actorPosition);
#endif
    }

    public class BoxCollider : ShapeCollider
    {
        public int Width;
        public int Height;

        public Vector2 CornerOffset
        {
            get { return new Vector2(Width, Height) * 0.5f; }
        }

        protected override bool Calculate(ref ActorsCollisionInfo info, CircleCollider collider, out Vector2 displacement)
        {
            Vector2 circleCenter = info.OtherPosition + collider.Offset;
            Vector2 boxTopLeft = info.Position + Offset - CornerOffset;
            Vector2 boxBottomRight = info.Position + Offset + CornerOffset;

            Vector2 projection = new Vector2
            {
                X = Math.Clamp(circleCenter.X, boxTopLeft.X, boxBottomRight.X),
                Y = Math.Clamp(circleCenter.Y, boxTopLeft.Y, boxBottomRight.Y)
            };
            Vector2 direction = circleCenter - projection;

            float length2 = direction.LengthSquared();
            bool isColliding = length2 < collider.Radius * collider.Radius;
            displacement = Vector2.Zero;
            if (!IsTrigger && !collider.IsTrigger && !info.DisabledDisplacement)
                displacement = -Vector2.Normalize(direction) * (collider.Radius - MathF.Sqrt(length2));

            return isColliding;
        }

        protected override bool Calculate(ref ActorsCollisionInfo info, BoxCollider collider, out Vector2 displacement)
        {
            bool isColliding = false;
            displacement = Vector2.Zero;

            Vector2 topLeft = info.Position + Offset - CornerOffset;
            Vector2 botRight = info.Position + Offset + CornerOffset;
            Vector2 otherTopLeft = info.OtherPosition + collider.Offset - collider.CornerOffset;
            Vector2 otherBotRight = info.OtherPosition + collider.Offset + collider.CornerOffset;

            if (Raylib.IsKeyPressed(KeyboardKey.K))
                isColliding = false;

            bool overlapX = topLeft.X < otherBotRight.X && botRight.X > otherTopLeft.X;
            bool overlapY = botRight.Y > otherTopLeft.Y && topLeft.Y < otherBotRight.Y;
            if (overlapX && overlapY)
            {
                isColliding = true;
                if (!IsTrigger && !collider.IsTrigger && !info.DisabledDisplacement)
                {
                    float penetrationX = MathF.Min(botRight.X, otherBotRight.X) - MathF.Max(topLeft.X, otherTopLeft.X);
                    float penetrationY = MathF.Min(botRight.Y, otherBotRight.Y) - MathF.Max(topLeft.Y, otherTopLeft.Y);
                    if (penetrationX < penetrationY)
                        displacement.X = topLeft.X < otherTopLeft.X ? -penetrationX : penetrationX;
                    else
                        displacement.Y = topLeft.Y < otherTopLeft.Y ? -penetrationY : penetrationY;
                }
            }
            return isColliding;
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

        protected override bool Calculate(ref ActorsCollisionInfo info, CircleCollider collider, out Vector2 displacement)
        {
            bool isColliding = false;
            displacement = Vector2.Zero;

            Vector2 direction = info.OtherPosition + collider.Offset - (info.Position + Offset);
            float length2 = direction.LengthSquared();
            float combinedRadius = Radius + collider.Radius;

            if (length2 < combinedRadius * combinedRadius)
            {
                isColliding = true;
                if (!IsTrigger && !collider.IsTrigger && !info.DisabledDisplacement)
                    displacement = -Vector2.Normalize(direction) * (combinedRadius - MathF.Sqrt(length2));
            }
            return isColliding;
        }

        protected override bool Calculate(ref ActorsCollisionInfo info, BoxCollider collider, out Vector2 displacement)
        {
            Vector2 circleCenter = info.Position + Offset;
            Vector2 boxTopLeft = info.OtherPosition + collider.Offset - collider.CornerOffset;
            Vector2 boxBottomRight = info.OtherPosition + collider.Offset + collider.CornerOffset;

            Vector2 projection = new Vector2
            {
                X = Math.Clamp(circleCenter.X, boxTopLeft.X, boxBottomRight.X),
                Y = Math.Clamp(circleCenter.Y, boxTopLeft.Y, boxBottomRight.Y)
            };
            Vector2 direction = circleCenter - projection;

            float length2 = direction.LengthSquared();
            bool isColliding = length2 < Radius * Radius;
            displacement = Vector2.Zero;
            if (!IsTrigger && !collider.IsTrigger && !info.DisabledDisplacement)
                displacement = Vector2.Normalize(direction) * (Radius - MathF.Sqrt(length2));

            return isColliding;
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