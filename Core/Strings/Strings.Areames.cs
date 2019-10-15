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
            public Areames()
            { }

            public static Areames Load() => Load<Areames>();

            protected override void DefaultValues() => SetValues(Memory.Archives.A_MENU, "areames.dc1");

            protected override void GetFileLocations(BinaryReader br)
            {
            }

            protected override void LoadArchiveFiles()
            {
                Settings = FF8StringReference.Settings.Namedic;
                LoadArchiveFiles_Simple();
            }
        }
    }
}