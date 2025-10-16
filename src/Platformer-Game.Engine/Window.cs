using System.Diagnostics;
using PlatformerGame.Engine.Events;
using Raylib_cs;

namespace PlatformerGame.Engine
{
    public enum WindowResolution
    {
        None,
        Auto,
        nHD,
        HD,
        FullHD,
        QHD,
        QHDPlus,
        UHD,
        UHD5K,
        UHD8K,
    }

    /// <summary>
    /// Window options mapping directly their Raylib_cs.ConfigFlags counterpart except the None entry as added
    /// </summary>
    [Flags]
    public enum WindowOptions
    {
        None = 0x00000000,
        VSyncHint = 0x00000040,
        FullscreenMode = 0x00000002,
        BorderlessMode = 0x00008000,
        ManualResizable = 0x00000004,
        Undecorated = 0x00000008,
        Hidden = 0x00000080,
        Minimized = 0x00000200,
        Maximized = 0x00000400,
        Unfocused = 0x00000800,
        Topmost = 0x00001000,
        AlwaysRun = 0x00000100,
        Transparent = 0x00000010,
        HighDpi = 0x00002000,
        MousePassthrough = 0x00004000,
        Msaa4xHint = 0x00000020,
        InterlacedHint = 0x00010000,
    }

    public class Window : IDisposable
    {
        public const WindowOptions DefaultOptions = WindowOptions.None;

        private string _title;
        private WindowResolution _resolution;
        private WindowOptions _options;
        private bool _running;

        public Window(string title, int limitFps, WindowResolution resolution, WindowOptions flags)
        {
            Debug.Assert(!Raylib.IsWindowReady(), "Cannot have multiple window instances");

            _title = title;
            _options = flags;
            _running = true;
            SetupInternal(resolution, limitFps);

            EventDispatcher.AddListener<WindowShouldClose>(this, OnWindowShouldClose);
        }

        public int Width => Raylib.GetScreenWidth();
        public int Height => Raylib.GetScreenHeight();

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Raylib.SetWindowTitle(_title);
            }
        }

        public WindowResolution Resoltuion
        {
            get { return _resolution; }
            set { SetResolution(value); }
        }

        public WindowOptions Options
        {
            get { return _options; }
            set
            {
                Raylib.ClearWindowState((ConfigFlags)_options);
                _options = value;
                Raylib.SetWindowState((ConfigFlags)_options);
            }
        }

        public bool IsRunning
        {
            get
            {
                FireEvents();
                return _running && !Raylib.WindowShouldClose();
            }
        }

        public void Dispose()
        {
            Raylib.CloseWindow();
        }

        public static (int, int) GetResolutionSize(WindowResolution resolution)
        {
            switch (resolution)
            {
                case WindowResolution.nHD:
                    return (640, 360);
                case WindowResolution.HD:
                    return (1280, 720);
                case WindowResolution.FullHD:
                    return (1920, 1080);
                case WindowResolution.QHD:
                    return (2560, 1440);
                case WindowResolution.QHDPlus:
                    return (3200, 1800);
                case WindowResolution.UHD:
                    return (3840, 2160);
                case WindowResolution.UHD5K:
                    return (5120, 2880);
                case WindowResolution.UHD8K:
                    return (7680, 4320);
                default:
                    return (0, 0);
            }
        }

        public static WindowResolution GetClosestResolution(int width, int height)
        {
            WindowResolution[] resolutions = Enum.GetValues<WindowResolution>();

            int closestWidth = int.MaxValue;
            WindowResolution closestResolution = WindowResolution.None;
            foreach (WindowResolution res in resolutions)
            {
                (int resWidth, int _) = GetResolutionSize(res);
                int diff = Math.Abs(width - resWidth);
                int currDiff = Math.Abs(width - closestWidth);

                if (diff < currDiff)
                {
                    closestWidth = resWidth;
                    closestResolution = res;
                }
            }
            return closestResolution;
        }

        public void SetResolution(WindowResolution resolution, bool center = false)
        {
            int monitor = Raylib.GetCurrentMonitor();
            int mWidth = Raylib.GetMonitorWidth(monitor);
            int mHeight = Raylib.GetMonitorHeight(monitor);

            if (resolution == WindowResolution.Auto || resolution == WindowResolution.None)
                resolution = GetClosestResolution(mWidth - mWidth / 3, mHeight - mHeight / 3);
            (int width, int height) = GetResolutionSize(resolution);
            if (width > mWidth)
            {
                Debug.WriteLine("Cannot set resolution size that is bigger than the monitor");
                return;
            }

            _resolution = resolution;
            Raylib.SetWindowSize(width, height);
            if (center)
                Raylib.SetWindowPosition(mWidth / 2 - width / 2, mHeight / 2 - height / 2);
        }

        private void SetupInternal(WindowResolution resolution, int limitFps)
        {
            Raylib.SetConfigFlags((ConfigFlags)_options);
            Raylib.InitWindow(0, 0, _title);
            if (!Raylib.IsWindowReady())
                throw new NullReferenceException("Failed to initialize Raylib's window for some reason");

            SetResolution(resolution, true);
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(limitFps);
        }

        private void FireEvents()
        {
            if (Raylib.IsWindowResized())
                EventDispatcher.FireEvent(new WindowResizeEvent(Width, Height, _resolution), this);
        }

        private void OnWindowShouldClose(Event eventData, object? sender)
        {
            _running = false;
        }
    }
}