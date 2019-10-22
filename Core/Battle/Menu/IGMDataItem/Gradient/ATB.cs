using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem.Gradient
{
    public class ATB : Texture
    {
        #region Constructors

        private ATB()
        {
        }

        #endregion Constructors

        #region Methods

        public static ATB Create(Rectangle? pos = null)
        {
            ATB r = new ATB()
            {
                _pos = pos ?? Rectangle.Empty,
                Restriction = pos ?? Rectangle.Empty,
            };
            r.Data = ThreadUnsafeOperations(r.Width);
            r.Width = r.Data.Width;
            return r;
        }

        private static Texture2D common;

        public static Texture2D ThreadUnsafeOperations(int width)
        {
            lock (locker)
            {
                if (common == null)
                {
                    if (Memory.IsMainThread)
                    {
                        float dark = 0.067f;
                        float fade = 0.933f;
                        int total = width;
                        Color lightline = new Color(118, 118, 118, 255);
                        Color darkline = new Color(58, 58, 58, 255);
                        Color[] cfade = new Color[total];
                        int i;
                        for (i = 0; i < cfade.Length - (dark * total); i++)
                            cfade[i] = Color.Lerp(Color.Black, lightline, i / (fade * total));

                        for (; i < cfade.Length; i++)
                            cfade[i] = darkline;

                        common = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1, false, SurfaceFormat.Color);
                        common.SetData(cfade);
                    }
                    else throw new Exception("Must be in main thread!");
                }
            }
            return common;
        }

        private static object locker = new object();

        //public override int Width { get => Data.Width; }
        public override void Refresh(Damageable damageable)
        {
            base.Refresh(damageable);
            damageable.Refresh();
        }

        public override bool Update()
        {
            if (Enabled)
            {
                if (Damageable != null)
                {
                    Damageable.Update();
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
                    else if ((Damageable.Statuses1 & Kernel_bin.Battle_Only_Statuses.Stop) != 0)
                    {
                        Color = Faded_Color = Color.DarkBlue * .8f;
                    }
                    else if ((Damageable.Statuses1 & Kernel_bin.Battle_Only_Statuses.Slow) != 0)
                    {
                        Color = Faded_Color = Color.DarkCyan * .8f;
                    }
                    else if ((Damageable.Statuses1 & Kernel_bin.Battle_Only_Statuses.Haste) != 0)
                    {
                        Color = Faded_Color = Color.Violet * .8f;
                    }
                    else Color = Faded_Color = Color.Orange * .8f;
                }
                return true;
            }
            return false;
            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }

        #endregion Methods
    }
}