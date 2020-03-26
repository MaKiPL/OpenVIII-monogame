using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public class MenuModule : Menu
    {
        #region Fields

        private static Mode state = 0;

        #endregion Fields

        #region Events

        public static event EventHandler<Mode> MainMenuStateChangedEvent;

        #endregion Events

        #region Enums

        protected override void Init()
        {
            Memory.ModuleChangeEvent += ModuleChanged;
            MainMenuStateChangedEvent += MainMenuStateChanged;
            base.Init();
        }

        private void MainMenuStateChanged(object sender, Mode e) => Refresh();

        private void ModuleChanged(object sender, MODULE e)
        {
            if(e == MODULE.MAINMENU_DEBUG)
            {
                Refresh();
            }
        }

        /// <summary>
        /// What state the menus are in.
        /// </summary>
        public enum Mode
        {
            //Init,
            MainLobby,

            DebugScreen,
            NewGameChoosed,
            LoadGameChooseSlot,
            SaveGameChooseSlot,
            LoadGameChooseGame,
            SaveGameChooseGame,
            LoadGameCheckingSlot,
            SaveGameCheckingSlot,
            LoadGameLoading,
            SaveGameSaving,
            IGM,
            IGM_Junction,
            IGM_Items,
            //BattleMenu
        }

        #endregion Enums

        #region Properties

        public Mode State
        {
            get => state; set
            {
                // Draw will call before the next update(). This prevents that.
                Memory.SuppressDraw = true;
                if (state != value)
                {
                    state = value;
                    MainMenuStateChangedEvent?.Invoke(null, value);
                }
            }
        }

        #endregion Properties

        #region Methods

        public static MenuModule Create() => Create<MenuModule>();

        public override void Draw()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            switch (State)
            {
                case Mode.MainLobby:
                    IGM_Lobby.Draw();
                    break;

                case Mode.DebugScreen:
                    Debug_Menu.Draw();
                    break;

                case Mode.LoadGameChooseSlot:
                case Mode.LoadGameCheckingSlot:
                case Mode.LoadGameChooseGame:
                case Mode.LoadGameLoading:
                case Mode.SaveGameChooseSlot:
                case Mode.SaveGameCheckingSlot:
                case Mode.SaveGameChooseGame:
                case Mode.SaveGameSaving:
                    IGM_LGSG.Draw();
                    break;

                case Mode.IGM:
                    IGM.Draw();
                    break;

                case Mode.IGM_Junction:
                    IGM_Junction.Draw();
                    break;

                case Mode.IGM_Items:
                    IGM_Items.Draw();
                    break;

                case Mode.NewGameChoosed:
                    goto case Mode.MainLobby;

                default:
                    State = Mode.MainLobby;
                    goto case Mode.MainLobby;
            }
            base.Draw();
        }

        public override void Refresh()
        {

            switch (State)
            {
                case Mode.MainLobby:
                    IGM_Lobby.Refresh();
                    break;

                case Mode.DebugScreen:
                    Debug_Menu.Refresh();
                    break;

                case Mode.LoadGameChooseSlot:
                case Mode.LoadGameCheckingSlot:
                case Mode.LoadGameChooseGame:
                case Mode.LoadGameLoading:
                case Mode.SaveGameChooseSlot:
                case Mode.SaveGameCheckingSlot:
                case Mode.SaveGameChooseGame:
                case Mode.SaveGameSaving:
                    IGM_LGSG.Refresh();
                    break;

                case Mode.IGM:
                    IGM.Refresh();
                    break;

                case Mode.IGM_Junction:
                    IGM_Junction.Refresh();
                    break;

                case Mode.IGM_Items:
                    IGM_Items.Refresh();
                    break;

               // case Mode.NewGameChoosed:
               //     goto case Mode.MainLobby;

                default:
                    State = Mode.MainLobby;
                    goto case Mode.MainLobby;
            }
            base.Refresh();
        }

        public override bool Update()
        {
            switch (State)
            {
                case Mode.NewGameChoosed:
                case Mode.LoadGameCheckingSlot:
                case Mode.LoadGameLoading:
                case Mode.SaveGameCheckingSlot:
                case Mode.SaveGameSaving:
                    Memory.IsMouseVisible = false;
                    break;

                default:
                    Memory.IsMouseVisible = true;
                    break;
            }
            var forceupdate = false;
            switch (State)
            {
                case Mode.MainLobby:
                    forceupdate = IGM_Lobby.Update();
                    break;

                case Mode.DebugScreen:
                    forceupdate = Debug_Menu.Update();
                    break;

                case Mode.LoadGameChooseSlot:
                case Mode.LoadGameCheckingSlot:
                case Mode.LoadGameChooseGame:
                case Mode.LoadGameLoading:
                case Mode.SaveGameChooseSlot:
                case Mode.SaveGameCheckingSlot:
                case Mode.SaveGameChooseGame:
                case Mode.SaveGameSaving:
                    forceupdate = IGM_LGSG.Update();
                    break;

                case Mode.IGM:
                    forceupdate = IGM.Update();
                    break;

                case Mode.IGM_Junction:
                    forceupdate = IGM_Junction.Update();
                    break;

                case Mode.IGM_Items:
                    forceupdate = IGM_Items.Update();
                    break;

                case Mode.NewGameChoosed:
                    goto case Mode.MainLobby;

                default:
                    State = Mode.MainLobby;
                    goto case Mode.MainLobby;
            }
            SkipFocus = true;
            forceupdate = base.Update() || forceupdate;
            //if (!forceupdate)
            //    Memory.SuppressDraw = false;
            return forceupdate;
        }

        #endregion Methods
    }
}