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

        // Protected as sprite atlas needs to set the ResouceType
        protected Sprite(ResourceType type, string srcPath)
            : base(type)
        {
            LoadTexture(srcPath);
        }

        public int Width
        {
            get { return _texture.Width; }
        }

        public int Height
        {
            get { return _texture.Height; }
        }

        public bool Exists
        {
            get { return _texture.Id != 0; }
        }

        public override void Dispose()
        {
            if (_texture.Id != 0)
                Raylib.UnloadTexture(_texture);
        }

        public virtual void Draw(Vector2 pos)
        {
            Draw(pos, Color.White);
        }

        public virtual void Draw(Vector2 pos, Color tint)
        {
            Raylib.DrawTextureV(_texture, pos, tint);
        }

        private void LoadTexture(string srcPath)
        {
            _texture = Raylib.LoadTexture(srcPath);
            if (_texture.Id == 0)
                throw new NullReferenceException("Failed to load sprite from \"" + srcPath + "\"");
        }
    }
}