using System.Numerics;

namespace PlatformerGame.Engine.Events
{
    public class WindowResizeEvent : Event
    {
        public int Width { get; }
        public int Height { get; }
        public WindowResolution Resolution { get; init; }

        public WindowResizeEvent(int width, int height, WindowResolution resolution)
        {
            Width = width;
            Height = height;
            Resolution = resolution;
        }
    }

    public class WindowShouldClose : Event
    {
        public WindowShouldClose()
        {
        }
    }

    public class WindowMovePositionEvent : Event
    {
        public Vector2 LastPosition { get; }
        public Vector2 NewPosition { get; }

        public WindowMovePositionEvent(Vector2 lastPos, Vector2 newPos)
        {
            LastPosition = lastPos;
            NewPosition = newPos;
        }
    }
}