using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Background : SpriteActor
    {
        Framebuffer _framebuffer;

        public Background(Sprite sprite, int width, int height, Vector2 position)
            : base(sprite, position)
        {
            _framebuffer = new Framebuffer(width, height);

            // Make a dynamic animation
            _framebuffer.DrawTo(() =>
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

        public override void OnDestroy()
        {
            _framebuffer.Dispose();
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
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                if (scene == null)
                    throw new NullReferenceException("A scene is required for creating a background Actor");

                // Get random background sprite name
                Random random = new Random();
                int rand = random.Next(1, 7);
                string spriteName = "Background" + rand;

                // Make sure to place at the top left of the scene
                position = new Vector2(scene.WorldX, scene.WorldY);

                Sprite sprite = resources.Get<Sprite>(spriteName);
                return new Background(sprite, scene.Width, scene.Height, position);
            }
        }
    }
}