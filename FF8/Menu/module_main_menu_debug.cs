using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static float fade, lastfade;

        private static bool LastActive = false;

        private static MainMenuStates State = 0;
        private static int vpHeight, vpWidth;
        private static bool blinkstate;

        #endregion Fields

        #region Enums

        /// <summary>
        /// What state the menus are in.
        /// </summary>
        private enum MainMenuStates
        {
            //Init,
            MainLobby,
            NewGameChoosed,
            LoadGameLoading,
            LoadGameChooseSlot,
            LoadGameChooseGame,
            DebugScreen,
            LoadGameCheckingSlot,
            SaveGameChooseSlot,
            SaveGameCheckingSlot,
            SaveGameChooseGame,
            SaveGameSaving,
            InGameMenu
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
            lastfade = fade;
            vpSpace = vpHeight * 0.09f * Memory.Scale().X;
            DFontPos = new Vector2(vpWidth * .10f * Memory.Scale().X, vpHeight * .05f * Memory.Scale().Y) + Offset;
            switch (State)
            {
                //case MainMenuStates.Init:
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
                    DrawLGCheckSlot();
                    break;

                case MainMenuStates.LoadGameChooseGame:
                    DrawLGChooseGame();
                    break;

                case MainMenuStates.LoadGameLoading:
                    DrawLGdata();
                    break;

                case MainMenuStates.SaveGameChooseSlot:
                    DrawSGChooseSlot();
                    break;

                case MainMenuStates.SaveGameCheckingSlot:
                    DrawSGCheckSlot();
                    break;

                case MainMenuStates.SaveGameChooseGame:
                    DrawSGChooseGame();
                    break;

                case MainMenuStates.SaveGameSaving:
                    DrawSGdata();
                    break;
                case MainMenuStates.InGameMenu:
                    DrawInGameMenu();
                    break;
            }
        }

        /// <summary>
        /// Triggers functions depending on state
        /// </summary>
        internal static void Update()
        {
            if (blinkstate)
                blink += Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f * 3;
            else
                blink -= Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f * 3;
            lastscale = scale;
            scale = Memory.Scale();
            bool forceupdate = false;
            Memory.SuppressDraw = true;
            if (LastActive != Memory.IsActive)
            {
                forceupdate = true;
                LastActive = Memory.IsActive;
            }
            if (lastscale != scale)
            {
                forceupdate = true;
            }
            if (Fade < 1.0f && State != MainMenuStates.NewGameChoosed)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }

            vpWidth = Memory.PreferredViewportWidth;//Memory.graphics.GraphicsDevice.Viewport.Width;
            vpHeight = Memory.PreferredViewportHeight;//Memory.graphics.GraphicsDevice.Viewport.Width;

            switch (State)
            {
                //case MainMenuStates.Init:
                //    State++;
                //    Init();
                //    break;

                case MainMenuStates.MainLobby:
                    Memory.IsMouseVisible = true;
                    Offset = new Vector2(-1000, 0);
                    if (UpdateMainLobby() || (lastfade != fade))
                    {
                        forceupdate = true;
                    }

                    break;

                case MainMenuStates.DebugScreen:
                    Memory.IsMouseVisible = true;
                    if (Offset != Vector2.Zero)
                    {
                        Offset = Vector2.SmoothStep(Offset, Vector2.Zero, .15f);
                    }
                    if (UpdateDebugLobby() || (lastfade != fade) || Offset != Vector2.Zero)
                    {
                        forceupdate = true;
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
                    UpdateLoadingData();
                    Memory.IsMouseVisible = false;
                    break;

                case MainMenuStates.SaveGameChooseSlot:
                    UpdateSGChooseSlot();
                    break;

                case MainMenuStates.SaveGameCheckingSlot:
                    UpdateSGCheckSlot();
                    break;

                case MainMenuStates.SaveGameChooseGame:
                    UpdateSGChooseGame();
                    break;

                case MainMenuStates.SaveGameSaving:
                    UpdateSGdata();
                    break;
                case MainMenuStates.InGameMenu:
                    Memory.IsMouseVisible = true;
                    UpdateInGameMenu();
                    break;

                default:
                    goto case 0;
            }
            //disabled because if you resize the window the next update call undoes this before drawing happens.
            //need a way to detect if drawing has happened before suppressing draw again.
            //if(forceupdate)
                Memory.SuppressDraw = false;

        }

        /// <summary>
        /// Init
        /// </summary>
        public static void Init()
        {
            InitMain();
            InitLoad();
            InitDebug();
            Init_InGameMenu();
            Memory.Strings.Close();
        }

        #endregion Methods
    }
}