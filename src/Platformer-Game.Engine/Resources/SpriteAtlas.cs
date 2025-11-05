using Raylib_cs;
using System.Numerics;

namespace PlatformerGame.Engine.Resources
{
    public class SpriteAtlas : Sprite
    {
        private Rectangle _currentGrid;

        public SpriteAtlas(int gridSize, string sourcePath)
            : base(ResourceType.SpriteAtlas, sourcePath)
        {
            _currentGrid = new Rectangle
            {
                Width = gridSize,
                Height = gridSize,
                X = 0,
                Y = 0,
            };
        }

        public int GridWidth
        {
            get { return (int)_currentGrid.Width; }
            set { _currentGrid.Width = value; }
        }

        public int GridHeight
        {
            get { return (int)_currentGrid.Height; }
            set { _currentGrid.Height = value; }
        }

        public Vector2 GridPosition
        {
            get { return _currentGrid.Position; }
            set { _currentGrid.Position = value; }
        }

        public Vector2 GridSize
        {
            get { return _currentGrid.Size; }
            set { _currentGrid.Size = value; }
        }

        public float GridX
        {
            get { return _currentGrid.X; }
            set { _currentGrid.X = value; }
        }

        public float GridY
        {
            get { return _currentGrid.Y; }
            set { _currentGrid.Y = value; }
        }

        public void SetGrid(int x, int y)
        {
            _currentGrid.X = x;
            _currentGrid.Y = y;
        }

        public void SetGrid(Vector2 position)
        {
            _currentGrid.X = position.X;
            _currentGrid.Y = position.Y;
        }

        public override void Draw(Vector2 position, bool flipX, bool flipY)
        {
            Draw(position, flipX, flipY, Color.White);
        }

        public override void Draw(Vector2 position, bool flipX, bool flipY, Color tint)
        {
            var source = new Rectangle
            {
                X = _currentGrid.X,
                Y = _currentGrid.Y,
                Width = flipX ? -_currentGrid.Width : _currentGrid.Width,
                Height = flipY ? -_currentGrid.Height : _currentGrid.Height,
            };
            Raylib.DrawTextureRec(_texture, source, position, tint);
        }
    }
}