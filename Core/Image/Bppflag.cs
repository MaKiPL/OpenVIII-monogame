using System;

namespace OpenVIII
{

        /// <summary>
        /// BPP indicator
        /// <para>4 BPP is default</para>
        /// <para>If 8 and 16 are set then it's 24/except in fields</para>
        /// <para>CLP should always be set for 4 and 8/except in fields, fields always have a clut even if not used.</para>
        /// </summary>
        [Flags]
        public enum Bppflag : byte
        {
            /// <summary>
            /// <para>4 BPP</para>
            /// <para>This is 0 so it will show as unset.</para>
            /// </summary>
            _4bpp = 0b0,

            /// <summary>
            /// <para>8 BPP</para>
            /// <para>if _8bpp and _16bpp are set then it's 24 bit</para>
            /// </summary>
            _8bpp = 0b1,

            /// <summary>
            /// <para>16 BPP</para>
            /// <para>if _8bpp and _16bpp are set then it's 24 bit</para>
            /// </summary>
            _16bpp = 0b10,

            /// <summary>
            /// <para>24 BPP / not used in fields</para>
            /// <para>Both flags must be set for this to be right</para>
            /// </summary>
            _24bpp = _8bpp | _16bpp,

            /// <summary>
            /// Color Lookup table Present / not used in fields. Fields have clut even if they aren't used.
            /// </summary>
            CLP = 0b1000,
        }

}