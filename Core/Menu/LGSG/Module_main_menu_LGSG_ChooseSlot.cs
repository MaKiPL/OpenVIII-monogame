using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {

        /// <summary>
        /// Draw Loading Choose Slot Screen
        /// </summary>
        private static void DrawLGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.LoadFF8].Text);

        /// <summary>
        /// Draw Save or Loading Slot Screen
        /// </summary>
        /// <param name="topright">Text in top right box</param>
        /// <param name="help">Text in help box</param>
        private static void DrawLGSGChooseSlot(FF8String topright, FF8String help)
        {
            DrawLGSGHeader(strLoadScreen[Litems.GameFolder].Text, topright, help);
            SlotLocs[0] = DrawLGSGSlot(Vector2.Zero, strLoadScreen[Litems.Slot1].Text, strLoadScreen[Litems.FF8].Text);
            SlotLocs[1] = DrawLGSGSlot(new Vector2(0, vp_per.Y * 0.216666667f), strLoadScreen[Litems.Slot2].Text, strLoadScreen[Litems.FF8].Text);
            Menu.DrawPointer(SlotLocs[SlotLoc].Item2);
        }
        /// <summary>
        /// Draw Save Choose Slot Screen
        /// </summary>
        private static void DrawSGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.SaveFF8].Text);

        private static void UpdateSGChooseSlot() => throw new NotImplementedException();

        private static bool UpdateLGChooseSlot()
        {
            bool ret = false;
            for (int i = 0; i < SlotLocs.Length; i++)
            {
                if (SlotLocs[i] != null && SlotLocs[i].Item1.Contains(ml))
                {
                    SlotLoc = (sbyte)i;
                    ret = true;

                    if (Input2.Button(MouseButtons.MouseWheelup) || Input2.Button(MouseButtons.MouseWheeldown))
                    {
                        return ret;
                    }
                    break;
                }
            }
            if (Input2.DelayedButton(FF8TextTagKey.Down))
            {
                init_debugger_Audio.PlaySound(0);
                SlotLoc++;
                ret = true;
            }
            else if (Input2.DelayedButton(FF8TextTagKey.Up))
            {
                init_debugger_Audio.PlaySound(0);
                SlotLoc--;
                ret = true;
            }
            if (Input2.DelayedButton(FF8TextTagKey.Cancel))
            {
                init_debugger_Audio.PlaySound(8);
                init_debugger_Audio.StopMusic();
                Dchoose = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;

                ret = true;
            }
            else if (Input2.DelayedButton(FF8TextTagKey.Confirm))
            {
                PercentLoaded = 0f;
                State = MainMenuStates.LoadGameCheckingSlot;
            }
            return ret;
        }

        /// <summary>
        /// Rectangle is hotspot for mouse, Point is where the finger points
        /// </summary>
        private static Tuple<Rectangle, Point, Rectangle>[] SlotLocs = new Tuple<Rectangle, Point, Rectangle>[2];
        private static sbyte _slotLoc;
        private static sbyte SlotLoc
        {
            get => _slotLoc; set
            {
                if (value >= SlotLocs.Length)
                    value = 0;
                else if (value < 0)
                    value = (sbyte)(SlotLocs.Length - 1);
                _slotLoc = value;
            }
        }
    }
}