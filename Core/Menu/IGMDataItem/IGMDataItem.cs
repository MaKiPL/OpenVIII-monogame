using Microsoft.Xna.Framework;

namespace OpenVIII
{

    public abstract class IGMDataItem : Menu_Base
    {
        #region Fields


        #endregion Fields

        #region Constructors

        public IGMDataItem(Rectangle? pos = null, Vector2? scale = null)
        {
            Pos = pos ?? Rectangle.Empty;
            Scale = scale ?? TextScale;
        }

        #endregion Constructors

        #region Properties

        public Color Color { get; set; } = Color.White;

        public Vector2 Scale { get; set; }
        public static float Blink_Amount => Menu.Blink_Amount;
        public static float Fade => Menu.Fade;
        public static Vector2 TextScale => Menu.TextScale;

        #endregion Properties

        #region Methods

        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false) => Menu.DrawPointer(cursor, offset, blink);
        public static implicit operator Color(IGMDataItem v) => v.Color;

        public static implicit operator IGMDataItem(IGMData v) => new IGMDataItem_IGMData(v);


        //public virtual object Data { get; public set; }
        //public virtual FF8String Data { get; public set; }
        public override void Draw() { }
        public override bool Inputs() => false;
        protected override void Init() { }
        public override void Refresh()
        { }

        public override bool Update() => false;

        #endregion Methods
    }

}