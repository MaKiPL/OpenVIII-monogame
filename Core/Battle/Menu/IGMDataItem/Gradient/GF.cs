using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem.Gradient
{
    public class GF : Texture
    {
        #region Fields

        private static Texture2D common;

        private static object locker = new object();

        #endregion Fields

        #region Constructors

        private GF()
        {
        }

        #endregion Constructors

        #region Properties

        ///Restriction controls the bounds of the drawing. And Pos is where it will draw.
        ///So if one is set and the other is not than you see no bar.
        public override Rectangle Pos { get => base.Pos; set => Restriction = base.Pos = value; }

        #endregion Properties

        #region Methods

        public static GF Create(Rectangle? pos = null)
        {
            var r = new GF()
            {
                _pos = pos ?? Rectangle.Empty,
                Restriction = pos ?? Rectangle.Empty,
            };
            Memory.MainThreadOnlyActions.Enqueue(
            () =>
            {
                r.Data = ThreadUnsafeOperations(r.Width);
                r.Width = r.Data.Width;
            }
            );
            return r;
        }

        public static Texture2D ThreadUnsafeOperations(int width)
        {
            lock (locker)
            {
                if (common == null)
                {
                    //if (Memory.IsMainThread)
                    //{
                    var dark = 0.00f;
                    var fade = 1.00f;
                    var total = width;
                    var lightline = new Color(1, 1, 255, 255);
                    var darkline = new Color(1, 1, 255, 255);
                    var fadeto = new Color(221, 237, 237, 255);
                    var cfade = new Color[total];
                    int i;
                    for (i = 0; i < cfade.Length - (dark * total); i++)
                        cfade[i] = Color.Lerp(lightline, fadeto, i / (fade * total));

                    for (; i < cfade.Length; i++)
                        cfade[i] = darkline;

                    common = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1, false, SurfaceFormat.Color);
                    common.SetData(cfade);
                    //}
                    //else throw new Exception("Must be in main thread!");
                }
            }
            return common;
        }

        //public override int Width { get => Data.Width; }
        public override void Refresh(Damageable damageable)
        {
            base.Refresh(damageable);
            damageable?.Refresh();
        }

        public override bool Update() => Update(null);

        public bool Update(float? Percent = null)
        {
            if (Enabled)
            {
                if (Damageable != null && (Percent != null || Damageable.SummonedGF != null))
                {
                    var r = Restriction;
                    r.Width = Lerp(Width, 0, Percent ?? Damageable.SummonedGF.ATBPercent);
                    Restriction = r;

                    Color = Faded_Color = Color.White * .8f;
                    if (Damageable.IsDead)
                    {
                        //Color = Faded_Color = Color.Red * .5f;
                        X = 0;
                    }
                }
                return true;
            }
            return false;
            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }

        #endregion Methods
    }
}