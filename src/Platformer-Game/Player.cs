using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using Raylib_cs;

namespace PlatformerGame
{
    public class Player : CharacterActor
    {
        private float _moveSpeed;
        private Vector2 _direction;

        private string[] _animNames;
        private int _currAnim;

        public Player(SpriteAtlas sprite, int id, Vector2 position, bool active = true)
            : base(sprite, id, position, active)
        {
            _moveSpeed = 100.0f;
            _direction = Vector2.Zero;

            _animNames = [
                "Double Jump",
                "Fall",
                "Hit",
                "Idle",
                "Jump",
                "Running",
                "Wall Slide",
            ];
            _currAnim = 3;

            AddAnimation("Double Jump", 0, 6);
            AddAnimation("Fall", 1, 1);
            AddAnimation("Hit", 2, 7);
            AddAnimation("Idle", 3, 11);
            AddAnimation("Jump", 4, 1);
            AddAnimation("Running", 5, 12);
            AddAnimation("Wall Slide", 6, 5);

            PlayAnimation(_animNames[_currAnim]);
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

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                _currAnim = ++_currAnim % _animNames.Count();
                PlayAnimation(_animNames[_currAnim]);
            }

            base.OnUpdate(deltaTime);
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>(def!.TilesetId);
                return new Player(sprite, def.UId, position);
            }
        }
    }
}