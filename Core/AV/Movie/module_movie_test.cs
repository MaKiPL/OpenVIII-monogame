using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;

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
            else if (Input2.DelayedButton(FF8TextTagKey.Left))
            {
                AV.Sound.Play(0);
                if (Module_movie_test.Index > 0)
                    Module_movie_test.Index--;
                else
                    Module_movie_test.Index = Movie.Files.Count - 1;
                Reset();
            }
            else if (Input2.DelayedButton(FF8TextTagKey.Right))
            {
                AV.Sound.Play(0);
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
            if (Player != null)
                Player.StateChanged += Player_StateChanged;
        }

        private static void Player_StateChanged(object sender, Movie.STATE e)
        {
            if (e == Movie.STATE.RETURN)
                Return();
        }

        public static void Update()
        {
            if (Player == null || Player.IsDisposed)
                Play();
            if (Player == null) // player is still null move on.
                Return();
            else
            {
                Player?.Update();
                Inputs();
            }
        }

        public static void Draw() => Player?.Draw();

        public static void Reset()
        {
            Player = null;
            ReturnState = defaultReturnState;
        }
    }
}