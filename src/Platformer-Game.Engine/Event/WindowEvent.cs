namespace PlatformerGame.Engine.Events
{
    public class WindowResizeEvent : Event
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