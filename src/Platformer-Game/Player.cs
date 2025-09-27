using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using Raylib_cs;

namespace PlatformerGame
{
    public class Player : SpriteActor
    {
        private float _moveSpeed;
        private Vector2 _direction;

        public Player(Sprite sprite, int id, Vector2 position, bool active = true)
            : base(sprite, id, position, active)
        {
            _moveSpeed = 100.0f;
            _direction = Vector2.Zero;
        }

        public override void OnUpdate(float deltaTime)
        {
            _direction = Vector2.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                _direction.X -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                _direction.X += 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.W))
                _direction.Y -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                _direction.Y += 1.0f;

            if (_direction != Vector2.Zero)
                Position += Vector2.Normalize(_direction) * _moveSpeed * deltaTime;
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override Actor Create(ResourceManager resources, LDtkDefinition.Entity def, Vector2 position)
            {
                Sprite sprite = resources.Get<Sprite>(def.TilesetId);
                return new Player(sprite, def.UId, position);
            }
        }
    }
}