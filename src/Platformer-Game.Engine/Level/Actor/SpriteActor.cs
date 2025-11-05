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
            Sprite.Draw(Position - Sprite.Size * 0.5f, false, false);
        }
    }

    public class SpriteAtlasActor : Actor
    {
        protected SpriteAtlas Sprite { get; }

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public SpriteAtlasActor(SpriteAtlas sprite, Vector2 position)
            : base(position)
        {
            Sprite = sprite;
        }

        public override void OnDraw()
        {
            Sprite.Draw(Position - Sprite.GridSize * 0.5f, FlipX, FlipY);
        }
    }

    public class CollidableSpriteActor : CollidableActor
    {
        private Sprite Sprite { get; }

        public CollidableSpriteActor(Sprite sprite, CollisionLayer layer, CollisionLayer mask, Vector2 position)
            : base(layer, mask, position)
        {
            Sprite = sprite;
        }

        public override void OnDraw()
        {
            Sprite.Draw(Position - Sprite.Size * 0.5f, false, false);
#if DEBUG
            base.OnDraw();
#endif
        }
    }
}
