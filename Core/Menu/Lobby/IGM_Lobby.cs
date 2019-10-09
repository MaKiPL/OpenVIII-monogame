using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class IGM_Lobby : Menu
    {
        private Dictionary<Enum, IGMData> Data0;

        public enum SectionName
        {
            BG,
            Selections,
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            //Data0 is scaled from 1280x720
            Data0 = new Dictionary<Enum, IGMData> {
                { SectionName.BG, new IGMData_Container(new IGMDataItem_TextureHandler(TextureHandler.Create("start{0:00}", 2), new Rectangle(0,-25, 1280, 0))) } //new Rectangle(-45,-25, 1280+100, 0)
            };
            //Data is scaled from Size
            Data.Add(SectionName.Selections, new IGMData_Selections());
            base.Init();
        }

        public override void StartDraw()
        {
            Matrix backupfocus = Focus;
            GenerateFocus(new Vector2(1280, 720),Box_Options.Top);
            base.StartDraw();
            Data0.Where(m => m.Value != null).ForEach(m => m.Value.Draw());
            base.EndDraw();
            Focus = backupfocus;
            base.StartDraw();
        }

        public override bool Inputs() => Data[SectionName.Selections].Inputs();


        private class IGMData_Selections : IGMData
        {
            private Dictionary<int, Action> OkayActions;
            private Dictionary<int, Action> FadeOutActions;
            private bool eventset = false;

            public IGMData_Selections() : base(count: 3, depth: 1, container: new IGMDataItem.Empty(new Rectangle(320, 445, 250, 170)), cols: 1, rows: 3)
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
                Module_movie_test.ReturnState = MODULE.FIELD_DEBUG;
                Memory.module = MODULE.MOVIETEST;
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
                Memory.IsMouseVisible = false;
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
            public override void Inputs_Cards() { }
            public override void Inputs_Menu() { }
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
            public override bool Inputs()
            {
                if (!FadingOut)
                    return base.Inputs();
                return false;
            }

            public override void Draw()
            {
                base.Draw();
            }
        }
    }
}