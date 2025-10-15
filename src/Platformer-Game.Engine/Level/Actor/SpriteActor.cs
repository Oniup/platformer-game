using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class SpriteActor : Actor
    {
        protected Sprite Sprite { get; }

        public SpriteActor(Sprite sprite, Vector2 position)
            : base(position)
        {
            Sprite = sprite;
        }

        public override void OnDraw()
        {
            Sprite.Draw(Position, false, false);
        }
    }
}