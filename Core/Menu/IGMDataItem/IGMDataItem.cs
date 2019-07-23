using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public abstract class IGMDataItem : Menu_Base
    {
        #region Fields

        protected static Texture2D blank;

        //protected T _data;
        protected Rectangle _pos;

        private bool _blink = false;

        #endregion Fields

        #region Constructors

        public IGMDataItem(Rectangle? pos = null, Vector2? scale = null)
        {
            _pos = pos ?? Rectangle.Empty;
            Scale = scale ?? TextScale;
            if (blank == null)
            {
                blank = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                blank.SetData(new Color[] { Color.White });
            }
        }

        #endregion Constructors

        #region Properties

        public virtual bool Blink { get => _blink; set => _blink = value; }
        public float Blink_Adjustment { get; set; }
        public Color Color { get; set; } = Color.White;
        public int Height { get => _pos.Height; set => _pos.Height = value; }
        /// <summary>
        /// Where to draw this item.
        /// </summary>
        public virtual Rectangle Pos { get => _pos; set => _pos = value; }

        public Vector2 Scale { get; set; }
        public int Width { get => _pos.Width; set => _pos.Width = value; }
        public int X { get => _pos.X; set => _pos.X = value; }
        public int Y { get => _pos.Y; set => _pos.Y = value; }
        public static float Blink_Amount => Menu.Blink_Amount;
        public static float Fade => Menu.Fade;
        public static Vector2 TextScale => Menu.TextScale;

        #endregion Properties

        #region Methods

        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false) => Menu.DrawPointer(cursor, offset, blink);

        public static implicit operator Color(IGMDataItem v) => v.Color;

        public static implicit operator IGMDataItem(IGMData v) => new IGMDataItem_IGMData(v);

        public static implicit operator Rectangle(IGMDataItem v) => v.Pos;

        //public virtual object Data { get; public set; }
        //public virtual FF8String Data { get; public set; }
        public override void Draw()
        { }

        public override bool Inputs() => false;

        public override void Refresh()
        { }

        public override bool Update() => false;

        protected override void Init()
        {
        }

        #endregion Methods
    }
}