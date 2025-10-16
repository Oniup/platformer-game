using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Background : SpriteActor
    {
        private float _moveSpeed = 60.0f;

        private Framebuffer _framebuffer;
        private Scene? _scene;
        private Vector2 _startPosition;
        private Vector2 _endPosition;

        public Background(Sprite sprite, Framebuffer framebuffer, Scene? scene, Vector2 moveDirection, Vector2 position)
            : base(sprite, position)
        {
            _framebuffer = framebuffer;

            // Only apply offset to those moving in a positive direction
            if (moveDirection.X > 0.0f)
                Position -= Vector2.UnitX * Sprite.Width;
            if (moveDirection.Y > 0.0f)
                Position -= Vector2.UnitY * Sprite.Height;

            _startPosition = Position;
            _endPosition = Position + moveDirection * Sprite.Size;

            if (scene != null)
            {
                _scene = scene;
                EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            }
            else
            {
                (int width, int height) = Window.GetResolutionSize(WindowResolution.nHD);
                _framebuffer.Resize(width + Sprite.Width, height + Sprite.Height);
                _framebuffer.DrawOnto(DrawBackgroundToFramebuffer);
            }
        }

        private void OnNewCurrentSceneEvent(Event eventData, object? sender)
        {
            var data = (NewCurrentSceneEvent)eventData;
            if (_scene != data.Scene)
                return;

            _framebuffer.Resize(data.Scene.Width + Sprite.Width, data.Scene.Height + Sprite.Height);
            _framebuffer.DrawOnto(DrawBackgroundToFramebuffer);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            // Used AI for helping me understand how to check if the point as passed a end position
            Vector2 toTargetBefore = _endPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(toTargetBefore);

            Position += moveDirection * _moveSpeed * deltaTime;

            Vector2 toTargetAfter = _endPosition - Position;
            if (Vector2.Dot(toTargetBefore, toTargetAfter) <= 0.0f)
                Position = _startPosition;
        }

        public override void OnDraw()
        {
            _framebuffer.Draw(Position);
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
        }

        private void DrawBackgroundToFramebuffer()
        {
            var pos = Vector2.Zero;
            while (pos.Y < _framebuffer.Height)
            {
                while (pos.X < _framebuffer.Width)
                {
                    Sprite.Draw(pos, false, false);
                    pos.X += Sprite.Width;
                }
                pos.X = 0.0f;
                pos.Y += Sprite.Height;
            }
        }

        public class CreateInfo : CreateInfo<Background>
        {
            private int _lastBackground = -1;

            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                (string, string)[] assets = [
                    ("Background1", "Blue.png"),
                    ("Background2", "Brown.png"),
                    ("Background3", "Gray.png"),
                    ("Background4", "Green.png"),
                    ("Background5", "Pink.png"),
                    ("Background6", "Purple.png"),
                    ("Background7", "Yellow.png"),
                ];
                foreach ((string name, string spriteName) in assets)
                {
                    string path = resources.AssetDirectory + "/Graphics/Background/" + spriteName;
                    resources.Load(name, new Sprite(path));
                }

                resources.Load("Background Framebuffer", new Framebuffer(1, 1));
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                Vector2[] moveDirections = [
                    new Vector2(-1),        // Blue
                    -Vector2.UnitY,         // Brown
                    new Vector2(1),         // Gray
                    -Vector2.UnitX,         // Green
                    new Vector2(0, 1),      // Pink
                    Vector2.UnitX,          // Purple
                    new Vector2(0, -1),     // Yellow
                ];

                // Get random background sprite name
                var random = new Random();
                int id;
                do
                {
                    id = random.Next(1, 7);
                }
                while (id == _lastBackground);
                _lastBackground = id;

                // Make sure to place at the top left of the scene
                Vector2 position = info.Position;
                if (info.Scene != null)
                    position = new Vector2(info.Scene.WorldX, info.Scene.WorldY);

                var sprite = resources.Get<Sprite>($"Background{id}");
                var framebuffer = resources.Get<Framebuffer>("Background Framebuffer");
                return new Background(sprite, framebuffer, info.Scene, moveDirections[id - 1], position);
            }
        }
    }
}