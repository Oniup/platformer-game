using System.Numerics;

namespace PlatformerGame.Engine.Level.Collision
{
    public abstract partial class CollisionShapeActor
    {
        public enum ShapeType
        {
            Rectangle,
            Circle,
        }

        public abstract class Shape
        {
            private Vector2 _position;
        }
    }
}