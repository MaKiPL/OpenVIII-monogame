using System;
using System.Diagnostics;

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
            start:
            try
            {
                using (Game1 game = new Game1())
                    game.Run();
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine(e);
                goto start;
            }
        }
    }

#endif
}