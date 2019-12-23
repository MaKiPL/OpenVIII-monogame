using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Fields

        /// <summary>
        /// Converts to GF value from GFflag using a dictionay.
        /// </summary>
        public static readonly IReadOnlyDictionary<GFflags, GFs> ConvertGFEnum = Enum.GetValues(typeof(GFflags)).Cast<GFflags>().Where(x => x > 0).ToDictionary(x => x, x => ConvertGFEnumLog(x));

        #endregion Fields

        #region Enums

        [Flags]
        public enum GFflags : ushort
        {
            None = 0,
            Quezacotl = 0x1,
            Shiva = 0x2,
            Ifrit = 0x4,
            Siren = 0x8,
            Brothers = 0x10,
            Diablos = 0x20,
            Carbuncle = 0x40,
            Leviathan = 0x80,
            Pandemona = 0x100,
            Cerberus = 0x200,
            Alexander = 0x400,
            Doomtrain = 0x800,
            Bahamut = 0x1000,
            Cactuar = 0x2000,
            Tonberry = 0x4000,
            Eden = 0x8000,
        }

        #endregion Enums

        #region Methods

        /// <summary>
        /// Converts to GF value from GFflag using math.
        /// </summary>
        /// <param name="flag">must be only one or all flags set</param>
        /// <returns></returns>
        /// <remarks>assuming the values are always in the same order</remarks>
        public static GFs ConvertGFEnumLog(GFflags flag)
        {
            if (flag == (GFflags)0xFFFF)
                return GFs.All;
            if ((flag & (flag - 1)) != 0)
                throw new Exception("ConvertGFEnumLog :: must only have one flag set");
            return flag.Equals(GFflags.None) ? GFs.Blank : (GFs)checked((byte)(Math.Log((double)flag, 2d)));
        }

        /// <summary>
        /// Converts to GF value to GFflag using math.
        /// </summary>
        /// <param name="gf">gf value</param>
        /// <returns></returns>
        /// <remarks>assuming the values are always in the same order</remarks>
        public static GFflags ConvertGFEnumLog(GFs gf)
        {
            if (gf.Equals(GFs.Blank)) return GFflags.None;
            if (gf.Equals(GFs.All)) return (GFflags)0xFFFF;
            return (GFflags)checked((int)Math.Pow(2, (double)gf));
        }

        #endregion Methods
    }
}