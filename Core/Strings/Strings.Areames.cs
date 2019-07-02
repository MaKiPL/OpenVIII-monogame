using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings
    {

        /// <summary>
        /// <para>Area Names</para>
        /// <para>Requires Namedic</para>
        /// </summary>
        public class Areames : StringsBase
        {
            public Areames() : base(Memory.Archives.A_MENU, "areames.dc1")
            {
            }

            protected override void GetFileLocations(BinaryReader br) { }
            protected override void Init()
            {
                Settings = FF8StringReference.Settings.Namedic;
                simple_init();
            }
        }
    }
}
