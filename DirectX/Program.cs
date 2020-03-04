using System;

namespace OpenVIII.DirectX
{
#if WINDOWS

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Arguments = args;
                game.Run();
            }
        }
    }

#endif
}