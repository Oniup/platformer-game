using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class SpriteActor : Actor
    {
        private Sprite _sprite;

        public SpriteActor(Sprite sprite, int id, Vector2 position, bool active = true)
            : base(id, position, active)
        {
            _sprite = sprite;
        }

        public override void OnDraw()
        {
            _sprite.Draw(Position);
        }
    }
}