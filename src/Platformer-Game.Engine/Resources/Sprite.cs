/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-25-2025
/// </summary>

using System.Numerics;
using Raylib_cs;

namespace PlatformerGame.Engine
{
    public class Sprite : Resource, IDisposable
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

        ~Sprite()
        {
            Dispose();
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

        public void Dispose()
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

    public class SpriteAtlas : Sprite
    {
        private Rectangle _currentGrid;

        public SpriteAtlas(int gridSize, string srcPath)
            : base(ResourceType.SpriteAtlas, srcPath)
        {
            _currentGrid = new Rectangle
            {
                Width = gridSize,
                Height = gridSize,
                X = 0,
                Y = 0,
            };
        }

        public int GridSize
        {
            get { return (int)_currentGrid.Width; }
        }

        public Point GridPosition
        {
            get { return new Point((int)_currentGrid.X, (int)_currentGrid.Y); }
            set
            {
                _currentGrid.X = value.X;
                _currentGrid.X = value.Y;
            }
        }

        public int GridX
        {
            get { return (int)_currentGrid.X; }
            set { _currentGrid.X = value; }
        }

        public int GridY
        {
            get { return (int)_currentGrid.Y; }
            set { _currentGrid.Y = value; }
        }

        public void SetGrid(int x, int y)
        {
            _currentGrid.X = x;
            _currentGrid.Y = y;
        }

        public void SetGrid(Point pt)
        {
            _currentGrid.X = pt.X;
            _currentGrid.Y = pt.Y;
        }

        public override void Draw(Vector2 pos)
        {
            Draw(pos, Color.White);
        }

        public override void Draw(Vector2 pos, Color tint)
        {
            Raylib.DrawTextureRec(_texture, _currentGrid, pos, tint);
        }
    }
}