using System.Numerics;
using PlatformerGame.Engine.Events;
using Raylib_cs;

namespace PlatformerGame.Engine.Resources
{
    public class MainFramebuffer : Resource
    {
        private RenderTexture2D _framebuffer;
        private Rectangle _sourceDestination;
        private float _virtualRatio;

        public MainFramebuffer(Window window)
            : base(ResourceType.MainFramebuffer)
        {
            EventDispatcher.AddListener<WindowResizeEvent>(this, OnWindowResizeEvent);

            (int width, int height) = Window.GetResolutionSize(WindowResolution.nHD);
            _framebuffer = Raylib.LoadRenderTexture(width, height);

            SetSourceDestinationSize(window.Width, window.Height);
            CameraPosition = Vector2.Zero;
        }

        public Vector2 CameraPosition { get; set; }

        public int FramebufferWidth
        {
            get { return _framebuffer.Texture.Width; }
        }

        public int FramebufferHeight
        {
            get { return _framebuffer.Texture.Height; }
        }

        public Camera2D GetWorldCamera()
        {
            return new Camera2D()
            {
                Target = Vector2.Truncate(CameraPosition),
                Zoom = 1.0f,
            };
        }

        public Camera2D GetSmoothCamera(Camera2D worldCamera)
        {
            return new Camera2D()
            {
                Target = (CameraPosition - worldCamera.Target) * _virtualRatio,
                Zoom = 1.0f,
            };
        }

        public void Draw(Camera2D worldCamera, Color clearColor, Action lambda)
        {
            Raylib.BeginTextureMode(_framebuffer);
            Raylib.BeginMode2D(worldCamera);
            {
                Raylib.ClearBackground(clearColor);
                lambda();
            }
            Raylib.EndMode2D();
            Raylib.EndTextureMode();
        }

        public void DrawFramebufferTexture()
        {
            Rectangle source = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = FramebufferWidth,
                Height = -FramebufferHeight
            };
            Raylib.DrawTexturePro(_framebuffer.Texture, source, _sourceDestination, Vector2.Zero, 0.0f, Color.White);
        }

        public override void Dispose()
        {
            if (_framebuffer.Id != 0)
                Raylib.UnloadRenderTexture(_framebuffer);
        }

        private void SetSourceDestinationSize(int winWidth, int winHeight)
        {
            _virtualRatio = winWidth / FramebufferWidth;
            _sourceDestination = new Rectangle
            {
                X = -_virtualRatio,
                Y = -_virtualRatio,
                Width = winWidth + (_virtualRatio * 2),
                Height = winHeight + (_virtualRatio * 2)
            };
        }

        private void OnWindowResizeEvent(Event data, object? sender)
        {
            WindowResizeEvent resizeData = (WindowResizeEvent)data;
            SetSourceDestinationSize(resizeData.Width, resizeData.Height);
        }
    }

    public class Framebuffer : IDisposable
    {
        private RenderTexture2D _framebuffer;

        public Framebuffer(int width, int height)
        {
            _framebuffer = Raylib.LoadRenderTexture(width, height);
        }

        public int Width
        {
            get { return _framebuffer.Texture.Width; }
        }

        public int Height
        {
            get { return _framebuffer.Texture.Height; }
        }

        public void DrawTo(Action lambda)
        {
            Raylib.BeginTextureMode(_framebuffer);
            {
                Raylib.ClearBackground(Color.White);
                lambda();
            }
            Raylib.EndTextureMode();
        }

        public void Draw(Vector2 position)
        {
            Draw(position, Color.White);
        }

        public void Draw(Vector2 position, Color tint)
        {
            Raylib.DrawTextureV(_framebuffer.Texture, position, tint);
        }

        public void Dispose()
        {
            if (_framebuffer.Id != 0)
                Raylib.UnloadRenderTexture(_framebuffer);
        }
    }
}