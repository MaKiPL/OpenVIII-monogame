using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Lobby
    {
        private class IGMData_Selections : IGMData
        {
            private Dictionary<int, Action> OkayActions;
            private Dictionary<int, Action> FadeOutActions;
            private bool eventset = false;

            public IGMData_Selections() : base(count: 3, depth: 1, container: new IGMDataItem_Empty(new Rectangle(320, 445, 250, 170)), cols: 1, rows: 3)
            {
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 1, 105), SIZE[0]);
                ITEM[1, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 1, 106), SIZE[1]);
                ITEM[2, 0] = new IGMDataItem_String("OpenVIII debug tools", SIZE[2]);
                Cursor_Status |= Cursor_Status.Enabled;
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
            public override void Refresh()
            {

                if (!eventset)
                {
                    Menu.FadedOutHandler += FadedOutEvent;
                    eventset = true;
                }
                base.Refresh();
            }

            private void FadedOutEvent(object sender, EventArgs e)
            {
                if (Module_main_menu_debug.State == Module_main_menu_debug.MainMenuStates.MainLobby && 
                    FadeOutActions.ContainsKey(CURSOR_SELECT))
                    FadeOutActions[CURSOR_SELECT]();
            }
            public void NewGameFadeOutAction()
            {
                /*reverse engineering notes:
                *
                * we should happen to reset wm2field values
                * also the basic party of Squall is now set: SG_PARTY_FIELD1 = 0, and other members are 0xFF
                */
                Module_main_menu_debug.FieldPointer = 74; //RE: startup stage ID is hardcoded. Probably we would want to change it for modding
                                   //the module changes to 1 now
                Module_field_debug.ResetField();

                Module_movie_test.Index = 30;
                Module_movie_test.ReturnState = Memory.MODULE_FIELD_DEBUG;
                Memory.module = Memory.MODULE_MOVIETEST;
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
            }
            public override void Inputs_OKAY()
            {
                if (OkayActions.ContainsKey(CURSOR_SELECT))
                    OkayActions[CURSOR_SELECT]();
            }
            public override bool Inputs_CANCEL()
            {
                return false;
            }

            private void NewGameOkayAction() {
                init_debugger_Audio.PlaySound(28);
                skipsnd = true;
                base.Inputs_OKAY();
                FadeOut();
            }
            private void LoadGameOkayAction() {
                base.Inputs_OKAY();
                FadeIn();
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.LoadGameChooseSlot;
            }
            private void DebugModeOkayAction()
            {
                base.Inputs_OKAY();
                FadeIn();
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.DebugScreen;
            }
            public override void Draw()
            {
                base.Draw();
            }
        }
    }
}