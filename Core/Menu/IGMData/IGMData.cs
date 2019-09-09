using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenVIII
{
    public class IGMData : Menu_Base
    {
        #region Fields

        public bool[] BLANKS;

        /// <summary>
        /// location of where pointer finger will point.
        /// </summary>
        public Point[] CURSOR;

        public Menu_Base[,] ITEM;

        public int PointerZIndex = byte.MaxValue;

        /// <summary>
        /// Size of the entire area
        /// </summary>
        public Rectangle[] SIZE;

        protected bool skipdata = false;
        protected bool skipsnd = false;
        private int _cursor_select;

        #endregion Fields

        #region Constructors

        public IGMData(int count = 0, int depth = 0, IGMDataItem container = null, int? cols = null, int? rows = null, Characters? character = null, Characters? visablecharacter = null, sbyte? partypos = null)
        {
            Init(character, visablecharacter, partypos);
            Init(count, depth, container, cols, rows);
        }

        #endregion Constructors

        #region Properties

        public int Cols { get; private set; }

        public IGMDataItem CONTAINER { get; set; }

        /// <summary>
        /// Total number of items
        /// </summary>
        public byte Count { get; private set; }

        public int CURSOR_SELECT
        {
            get => GetCursor_select(); set
            {
                if ((Cursor_Status & Cursor_Status.Enabled) != 0 && value >= 0 && value < CURSOR.Length && CURSOR[value] != Point.Zero)
                    SetCursor_select(value);
            }
        }

        public Cursor_Status Cursor_Status { get; set; } = Cursor_Status.Disabled;

        /// <summary>
        /// How many Peices per Item. Example 1 box could have 9 things to draw in it.
        /// </summary>
        public byte Depth { get; private set; }

        public Dictionary<int, FF8String> Descriptions { get; protected set; }

        public int Rows { get; private set; }

        public Table_Options Table_Options { get; set; } = Table_Options.Default;

        public static Point MouseLocation => Menu.MouseLocation;

        public static Vector2 TextScale => Menu.TextScale;

        /// <summary>
        /// Container's Height
        /// </summary>
        public int Height => CONTAINER != null ? CONTAINER.Pos.Height : 0;

        /// <summary>
        /// Container's Width
        /// </summary>
        public int Width => CONTAINER != null ? CONTAINER.Pos.Width : 0;

        /// <summary>
        /// Container's X Position
        /// </summary>
        public int X => CONTAINER != null ? CONTAINER.Pos.X : 0;

        /// <summary>
        /// Container's Y Position
        /// </summary>
        public int Y => CONTAINER != null ? CONTAINER.Pos.Y : 0;

        #endregion Properties

        #region Indexers

        public Menu_Base this[int pos, int i] { get => ITEM[pos, i]; set => ITEM[pos, i] = value; }

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Convert to rectangle based on container.
        /// </summary>
        /// <param name="v">Input data</param>
        public static implicit operator Rectangle(IGMData v) => v.CONTAINER ?? Rectangle.Empty;

        //public object PrevSetting { get; protected set; } = null;
        //public object Setting { get; protected set; } = null;
        public virtual int CURSOR_NEXT()
        {
            if ((Cursor_Status & Cursor_Status.Enabled) != 0)
            {
                int value = GetCursor_select();
                int loop = 0;
                while (true)
                {
                    if (++value >= CURSOR.Length)
                    {
                        value = 0;
                        if (loop++ > 1) break;
                    }
                    if ((CURSOR[value] != Point.Zero && !BLANKS[value])) break;
                }
                SetCursor_select(value);
            }
            return GetCursor_select();
        }

        public virtual int CURSOR_PREV()
        {
            if ((Cursor_Status & Cursor_Status.Enabled) != 0)
            {
                int value = GetCursor_select();
                int loop = 0;
                while (true)
                {
                    if (--value < 0)
                    {
                        value = CURSOR.Length - 1;
                        if (loop++ > 1) break;
                    }
                    if ((CURSOR[value] != Point.Zero && !BLANKS[value])) break;
                }
                SetCursor_select(value);
            }
            return GetCursor_select();
        }

        protected bool DepthFirst = false;

        /// <summary>
        /// Draw all items
        /// </summary>
        public override void Draw()
        {
            if (Enabled)
            {
                if (CONTAINER != null)
                    CONTAINER.Draw();
                bool pointer = false;
                if (!skipdata && ITEM != null)
                    if (DepthFirst)
                        for (int d = 0; d < Depth; d++)
                            for (int i = 0; i < Count; i++)
                            {
                                if (i == PointerZIndex && !pointer)
                                    pointer = DrawPointer();
                                DrawITEM(i, d);
                            }
                    else
                        for (int i = 0; i < Count; i++)
                            for (int d = 0; d < Depth; d++)
                            {
                                if (i == PointerZIndex && !pointer)
                                    pointer = DrawPointer();
                                DrawITEM(i, d);
                            }

                if (!pointer)
                {
                    pointer = DrawPointer();
                }
            }
            bool DrawPointer()
            {
                if ((Cursor_Status & (Cursor_Status.Enabled | Cursor_Status.Draw)) != 0 &&
                    (Cursor_Status & Cursor_Status.Hidden) == 0)
                {
                    this.DrawPointer(CURSOR[CURSOR_SELECT], blink: ((Cursor_Status & Cursor_Status.Blinking) != 0));
                    return true;
                }
                return false;
            }
        }

        public void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false) => Menu.DrawPointer(cursor, offset, blink);

        /// <summary>
        /// Check inputs
        /// </summary>
        /// <returns>True = input detected</returns>
        public override bool Inputs()
        {
            bool ret = false;
            bool mouse = false;
            if ((Cursor_Status & Cursor_Status.Enabled) != 0)
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                if ((Cursor_Status & Cursor_Status.Static) == 0)
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        if (SIZE[i].Contains(MouseLocation) && !SIZE[i].IsEmpty && CURSOR[i] != Point.Zero && !BLANKS[i])
                        {
                            CURSOR_SELECT = i;
                            ret = true;
                            mouse = true;
                        }
                    }
                if (!ret && (Cursor_Status & Cursor_Status.Horizontal) != 0 && (Cursor_Status & Cursor_Status.Static) == 0)
                {
                    if (Input2.DelayedButton(FF8TextTagKey.Left))
                    {
                        CURSOR_PREV();
                        ret = true;
                    }
                    else if (Input2.DelayedButton(FF8TextTagKey.Right))
                    {
                        CURSOR_NEXT();
                        ret = true;
                    }
                }
                if ((!ret && (Cursor_Status & Cursor_Status.Horizontal) == 0 || (Cursor_Status & Cursor_Status.Vertical) != 0) && (Cursor_Status & Cursor_Status.Static) == 0)
                {
                    if (Input2.DelayedButton(FF8TextTagKey.Up))
                    {
                        CURSOR_PREV();
                        ret = true;
                    }
                    else if (Input2.DelayedButton(FF8TextTagKey.Down))
                    {
                        CURSOR_NEXT();
                        ret = true;
                    }
                }
                if (mouse || !ret)
                {
                    if (Input2.DelayedButton(FF8TextTagKey.Confirm))
                    {
                        return Inputs_OKAY();
                    }
                    else if (Input2.DelayedButton(FF8TextTagKey.Cancel))
                    {
                        return Inputs_CANCEL();
                    }
                    else if (Input2.DelayedButton(FF8TextTagKey.Cards))
                    {
                        Inputs_Cards();
                        return true;
                    }
                    else if (Input2.DelayedButton(FF8TextTagKey.Menu))
                    {
                        Inputs_Menu();
                        return true;
                    }
                    else if ((Cursor_Status & Cursor_Status.Horizontal) == 0 && (Cursor_Status & Cursor_Status.Static) == 0)
                    {
                        if (Input2.DelayedButton(FF8TextTagKey.Left))
                        {
                            Inputs_Left();
                            return true;
                        }
                        else if (Input2.DelayedButton(FF8TextTagKey.Right))
                        {
                            Inputs_Right();
                            return true;
                        }
                    }
                }
                if (ret && !mouse)
                {
                    if (!skipsnd)
                        init_debugger_Audio.PlaySound(0);
                }
            }
            skipsnd = false;
            return ret;
        }

        public virtual bool Inputs_CANCEL()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(8);
            return false;
        }

        public virtual void Inputs_Cards()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(0);
        }

        public virtual void Inputs_Left()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(0);
        }

        public virtual void Inputs_Menu()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(31);
        }

        public virtual bool Inputs_OKAY()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(0);
            return false;
        }

        public virtual void Inputs_Right()
        {
            if (!skipsnd)
                init_debugger_Audio.PlaySound(0);
        }

        public virtual void SetModeChangeEvent(ref EventHandler<Enum> eventHandler) => eventHandler += ModeChangeEvent;

        /// <summary>
        /// Things that change on every update.
        /// </summary>
        /// <returns>True = signifigant change</returns>
        public override bool Update()
        {
            bool ret = false;
            if (!skipdata && ITEM != null)
                foreach (Menu_Base i in ITEM)
                {
                    if (i != null)
                        ret = i.Update() || ret;
                }
            return ret;
        }

        protected virtual void DrawITEM(int i, int d) => ITEM[i, d]?.Draw();

        protected int GetCursor_select() => _cursor_select;

        protected void Init(Characters? character, Characters? visablecharacter, sbyte? partypos)
        {
            if (partypos != null)
            {
                Character = Memory.State?.PartyData?[partypos.Value] ?? Characters.Blank;
                VisableCharacter = Memory.State?.Party?[partypos.Value] ?? Character;
                PartyPos = partypos.Value;
            }
            else
            {
                Character = character ?? Characters.Blank;
                VisableCharacter = visablecharacter ?? Character;
                PartyPos = (sbyte)(Memory.State?.PartyData?.FindIndex(x => x.Equals(Character)) ?? -1);
            }
        }

        protected void Init(int count, int depth, IGMDataItem container = null, int? cols = null, int? rows = null)
        {
            CONTAINER = container ?? new IGMDataItem_Empty();
            if (count <= 0 || depth <= 0)
            {
                if (CONTAINER.Pos == Rectangle.Empty)
                {
                    Debug.WriteLine($"{this}:: count {count} or depth {depth}, is invalid must be >= 1, or a CONTAINER {CONTAINER} and CONTAINER.Pos { CONTAINER.Pos.ToString() } must be set instead, Skipping Init()");
                    return;
                }
            }
            else
            {
                SIZE = new Rectangle[count];
                ITEM = new Menu_Base[count, depth];
                CURSOR = new Point[count];

                Count = (byte)count;
                Depth = (byte)depth;
                BLANKS = new bool[count];
                Descriptions = new Dictionary<int, FF8String>(count);
                this.Cols = cols ?? 1;
                this.Rows = rows ?? 1;
            }
            Init();
            Refresh();
            Update();
        }

        /// <summary>
        /// Things that are fixed values at startup.
        /// </summary>
        protected override void Init()
        {
            if (SIZE != null && SIZE.Length > 0)
            {
                for (int i = 0; i < SIZE.Length; i++)
                {
                    int col = (Table_Options & Table_Options.FillRows) != 0 ? i % Cols : i / Rows;
                    int row = (Table_Options & Table_Options.FillRows) != 0 ? i / Cols : i % Rows;
                    if (col < Cols && row < Rows)
                    {
                        if (SIZE[i].IsEmpty) //allows for override a size value before the loop.
                        {
                            SIZE[i] = new Rectangle
                            {
                                X = X + (Width * col) / Cols,
                                Y = Y + (Height * row) / Rows,
                                Width = Width / Cols,
                                Height = Height / Rows,
                            };
                        }
                        CURSOR[i] = Point.Zero;
                        InitShift(i, col, row);
                        CURSOR[i].Y += (int)(SIZE[i].Y + 6 * TextScale.Y);
                        CURSOR[i].X += SIZE[i].X;
                    }
                }
            }
            if (SIZE == null || SIZE.Length == 0 || SIZE[0].IsEmpty)
            {
                if (CURSOR == null || CURSOR.Length == 0 || SIZE.Length == 0)
                {
                    CURSOR = new Point[1];
                    SIZE = new Rectangle[1];
                }
                CURSOR[0].Y = (int)(Y + Height / 2 - 6 * TextScale.Y);
                CURSOR[0].X = X;
                SIZE[0] = new Rectangle(X, Y, Width, Height);
            }
        }

        protected virtual void InitShift(int i, int col, int row)
        {
        }

        protected bool InputITEM(int i, int d, ref bool ret)
        {
            if (ITEM[i, d] != null && ITEM[i, d].Enabled)
            {
                Cursor_Status |= (Cursor_Status.Enabled | Cursor_Status.Blinking);
                ret = ITEM[i, d].Inputs();
                return true;
            }
            return false;
        }

        protected virtual void ModeChangeEvent(object sender, Enum e)
        {
        }

        protected override void RefreshChild()
        {
            base.RefreshChild();
            if (!skipdata)
            {
                if (CONTAINER != null)
                    CONTAINER.Refresh(Character, VisableCharacter);
                if (ITEM != null)
                    for (int i = 0; i < Count; i++)
                        for (int d = 0; d < Depth; d++)
                        {
                            ITEM[i, d]?.Refresh(Character, VisableCharacter);
                        }
            }
        }

        protected virtual void SetCursor_select(int value) => _cursor_select = value;

        #endregion Methods
    }
}