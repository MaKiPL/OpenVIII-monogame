using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static sbyte _slotLoc = 0;
        /// <summary>
        /// Rectangle is hotspot for mouse, Point is where the finger points
        /// </summary>
        private static Tuple<Rectangle, Point>[] SlotLocs = new Tuple<Rectangle, Point>[2];

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
            FF8,
            Save,
            SaveFF8,
            GameFolderSlot1,
            GameFolderSlot2,
            CheckGameFolder,
            BlockToLoad,
            BlockToSave
        }

        #endregion Enums

        #region Properties

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

        private static float PercentLoaded { get; set; } = .5f;

        #endregion Properties

        #region Methods

        private static Tuple<Rectangle, Point> DrawBox(byte[] buffer, Icons.ID? title, Rectangle dst, bool indent = true, bool bottom = false)
        {
            Point cursor = new Point(0);
            Vector2 scale = Memory.Scale();
            dst.Size = (dst.Size.ToVector2() * scale).ToPoint();
            dst.Location = (dst.Location.ToVector2() * scale).ToPoint();
            Vector2 bgscale = 2f * scale;
            if (dst.Width > 256 * bgscale.X)
                Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, dst, bgscale, Fade);
            else
                Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, dst, bgscale, Fade);
            Rectangle hotspot = new Rectangle(dst.Location, dst.Size);
            dst.Offset(0.01171875f, 0);
            if (title != null)
            {
                //dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2() * scale * 2.823317308f).ToPoint();
                Memory.Icons.Draw(title.Value, 2, dst, bgscale + (.5f * scale), fade);
            }

            if (buffer!= null && buffer.Length > 0)
            {
                if (indent)
                    dst.Offset(0.0546875f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);
                else if (bottom)
                    dst.Offset(0.01953125f * vpWidth * scale.X, dst.Height - 0.066666667f * vpHeight * scale.Y);
                else
                    dst.Offset(0.01953125f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);
                Memory.font.RenderBasicText(buffer, dst.Location, new Vector2(2.545454545f, 3.0375f), 1, 0, fade);
                cursor = dst.Location;
                cursor.Y += (int)(18.225f * scale.Y); // 12 * (3.0375/2) * scale.Y
            }
            return new Tuple<Rectangle, Point>(hotspot, cursor);
        }

        private static void DrawPointer(Point cursor)
        {
            Vector2 scale = Memory.Scale();
            Rectangle dst = new Rectangle(cursor, new Point((int)(24 * 2 * scale.X), (int)(16 * 2 * scale.Y)));
            dst.Offset(-(dst.Width + 10), -(dst.Height * .25f));
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, 2f * scale, fade);
        }

        private static void DrawLGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.BlockToLoad].Text);
        private static void DrawSGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.BlockToSave].Text);
        private static void DrawLGSGChooseGame(byte[] topright, byte[] help)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);
            Memory.SpriteBatchEnd();

        }

        /// <summary>
        /// Draw Loading Choose Slot Screen
        /// </summary>
        private static void DrawLGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.LoadFF8].Text);

        /// <summary>
        /// Draw Save or Loading Slot Screen
        /// </summary>
        /// <param name="topright">Text in top right box</param>
        /// <param name="help">Text in help box</param>
        private static void DrawLGSGChooseSlot(byte[] topright, byte[] help)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolder].Text, topright, help);
            SlotLocs[0] = DrawLGSGSlot(Vector2.Zero, strLoadScreen[Litems.Slot1].Text, strLoadScreen[Litems.FF8].Text);
            SlotLocs[1] = DrawLGSGSlot(new Vector2(0, vpHeight * 0.216666667f), strLoadScreen[Litems.Slot2].Text, strLoadScreen[Litems.FF8].Text);
            DrawPointer(SlotLocs[SlotLoc].Item2);
            Memory.SpriteBatchEnd();
        }

        private static Rectangle DrawLGSGHeader(byte[] info, byte[] name, byte[] help)
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.82421875f), (int)(vpHeight * 0.0583333333333333f), (int)(vpWidth * 0.17578125f), (int)(vpHeight * 0.0916666666666667f));
            DrawBox(name, null, dst, false);
            dst = new Rectangle(0, dst.Y, (int)(vpWidth * 0.8203125f), dst.Height);
            DrawBox(info, Icons.ID.INFO, dst);
            dst = new Rectangle((int)(vpWidth * 0.0282101167315175f), (int)(dst.Height + dst.Y + vpHeight * 0.0041666666666667f), (int)(vpWidth * 0.943579766536965f), dst.Height);
            DrawBox(help, Icons.ID.HELP, dst, false);
            return dst;
        }

        private static Tuple<Rectangle, Point> DrawLGSGSlot(Vector2 offset, byte[] title, byte[] main)
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.3703125f), (int)(vpHeight * 0.386111111f), (int)(vpWidth * 0.259375f), (int)(vpHeight * 0.141666667f));
            Rectangle slot = new Rectangle(dst.Location, new Point((int)(vpWidth * 0.1f), (int)(vpHeight * 0.0875f)));
            slot.Offset(vpWidth * -0.00859375f, vpHeight * -0.033333333f);
            slot.Offset(offset);
            dst.Offset(offset);
            Tuple<Rectangle, Point> location = DrawBox(main, null, dst, false, true);
            DrawBox(title, null, slot, false, false);
            return location;
        }

        private static void DrawLoadingGame() => throw new NotImplementedException();

        private static void DrawLGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Load].Text);
        private static void DrawSGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Save].Text);
        private static void DrawLGSGCheckSlot(byte[] topright)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1+SlotLoc].Text, topright, strLoadScreen[Litems.CheckGameFolder].Text);
            DrawLGSGLoadBar();
            Memory.SpriteBatchEnd();
        }
        private static void DrawLGSGLoadBar()
        {
            Rectangle dst = new Rectangle
            {
                X = (int)(vpWidth * 0.328125f),
                Y = (int)(vpHeight * 0.45f),
                Width = (int)(vpWidth * 0.34375f),
                Height = (int)(vpHeight * 0.1f),
            };
            dst = DrawBox(null, Icons.ID.INFO, dst).Item1;
            Vector2 scale = Memory.Scale();
            dst.Offset(new Vector2
            {
                X = (vpWidth * 0.01328125f),
                Y = (vpHeight * 0.0333333333333333f),
            } * scale);
            dst.Size = (new Point
            {
                X = (int)(vpWidth * 0.3171875f),
                Y = (int)(vpHeight * 0.0333333333333333f),
            }.ToVector2() * scale).ToPoint();
            Memory.Icons.Draw(Icons.ID.Bar_BG, -1, dst, Vector2.UnitY, fade);
            dst.Width = (int)(dst.Width * PercentLoaded);
            Memory.Icons.Draw(Icons.ID.Bar_Fill, -1, dst, Vector2.UnitY, fade);
        }
        /// <summary>
        /// Draw Save Choose Slot Screen
        /// </summary>
        private static void DrawSGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.SaveFF8].Text);

        private static void InitLoad()
        {
            strLoadScreen = new Dictionary<Enum, Item>()
            {
                { Litems.GameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,86) } },
                { Litems.Load, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,54) } },
                { Litems.LoadFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,128) } },
                { Litems.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,14) } },
                { Litems.SaveFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,117) } },
                { Litems.FF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,127) } },
                { Litems.Loading, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,93) } },
                { Litems.Slot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,87) } },
                { Litems.Slot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,88) } },
                { Litems.GameFolderSlot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,121) } },
                { Litems.GameFolderSlot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,122) } },
                { Litems.CheckGameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,110) } },
                { Litems.BlockToLoad, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,114) } },
                { Litems.BlockToSave, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,89) } },
            };
            SlotLoc = 0;
        }

        private static void UpdateLGChooseGame() { }

        private static bool UpdateLGChooseSlot()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;
            for (int i = 0; i < SlotLocs.Length; i++)
            {
                if (SlotLocs[i].Item1.Contains(ml))
                {
                    SlotLoc = (sbyte)i;
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
                SlotLoc++;
                ret = true;
            }
            else if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                SlotLoc--;
                ret = true;
            }
            if (Input.Button(Buttons.Cancel))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(8);
                init_debugger_Audio.StopAudio();
                Dchoose = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
                ret = true;
            }
            else if (Input.Button(Buttons.Okay))
            {
                PercentLoaded = 0f;
                State = MainMenuStates.LoadGameCheckingSlot;
            }
            return ret;
        }

        private static void UpdateLoading() => throw new NotImplementedException();

        private static void UpdateLoadingSlot()
        {

            if (PercentLoaded < 1.0f)
            {
                PercentLoaded += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
            else
            {
                State = MainMenuStates.LoadGameChooseGame;
            }
        }

        #endregion Methods
    }
}