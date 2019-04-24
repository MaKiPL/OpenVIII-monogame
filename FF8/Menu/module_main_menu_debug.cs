using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static float fade;

        //        case 1:
        //            filename = aw.GetListOfFiles().First(x => x.ToLower().Contains("start01"));
        //            break;
        //    }
        //    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, filename));
        //    return TextureHandler.UseBest(tex,TextureHandler.LoadPNG(filename));
        //}
        private static bool LastActive = false;

        //private static float fScaleHeight;
        //private static float fScaleWidth;
        private static float lastfade;

        private static MainMenuStates State = 0;
        private static int vpHeight;
        private static int vpWidth;

        #endregion Fields

        #region Enums

        /// <summary>
        /// What state the menus are in.
        /// </summary>
        private enum MainMenuStates
        {
            Init,
            MainLobby,
            NewGameChoosed,
            LoadGameLoading,
            LoadGameChooseSlot,
            LoadGameChooseGame,
            DebugScreen,
            LoadGameCheckingSlot
        }

        #endregion Enums

        #region Properties

        public static float vpSpace { get; private set; }
        private static float Fade { get => fade; set => fade = value; }

        private static Dictionary<Enum, Item> strLoadScreen { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Trigger required draw function.
        /// </summary>
        internal static void Draw()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            //fScaleWidth = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
            //fScaleHeight = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;
            vpWidth = Memory.PreferredViewportWidth;//Memory.graphics.GraphicsDevice.Viewport.Width;
            vpHeight = Memory.PreferredViewportHeight;//Memory.graphics.GraphicsDevice.Viewport.Width;
            lastfade = fade;
            vpSpace = vpHeight * 0.09f * Memory.Scale().X;
            DFontPos = new Vector2(vpWidth * .10f * Memory.Scale().X, vpHeight * .05f * Memory.Scale().Y) + Offset;
            switch (State)
            {
                case MainMenuStates.Init:
                case MainMenuStates.MainLobby:
                    DrawMainLobby();
                    break;

                case MainMenuStates.DebugScreen:
                    DrawDebugLobby();
                    break;

                case MainMenuStates.NewGameChoosed:
                    DrawMainLobby();
                    break;

                case MainMenuStates.LoadGameChooseSlot:
                    DrawLGChooseSlot();
                    break;

                case MainMenuStates.LoadGameCheckingSlot:
                    DrawLoadingSlot();
                    break;

                case MainMenuStates.LoadGameChooseGame:
                    DrawLGChooseGame();
                    break;

                case MainMenuStates.LoadGameLoading:
                    DrawLoadingGame();
                    break;
            }
        }

        /// <summary>
        /// Triggers functions depending on state
        /// </summary>
        internal static void Update()
        {
            bool forceupdate = false;
            if (LastActive != Memory.IsActive)
            {
                forceupdate = true;
                LastActive = Memory.IsActive;
            }
            if (Fade < 1.0f && State != MainMenuStates.NewGameChoosed)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }

            switch (State)
            {
                case MainMenuStates.Init:
                    State++;
                    Init();
                    break;

                case MainMenuStates.MainLobby:
                    Memory.IsMouseVisible = true;
                    Offset = new Vector2(-1000, 0);
                    if (!UpdateMainLobby() && (lastfade == fade) && !forceupdate)
                    {
                        Memory.SuppressDraw = true;
                    }

                    break;

                case MainMenuStates.DebugScreen:
                    Memory.IsMouseVisible = true;
                    if (Offset != Vector2.Zero)
                    {
                        Offset = Vector2.SmoothStep(Offset, Vector2.Zero, .15f);
                    }
                    if (!UpdateDebugLobby() && (lastfade == fade) && Offset == Vector2.Zero && !forceupdate)
                    {
                        Memory.SuppressDraw = true;
                    }

                    break;

                case MainMenuStates.NewGameChoosed:
                    Memory.IsMouseVisible = false;
                    UpdateNewGame();
                    break;

                case MainMenuStates.LoadGameChooseSlot:
                    UpdateLGChooseSlot();
                    Memory.IsMouseVisible = true;
                    break;

                case MainMenuStates.LoadGameCheckingSlot:
                    UpdateLoadingSlot();
                    Memory.IsMouseVisible = false;
                    break;

                case MainMenuStates.LoadGameChooseGame:
                    UpdateLGChooseGame();
                    Memory.IsMouseVisible = true;
                    break;

                case MainMenuStates.LoadGameLoading:
                    UpdateLoading();
                    Memory.IsMouseVisible = false;
                    break;

                default:
                    goto case 0;
            }
        }

        /// <summary>
        /// Init
        /// </summary>
        private static void Init()
        {
            InitMain();
            InitLoad();
            InitDebug();
            Memory.Strings.Close();
        }

        #endregion Methods
    }
}