using Raylib_cs;
using System.Numerics;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Resources
{
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

        public int GridWidth
        {
            get { return (int)_currentGrid.Width; }
        }

        public int GridHeight
        {
            get { return (int)_currentGrid.Height; }
        }

        public Point GridPosition
        {
            get { return new Point((int)_currentGrid.X, (int)_currentGrid.Y); }
            set
            {
                _currentGrid.X = value.X;
                _currentGrid.Y = value.Y;
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

        public void SetGrid(Point point)
        {
            _currentGrid.X = point.X;
            _currentGrid.Y = point.Y;
        }

        public override void Draw(Vector2 position, bool flipX, bool flipY)
        {
            Draw(position, flipX, flipY, Color.White);
        }

        public override void Draw(Vector2 position, bool flipX, bool flipY, Color tint)
        {
            Rectangle source = new()
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