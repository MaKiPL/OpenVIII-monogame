using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        private static sbyte Lchoose = 0;
        #region Methods

        private static void DrawLGChooseGame() => throw new NotImplementedException();

        private static void DrawLGChooseSlot()
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.8203125f), 0, (int)(vpWidth * 0.17578125f), (int)(vpHeight * 0.0916666666666667f));
            DrawBox(strLoadScreen[Litems.Load].Text, null, dst,false);
            dst = new Rectangle(0, 0, (int)(vpWidth - dst.Width), dst.Height);
            DrawBox(strLoadScreen[Litems.GameFolder].Text, Icons.ID.INFO, dst);
            dst = new Rectangle((int)(vpWidth* 0.0282101167315175f), (int) (dst.Height + vpHeight * 0.0041666666666667f), (int)(vpWidth * 0.943579766536965f), dst.Height);
            DrawBox(strLoadScreen[Litems.LoadFF8].Text, Icons.ID.HELP, dst,false);
        }
        private static void DrawBox(byte[] buffer, Icons.ID? title, Rectangle dst, bool wide = true)
        {
            Vector2 scale = Memory.Scale();
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            dst.Size = (dst.Size.ToVector2() * scale).ToPoint();
            dst.Location = (dst.Location.ToVector2() * scale).ToPoint();
            dst.Offset(0, vpHeight * 0.0583333333333333f * scale.Y);
            Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, dst, 0, Fade);
            dst.Offset(0.01171875f, 0);
            if(title!=null)
            { 
                dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2() * scale * 2.25f).ToPoint();
                Memory.Icons.Draw(title.Value, 2, dst, 0, fade);
            }
            if(wide)
                dst.Offset(0.0546875f * vpWidth * scale.X, 0.0291666666666667f * vpHeight);
            else
                dst.Offset(0.01953125f * vpWidth * scale.X, 0.0291666666666667f * vpHeight);
            
            Memory.SpriteBatchEnd();
            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(buffer, dst.Location, new Vector2(1.25f, 2.25f), 0, 1, fade);
            Memory.SpriteBatchEnd();
        }
        private static void DrawLoading() => throw new NotImplementedException();

        private static void InitLoad() => strLoadScreen = new Dictionary<Enum, Item>()
            {
                { Litems.GameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,86) } },
                { Litems.Load, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,54) } },
                { Litems.LoadFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,128) } },
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

        #endregion Methods
    }
}