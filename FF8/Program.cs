using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FF8.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.OpenAL;
using MonoGame.Utilities;

namespace FF8
{
    /// <summary>
    /// The main class.
    /// </summary>
    internal static class Program
    {
        internal static Game1 Game { get; private set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MonoGameHooks.Initialize();

            using (Game = new Game1())
                Game.Run();
        }
    }
}