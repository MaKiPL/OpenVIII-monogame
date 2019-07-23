using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    /// <summary>
    /// Root class all menu objects can grow from.
    /// </summary>
    public abstract class Menu_Base
    {

        #region Fields

        public Point[] CURSOR;
        private int _cursor_select;
        private Characters character = Characters.Blank;
        private Characters visableCharacter = Characters.Blank;
        private Rectangle _pos;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Character who has the junctions and inventory. Same as VisableCharacter unless TeamLaguna.
        /// </summary>
        public Characters Character
        {
            get => character; protected set
            {
                if (character != value && value != Characters.Blank)
                    character = value;
            }
        }

        public Menu_Base CONTAINER { get; set; }
        public int CURSOR_SELECT
        {
            get => GetCursor_select(); set
            {
                if ((Cursor_Status & Cursor_Status.Enabled) != 0 && value >= 0 && value < CURSOR.Length && CURSOR[value] != Point.Zero)
                    SetCursor_select(value);
            }
        }

        public Cursor_Status Cursor_Status { get; protected set; } = Cursor_Status.Disabled;
        /// <summary>
        /// If enabled the menu is visable and all functionality works. Else everything is hidden and
        /// nothing functions.
        /// </summary>
        public bool Enabled { get; private set; } = true;

        public virtual Rectangle Pos { get => _pos; set => _pos = value; }

        public virtual int Height { get => Pos.Height; set => _pos.Height = value; }
        public virtual int Width { get => Pos.Width; set => _pos.Width = value; }
        public virtual int X { get => Pos.X; set => _pos.X = value; }
        public virtual int Y { get => Pos.Y; set => _pos.Y = value; }
        public static implicit operator Rectangle(Menu_Base v) => v.Pos;
        /// <summary>
        /// Required to support Laguna's Party. They have unique stats but share junctions and inventory.
        /// </summary>
        public Characters VisableCharacter
        {
            get => visableCharacter; protected set
            {
                if (visableCharacter != value && value != Characters.Blank)
                    visableCharacter = value;
            }
        }

        #endregion Properties

        #region Methods

        public abstract void Draw();

        /// <summary>
        /// Hide object prevents drawing, update, inputs.
        /// </summary>
        public virtual void Hide() => Enabled = false;

        public abstract bool Inputs();

        public abstract void Refresh();

        public virtual void Refresh(Characters character, Characters? visableCharacter)
        {
            Character = character;
            VisableCharacter = visableCharacter ?? character;
            Refresh();
        }

        public virtual void SetModeChangeEvent(ref EventHandler<Enum> modeChangeHandler)
        {
        }

        /// <summary>
        /// Show object enables drawing, update, inputs.
        /// </summary>
        public virtual void Show() => Enabled = true;

        public abstract bool Update();

        protected int GetCursor_select() => _cursor_select;
        protected abstract void Init();

        protected virtual void SetCursor_select(int value) => _cursor_select = value;

        #endregion Methods
    }
}