using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static sbyte Lchoose = 0;

        #endregion Fields

        #region Enums

        private enum Litems
        {
            GameFolder,
            Load,
            LoadFF8,
            Loading,
            Slot1,
            Slot2,
            FF8
        }

        #endregion Enums

        #region Methods

        private static void DrawBox(byte[] buffer, Icons.ID? title, Rectangle dst, bool indent = true, bool bottom = false)
        {
            Vector2 scale = Memory.Scale();
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            dst.Size = (dst.Size.ToVector2() * scale).ToPoint();
            dst.Location = (dst.Location.ToVector2() * scale).ToPoint();
            dst.Offset(0, vpHeight * 0.0583333333333333f * scale.Y);
            if (dst.Width > .5f * vpWidth)
                Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, dst, 2f * scale, Fade);
            else
                Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, dst, 2f * scale, Fade);
            dst.Offset(0.01171875f, 0);
            if (title != null)
            {
                dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2() * scale * 2.823317308f).ToPoint();
                Memory.Icons.Draw(title.Value, 2, dst, Vector2.Zero, fade);
            }
            if (indent)
                dst.Offset(0.0546875f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);
            else if (bottom)
                dst.Offset(0.01953125f * vpWidth * scale.X, dst.Height - 0.066666667f * vpHeight * scale.Y);
            else
                dst.Offset(0.01953125f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);

            //Memory.SpriteBatchEnd();
            //Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(buffer, dst.Location, new Vector2(2.545454545f, 3.0375f), 1, 0, fade);
            Memory.SpriteBatchEnd();
        }

        private static void DrawLGChooseGame() => throw new NotImplementedException();

        private static Rectangle DrawLGSGHeader(byte[] info, byte[] name, byte[] help)
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.82421875f), 0, (int)(vpWidth * 0.17578125f), (int)(vpHeight * 0.0916666666666667f));
            DrawBox(name, null, dst, false);
            dst = new Rectangle(0, 0, (int)(vpWidth * 0.8203125f), dst.Height);
            DrawBox(info, Icons.ID.INFO, dst);
            dst = new Rectangle((int)(vpWidth * 0.0282101167315175f), (int)(dst.Height + vpHeight * 0.0041666666666667f), (int)(vpWidth * 0.943579766536965f), dst.Height);
            DrawBox(help, Icons.ID.HELP, dst, false);
            dst = new Rectangle((int)(vpWidth * 0.3703125f), (int)(vpHeight * 0.386111111f), (int)(vpWidth * 0.259375f), (int)(vpHeight * 0.141666667f));
            return dst;
        }

        private static void DrawLGChooseSlot()
        {
            Rectangle dst = DrawLGSGHeader(strLoadScreen[Litems.GameFolder].Text, strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.LoadFF8].Text);
            Rectangle slot = new Rectangle(dst.Location, new Point((int)(vpWidth * 0.1f), (int)(vpHeight * 0.0875f)));
            slot.Offset(vpWidth * -0.00859375f, vpHeight * -0.033333333f);
            DrawBox(strLoadScreen[Litems.FF8].Text, null, dst, false, true);
            DrawBox(strLoadScreen[Litems.Slot1].Text, null, slot, false, false);
            dst.Offset(0, vpHeight * 0.216666667f);
            slot.Offset(0, vpHeight * 0.216666667f);
            DrawBox(strLoadScreen[Litems.FF8].Text, null, dst, false, true);
            DrawBox(strLoadScreen[Litems.Slot2].Text, null, slot, false, false);
        }

        private static void DrawLoadingGame() => throw new NotImplementedException();

        private static void DrawLoadingSlot() => throw new NotImplementedException();

        private static void InitLoad() => strLoadScreen = new Dictionary<Enum, Item>()
            {
                { Litems.GameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,86) } },
                { Litems.Load, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,54) } },
                { Litems.LoadFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,128) } },
                { Litems.FF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,127) } },
                { Litems.Loading, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,93) } },
                { Litems.Slot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,87) } },
                { Litems.Slot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,88) } }
            };

        private static void UpdateLGChooseGame() => throw new NotImplementedException();

        private static bool UpdateLGChooseSlot()
        {
            bool ret = false;
            if (Input.Button(Buttons.Okay) && Dchoose == Ditems.Reset || Input.Button(Buttons.Cancel))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(8);
                init_debugger_Audio.StopAudio();
                Dchoose = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
                ret = true;
            }
            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Lchoose++;
                ret = true;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Lchoose--;
                ret = true;
            }
            return ret;
        }

        private static void UpdateLoading() => throw new NotImplementedException();

        private static void UpdateLoadingSlot() => throw new NotImplementedException();

        #endregion Methods
    }
}