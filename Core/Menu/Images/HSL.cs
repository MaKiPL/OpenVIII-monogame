using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    /// <summary>
    /// </summary>
    /// <seealso cref="https://medium.com/@donatbalipapp/colours-maths-90346fb5abda"/>
    /// <see cref="https://www.rapidtables.com/convert/color/rgb-to-hsl.html"/>
    /// <seealso cref="https://en.wikipedia.org/wiki/HSL_and_HSV"/>
    public struct HSL
    {
        #region Fields

        /// <summary>
        /// Alpha
        /// </summary>
        /// <remarks>Percent</remarks>
        public float A;

        /// <summary>
        /// Hue
        /// </summary>
        /// <remarks>Degrees</remarks>
        public float H;

        /// <summary>
        /// Luminosity
        /// </summary>
        /// <remarks>Percent</remarks>
        public float L;

        /// <summary>
        /// Saturation
        /// </summary>
        /// <remarks>Percent</remarks>
        public float S;

        #endregion Fields

        #region Constructors

        public HSL(Color @in)
        {
            float cMax = @in.Max() / 255f;
            float cMin = @in.Min() / 255f;
            float delta = cMax - cMin;
            float R = @in.R / 255f, G = @in.G / 255f, B = @in.B / 255f;
            H = Hue();
            float l = L = Luminosity();
            S = Saturation();
            A = @in.A / 255f;

            float Hue()
            {
                float ret = 0f;
                if (delta == 0)
                { }
                else if (cMax == R)
                {
                    ret = 60f * (((G - B) / delta) % 6);
                }
                else if (cMax == G)
                {
                    ret = 60f * (((B - R) / delta) + 2);
                }
                else if (cMax == B)
                {
                    ret = 60f * (((R - G) / delta) + 4);
                }
                return ret;
            }
            float Luminosity() => (cMax + cMin) / 2;
            float Saturation() => delta == 0 || l == 1 ? 0f : (delta) / (1 - Math.Abs(l * 2 - 1));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// HSL to Color
        /// </summary>
        /// <see cref="https://www.rapidtables.com/convert/color/hsl-to-rgb.html"/>
        public static implicit operator Color(HSL @in)
        {
            float C = (1 - Math.Abs(2 * @in.L - 1)) * @in.S;
            float H = @in.H / 60f;
            float X = C * (1 - Math.Abs(H % 2 - 1));
            float m = @in.L - C / 2;
            Vector3 rgb = getRGB();

            return new Color()
            {
                A = (byte)(@in.A * 255),
                R = (byte)Math.Round(rgb.X),
                G = (byte)Math.Round(rgb.Y),
                B = (byte)Math.Round(rgb.Z),
            };
            Vector3 getPrime()
            {
                if (H >= 0)
                {
                    if (H <= 1)
                        return new Vector3(C, X, 0);
                    else if (H <= 2)
                        return new Vector3(X, C, 0);
                    else if (H <= 3)
                        return new Vector3(0, C, X);
                    else if (H <= 4)
                        return new Vector3(0, X, C);
                    else if (H <= 5)
                        return new Vector3(X, 0, C);
                    else if (H <= 6)
                        return new Vector3(C, 0, X);
                }
                return Vector3.Zero;
            }
            Vector3 getRGB()
            {
                Vector3 prime = getPrime();
                prime += new Vector3(m);
                prime *= new Vector3(255);
                return prime;
            }
        }

        public static implicit operator HSL(Color @in) => new HSL(@in);

        public override string ToString() => $"{H}° {S * 100}% {L * 100}% {A * 100}%";

        #endregion Methods
    }

    public static class ColorExt
    {
        #region Methods

        public static HSL HSL(this Color @in) => new HSL(@in);

        public static byte Max(this Color @in) => (byte)MathHelper.Clamp((MathHelper.Max(MathHelper.Max(@in.R, @in.G), @in.B)), 0, 255);

        public static byte Min(this Color @in) => (byte)MathHelper.Clamp((MathHelper.Min(MathHelper.Min(@in.R, @in.G), @in.B)), 0, 255);

        #endregion Methods
    }
}