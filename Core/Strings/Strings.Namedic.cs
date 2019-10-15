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
            public Namedic()
            { }

            public static Namedic Load() => Load<Namedic>();

            protected override void DefaultValues() =>
                SetValues(Memory.Archives.A_MAIN, "namedic.bin");

            protected override void GetFileLocations(BinaryReader br)
            {
            }

            protected override void LoadArchiveFiles() => LoadArchiveFiles_Simple();
        }
    }
}