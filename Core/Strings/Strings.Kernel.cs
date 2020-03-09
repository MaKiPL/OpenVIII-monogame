using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings
    {
        #region Classes

        /// <summary>
        /// <para>Kernel.bin Strings</para>
        /// <para>Has Multi-byte Characters, Requires Namedic</para>
        /// </summary>
        public sealed class Kernel : StringsBase
        {

            #region Constructors


            public static Kernel Load() => Load<Kernel>();

            protected override void DefaultValues() => SetValues(Memory.Archives.A_MAIN, "kernel.bin");

            #endregion Constructors

            #region Properties

            /// <summary>
            /// <para>uint pointer locations, tuple(uint StringLocation,uint get, unit skip)</para>
            /// <para>
            /// So you read the pointers at location, you get so many pointers then skip so many
            /// bytes before getting more pointers. Do this till start of next section.
            /// </para>
            /// </summary>
            /// <remarks>Colly's list of string pointers. Adapted.</remarks>
            /// <see cref="http://www.balamb.pl/qh/kernel-pointers.htm"/>
            public IReadOnlyDictionary<int, (uint StringLocation, uint Get, uint Skip)> StringLocations
            { get; } = new Dictionary<int, (uint StringLocation, uint Get, uint Skip)>(){

                {0, (31,2,4)},
                {1, (32,2,56) },
                {2, (33,2,128) },
                {3, (34,1,18) },//38,58,178, or 78
                {4, (35,1,10) },
                {5, (36,2,20) },
                {6, (37,1,34) },//+1interval 70 //character names here.
                {7, (38,2,20) },
                {8, (39,1,0) },
                {9, (40,1,18) },
                {11, (41,2,4) },
                {12, (42,2,4) },
                {13, (43,2,4) },
                {14, (44,2,4) },
                {15, (45,2,4) },
                {16, (46,2,4) },
                {17, (47,2,4) },
                {18, (48,2,20) },
                {19, (49,2,12) },
                {21, (50,2,20) },
                {22, (51,2,28) },
                {24, (52,2,4) },
                {25, (53,1,18) },
                {28, (54,1,10) },
                {30, (55,1,0) },
            };

            #endregion Properties

            #region Methods

            /// <summary>
            /// Read Section Pointers
            /// </summary>
            /// <param name="br"></param>
            private void GetFileLocations(BinaryReader br)
            {
                uint count = br.ReadUInt32();
                while (count-- > 0)
                {
                    uint seek = br.ReadUInt32();
                    uint length;
                    if (count <= 0) length = (uint)br.BaseStream.Length - seek;
                    else
                    {
                        length = br.ReadUInt32() - seek;
                        br.BaseStream.Seek(-4, SeekOrigin.Current);
                    }
                    StringFiles.SubPositions.Add(Loc.CreateInstance(seek, length));
                }
            }

            /// <summary>
            /// Fetch strings from kernel.bin
            /// </summary>
            /// <see cref="http://www.balamb.pl/qh/kernel-pointers.htm"/>
            protected override void LoadArchiveFiles()
            {
                Settings = (FF8StringReference.Settings.MultiCharByte | FF8StringReference.Settings.Namedic);
                ArchiveBase aw = ArchiveWorker.Load(Archive);
                StringFiles = new StringFile(56);
                byte[] buffer = aw.GetBinaryFile(FileNames[0], true);
                if (buffer == null) return;
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                {
                    GetFileLocations(br);

                    for (int key = 0; key < StringFiles.SubPositions.Count; key++)
                    {
                        if (StringLocations.ContainsKey(key))
                        {
                            Get_Strings_BinMSG(br, FileNames[0], key, StringFiles.SubPositions[(int)(StringLocations[key].Item1)].Seek, StringLocations[key].Item2, StringLocations[key].Item3);
                        }
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}