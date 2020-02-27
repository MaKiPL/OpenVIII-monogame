using System;

namespace OpenVIII.OpenGL
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (Game1 game = new Game1())
                game.Run();
        }

        #endregion Methods
    }
}