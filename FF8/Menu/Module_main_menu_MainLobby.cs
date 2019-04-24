using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static Mitems s_mchoose;

        private static TextureHandler start;

        /// <summary>
        /// Strings for the main menu
        /// </summary>
        private static Dictionary<Enum, Item> strMainLobby;

        #endregion Fields

        #region Enums

        /// <summary>
        /// Identifiers and Ordering of main menu items
        /// </summary>
        private enum Mitems
        {
            New,
            Load,
            Debug
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Current choice on main menu
        /// </summary>
        private static Mitems Mchoose
        {
            get => s_mchoose;
            set
            {
                if (value > s_mchoose && value > (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max())
                {
                    value = 0;
                }
                else if (value < s_mchoose && s_mchoose <= 0)
                {
                    value = (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max();
                }
                s_mchoose = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Draw Main menu
        /// </summary>
        private static void DrawMainLobby()
        {
            Vector2 scale = Memory.Scale();
            Vector2 vp = (new Vector2(vpWidth, vpHeight)) * scale;
            float item = 0;
            Vector2 textSize = new Vector2(2.545454545f, 3.0375f);// scaled in render function.
            Vector2 textStart = new Vector2(vp.X * 0.45078125f, vp.Y * .65f);
            Memory.SpriteBatchStartAlpha();
            Rectangle dst = new Rectangle()
            {
                Location = new Point(0),
                Size = vp.ToPoint()
            };
            start.Draw(dst, null, Color.White * fade);
            Memory.SpriteBatchEnd();
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            foreach (Mitems i in (Mitems[])Enum.GetValues(typeof(Mitems)))
            {
                Item c = strMainLobby[i];
                c.Loc = (Memory.font.RenderBasicText(c.Text,
                    (int)(textStart.X), (int)(textStart.Y + ((textSize.Y + vpSpace) * item++)), textSize.X, textSize.Y, 1, 0, Fade));
                strMainLobby[i] = c;
            }
            DrawPointer(new Point((int)(textStart.X), (int)((((textSize.Y + vpSpace) * (float)Mchoose)+textStart.Y+(6*textSize.Y))*scale.Y)));
            Memory.SpriteBatchEnd();
        }

        private static void InitMain()
        {
            strMainLobby = new Dictionary<Enum, Item>()
            {
                { Mitems.New, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,105) } },
                { Mitems.Load, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,106) } },
                { Mitems.Debug, new Item{Text=Font.CipherDirty("OpenVIII debug tools") } }
            };
            if (start == null)
            {
                start = new TextureHandler("start{0:00}", 2);
            }
        }

        /// <summary>
        /// Update Main Lobby
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateMainLobby()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;
            foreach (KeyValuePair<Enum, Item> entry in strMainLobby)
            {
                if (entry.Value.Loc.Contains(ml))
                {
                    Mchoose = (Mitems)entry.Key;
                    ret = true;

                    if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
                    {
                        return ret;
                    }
                    break;
                }
            }
            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose++;
                ret = true;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose--;
                ret = true;
            }
            if (Input.Button(Buttons.Okay))
            {
                Input.ResetInputLimit();
                switch (Mchoose)
                {
                    case Mitems.New:
                        init_debugger_Audio.PlaySound(28);
                        State = MainMenuStates.NewGameChoosed;
                        break;

                    case Mitems.Load:
                        init_debugger_Audio.PlaySound(0);
                        State = MainMenuStates.LoadGameChooseSlot;
                        break;

                    case Mitems.Debug:
                        init_debugger_Audio.PlaySound(0);
                        Mchoose = 0;
                        Dchoose = 0;
                        State = MainMenuStates.DebugScreen;
                        Fade = 0.0f;
                        break;
                }
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Waits for fade to end then triggers new game.
        /// </summary>
        private static void UpdateNewGame()
        {
            if (Fade > 0.0f)
            {
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f / 2;
                return;
            }
            /*reverse engineering notes:
            *
            * we should happen to reset wm2field values
            * also the basic party of Squall is now set: SG_PARTY_FIELD1 = 0, and other members are 0xFF
            */
            FieldPointer = 74; //RE: startup stage ID is hardcoded. Probably we would want to change it for modding
            //the module changes to 1 now
            Module_field_debug.ResetField();

            Module_movie_test.Index = 30;
            Module_movie_test.ReturnState = Memory.MODULE_FIELD_DEBUG;
            Memory.module = Memory.MODULE_MOVIETEST;
            State = MainMenuStates.MainLobby;
        }

        #endregion Methods
    }
}