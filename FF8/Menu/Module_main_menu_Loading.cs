using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FF8
{
    /// <summary>
    /// class to add offset to point
    /// </summary>
    public static class PointEx
    {
        #region Methods

        public static Point Offset(this ref Point source, Point offset)
        {
            source = (source.ToVector2() + offset.ToVector2()).ToPoint();
            return source;
        }

        public static Point Offset(this ref Point source, Vector2 offset)
        {
            source = (source.ToVector2() + offset).ToPoint();
            return source;
        }

        #endregion Methods
    }

    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static sbyte _slotLoc = 0;

        private static bool blinkstate;

        private static int blockpage;

        private static RenderTarget2D OffScreenBuffer;

        private static float s_blink;

        /// <summary>
        /// Rectangle is hotspot for mouse, Point is where the finger points
        /// </summary>
        private static Tuple<Rectangle, Point>[] SlotLocs = new Tuple<Rectangle, Point>[2];

        private static Tuple<Rectangle, Point>[] BlockLocs = new Tuple<Rectangle, Point>[3];
        private static sbyte _blockLoc;

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

        public static float blink
        {
            get => s_blink; private set
            {
                if (value > 1f)
                    blinkstate = false;
                if (value < 0f)
                    blinkstate = true;
                s_blink = value;
            }
        }

        private static float PercentLoaded { get; set; } = .5f;

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

        private static sbyte BlockLoc
        {
            get => _blockLoc; set
            {
                if (value >= BlockLocs.Length)
                    value = 0;
                else if (value < 0)
                    value = (sbyte)(BlockLocs.Length - 1);
                _blockLoc = value;
            }
        }

        public static int Blockpage
        {
            get => blockpage; set
            {
                if (value >= 10)
                    value = 0;
                else if (value < 0)
                    value = 9;
                blockpage = value;
            }
        }

        #endregion Properties

        #region Methods

        private static Tuple<Rectangle, Point> DrawBlock(int block, Ff8files.Data d)
        {
            block++;
            Rectangle dst = new Rectangle
            {
                X = (int)(vpWidth * 0f),
                Y = (int)(vpHeight * 0.220833333333333f * ((block - 1) % 3)),
                Width = (int)(vpWidth * 0.65625f),
                Height = (int)(vpHeight * 0.220833333333333f),
            };
            Vector2 offset = dst.Location.ToVector2();

            DrawBox(null, null, dst, false, false, true);

            Vector2 blocknumpos = new Vector2
            {
                X = (vpWidth * 0.00625f),
                Y = 0,
            } + offset;
            Memory.Icons.Draw(block++, 4, 2, "D2", blocknumpos, new Vector2(3f), fade); // 2,2 looks close

            Rectangle faceRect = new Rectangle
            {
                X = (int)(vpWidth * 0.04296875f),
                Y = (int)(vpHeight * 0.0104166666666667f),
                Width = (int)(vpWidth * 0.07421875f),
                Height = (int)(vpHeight * 0.2f),
            };
            faceRect.Offset(offset);
            sbyte mainchar = -1;
            for (byte face = 0; d != null && d.charactersportraits != null && face < d.charactersportraits.Length; face++)
            {
                if (face != 0)
                    faceRect.Offset(faceRect.Width, 0);
                if (d.charactersportraits[face] != 0xFF)
                {
                    Memory.Faces.Draw((Faces.ID)d.charactersportraits[face], faceRect, Vector2.UnitY, fade);
                    Memory.Icons.Draw(Icons.ID.MenuBorder, 2, faceRect, new Vector2(1f), fade);
                    if (mainchar == -1) mainchar = (sbyte)face;
                }
            }
            if (mainchar > -1 &&  d!=null && d.charactersportraits != null && d.charactersportraits[mainchar] != 0xFF)
            {
                Point detailsLoc = (new Point
                {
                    X = (int)(vpWidth * 0.26796875f),
                    Y = (int)(vpHeight * 0.0166666666666667f),
                }.ToVector2() + offset).ToPoint();
                FF8String name = new FF8String(Enum.GetName(typeof(Faces.ID), (Faces.ID)d.charactersportraits[mainchar]).Replace('_', ' '));
                FF8String lv_ = new FF8String($"LV.   {d.firstcharacterslevel}");
                Memory.font.RenderBasicText(name, detailsLoc, new Vector2(2.545454545f, 3.0375f), 1, 0, fade, true);
                int playy = detailsLoc.Y;
                detailsLoc.Y += (int)(vpHeight * 0.0541666666666667f);
                Rectangle disc = Memory.font.RenderBasicText(lv_, detailsLoc, new Vector2(2.545454545f, 3.0375f), 1, 0, fade, true);
                disc.Offset(0, (int)(vpHeight * 0.00833333333333333f));
                disc.X = (int)(vpWidth * 0.3828125f);
                Memory.Icons.Draw(Icons.ID.DISC, 2, disc, new Vector2(2.90909090857143f), fade);
                Memory.Icons.Draw((int)d.CurrentDisk+1, 0, 2, "D1", new Vector2(disc.X + (0.078125f * vpWidth), disc.Y), new Vector2(2.90909090857143f), fade);

                disc.Location = new Vector2 { X = (vpWidth * 0.625f), Y = disc.Y }.ToPoint();
                Memory.Icons.Draw(Icons.ID.G, 2, disc, new Vector2(2.90909090857143f), fade);
                double digits = Math.Floor(Math.Log10(d.AmountofGil) + 2);
                disc.Offset(new Vector2 { X = (float)(digits * vpWidth * -0.015625f) });
                Memory.Icons.Draw((int)d.AmountofGil, 0, 2, "D1", disc.Location.ToVector2(), new Vector2(2.90909090857143f), fade);
                disc.Location = new Vector2 { X = (vpWidth * 0.625f), Y = playy }.ToPoint();
                disc.Offset(new Vector2 { X = 1 * vpWidth * -0.015625f });
                Memory.Icons.Draw(d.timeplayed.Minutes, 0, 2, "D2", disc.Location.ToVector2(), new Vector2(2.90909090857143f), fade);

                if ((int)d.timeplayed.TotalHours > 0)
                {
                    disc.Offset(new Vector2 { X = 1 * vpWidth * -0.015625f });
                    Memory.Icons.Draw(Icons.ID.Colon, 13, disc, new Vector2(2.90909090857143f), fade);
                    disc.Offset(new Vector2 { X = (float)Math.Floor(Math.Log10((int)d.timeplayed.TotalHours) + 1) * vpWidth * -0.015625f });
                    Memory.Icons.Draw((int)d.timeplayed.TotalHours, 0, 2, "D1", disc.Location.ToVector2(), new Vector2(2.90909090857143f), fade);
                }
                disc.Offset(new Vector2 { X = vpWidth * -0.0625f + vpWidth * -0.015625f });
                Memory.Icons.Draw(Icons.ID.PLAY, 13, disc, new Vector2(2.90909090857143f), fade);
                Rectangle locbox = new Rectangle
                {
                    X = faceRect.Width + faceRect.X,
                    Y = (int)(vpHeight * 0.129166666666667f),
                    Width = dst.Width - faceRect.Width - faceRect.X,
                    Height = (int)(vpHeight * 0.0916666666666667f),
                };
                locbox.Offset(offset);
                DrawBox(Memory.Strings.Read(Strings.FileID.AREAMES,0,d.LocationID), null, locbox, false, false, true);
            }
            return new Tuple<Rectangle, Point>(dst, (dst.Location.ToVector2() + new Vector2(0, dst.Height / 2)).ToPoint());
        }

        private static Tuple<Rectangle, Point> DrawBox(FF8String buffer, Icons.ID? title, Rectangle dst, bool indent = true, bool bottom = false, bool prescaled = false)
        {
            Point cursor = new Point(0);
            Vector2 scale = prescaled ? Vector2.One : Memory.Scale();
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

            if (buffer != null && buffer.Length > 0)
            {
                if (indent)
                    dst.Offset(0.0546875f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);
                else if (bottom)
                    dst.Offset(0.01953125f * vpWidth * scale.X, dst.Height - 0.066666667f * vpHeight * scale.Y);
                else
                    dst.Offset(0.01953125f * vpWidth * scale.X, 0.0291666666666667f * vpHeight * scale.Y);
                Memory.font.RenderBasicText(buffer, dst.Location, new Vector2(2.545454545f, 3.0375f), 1, 0, fade, prescaled);
                cursor = dst.Location;
                cursor.Y += (int)(18.225f * scale.Y); // 12 * (3.0375/2) * scale.Y
            }
            return new Tuple<Rectangle, Point>(hotspot, cursor);
        }

        private static void DrawLGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Load].Text);

        private static void DrawLGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.BlockToLoad].Text);

        /// <summary>
        /// Draw Loading Choose Slot Screen
        /// </summary>
        private static void DrawLGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.LoadFF8].Text);

        private static void DrawLGSGCheckSlot(byte[] topright)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, strLoadScreen[Litems.CheckGameFolder].Text);
            DrawLGSGLoadBar();
            Memory.SpriteBatchEnd();
        }

        private static void DrawLGSGChooseGame(byte[] topright, byte[] help)
        {
            Rectangle dst = new Rectangle
            {
                X = (int)(vpWidth * 0.171875f),
                Y = (int)(vpHeight * 0.266666666666667f),
                Width = (int)(vpWidth * 0.65625f),
                Height = (int)(vpHeight * 0.6625f),
            };

            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                Memory.graphics.GraphicsDevice.SetRenderTarget(OffScreenBuffer);
                Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
                int bloc = 0;
                for (int block = 0 + 3 * (Blockpage); block < 3 + 3 * (Blockpage); block++)
                {
                    Ff8files.Data d = Ff8files.FileList[SlotLoc, block];
                    Tuple<Rectangle, Point> b = DrawBlock(block, d);
                    //cords returned by drawblock assume being in the offscreen buffer.
                    //Which is the size of the 3 blocks. So we need to offset
                    //everything by the draw location.
                    Rectangle r = b.Item1;
                    r.Offset(dst.Location);
                    Point p = b.Item2;
                    p.Offset(dst.Location);
                    BlockLocs[bloc++] = new Tuple<Rectangle, Point>(r, p);
                }
                Memory.SpriteBatchEnd();
                Memory.graphics.GraphicsDevice.SetRenderTarget(null);
            }
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);
            Entry e = Memory.Icons.GetEntry(Icons.ID.Arrow_Left);
            Rectangle arrow = new Rectangle
            {
                X = dst.X - (int)((e.Width - 2) * 3f),
                Y = dst.Y + dst.Height / 2
            };
            Memory.Icons.Draw(Icons.ID.Arrow_Left, 1, arrow, new Vector2(3f), fade);
            Memory.Icons.Draw(Icons.ID.Arrow_Left, 2, arrow, new Vector2(3f), fade * blink);
            arrow = new Rectangle
            {
                X = dst.X + dst.Width + (int)(-2 * 3f),
                Y = dst.Y + dst.Height / 2
            };
            Memory.Icons.Draw(Icons.ID.Arrow_Right2, 1, arrow, new Vector2(3f), fade);
            Memory.Icons.Draw(Icons.ID.Arrow_Right2, 2, arrow, new Vector2(3f), fade * blink);
            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                Vector2 scale = Memory.Scale();
                dst.Location = (dst.Location.ToVector2() * scale).ToPoint();
                dst.Size = (dst.Size.ToVector2() * scale).ToPoint();
                Memory.spriteBatch.Draw(OffScreenBuffer, dst, Color.White * fade);
            }

            DrawPointer(BlockLocs[BlockLoc].Item2, +10);
            Memory.SpriteBatchEnd();
        }

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

        private static void DrawPointer(Point cursor, sbyte xoffset = -10)
        {
            Vector2 scale = Memory.Scale();
            Rectangle dst = new Rectangle(cursor, new Point((int)(24 * 2 * scale.X), (int)(16 * 2 * scale.Y)));
            dst.Offset(-(dst.Width) + xoffset, -(dst.Height * .25f));
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, 2f * scale, fade);
        }

        private static void DrawSGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Save].Text);

        private static void DrawSGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.BlockToSave].Text);

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

        private static bool UpdateLGChooseGame()
        {
            if (blinkstate)
                blink += Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f * 3;
            else
                blink -= Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f * 3;

            if (OffScreenBuffer == null || OffScreenBuffer.IsDisposed)
                //if you make these with out disposing enough times gfx driver crashes.
                //happened many times if i left this running and had this just ran every draw call. heh.
                OffScreenBuffer = new RenderTarget2D(Memory.graphics.GraphicsDevice, (int)(vpWidth * 0.65625f), (int)(vpHeight * 0.6625f), false, SurfaceFormat.Color, DepthFormat.None);
            bool ret = false;
            Point ml = Input.MouseLocation;
            if (BlockLocs != null && BlockLocs.Length > 0 && BlockLocs[0] != null)
            {
                for (int i = 0; i < BlockLocs.Length && BlockLocs[i] != null; i++)
                {
                    if (BlockLocs[i].Item1.Contains(ml))
                    {
                        BlockLoc = (sbyte)i;
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
                    BlockLoc++;
                    ret = true;
                }
                else if (Input.Button(Buttons.Left))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    Blockpage--;
                    ret = true;
                }
                if (Input.Button(Buttons.Right))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    Blockpage++;
                    ret = true;
                }
                else if (Input.Button(Buttons.Up))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    BlockLoc--;
                    ret = true;
                }
                if (Input.Button(Keys.PageDown))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    SlotLoc++;
                    ret = true;
                }
                else if (Input.Button(Keys.PageUp))
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
                    State = MainMenuStates.LoadGameChooseSlot;
                    ret = true;
                }
                else if (Input.Button(Buttons.Okay))
                {
                    PercentLoaded = 0f;
                    State = MainMenuStates.LoadGameCheckingSlot;
                }
            }
            return ret;
        }

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
            UpdateLGChooseGame();
        }

        #endregion Methods
    }
}