using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem.Gradient
{
    public class ATB : Texture
    {
        #region Fields

        private static readonly object Locker = new object();
        private static Texture2D _common;

        #endregion Fields

        #region Constructors

        private ATB()
        {
        }

        #endregion Constructors

        #region Properties

        ///Restriction controls the bounds of the drawing. And Pos is where it will draw.
        ///So if one is set and the other is not than you see no bar.
        public override Rectangle Pos { get => base.Pos; set => Restriction = base.Pos = value; }

        #endregion Properties

        #region Methods

        public static ATB Create(Rectangle? pos = null)
        {
            var r = new ATB()
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
            if (_common != null) return _common;
            lock (Locker)
            {
                const float dark = 0.067f;
                const float fade = 0.933f;
                var total = width;
                var lightLine = new Color(118, 118, 118, 255);
                var darkLine = new Color(58, 58, 58, 255);
                var cFade = new Color[total];
                int i;
                for (i = 0; i < cFade.Length - (dark * total); i++)
                    cFade[i] = Color.Lerp(Color.Black, lightLine, i / (fade * total));

                for (; i < cFade.Length; i++)
                    cFade[i] = darkLine;

                _common = new Texture2D(Memory.graphics.GraphicsDevice, cFade.Length, 1, false, SurfaceFormat.Color);
                _common.SetData(cFade);
            }
            return _common;
        }

        public override void Refresh(Damageable damageable)
        {
            base.Refresh(damageable);
            damageable?.Refresh();
        }

        public override bool Update()
        {
            if (!Enabled) return false;
            if (Damageable == null) return true;
            X = Lerp(Restriction.X - Width, Restriction.X, Damageable.ATBPercent);

            if (Damageable.IsDead)
            {
                //Color = Faded_Color = Color.Red * .5f;
                X = 0;
            }
            else if (Damageable.IsPetrify)
            {
                Color = Faded_Color = Color.Gray * .8f;
            }
            else if ((Damageable.Statuses1 & Kernel.BattleOnlyStatuses.Stop) != 0)
            {
                Color = Faded_Color = Color.DarkBlue * .8f;
            }
            else if ((Damageable.Statuses1 & Kernel.BattleOnlyStatuses.Slow) != 0)
            {
                Color = Faded_Color = Color.DarkCyan * .8f;
            }
            else if ((Damageable.Statuses1 & Kernel.BattleOnlyStatuses.Haste) != 0)
            {
                Color = Faded_Color = Color.Violet * .8f;
            }
            else Color = Faded_Color = Color.Orange * .8f;
            return true;
            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }

        #endregion Methods
    }
}