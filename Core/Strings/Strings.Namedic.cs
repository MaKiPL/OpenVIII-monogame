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

        public class Namedic : StringsBase
        {
            public Namedic() : base(Memory.Archives.A_MAIN, "namedic.bin")
            {
            }

            protected override void GetFileLocations(BinaryReader br) { }
            protected override void Init() => simple_init();
        }
    }
}
