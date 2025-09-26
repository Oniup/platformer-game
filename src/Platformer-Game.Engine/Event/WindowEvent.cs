namespace PlatformerGame.Engine.Event
{
    public class WindowResizeEvent : IEvent
    {
        public WindowResizeEvent(int width, int height, WindowResolution resolution)
        {
            Width = width;
            Height = height;
            Resolution = resolution;
        }

        public int Width { get; init; }
        public int Height { get; init; }
        public WindowResolution Resolution { get; init; }
    }
}