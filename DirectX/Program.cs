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
        static void Main()
        {
            using (var game = new OpenVIII.Game1())
                game.Run();
        }
    }
#endif
}
