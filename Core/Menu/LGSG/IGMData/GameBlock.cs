using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
    namespace IGMData
    {
        public class GameBlock : IGMData.Base, I_Data<Saves.Data>
        {
            #region Fields

            public Slide<Vector2> Slider;
            private static TimeSpan time => TimeSpan.FromMilliseconds(500d);
            private int _lastpage = -1;
            private int _page = -1;

            #endregion Fields

            #region Properties

            public Saves.Data Data { get; set; }

            public int ExpectedPageNumber => (ID - 1) / ParentRows;

            public byte ID
            {
                get => checked((byte)(BlockNumber?.Data ?? 0)); private set
                {
                    if (BlockNumber != null)
                        BlockNumber.Data = value;
                }
            }

            public bool IsMyPage => ExpectedPageNumber == _page;
            public int ParentRows { get; set; } = 3;

            private IGMDataItem.Integer BlockNumber { get => (IGMDataItem.Integer)ITEM[0, 0]; set => ITEM[0, 0] = value; }

            private IGMDataItem.Icon Colon { get => (IGMDataItem.Icon)ITEM[0, 11]; set => ITEM[0, 11] = value; }

            private IGMDataItem.Icon Disc { get => (IGMDataItem.Icon)ITEM[0, 7]; set => ITEM[0, 7] = value; }

            private IGMDataItem.Integer Disc_Num { get => (IGMDataItem.Integer)ITEM[0, 8]; set => ITEM[0, 8] = value; }
            private IGMDataItem.Face Face1 { get => (IGMDataItem.Face)ITEM[0, 1]; set => ITEM[0, 1] = value; }

            private IGMDataItem.Face Face2 { get => (IGMDataItem.Face)ITEM[0, 2]; set => ITEM[0, 2] = value; }

            private IGMDataItem.Face Face3 { get => (IGMDataItem.Face)ITEM[0, 3]; set => ITEM[0, 3] = value; }

            private IGMDataItem.Icon G { get => (IGMDataItem.Icon)ITEM[0, 14]; set => ITEM[0, 14] = value; }

            private IGMDataItem.Integer Gil { get => (IGMDataItem.Integer)ITEM[0, 13]; set => ITEM[0, 13] = value; }

            private IGMDataItem.Integer Hours { get => (IGMDataItem.Integer)ITEM[0, 10]; set => ITEM[0, 10] = value; }

            private Vector2 left
            {
                get
                {
                    Vector2 p = ScreenTopLeft.ToVector2();
                    p.X -= Width;
                    return p;
                }
            }

            private IGMDataItem.Box Location { get => (IGMDataItem.Box)ITEM[0, 15]; set => ITEM[0, 15] = value; }

            private IGMDataItem.Text LV { get => (IGMDataItem.Text)ITEM[0, 5]; set => ITEM[0, 5] = value; }

            private IGMDataItem.Integer LV_Num { get => (IGMDataItem.Integer)ITEM[0, 6]; set => ITEM[0, 6] = value; }

            private IGMDataItem.Integer Mins { get => (IGMDataItem.Integer)ITEM[0, 12]; set => ITEM[0, 12] = value; }
            private IGMDataItem.Text Name { get => (IGMDataItem.Text)ITEM[0, 4]; set => ITEM[0, 4] = value; }

            private IGMDataItem.Icon Play { get => (IGMDataItem.Icon)ITEM[0, 9]; set => ITEM[0, 9] = value; }

            private Vector2 right => ScreenTopRight.ToVector2();

            #endregion Properties

            #region Methods

            public static GameBlock Create(Rectangle pos)
            {
                GameBlock r = Create<GameBlock>(1, 16, new IGMDataItem.Box { Pos = pos });
                return r;
            }

            public void AddPageChangeEvent(ref EventHandler<Pool.GameChoose.PageInfo> pageChangeEventHandler) => pageChangeEventHandler += PageChangeEvent;

            public override void Draw() => base.Draw();

            public override void Refresh()
            {
                base.Refresh();
                if (Data != null)
                {
                    Face1.Data = Data.Party[0].ToFacesID();
                    Face2.Data = Data.Party[1].ToFacesID();
                    Face3.Data = Data.Party[2].ToFacesID();
                    Saves.CharacterData characterData = Data[Data.Party.First(x => !x.Equals(Characters.Blank))];
                    Name.Data = characterData.Name;
                    LV_Num.Data = characterData.Level;
                    Disc_Num.Data = checked((int)Data.CurrentDisk);
                    Hours.Data = checked((int)MathHelper.Clamp(checked((float)Data.TimePlayed.TotalHours), 0, 99));
                    if (Hours.Data < 99)
                        Mins.Data = checked((int)MathHelper.Clamp(checked((float)Data.TimePlayed.Minutes), 0, 99));
                    else
                        Mins.Data = 99;
                    Gil.Data = checked((int)Data.AmountOfGil);
                    Location.Data = Memory.Strings.Read(Strings.FileID.AreaNames, 0, Data.LocationID);
                    foreach (Menu_Base i in ITEM)
                        i?.Show();
                }
                else
                {
                    foreach (Menu_Base i in ITEM)
                        i?.Hide();
                }
            }

            public void Refresh(byte id, Saves.Data data)
            {
                ID = ++id;
                Data = data;
                Refresh();
            }

            public override bool Update()
            {
                base.Update();
                if (Slider.Done)
                {
                    if (!IsMyPage)
                    {
                        Hide();
                        return true;
                    }
                    return false;
                }
                else
                {
                    CONTAINER.OffsetAnchor?.Set(Slider.Update());
                    return true;
                }
            }

            protected override void Init()
            {
                base.Init();
                Slider = new Slide<Vector2>(Vector2.Zero, Vector2.Zero, time, Vector2.SmoothStep);
                BlockNumber = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), NumType = Icons.NumType.Num8X16, Spaces = 2, Padding = 2 };
                Face1 = new IGMDataItem.Face { Pos = new Rectangle(BlockNumber.X + 44, SIZE[0].Y, 124, SIZE[0].Height), Border = true };
                Face2 = new IGMDataItem.Face { Pos = new Rectangle(Face1.X + Face1.Width, SIZE[0].Y, Face1.Width, SIZE[0].Height), Border = true };
                Face3 = new IGMDataItem.Face { Pos = new Rectangle(Face2.X + Face1.Width, SIZE[0].Y, Face1.Width, SIZE[0].Height), Border = true };
                int Face3offsetx = Face3.X + Face1.Width + 4;
                Name = new IGMDataItem.Text { Pos = new Rectangle(Face3offsetx, SIZE[0].Y, 0, 0) };
                LV = new IGMDataItem.Text { Data = Strings.Name.LV, Pos = new Rectangle(Name.X, SIZE[0].Y + 64, 0, 0) };
                LV_Num = new IGMDataItem.Integer { Pos = new Rectangle(Name.X + 80, LV.Y, 0, 0), NumType = Icons.NumType.SysFntBig };
                Disc = new IGMDataItem.Icon { Data = Icons.ID.DISC, Pos = new Rectangle(LV_Num.X + 100, LV.Y, 0, 0) };
                Disc_Num = new IGMDataItem.Integer { Pos = new Rectangle(Disc.X + 80, LV.Y, 0, 0) };
                int col3x = SIZE[0].X + SIZE[0].Width - 180;
                Play = new IGMDataItem.Icon { Data = Icons.ID.PLAY, Pos = new Rectangle(col3x, SIZE[0].Y, 0, 0), Palette = 13 };
                Hours = new IGMDataItem.Integer { Pos = new Rectangle(Play.X + 80, SIZE[0].Y, 0, 0), Spaces = 2 };
                Colon = new IGMDataItem.Icon { Data = Icons.ID.Colon, Pos = new Rectangle(Hours.X + 40, SIZE[0].Y, 0, 0), Blink = true, Palette = 13, Faded_Palette = 2, Blink_Adjustment = .5f };
                Mins = new IGMDataItem.Integer { Pos = new Rectangle(Colon.X + 20, SIZE[0].Y, 0, 0), Spaces = 2, Padding = 2 };
                Gil = new IGMDataItem.Integer { Pos = new Rectangle(col3x, LV.Y, 0, 0), Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 8 };
                G = new IGMDataItem.Icon { Data = Icons.ID.G, Pos = new Rectangle(Gil.X + 20 * Gil.Spaces, LV.Y, 0, 0), Palette = 2 };
                const int locationheight = 72;
                Location = new IGMDataItem.Box { Pos = new Rectangle(Face3offsetx, base.Y + base.Height - locationheight, base.Width + base.X - Face3offsetx, locationheight) };

                CONTAINER.OffsetAnchor = new OffsetAnchor();
                foreach (Menu_Base i in ITEM)
                {
                    if (i != null)
                        i.OffsetAnchor = CONTAINER.OffsetAnchor;
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-8, -8);
            }

            private void PageChangeEvent(object sender, Pool.GameChoose.PageInfo pinfo)
            {
                if (ID - 1 < 0) return;
                if (_lastpage != -1)
                {
                    _lastpage = _page;
                    _page = pinfo.PageNumber;
                }
                else
                {
                    _lastpage = _page = pinfo.PageNumber;
                    if (!IsMyPage) Hide();
                    else Show();
                }
                if (_lastpage != _page)
                {
                    if (!pinfo.previous)
                    {
                        //going left to right
                        if (IsMyPage)
                        {
                            Slider.End = Vector2.Zero;
                            if (!Enabled)
                            {
                                Slider.Start = right;
                            }
                            else
                                Slider.Start = CONTAINER.OffsetAnchor;
                            Slider.Restart();

                            Show();
                        }
                        else if (Slider.End == Vector2.Zero)
                        {
                            Slider.End = left;
                            Slider.Start = CONTAINER.OffsetAnchor;
                            Slider.Restart();
                        }
                    }
                    else
                    {
                        //going right to left
                        if (IsMyPage)
                        {
                            Slider.End = Vector2.Zero;
                            if (!Enabled)
                                Slider.Start = left;
                            else
                                Slider.Start = CONTAINER.OffsetAnchor;
                            Slider.Restart();

                            Show();
                        }
                        else if (Slider.End == Vector2.Zero)
                        {
                            Slider.End = right;
                            Slider.Start = CONTAINER.OffsetAnchor;
                            Slider.Restart();
                        }
                    }
                }
                Refresh();
            }

            #endregion Methods
        }
    }
}