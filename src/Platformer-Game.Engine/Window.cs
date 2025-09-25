/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-19-2025
/// Last Edited:    9-25-2025
/// </summary>

using System.Diagnostics;
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

    public class Window : IDisposable
    {
        public const ConfigFlags DefaultConfigFlags = ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint;

        private string _title;
        private WindowResolution _resolution;
        private ConfigFlags _flags;

        public Window(string title, WindowResolution resolution = WindowResolution.Auto, ConfigFlags flags = DefaultConfigFlags)
        {
            Debug.Assert(!Raylib.IsWindowReady(), "Cannot have multiple window instances");

            _title = title;
            _flags = flags;
            SetupInternal(resolution);
        }

        ~Window()
        {
            Dispose();
            Console.WriteLine("Destroyed Window");
        }

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

        public ConfigFlags Config
        {
            get { return _flags; }
            set
            {
                if (Raylib.IsWindowState(value))
                    Raylib.ClearWindowState(value);
                else
                    Raylib.SetWindowState(value);
            }
        }

        public bool IsRunning
        {
            get { return !Raylib.WindowShouldClose(); }
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

        private void SetupInternal(WindowResolution resolution)
        {
            Raylib.SetConfigFlags(_flags);
            Raylib.InitWindow(0, 0, _title);
            if (!Raylib.IsWindowReady())
                throw new NullReferenceException("Failed to initialize Raylib's window for some reason");

            SetResolution(resolution, true);
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(Raylib.GetCurrentMonitor());
        }
    }
}