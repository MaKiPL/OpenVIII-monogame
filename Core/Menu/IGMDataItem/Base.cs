using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.IGMDataItem
{
    
    public abstract class Base : Menu_Base
    {
        #region Fields

        protected static Texture2D blank;


        private bool _blink = false;

        #endregion Fields

        #region Constructors

        public Base(Rectangle? pos = null, Vector2? scale = null)
        {
            Pos = pos ?? Rectangle.Empty;
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
        public Color Faded_Color { get; set; } = Color.White;
        public Vector2 Scale { get; set; }
        public static float Blink_Amount => Menu.Blink_Amount;
        public static float Fade => Menu.Fade;
        public static Vector2 TextScale => Menu.TextScale;

        #endregion Properties

        #region Methods

        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false) => Menu.DrawPointer(cursor, offset, blink);

        public static implicit operator Color(Base v) => v.Color;


        //public virtual object Data { get; public set; }
        //public virtual FF8String Data { get; public set; }
        public override void Draw()
        { }

        public override bool Inputs() => false;
        
        public override bool Update() => false;

        protected override void Init()
        {
        }

        #endregion Methods
    }
}