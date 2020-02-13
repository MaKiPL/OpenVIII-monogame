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
        private static void Main()
        {
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

#endif
}