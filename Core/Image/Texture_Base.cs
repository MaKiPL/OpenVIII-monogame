using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{

    public abstract class Texture_Base
    {
        #region Fields

        private const ushort blue_mask = 0x7C00;
        private const ushort green_mask = 0x3E0;
        private const ushort red_mask = 0x1F;
        /// <summary>
        /// Special Transparency Processing bit.
        /// </summary>
        /// <remarks>
        /// The STP (special transparency processing) bit has varying special meanings. Depending on
        /// the current transparency processing mode, it denotes if pixels of this color should be
        /// treated as transparent or not. If transparency processing is enabled, pixels of this
        /// color will be rendered transparent if the STP bit is set. A special case is black pixels
        /// (RGB 0,0,0), which by default are treated as transparent by the PlayStation unless the
        /// STP bit is set.
        /// </remarks>
        /// <see cref="http://www.psxdev.net/forum/viewtopic.php?t=109"/>
        /// <seealso cref="http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt"/>
        private const ushort STP_mask = 0x8000;

        #endregion Fields

        #region Properties

        public abstract int GetClutCount { get; }
        public abstract int GetHeight { get; }
        public abstract int GetOrigX { get; }
        public abstract int GetOrigY { get; }
        public abstract int GetWidth { get; }

        #endregion Properties

        #region Methods

        public abstract Color[] GetClutColors(ushort clut);

        public abstract Texture2D GetTexture();

        public abstract Texture2D GetTexture(Color[] colors = null);

        public abstract Texture2D GetTexture(ushort? clut = null);

        public abstract void Save(string path);
        /// <summary>
        /// Convert ABGR1555 color to RGBA 32bit color
        /// </summary>
        /// <remarks>
        /// FromPsColor from TEX does the same thing I think. Unsure which is better. I like the masks
        /// </remarks>
        protected static Color ABGR1555toRGBA32bit(ushort pixel)
        {
            if (pixel == 0) return Color.TransparentBlack;
            //https://docs.microsoft.com/en-us/windows/win32/directshow/working-with-16-bit-rgb
            // had the masks. though they were doing rgb but we are doing bgr so i switched red and blue.
            return new Color
            {
                R = (byte)MathHelper.Clamp((pixel & red_mask) << 3, 0, 255),
                G = (byte)MathHelper.Clamp(((pixel & green_mask) >> 5) << 3, 0, 255),
                B = (byte)MathHelper.Clamp(((pixel & blue_mask) >> 10) << 3, 0, 255),
                A = 255
            };
        }

        /// <summary>
        /// Get Special Transparency Processing bit.
        /// </summary>
        /// <param name="pixel"> 16 bit color</param>
        /// <returns>true if bit is set and not black, otherwise false.</returns>
        protected static bool GetSTP(ushort pixel)
        {
            // I set this to always return false for black. As it's transparency is handled in
            // conversion method.
            return (pixel & 0x7FFF) == 0 ? false : ((pixel & STP_mask) >> 15) > 0;
            //If color is not black and STP bit on, possible semi-transparency.
            //http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt
            //4 modes psx could use for Semi-Transparency. I donno which one ff8 uses or if it uses only one.
            //If Black is transparent when bit is off. And visible when bit is on.
        }

        ///// <summary>
        ///// Ratio needed to convert 16 bit to 32 bit color
        ///// </summary>
        ///// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.h"/>
        //public const double COEFF_COLOR = (double)255 / 31;

        ///// <summary>
        ///// converts 16 bit color to 32bit with alpha. alpha needs to be set true manually per pixel
        ///// unless you know the color value.
        ///// </summary>
        ///// <param name="color">16 bit color</param>
        ///// <param name="useAlpha">area is visable or not</param>
        ///// <returns>byte[4] red green blue alpha, i think</returns>
        ///// <see cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.cpp"/>
        //public static Color FromPsColor(ushort color, bool useAlpha = false) => new Color((byte)Math.Round((color & 31) * COEFF_COLOR), (byte)Math.Round(((color >> 5) & 31) * COEFF_COLOR), (byte)Math.Round(((color >> 10) & 31) * COEFF_COLOR), (byte)(color == 0 && useAlpha ? 0 : 255));
        #endregion Methods
    }
}