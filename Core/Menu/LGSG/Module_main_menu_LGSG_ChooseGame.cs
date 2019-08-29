using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private static void DrawLGSGChooseBlocks()
        {
            Rectangle dst = new Rectangle
            {
                X = (int)(vp_per.X * 0.171875f),
                Y = (int)(vp_per.Y * 0.266666666666667f),
                Width = (int)(vp_per.X * 0.65625f),
                Height = (int)(vp_per.Y * 0.6625f),
            };

            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                Memory.graphics.GraphicsDevice.SetRenderTarget(OffScreenBuffer);

                // Matrix osbMatrix = Matrix.CreateScale(new Vector3())
                Memory.SpriteBatchStartAlpha();
                for (int bloc = 0; bloc < 3; bloc++)
                {
                    int block = bloc + 3 * (Blockpage);
                    Saves.Data d = Saves.FileList?[SlotLoc, block];
                    Tuple<Rectangle, Point> b = DrawBlock(block, d);
                    //cords returned by drawblock assume being in the offscreen buffer.
                    //Which is the size of the 3 blocks. So we need to offset
                    //everything by the draw location.
                    Rectangle r = b.Item1;
                    r.Offset(dst.Location);
                    Point p = b.Item2;
                    p.Offset(dst.Location);
                    BlockLocs[bloc] = new Tuple<Rectangle, Point, Rectangle>(r, p, new Rectangle());
                }
                Memory.SpriteBatchEnd();
                Memory.graphics.GraphicsDevice.SetRenderTarget(null);
            }
        }

        private static void DrawLGSGChooseGame(FF8String topright, FF8String help)
        {
            Rectangle dst = new Rectangle
            {
                X = (int)(vp_per.X * 0.171875f),
                Y = (int)(vp_per.Y * 0.266666666666667f),
                Width = (int)(vp_per.X * 0.65625f),
                Height = (int)(vp_per.Y * 0.6625f),
            };

            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);

            if (LastPage == null || LastPage.IsDisposed)
            {
                Entry e = Memory.Icons.GetEntry(Icons.ID.Arrow_Left);
                Rectangle arrow = new Rectangle
                {
                    X = (int)(dst.X - ((e.Width - 2) * 3f)),
                    Y = (dst.Y + dst.Height / 2)
                };
                Memory.Icons.Draw(Icons.ID.Arrow_Left, 1, arrow, new Vector2(3f), fade);
                Memory.Icons.Draw(Icons.ID.Arrow_Left, 2, arrow, new Vector2(3f), fade * Blink_Amount);
                arrow = new Rectangle
                {
                    X = (int)((dst.X + dst.Width) + -2 * 3f),
                    Y = arrow.Y
                };
                Memory.Icons.Draw(Icons.ID.Arrow_Right2, 1, arrow, new Vector2(3f), fade);
                Memory.Icons.Draw(Icons.ID.Arrow_Right2, 2, arrow, new Vector2(3f), fade * Blink_Amount);
            }
            UpdateTime();
            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                dst.Location = (dst.Location.ToVector2()).ToPoint();
                PageTarget = dst.Location.ToVector2();
                dst.Size = (dst.Size.ToVector2()).ToPoint();
                PageSize = dst.Size.ToVector2();
                CurrentPageLoc = CurrentPageLoc == Vector2.Zero || speed == 0f ? PageTarget : Vector2.Lerp(CurrentPageLoc, PageTarget, speed);//.FloorOrCeiling(PageTarget);
                dst.Location = CurrentPageLoc.RoundedPoint();
                Memory.spriteBatch.Draw(OffScreenBuffer, dst, Color.White * fade);
            }
            if (LastPage != null && !LastPage.IsDisposed)
            {
                CurrentLastPageLoc = CurrentLastPageLoc == Vector2.Zero || speed == 0f ? PageTarget : Vector2.Lerp(CurrentLastPageLoc, LastPageTarget, speed);//.FloorOrCeiling(LastPageTarget);
                if (LastPageTarget == CurrentLastPageLoc)
                {
                    LastPage.Dispose(); CurrentLastPageLoc = Vector2.Zero;
                }
                else
                {
                    dst.Location = CurrentLastPageLoc.RoundedPoint();
                    Memory.spriteBatch.Draw(LastPage, dst, Color.White * fade);
                }
            }
            else //if(BlockLocs[BlockLoc] != null)
            {
                Point ptr = BlockLocs[BlockLoc].Item2;
                Menu.DrawPointer(ptr);
            }
        }

        private static void DrawSGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.BlockToSave].Text);

        private static bool UpdateLGChooseGame()
        {
            bool ret = false;

            if (BlockLocs != null && BlockLocs.Length > 0 && BlockLocs[0] != null)
            {
                for (int i = 0; i < BlockLocs.Length && BlockLocs[i] != null; i++)
                {
                    if (BlockLocs[i].Item1.Contains(ml))
                    {
                        BlockLoc = (sbyte)i;
                        ret = true;

                        if (Input2.DelayedButton(MouseButtons.MouseWheelup))
                        {
                            UpdateLGChooseGameLEFT();
                            return true;
                        }
                        else if (Input2.DelayedButton(MouseButtons.MouseWheeldown))
                        {
                            UpdateLGChooseGameRIGHT();
                            return true;
                        }
                        break;
                    }
                }

                if (Input2.Button(FF8TextTagKey.Down))
                {
                    init_debugger_Audio.PlaySound(0);
                    BlockLoc++;
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.Left))
                {
                    UpdateLGChooseGameLEFT();
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.Right))
                {
                    UpdateLGChooseGameRIGHT();
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.Up))
                {
                    init_debugger_Audio.PlaySound(0);
                    BlockLoc--;
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.RotateLeft))//(Input2.Button(Keys.PageDown))
                {
                    init_debugger_Audio.PlaySound(0);
                    SlotLoc++;
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.RotateRight)) // (Input2.Button(Keys.PageUp))
                {
                    init_debugger_Audio.PlaySound(0);
                    SlotLoc--;
                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.Cancel))
                {
                    init_debugger_Audio.PlaySound(8);
                    init_debugger_Audio.StopMusic();
                    Dchoose = 0;
                    Fade = 0.0f;
                    State = MainMenuStates.LoadGameChooseSlot;

                    ret = true;
                }
                else if (Input2.DelayedButton(FF8TextTagKey.Confirm))
                {
                    PercentLoaded = 0f;
                    //State = MainMenuStates.LoadGameCheckingSlot;
                    State = MainMenuStates.LoadGameLoading;
                }
            }
            if (scale != lastscale && OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
                OffScreenBuffer.Dispose();
            if (OffScreenBuffer == null || OffScreenBuffer.IsDisposed)
                //if you make these with out disposing enough times gfx driver crashes.
                //happened many times if i left this running and had this just ran every draw call. heh.
                OffScreenBuffer = new RenderTarget2D(Memory.graphics.GraphicsDevice, (int)(vp_per.X * 0.65625f), (int)(vp_per.Y * 0.6625f), false, SurfaceFormat.Color, DepthFormat.None);
            return ret;
        }

        private static void UpdateLGChooseGameRIGHT()
        {
            init_debugger_Audio.PlaySound(0);
            if (LastPage != null && !LastPage.IsDisposed)
            {
                LastPage.Dispose();
                CurrentLastPageLoc = Vector2.Zero;
            }
            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                lock (OffScreenBuffer)
                {
                    LastPage = new Texture2D(Memory.graphics.GraphicsDevice, OffScreenBuffer.Width, OffScreenBuffer.Height);
                    lock (LastPage)
                    {
                        Color[] texdata = new Color[OffScreenBuffer.Width * OffScreenBuffer.Height];
                        OffScreenBuffer.GetData(texdata);
                        LastPage.SetData(texdata);
                        LastPageTarget = new Vector2(-PageSize.X, CurrentPageLoc.Y);
                        CurrentPageLoc = new Vector2(vp_per.X, CurrentPageLoc.Y);
                    }
                }
            }
            Blockpage++;
        }

        private static void UpdateLGChooseGameLEFT()
        {
            init_debugger_Audio.PlaySound(0);
            if (LastPage != null && !LastPage.IsDisposed)
            {
                LastPage.Dispose();
                CurrentLastPageLoc = Vector2.Zero;
            }
            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                lock (OffScreenBuffer)
                {
                    LastPage = new Texture2D(Memory.graphics.GraphicsDevice, OffScreenBuffer.Width, OffScreenBuffer.Height);
                    lock (LastPage)
                    {
                        Color[] texdata = new Color[OffScreenBuffer.Width * OffScreenBuffer.Height];
                        OffScreenBuffer.GetData(texdata);
                        LastPage.SetData(texdata);
                        LastPageTarget = new Vector2(vp_per.X, CurrentPageLoc.Y);
                        CurrentPageLoc = new Vector2(-PageSize.X, CurrentPageLoc.Y);
                    }
                }
            }
            Blockpage--;
        }

        private static void UpdateSGChooseGame() => throw new NotImplementedException();

        private static void DrawLGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.BlockToLoad].Text);

        private static RenderTarget2D OffScreenBuffer;

        private static Tuple<Rectangle, Point> DrawBlock(int block, Saves.Data d)
        {
            block++;
            Rectangle dst = new Rectangle
            {
                X = 0,
                Y = (OffScreenBuffer.Height / 3 * ((block - 1) % 3)),
                Width = (OffScreenBuffer.Width),
                Height = OffScreenBuffer.Height / 3,
            };
            Vector2 offset = dst.Location.ToVector2();

            Menu.DrawBox(dst);

            Vector2 blocknumpos = new Vector2
            {
                X = (OffScreenBuffer.Width * 0.00952381f),
                Y = 0,
            } + offset;
            Vector2 blocknumsize = new Vector2(OffScreenBuffer.Height * 0.00628930817610063f);
            Memory.Icons.Draw(block, Icons.NumType.Num_8x16_0, 2, "D2", blocknumpos, blocknumsize, fade); // 2,2 looks close

            Rectangle faceRect = new Rectangle
            {
                X = (int)(OffScreenBuffer.Width * 0.0654761904761905f),
                Y = (int)(OffScreenBuffer.Height * 0.0157232704402516f),
                Width = (int)(OffScreenBuffer.Width * 0.113095238095238f),
                Height = (int)(OffScreenBuffer.Height * 0.30188679245283f),
            };
            faceRect.Offset(offset);
            sbyte mainchar = -1;
            for (byte face = 0; d != null && d.Party != null && face < d.Party.Count; face++)
            {
                if (face != 0)
                    faceRect.Offset(faceRect.Width, 0);
                if (d.Party[face] != Characters.Blank)
                {
                    Memory.Faces.Draw(d.Party[face], faceRect, Vector2.UnitY, fade);
                    Memory.Icons.Draw(Icons.ID.MenuBorder, 2, faceRect, new Vector2(1f), fade);
                    if (mainchar == -1) mainchar = (sbyte)face;
                }
            }
            if (mainchar > -1 && d != null && d.Party != null && d.Party[mainchar] != Characters.Blank)
            {
                Vector2 detailsLoc = new Vector2
                {
                    X = (int)(OffScreenBuffer.Width * 0.408333333333333f),
                    Y = (int)(OffScreenBuffer.Height * 0.0251572327044026f),
                } + offset;
                FF8String name = Memory.Strings.GetName(d.Party[mainchar], d);
                FF8String lv_ = new FF8String($"LV.   {d.firstcharacterslevel}");
                //TextScale1 = new Vector2(OffScreenBuffer.Width * 0.0030303030297619f, OffScreenBuffer.Height * 0.00636792452830189f);
                Memory.font.RenderBasicText(name, detailsLoc, TextScale, Fade: fade);

                int playy = (int)detailsLoc.Y;
                detailsLoc.Y += (int)(OffScreenBuffer.Height * 0.0817610062893082f);
                Rectangle disc = Memory.font.RenderBasicText(lv_, detailsLoc, TextScale, Fade: fade);
                disc.Offset(0, (int)(OffScreenBuffer.Height * 0.0125786163522013f));
                disc.X = (int)(OffScreenBuffer.Width * 0.583333333333333f);
                Vector2 DiscScale = new Vector2(OffScreenBuffer.Height * 0.0060987230787661f);
                Memory.Icons.Draw(Icons.ID.DISC, 2, disc, DiscScale, fade);
                Vector2 CurrDiscLoc = new Vector2(disc.X + (0.119047619047619f * OffScreenBuffer.Width), disc.Y);
                Memory.Icons.Draw((int)d.CurrentDisk + 1, 0, 2, "D1", CurrDiscLoc, DiscScale, fade);
                float X1 = OffScreenBuffer.Width * 0.952380952380952f;
                float X2 = OffScreenBuffer.Width * -0.0238095238095238f;
                float X3 = OffScreenBuffer.Width * -0.0952380952380952f;
                disc.Location = new Vector2 { X = X1, Y = disc.Y }.ToPoint();
                Memory.Icons.Draw(Icons.ID.G, 2, disc, DiscScale, fade);
                double digits = (d.AmountofGil == 0 ? 1 : Math.Floor(Math.Log10(d.AmountofGil) + 2));

                disc.Offset(new Vector2 { X = (float)(digits * X2) });
                Memory.Icons.Draw((int)d.AmountofGil, 0, 2, "D1", disc.Location.ToVector2(), DiscScale, fade);
                disc.Location = new Vector2 { X = X1, Y = playy }.ToPoint();
                disc.Offset(new Vector2 { X = X2 });
                Memory.Icons.Draw(d.Timeplayed.Minutes, 0, 2, "D2", disc.Location.ToVector2(), DiscScale, fade);

                if ((int)d.Timeplayed.TotalHours > 0)
                {
                    disc.Offset(new Vector2 { X = 1 * X2 });
                    Memory.Icons.Draw(Icons.ID.Colon, 13, disc, DiscScale, fade);
                    disc.Offset(new Vector2 { X = (float)Math.Floor(Math.Log10((int)d.Timeplayed.TotalHours) + 1) * X2 });
                    Memory.Icons.Draw((int)d.Timeplayed.TotalHours, 0, 2, "D1", disc.Location.ToVector2(), DiscScale, fade);
                }
                disc.Offset(new Vector2 { X = X3 + X2 });
                Memory.Icons.Draw(Icons.ID.PLAY, 13, disc, DiscScale, fade);
                Rectangle locbox = new Rectangle
                {
                    X = faceRect.Width + faceRect.X,
                    Y = (int)(OffScreenBuffer.Height * 0.19496855345912f),
                    Width = OffScreenBuffer.Width - faceRect.Width - faceRect.X,
                    Height = (int)(OffScreenBuffer.Height * 0.138364779874214f),
                };
                locbox.Offset(offset);
                Menu.DrawBox(locbox);
                FF8String loc = Memory.Strings.Read(Strings.FileID.AREAMES, 0, d.LocationID);
                locbox.Offset(0.0297619047619048f * OffScreenBuffer.Width, 0.0440251572327044f * OffScreenBuffer.Height);
                Memory.font.RenderBasicText(loc, locbox.Location, TextScale, Fade: fade);
            }

            dst.X = (int)(vp_per.X * 0f);
            dst.Y = (int)(vp_per.Y * 0.220833333333333f * ((block - 1) % 3));
            dst.Width = (int)(vp_per.X * 0.65625f);
            dst.Height = (int)(vp_per.Y * 0.220833333333333f);
            return new Tuple<Rectangle, Point>(dst, (dst.Location.ToVector2() + new Vector2(25f, dst.Height / 2)).ToPoint());
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

        private static sbyte _blockLoc;

        private static sbyte blockpage;
        public static sbyte Blockpage
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

        public static float speed
        {
            get;set;
        }

        private static void UpdateTime()
        {
            if (CurrentTime < TotalTime)
            {
                CurrentTime += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;

                speed = (float)(CurrentTime / TotalTime);
            }
            else
            {
                CurrentTime = 0;
                speed = 0f;
            }
        }

        private static Tuple<Rectangle, Point, Rectangle>[] BlockLocs = new Tuple<Rectangle, Point, Rectangle>[3];
        private static Texture2D LastPage;
        private static double CurrentTime;
        private static double TotalTime => 200d;
    }
}