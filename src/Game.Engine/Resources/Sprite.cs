/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-22-2025
/// </summary>

using System.Numerics;
using Raylib_cs;

namespace Game.Engine
{
    public class Sprite : Resource, IDisposable
    {
        protected Texture2D _texture;

        public Sprite(int id, string srcPath)
            : this(ResourceType.Sprite, id, srcPath)
        {
        }

        public Sprite(string name, string srcPath)
            : this(ResourceType.Sprite, name, srcPath)
        {
        }

        // Protected as sprite atlas needs to set the ResouceType
        protected Sprite(ResourceType type, int id, string srcPath)
            : base(id, type)
        {
            LoadTexture(srcPath);
        }

        protected Sprite(ResourceType type, string name, string srcPath)
            : base(name, type)
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

        public SpriteAtlas(int id, int gridSize, string srcPath)
            : base(ResourceType.SpriteAtlas, id, srcPath)
        {
            _currentGrid = new Rectangle
            {
                Width = gridSize,
                Height = gridSize,
                X = 0,
                Y = 0,
            };
        }

        public SpriteAtlas(string name, int gridSize, string srcPath)
            : base(ResourceType.SpriteAtlas, name, srcPath)
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