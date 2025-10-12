using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Background : SpriteActor
    {
        Framebuffer _framebuffer;
        Scene _scene;

        public Background(Sprite sprite, Framebuffer framebuffer, Scene scene, Vector2 position)
            : base(sprite, position)
        {
            _framebuffer = framebuffer;
            _scene = scene;

            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
        }

        private void OnNewCurrentSceneEvent(Event eventData, object? sender)
        {
            NewCurrentSceneEvent data = (NewCurrentSceneEvent)eventData;
            if (_scene != data.Scene)
                return;

            _framebuffer.Resize(data.Scene.Width, data.Scene.Height);
            _framebuffer.DrawOnto(() =>
            {
                Vector2 pos = Vector2.Zero;
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
            });
        }

        public override void OnDraw()
        {
            _framebuffer.Draw(Position);
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
        }

        public class CreateInfo : CreateInfo<Background>
        {
            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
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
                if (info.Scene == null)
                    throw new NullReferenceException("A scene is required for creating a background Actor");

                // Get random background sprite name
                Random random = new Random();
                int rand = random.Next(1, 7);
                string spriteName = "Background" + rand;

                // Make sure to place at the top left of the scene
                Vector2 position = new Vector2(info.Scene.WorldX, info.Scene.WorldY);

                Sprite sprite = resources.Get<Sprite>(spriteName);
                Framebuffer framebuffer = resources.Get<Framebuffer>("Background Framebuffer");
                return new Background(sprite, framebuffer, info.Scene!, position);
            }
        }
    }
}