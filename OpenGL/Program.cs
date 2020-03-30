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
        private static void Main(string[] args)
        {
            using (var game = new Game1())
            {
                game.Arguments = args;
                game.Run();
            }
        }

        #endregion Methods
    }
}