using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Lobby
    {
        #region Classes

        private class IGMData_Selections : IGMData.Base
        {
            #region Fields

            private bool eventset = false;
            private Dictionary<int, Action> FadeOutActions;
            private Dictionary<int, Action> OkayActions;

            #endregion Fields

            #region Methods

            public static IGMData_Selections Create() =>
                Create<IGMData_Selections>(count: 3, depth: 1, container: new IGMDataItem.Empty { Pos = new Rectangle(320, 445, 250, 170) }, cols: 1, rows: 3);

            public override void Draw() => base.Draw();

            public override bool Inputs()
            {
                if (!FadingOut)
                    return base.Inputs();
                return false;
            }

            public override bool Inputs_CANCEL() => false;

            public override void Inputs_Cards()
            {
            }

            public override void Inputs_Menu()
            {
            }

            public override bool Inputs_OKAY()
            {
                if (OkayActions.TryGetValue(CURSOR_SELECT, out Action a))
                {
                    a();
                    return true;
                }
                return false;
            }

            public void NewGameFadeOutAction()
            {
                /*reverse engineering notes:
                *
                * we should happen to reset wm2field values
                * also the basic party of Squall is now set: SG_PARTY_FIELD1 = 0, and other members are 0xFF
                */
                Memory.FieldHolder.FieldID = 74; //RE: startup stage BattleID is hardcoded. Probably we would want to change it for modding
                                                 //the module changes to 1 now
                Fields.Module.ResetField();

                ModuleMovieTest.Index = 30;
                ModuleMovieTest.ReturnState = MODULE.FIELD_DEBUG;
                Memory.Module = MODULE.MOVIETEST;
                Menu.Module.State = MenuModule.Mode.MainLobby;
                Memory.IsMouseVisible = false;
                //wait till next update to start drawing.
                Memory.SuppressDraw = true;
            }

            public override void Refresh()
            {
                if (!eventset)
                {
                    Menu.FadedOutHandler += FadedOutEvent;
                    eventset = true;
                }
                base.Refresh();
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem.Text() { Data = Strings.Name.New_Game, Pos = SIZE[0] };
                ITEM[1, 0] = new IGMDataItem.Text() { Data = Strings.Name.Load_Game, Pos = SIZE[1] };
                ITEM[2, 0] = new IGMDataItem.Text() { Data = "OpenVIII debug tools", Pos = SIZE[2] };
                Cursor_Status = Cursor_Status.Enabled | Cursor_Status.Vertical | Cursor_Status.Horizontal;
                OkayActions = new Dictionary<int, Action>()
                {
                    {0, NewGameOkayAction },
                    {1, LoadGameOkayAction },
                    {2, DebugModeOkayAction },
                };
                FadeOutActions = new Dictionary<int, Action>()
                {
                    {0, NewGameFadeOutAction },
                    //{1, LoadGameOkayAction },
                    //{2, DebugModeOkayAction },
                };
            }

            private void DebugModeOkayAction()
            {
                base.Inputs_OKAY();
                FadeIn();
                Menu.Module.State = MenuModule.Mode.DebugScreen;
            }

            private void FadedOutEvent(object sender, EventArgs e)
            {
                if (Menu.Module.State == MenuModule.Mode.MainLobby &&
                    FadeOutActions.ContainsKey(CURSOR_SELECT))
                    FadeOutActions[CURSOR_SELECT]();
            }

            private void LoadGameOkayAction()
            {
                base.Inputs_OKAY();
                FadeIn();
                Menu.Module.State = MenuModule.Mode.LoadGameChooseSlot;
            }

            private void NewGameOkayAction()
            {
                AV.Sound.Play(28);
                skipsnd = true;
                base.Inputs_OKAY();
                FadeOut();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}