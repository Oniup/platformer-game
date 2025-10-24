using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame
{
    public class Chain
    {
        private SpriteAtlas _atlas;

        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }

        public Chain(SpriteAtlas atlas, Type type, Vector2 startPosition, Vector2 endPosition)
        {
            _atlas = atlas;
            StartPosition = startPosition;
            EndPosition = endPosition;
            SetChainType(type);
        }

        public void SetChainType(Type type)
        {
            switch (type)
            {
                case Type.Metal:
                    _atlas.GridPosition = Vector2.Zero;
                    break;
                case Type.WoodenDot:
                    _atlas.GridPosition = new Vector2(0, 8);
                    break;
                case Type.MetalDot:
                    _atlas.GridPosition = new Vector2(0, 16);
                    break;
            }
        }

        public void Draw()
        {
            Vector2 drawPosition = StartPosition;
            Vector2 direction = Vector2.Normalize(EndPosition - StartPosition);

            float stopLength = Vector2.DistanceSquared(EndPosition, StartPosition);
            while (Vector2.DistanceSquared(StartPosition, drawPosition) < stopLength)
            {
                _atlas.Draw(drawPosition - _atlas.GridSize * 0.5f, false, false);

                Vector2 moveDirection = direction * _atlas.GridSize;
                drawPosition +=  moveDirection;
            }
        }

        public enum Type
        {
            Metal,
            WoodenDot,
            MetalDot,
        }
    }
}