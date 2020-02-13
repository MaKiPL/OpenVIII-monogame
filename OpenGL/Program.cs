using System;

namespace OpenVIII.OpenGL
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
        start:
            try
            {
                using (Game1 game = new Game1())
                    game.Run();
            }
            catch (InvalidOperationException e)
            {
                goto start;
            }
        }
    }
}