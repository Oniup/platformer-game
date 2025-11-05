using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public abstract class ProjectileActor : CollidableActor
    {
        private SpriteAtlas _atlas;
        private Vector2 _gridPosition;

        public Vector2 Direction { get; set; }
        public float Speed { get; set; }

        public ProjectileActor(SpriteAtlas atlas, Vector2 gridPosition, float moveSpeed, CollisionLayer mask, Vector2 position)
            : base(CollisionLayer.Projectile, mask, position)
        {
            _atlas = atlas;
            _gridPosition = gridPosition;
            Speed = moveSpeed;

            AddCircleCollider(Vector2.Zero, _atlas.GridWidth / 4, OnTriggerEnter);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!World.Paused)
                Position += Direction * (Speed * deltaTime);
        }

        public override void OnDraw()
        {
            _atlas.SetGrid(_gridPosition);
            _atlas.Draw(Position - _atlas.GridSize * 0.5f, false, false);

#if DEBUG
            base.OnDraw();
#endif
        }

        protected abstract void OnTriggerEnter(CollidableActor actor, ShapeCollider collider);
    }
}
