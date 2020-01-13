using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System.Linq;

namespace OpenVIII
{

    public static class Module_movie_test
    {
        private const MODULE defaultReturnState = MODULE.MAINMENU_DEBUG;
        private static Movie.Player Player;

        /// <summary>
        /// Movie file list
        /// </summary>

        public static MODULE ReturnState { get; set; } = defaultReturnState;
        public static int Index { get; set; }

        public static void Inputs()
        {
            if (Input2.DelayedButton(FF8TextTagKey.Confirm) || Input2.DelayedButton(FF8TextTagKey.Cancel) || Input2.DelayedButton(Keys.Space))
            {
                Return();
            }
#if DEBUG
            // lets you move through all the feilds just holding left or right. it will just loop
            // when it runs out.
            if (Input2.DelayedButton(FF8TextTagKey.Left))
            {
                init_debugger_Audio.PlaySound(0);
                if (Module_movie_test.Index > 0)
                    Module_movie_test.Index--;
                else
                    Module_movie_test.Index = Movie.Files.Count - 1;
                Reset();
            }
            if (Input2.DelayedButton(FF8TextTagKey.Right))
            {
                init_debugger_Audio.PlaySound(0);
                if (Module_movie_test.Index < Movie.Files.Count - 1)
                    Module_movie_test.Index++;
                else
                    Module_movie_test.Index = 0;
                Reset();
            }
#endif
        }

        private static void Return()
        {
            Memory.Module = ReturnState;
            Reset();
        }

        public static void Play()
        {
            Player = Movie.Player.Load(Index);
            Player.StateChanged += Player_StateChanged;
        }

        private static void Player_StateChanged(object sender, Movie.STATE e)
        {
            if (e == Movie.STATE.RETURN)
                Return();
        }

        public static void Update()
        {
            Inputs();
            if (Player == null || Player.IsDisposed)
                Play();
            Player.Update();
        }

        public static void Draw() => Player.Draw();

        public static void Reset()
        {
            Player = null;
            ReturnState= defaultReturnState;
        }
    }
}