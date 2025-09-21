/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Date Created:   9-19-2025
/// Date Created:   9-21-2025
/// </summary>

using Game.Engine;

namespace Game
{
    internal class Program : Application
    {
        public Program(ApplicationCreateInfo createInfo)
            : base(createInfo)
        {
        }

        public static void Main(string[] args)
        {
            ApplicationCreateInfo createInfo = new ApplicationCreateInfo
            {
                Title = "Platformer Game",
                WindowOptions = Window.DefaultConfigFlags | Raylib_cs.ConfigFlags.ResizableWindow
            };

            Program program = new Program(createInfo);
            program.LoadExtraResources();
            program.Run();
        }
    }
}