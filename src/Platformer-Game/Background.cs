using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Background : SpriteActor
    {
        private int _rows;
        private int _columns;

        public Background(Sprite sprite, int width, int height, int id, Vector2 position, bool active = true)
            : base(sprite, id, position, active)
        {
            width -= sprite.Width;
            height -= sprite.Height;
            _rows = width / sprite.Width;
            _columns = height / sprite.Height;

            Position += new Vector2(sprite.Width, sprite.Height) * 0.5f;
        }

        public override void OnDraw()
        {
            for (int i = 0; i < _columns; ++i)
            {
                for (int j = 0; j < _rows; ++j)
                    Sprite.Draw(Position + new Vector2(j * Sprite.Width, i * Sprite.Height));
            }
        }

        public class CreateInfo : CreateInfo<Background>
        {
            public override void SetupRequiredResources(ResourceManager resources)
            {
                Console.WriteLine("Loaded resources for background");
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
                    string path = resources.GetAssetPath("Graphics/Background/" + spriteName);
                    resources.Load(name, new Sprite(path));
                }
            }

            public override Actor Create(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
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
                return new Background(sprite, scene.Width, scene.Height, ActorTypeId, position);
            }
        }
    }
}