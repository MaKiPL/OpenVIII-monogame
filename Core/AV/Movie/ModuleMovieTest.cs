using Microsoft.Xna.Framework.Input;
using OpenVIII.AV;
using OpenVIII.Encoding.Tags;
using OpenVIII.Movie;

namespace OpenVIII
{
    public static class ModuleMovieTest
    {
        private const MODULE DefaultReturnState = MODULE.MAINMENU_DEBUG;
        private static Player _player;

        /// <summary>
        /// Movie file list
        /// </summary>

        public static MODULE ReturnState { get; set; } = DefaultReturnState;
        public static int Index { get; set; }

        public static void Inputs()
        {
            Files files = Files.Instance;
            if (Input2.DelayedButton(FF8TextTagKey.Confirm) || Input2.DelayedButton(FF8TextTagKey.Cancel) || Input2.DelayedButton(Keys.Space))
            {
                Return();
            }
#if DEBUG
            // lets you move through all the fields just holding left or right. it will just loop
            // when it runs out.
            else if (Input2.DelayedButton(FF8TextTagKey.Left))
            {
                Sound.Play(0);
                if (Index > 0)
                    Index--;
                else
                    Index = files.Count - 1;
                Reset();
            }
            else if (Input2.DelayedButton(FF8TextTagKey.Right))
            {
                Sound.Play(0);
                if (Index < files.Count - 1)
                    Index++;
                else
                    Index = 0;
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
            _player = Player.Load(Index);
            if (_player != null)
                _player.StateChanged += Player_StateChanged;
        }

        private static void Player_StateChanged(object sender, State e)
        {
            if (e == State.Return)
                Return();
        }

        public static void Update()
        {
            if (_player == null || _player.IsDisposed)
                Play();
            if (_player == null) // player is still null move on.
                Return();
            else
            {
                _player?.Update();
                Inputs();
            }
        }

        public static void Draw() => _player?.Draw();

        public static void Reset()
        {
            _player = null;
            ReturnState = DefaultReturnState;
        }
    }
}