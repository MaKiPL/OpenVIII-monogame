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
            using (var game = new Game1())
            {
                game.Arguments = args;
                game.Run();
            }
        }
    }

#endif
}