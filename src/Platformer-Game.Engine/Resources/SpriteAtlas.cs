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