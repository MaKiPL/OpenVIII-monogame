using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FF8
{

    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static sbyte _slotLoc, _blockLoc;


        private static int blockpage;

        private static RenderTarget2D OffScreenBuffer;

        private static float s_blink;

        /// <summary>
        /// Rectangle is hotspot for mouse, Point is where the finger points
        /// </summary>
        private static Tuple<Rectangle, Point, Rectangle>[] SlotLocs = new Tuple<Rectangle, Point, Rectangle>[2];

        private static Tuple<Rectangle, Point, Rectangle>[] BlockLocs = new Tuple<Rectangle, Point, Rectangle>[3];
        private static Texture2D LastPage;
        private static Vector2 PageTarget;
        private static Vector2 PageSize;
        private static Vector2 CurrentPageLoc;
        private static Vector2 CurrentLastPageLoc;
        private static Vector2 LastPageTarget;
        private static Vector2 lastscale;
        private static Vector2 scale;
        private static Vector2 TextScale;

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
            BlockToSave,
            Saving
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

        private static Tuple<Rectangle, Point> DrawBlock(int block, Saves.Data d)
        {
            block++;
            Rectangle dst = new Rectangle
            {
                X = 0,
                Y = (OffScreenBuffer.Height/3 * ((block - 1) % 3)),
                Width = (OffScreenBuffer.Width),
                Height = OffScreenBuffer.Height/3,
            };
            Vector2 offset = dst.Location.ToVector2();

            DrawBox(dst, null, null, false, false, true);

            Vector2 blocknumpos = new Vector2
            {
                X = (OffScreenBuffer.Width * 0.00952381f),
                Y = 0,
            } + offset;
            Vector2 blocknumsize = new Vector2(OffScreenBuffer.Height * 0.00628930817610063f);
            Memory.Icons.Draw(block, 4, 2, "D2", blocknumpos, blocknumsize, fade); // 2,2 looks close

            Rectangle faceRect = new Rectangle
            {
                X = (int)(OffScreenBuffer.Width * 0.0654761904761905f),
                Y = (int)(OffScreenBuffer.Height * 0.0157232704402516f),
                Width = (int)(OffScreenBuffer.Width * 0.113095238095238f),
                Height = (int)(OffScreenBuffer.Height * 0.30188679245283f),
            };
            faceRect.Offset(offset);
            sbyte mainchar = -1;
            for (byte face = 0; d != null && d.charactersportraits != null && face < d.charactersportraits.Length; face++)
            {
                if (face != 0)
                    faceRect.Offset(faceRect.Width, 0);
                if (d.charactersportraits[face] != Faces.ID.Blank)
                {
                    Memory.Faces.Draw((Faces.ID)d.charactersportraits[face], faceRect, Vector2.UnitY, fade);
                    Memory.Icons.Draw(Icons.ID.MenuBorder, 2, faceRect, new Vector2(1f), fade);
                    if (mainchar == -1) mainchar = (sbyte)face;
                }
            }
            if (mainchar > -1 &&  d!=null && d.charactersportraits != null && d.charactersportraits[mainchar] != Faces.ID.Blank)
            {
                Point detailsLoc = (new Point
                {
                    X = (int)(OffScreenBuffer.Width * 0.408333333333333f),
                    Y = (int)(OffScreenBuffer.Height * 0.0251572327044026f),
                }.ToVector2() + offset).ToPoint();
                FF8String name = Memory.Strings.GetName((Faces.ID)d.charactersportraits[mainchar],d);
                FF8String lv_ = new FF8String($"LV.   {d.firstcharacterslevel}");
                TextScale = new Vector2(OffScreenBuffer.Width * 0.0030303030297619f, OffScreenBuffer.Height * 0.00636792452830189f);
                Memory.font.RenderBasicText(name, detailsLoc, TextScale, 1, 0, fade, true);

                int playy = detailsLoc.Y;
                detailsLoc.Y += (int)(OffScreenBuffer.Height * 0.0817610062893082f);
                Rectangle disc = Memory.font.RenderBasicText(lv_, detailsLoc, TextScale, 1, 0, fade, true);
                disc.Offset(0, (int)(OffScreenBuffer.Height * 0.0125786163522013f));
                disc.X = (int)(OffScreenBuffer.Width * 0.583333333333333f);
                Vector2 DiscScale = new Vector2(OffScreenBuffer.Height * 0.0060987230787661f);
                Memory.Icons.Draw(Icons.ID.DISC, 2, disc, DiscScale, fade);
                Vector2 CurrDiscLoc = new Vector2(disc.X + (0.119047619047619f * OffScreenBuffer.Width), disc.Y);
                Memory.Icons.Draw((int)d.CurrentDisk+1, 0, 2, "D1", CurrDiscLoc, DiscScale, fade);
                float X1 = OffScreenBuffer.Width * 0.952380952380952f;
                float X2 = OffScreenBuffer.Width * -0.0238095238095238f;
                float X3 = OffScreenBuffer.Width * -0.0952380952380952f;
                disc.Location = new Vector2 { X = X1, Y = disc.Y }.ToPoint();
                Memory.Icons.Draw(Icons.ID.G, 2, disc, DiscScale, fade);
                double digits = Math.Floor(Math.Log10(d.AmountofGil) + 2);
                disc.Offset(new Vector2 { X = (float)(digits *X2) });
                Memory.Icons.Draw((int)d.AmountofGil, 0, 2, "D1", disc.Location.ToVector2(), DiscScale, fade);
                disc.Location = new Vector2 { X = X1, Y = playy }.ToPoint();
                disc.Offset(new Vector2 { X =X2 });
                Memory.Icons.Draw(d.timeplayed.Minutes, 0, 2, "D2", disc.Location.ToVector2(), DiscScale, fade);

                if ((int)d.timeplayed.TotalHours > 0)
                {
                    disc.Offset(new Vector2 { X = 1 *X2 });
                    Memory.Icons.Draw(Icons.ID.Colon, 13, disc, DiscScale, fade);
                    disc.Offset(new Vector2 { X = (float)Math.Floor(Math.Log10((int)d.timeplayed.TotalHours) + 1) *X2 });
                    Memory.Icons.Draw((int)d.timeplayed.TotalHours, 0, 2, "D1", disc.Location.ToVector2(), DiscScale, fade);
                }
                disc.Offset(new Vector2 { X = X3+X2 });
                Memory.Icons.Draw(Icons.ID.PLAY, 13, disc, DiscScale, fade);
                Rectangle locbox = new Rectangle
                {
                    X = faceRect.Width + faceRect.X,
                    Y = (int)(OffScreenBuffer.Height * 0.19496855345912f),
                    Width = OffScreenBuffer.Width - faceRect.Width - faceRect.X,
                    Height = (int)(OffScreenBuffer.Height * 0.138364779874214f),
                };
                locbox.Offset(offset);
                DrawBox(locbox, null, null, false, false, true);
                FF8String loc = Memory.Strings.Read(Strings.FileID.AREAMES, 0, d.LocationID).ReplaceRegion();
                locbox.Offset(0.0297619047619048f * OffScreenBuffer.Width, 0.0440251572327044f * OffScreenBuffer.Height);
                Memory.font.RenderBasicText(loc, locbox.Location, TextScale, 1, 0, fade, true);

            }

            dst.X = (int)(vpWidth * 0f);
            dst.Y = (int)(vpHeight * 0.220833333333333f * ((block - 1) % 3));
            dst.Width = (int)(vpWidth * 0.65625f);
            dst.Height = (int)(vpHeight * 0.220833333333333f);
            return new Tuple<Rectangle, Point>(dst, (dst.Location.ToVector2() + new Vector2(25f, dst.Height / 2)).ToPoint());
        }

        private static Tuple<Rectangle, Point, Rectangle> DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, bool indent = true, bool bottom = false, bool prescaled = false)
        {
            Point cursor = new Point(0);
            dst.Size = (dst.Size.ToVector2() ).ToPoint();
            dst.Location = (dst.Location.ToVector2() ).ToPoint();
            Vector2 bgscale = new Vector2(2f) ;
            if (dst.Width > 256 * bgscale.X)
                Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, dst, bgscale, Fade);
            else
                Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, dst, bgscale, Fade);
            Rectangle hotspot = new Rectangle(dst.Location, dst.Size);
            dst.Offset(0.01171875f, 0);
            if (title != null)
            {
                //dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2()  * 2.823317308f).ToPoint();
                Memory.Icons.Draw(title.Value, 2, dst, bgscale + new Vector2(.5f), fade);
            }
            Rectangle font = new Rectangle();
            if (buffer != null && buffer.Length > 0)
            {
                if (indent)
                    dst.Offset(0.0546875f * vpWidth, 0.0291666666666667f * vpHeight);
                else if (bottom)
                    dst.Offset(0.01953125f * vpWidth, dst.Height - 0.066666667f * vpHeight);
                else
                    dst.Offset(0.01953125f * vpWidth, 0.0291666666666667f * vpHeight);
                font = Memory.font.RenderBasicText(buffer, dst.Location, new Vector2(2.545454545f, 3.0375f), 1, 0, fade, prescaled);
                cursor = dst.Location;
                cursor.Y += (int)(18.225f); // 12 * (3.0375/2)
            }
            return new Tuple<Rectangle, Point, Rectangle>(hotspot, cursor, font);
        }

        private static void DrawLGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Load].Text);

        private static void DrawLGChooseGame() => DrawLGSGChooseGame(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.BlockToLoad].Text);

        /// <summary>
        /// Draw Loading Choose Slot Screen
        /// </summary>
        private static void DrawLGChooseSlot() => DrawLGSGChooseSlot(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.LoadFF8].Text);

        private static void DrawLGSGCheckSlot(FF8String topright)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, strLoadScreen[Litems.CheckGameFolder].Text);
            DrawLGSGLoadBar();
            Memory.SpriteBatchEnd();
        }

        private static void DrawLGSGChooseGame(FF8String topright, FF8String help)
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
                for (int bloc = 0; bloc<3; bloc++)                {
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
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);

            
            if (LastPage == null || LastPage.IsDisposed)
            {
                Entry e = Memory.Icons.GetEntry(Icons.ID.Arrow_Left);
                Rectangle arrow = new Rectangle
                {
                    X = (int)(dst.X - ((e.Width - 2) * 3f)),
                    Y = (int)((dst.Y + dst.Height / 2))
                };
                Memory.Icons.Draw(Icons.ID.Arrow_Left, 1, arrow, new Vector2(3f), fade);
                Memory.Icons.Draw(Icons.ID.Arrow_Left, 2, arrow, new Vector2(3f), fade * blink);
                arrow = new Rectangle
                {
                    X = (int)((dst.X + dst.Width) + -2 * 3f),
                    Y = arrow.Y
                };
                Memory.Icons.Draw(Icons.ID.Arrow_Right2, 1, arrow, new Vector2(3f), fade);
                Memory.Icons.Draw(Icons.ID.Arrow_Right2, 2, arrow, new Vector2(3f), fade * blink);
            }
            float speed = .17f;
            if (OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
            {
                dst.Location = (dst.Location.ToVector2() ).ToPoint();
                PageTarget = dst.Location.ToVector2();
                dst.Size = (dst.Size.ToVector2() ).ToPoint();
                PageSize = dst.Size.ToVector2();
                CurrentPageLoc = CurrentPageLoc == Vector2.Zero ? PageTarget : Vector2.SmoothStep(CurrentPageLoc, PageTarget, speed).FloorOrCeiling(PageTarget);
                dst.Location = CurrentPageLoc.RoundedPoint();
                Memory.spriteBatch.Draw(OffScreenBuffer, dst, Color.White * fade);
            }
            if (LastPage != null && !LastPage.IsDisposed)
            {
                CurrentLastPageLoc = CurrentLastPageLoc == Vector2.Zero ? PageTarget : Vector2.SmoothStep(CurrentLastPageLoc, LastPageTarget, speed).FloorOrCeiling(LastPageTarget);
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
            else
            {
                Point ptr = BlockLocs[BlockLoc].Item2;
                ptr = ptr.Scale(scale);
                DrawPointer(ptr);
            }
            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Draw Save or Loading Slot Screen
        /// </summary>
        /// <param name="topright">Text in top right box</param>
        /// <param name="help">Text in help box</param>
        private static void DrawLGSGChooseSlot(FF8String topright, FF8String help)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolder].Text, topright, help);
            SlotLocs[0] = DrawLGSGSlot(Vector2.Zero, strLoadScreen[Litems.Slot1].Text, strLoadScreen[Litems.FF8].Text);
            SlotLocs[1] = DrawLGSGSlot(new Vector2(0, vpHeight * 0.216666667f), strLoadScreen[Litems.Slot2].Text, strLoadScreen[Litems.FF8].Text);
            DrawPointer(SlotLocs[SlotLoc].Item2);
            Memory.SpriteBatchEnd();
        }

        private static Rectangle DrawLGSGHeader(FF8String info, FF8String name, FF8String help)
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.82421875f), (int)(vpHeight * 0.0583333333333333f), (int)(vpWidth * 0.17578125f), (int)(vpHeight * 0.0916666666666667f));
            DrawBox(dst, name, null, false);
            dst = new Rectangle(0, dst.Y, (int)(vpWidth * 0.8203125f), dst.Height);
            DrawBox(dst, info, Icons.ID.INFO);
            dst = new Rectangle((int)(vpWidth * 0.0282101167315175f), (int)(dst.Height + dst.Y + vpHeight * 0.0041666666666667f), (int)(vpWidth * 0.943579766536965f), dst.Height);
            DrawBox(dst, help, Icons.ID.HELP, false);
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
            dst = DrawBox(dst, null, Icons.ID.INFO).Item1;
            
            dst.Offset(new Vector2
            {
                X = (vpWidth * 0.01328125f),
                Y = (vpHeight * 0.0333333333333333f),
            } );
            dst.Size = (new Point
            {
                X = (int)(vpWidth * 0.3171875f),
                Y = (int)(vpHeight * 0.0333333333333333f),
            }.ToVector2() ).ToPoint();
            Memory.Icons.Draw(Icons.ID.Bar_BG, -1, dst, Vector2.UnitY, fade);
            dst.Width = (int)(dst.Width * PercentLoaded);
            Memory.Icons.Draw(Icons.ID.Bar_Fill, -1, dst, Vector2.UnitY, fade);
        }

        private static Tuple<Rectangle, Point,Rectangle> DrawLGSGSlot(Vector2 offset, FF8String title, FF8String main)
        {
            Rectangle dst = new Rectangle((int)(vpWidth * 0.3703125f), (int)(vpHeight * 0.386111111f), (int)(vpWidth * 0.259375f), (int)(vpHeight * 0.141666667f));
            Rectangle slot = new Rectangle(dst.Location, new Point((int)(vpWidth * 0.1f), (int)(vpHeight * 0.0875f)));
            slot.Offset(vpWidth * -0.00859375f, vpHeight * -0.033333333f);
            slot.Offset(offset);
            dst.Offset(offset);
            Tuple<Rectangle, Point, Rectangle> location = DrawBox(dst, main, null, false, true);
            DrawBox(slot, title, null, false, false);
            return location;
        }
        private static void DrawLGdata() => DrawLGSGdata(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.Loading].Text);
        private static void DrawSGdata() => DrawLGSGdata(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.Saving].Text);
        private static void DrawLGSGdata(FF8String topright,FF8String help)
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);
            DrawLGSGLoadBar();
            Memory.SpriteBatchEnd();
        }

        private static void DrawPointer(Point cursor, sbyte xoffset = -10)
        {
            
            Rectangle dst = new Rectangle(cursor, new Point((int)(24 * 2), (int)(16 * 2)));
            dst.Offset(-(dst.Width) + xoffset, -(dst.Height * .25f));
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, new Vector2(2f) , fade);
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
                { Litems.Saving, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,94) } },
                { Litems.Slot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,87) } },
                { Litems.Slot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,88) } },
                { Litems.GameFolderSlot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,121) } },
                { Litems.GameFolderSlot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,122) } },
                { Litems.CheckGameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,110) } },
                { Litems.BlockToLoad, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,114) } },
                { Litems.BlockToSave, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,89) } },
            };
            SlotLoc = 0;
            BlockLoc = 0;

            SlotLocs = new Tuple<Rectangle, Point, Rectangle>[2];
            BlockLocs = new Tuple<Rectangle, Point, Rectangle>[3];
    }

        private static bool UpdateLGChooseGame()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;

            if (BlockLocs != null && BlockLocs.Length > 0 && BlockLocs[0] != null)
            {
                for (int i = 0; i < BlockLocs.Length && BlockLocs[i] != null; i++)
                {
                    if (BlockLocs[i].Item1.Scale(scale).Contains(ml))
                    {
                        BlockLoc = (sbyte)i;
                        ret = true;

                        if (Input.Button(Buttons.MouseWheelup))
                        {
                            UpdateLGChooseGameLEFT();
                            return true;
                        }
                        else if(Input.Button(Buttons.MouseWheeldown))
                        {
                            UpdateLGChooseGameRIGHT();
                            return true;
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
                    UpdateLGChooseGameLEFT();
                    ret = true;
                }
                else if (Input.Button(Buttons.Right))
                {
                    UpdateLGChooseGameRIGHT();
                    ret = true;
                }
                else if (Input.Button(Buttons.Up))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    BlockLoc--;
                    ret = true;
                }
                else if (Input.Button(Keys.PageDown))
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
                else if (Input.Button(Buttons.Cancel))
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
                    //State = MainMenuStates.LoadGameCheckingSlot;
                    State = MainMenuStates.LoadGameLoading;
                }
            }
            if (scale != lastscale && OffScreenBuffer != null && !OffScreenBuffer.IsDisposed)
                OffScreenBuffer.Dispose();
            if (OffScreenBuffer == null || OffScreenBuffer.IsDisposed)
                //if you make these with out disposing enough times gfx driver crashes.
                //happened many times if i left this running and had this just ran every draw call. heh.
                OffScreenBuffer = new RenderTarget2D(Memory.graphics.GraphicsDevice, (int)(vpWidth * 0.65625f *scale.X), (int)(vpHeight * 0.6625f*scale.Y), false, SurfaceFormat.Color, DepthFormat.None);
            return ret;
        }

        private static void UpdateLGChooseGameRIGHT()
        {
            Input.ResetInputLimit();
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
                        CurrentPageLoc = new Vector2(vpWidth, CurrentPageLoc.Y);
                    }
                }
            }
            Blockpage++;
        }

        private static void UpdateLGChooseGameLEFT()
        {
            Input.ResetInputLimit();
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
                        LastPageTarget = new Vector2(vpWidth, CurrentPageLoc.Y);
                        CurrentPageLoc = new Vector2(-PageSize.X, CurrentPageLoc.Y);
                    }
                }
            }
            Blockpage--;
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

        private static void UpdateLoadingData()
        {
            if (PercentLoaded < 1.0f)
            {
                PercentLoaded += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
            else
            {
                State = MainMenuStates.InGameMenu; // start loaded game.
                Memory.State = Saves.FileList[SlotLoc, BlockLoc + blockpage*3];
                //till we have a game to load i'm going to display ingame menu.
                init_debugger_Audio.PlaySound(36);
            }
            UpdateLGChooseGame();
        }

        private static void UpdateLoadingSlot()
        {
            if (PercentLoaded < 1.0f)
            {
                PercentLoaded += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
            else
            {
                State = MainMenuStates.LoadGameChooseGame;
                init_debugger_Audio.PlaySound(35);
            }
            UpdateLGChooseGame();
        }

        private static void UpdateSGChooseSlot() => throw new NotImplementedException();
        private static void UpdateSGCheckSlot() => throw new NotImplementedException();
        private static void UpdateSGChooseGame() => throw new NotImplementedException();
        private static void UpdateSGdata() => throw new NotImplementedException();
        #endregion Methods
    }
}