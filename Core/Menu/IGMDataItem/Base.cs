using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem
{
    public abstract class Base : Menu_Base, IDisposable
    {
        #region Fields

        protected static Texture2D blank;

        private bool _blink = false;

        #endregion Fields

        #region Constructors

        public Base(Rectangle? pos = null, Vector2? scale = null)
        {
            _pos = pos ?? Rectangle.Empty;
            Scale = scale ?? TextScale;

        }

        #endregion Constructors

        #region Properties

        public virtual bool Blink { get => _blink; set => _blink = value; }
        public float Blink_Adjustment { get; set; } = 1f;
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
            if (blank == null)
            {
                blank = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                blank.SetData(new Color[] { Color.White });
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (blank != null && !blank.IsDisposed)
                {
                    blank.Dispose();
                    blank = null;
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Base()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        #endregion Methods
    }
}