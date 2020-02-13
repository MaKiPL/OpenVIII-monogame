using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;

namespace OpenVIII
{
    namespace IGMData.Pool
    {
        public class GameChoose : IGMData.Pool.Base<Saves.Data, Saves.Data>
        {
            #region Fields

            private bool first = true;

            #endregion Fields

            #region Events

            private event EventHandler<PageInfo> PageChangeEventHandler;

            #endregion Events

            #region Properties

            public bool Save { get; private set; }

            public byte Slot { get; private set; }

            #endregion Properties

            #region Methods

            public static GameChoose Create(Rectangle pos)
            {
                GameChoose r = Create<GameChoose>(30, 1, new IGMDataItem.Empty { Pos = pos }, 3, 10);
                return r;
            }

            public override bool Inputs()
            {
                if (Input2.DelayedButton(FF8TextTagKey.RotateLeft) || Input2.DelayedButton(FF8TextTagKey.RotateRight))
                {
                    IGM_LGSG.Mode mode = (IGM_LGSG.Mode)Menu.IGM_LGSG.GetMode();
                    AV.Sound.Play(0);
                    mode ^= IGM_LGSG.Mode.Slot1;
                    mode ^= IGM_LGSG.Mode.Slot2;
                    Menu.IGM_LGSG.SetMode(mode);
                    return true;
                }
                else
                    return base.Inputs();
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Menu.IGM_LGSG.SetMode(IGM_LGSG.Mode.Slot |
                        IGM_LGSG.Mode.Choose |
                        (Save ? IGM_LGSG.Mode.Save : IGM_LGSG.Mode.Nothing));
                return true;
            }

            public override bool Inputs_OKAY()
            {
                Saves.Data save = Contents[CURSOR_SELECT];
                if (save != null && save.Characters != null)
                {
                    base.Inputs_OKAY();
                    if (!Save)
                        Memory.State = save.Clone();
                    else
                    {
                        //TODO. Save game here.
                    }
                    Menu.IGM_LGSG.SetMode(
                        IGM_LGSG.Mode.Checking |
                        IGM_LGSG.Mode.Game |
                        (Menu.IGM_LGSG.GetMode().HasFlag(IGM_LGSG.Mode.Slot1) ? IGM_LGSG.Mode.Slot1 : IGM_LGSG.Mode.Slot2) |
                        (Save ? IGM_LGSG.Mode.Save : 0));
                }
                return false;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(IGM_LGSG.Mode))
                {
                    Save = e.HasFlag(IGM_LGSG.Mode.Save);
                    if (e.HasFlag(IGM_LGSG.Mode.Game) && e.HasFlag(IGM_LGSG.Mode.Choose))
                    {
                        if (e.HasFlag(IGM_LGSG.Mode.Slot1))
                            Slot = 0;
                        else
                            Slot = 1;
                        int total = Count - ExtraCount;

                        int r = 0;
                        for (byte i = 0; i < total; i++)
                        {
                            ((GameBlock)ITEM[i, 0]).Refresh(i, Saves.FileList?[Slot, i]);

                            if (r < Contents.Length)
                                Contents[r++] = Saves.FileList?[Slot, i];
                        }
                        Show();
                        Refresh();

                        if (first)
                        {
                            PageChangeEventHandler?.Invoke(this, new PageInfo(Page, false));
                            ITEM[0, 0].Show();
                            ITEM[1, 0].Show();
                            ITEM[2, 0].Show();
                            first = false;
                        }
                    }
                    else
                        Hide();
                }
            }

            public override void Refresh()
            {
                base.Refresh();
                int r = 0;
                foreach (Menu_Base i in ITEM)
                {
                    if (i != null && i.GetType() == typeof(GameBlock) && ((GameBlock)i).ExpectedPageNumber == Page)
                    {
                        Contents[r++] = ((GameBlock)i).Data;
                        if (r >= Contents.Length) break;
                    }
                }
            }

            protected override void Init()
            {
                base.Init();
                RightArrow.Y = Y + Height / 2 - RightArrow.Height / 2;
                LeftArrow.Y = Y + Height / 2 - LeftArrow.Height / 2;

                for (int i = 0; i < Count - ExtraCount; i++)
                {
                    ITEM[i, 0] = GameBlock.Create(SIZE[i % Rows]);
                    ((GameBlock)ITEM[i, 0]).AddPageChangeEvent(ref PageChangeEventHandler);
                    ((GameBlock)ITEM[i, 0]).ParentRows = Rows;
                }
                Cursor_Status &= ~Cursor_Status.Horizontal;
            }

            protected override void InitCursor(int i, int col, int row, bool zero = false) =>
                //base.InitCursor(i, zero);
                CURSOR[i] = new Point(SIZE[i].X + 20, SIZE[i].Y + SIZE[i].Height / 2 - 4);

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-20, 0);
            }

            protected override void PAGE_NEXT()
            {
                do
                {
                    base.PAGE_NEXT();
                    Refresh();
                }
                while (Contents[0] == null && Page != 0);
                PageChangeEventHandler?.Invoke(this, new PageInfo(Page, false));
            }

            protected override void PAGE_PREV()
            {
                do
                {
                    base.PAGE_PREV();
                    Refresh();
                }
                while (Contents[0] == null && Page != 0);

                PageChangeEventHandler?.Invoke(this, new PageInfo(Page, true));
            }

            #endregion Methods

            #region Structs

            public struct PageInfo
            {
                #region Fields

                public int PageNumber;

                //if false, is next
                public bool previous;

                #endregion Fields

                #region Constructors

                public PageInfo(int pageNumber, bool previous = false)
                {
                    PageNumber = pageNumber;
                    this.previous = previous;
                }

                #endregion Constructors
            }

            #endregion Structs
        }
    }
}