using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class SpriteActor : Actor
    {
        private Sprite _sprite;

        public SpriteActor(Sprite sprite, Vector2 position)
            : base(position)
        {
            _sprite = sprite;
        }

        protected Sprite Sprite
        {
            get { return _sprite; }
        }

        public override void OnDraw()
        {
            _sprite.Draw(Position, false, false);
        }
    }
}