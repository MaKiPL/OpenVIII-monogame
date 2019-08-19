using System;

namespace OpenVIII
{

        #region Enums

        [Flags]
        public enum Table_Options
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            Default = 0x0,

            /// <summary>
            /// Default fills 1 col at a time. This will make it fill 1 row at a time.
            /// </summary>
            FillRows = 0x1,
        }

        #endregion Enums

    
}