using System.Numerics;
using Raylib_cs;

namespace PlatformerGame.Engine.Resources
{
    public class Sprite : Resource
    {
        protected Texture2D _texture;

        public Sprite(string srcPath)
            : this(ResourceType.Sprite, srcPath)
        {
        }

        protected Sprite(ResourceType type, string sourcePath)
            : base(type)
        {
            LoadTexture(sourcePath);
        }

        public int Width => _texture.Width;
        public int Height =>_texture.Height;
        public Vector2 Size => new Vector2(_texture.Width, _texture.Height);
        public bool Exists => _texture.Id != 0;

        public override void Dispose()
        {
            if (_texture.Id != 0)
                Raylib.UnloadTexture(_texture);
        }

        public virtual void Draw(Vector2 position, bool flipX, bool flipY)
        {
            Draw(position, flipX, flipY, Color.White);
        }

        public virtual void Draw(Vector2 position, bool flipX, bool flipY, Color tint)
        {
            var source = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = flipX ? -_texture.Width : _texture.Width,
                Height = flipY ? -_texture.Height : _texture.Height,
            };
            Raylib.DrawTextureRec(_texture, source, position, tint);
        }

        private void LoadTexture(string sourcePath)
        {
            _texture = Raylib.LoadTexture(sourcePath);
            if (_texture.Id == 0)
                throw new NullReferenceException("Failed to load sprite from \"" + sourcePath + "\"");
        }
    }
}