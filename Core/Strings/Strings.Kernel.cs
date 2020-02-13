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
        #region Classes

        /// <summary>
        /// <para>Kernel.bin Strings</para>
        /// <para>Has Multibyte Characters, Requires Namedic</para>
        /// </summary>
        public class Kernel : StringsBase
        {
            #region Fields

            protected uint[] StringsPadLoc;

            #endregion Fields

            #region Constructors

            public Kernel()
            { }

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
            public Dictionary<uint, Tuple<uint, uint, uint>> StringLocations
            { get; private set; }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Read Section Pointers
            /// </summary>
            /// <param name="br"></param>
            protected override void GetFileLocations(BinaryReader br)
            {
                uint count = br.ReadUInt32();
                while (count-- > 0)
                {
                    Loc l = new Loc { seek = br.ReadUInt32() };
                    if (count <= 0) l.length = (uint)br.BaseStream.Length - l.seek;
                    else
                    {
                        l.length = br.ReadUInt32() - l.seek;
                        br.BaseStream.Seek(-4, SeekOrigin.Current);
                    }
                    Files.subPositions.Add(l);
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
                Files = new StringFile(56);
                MemoryStream ms = null;
                byte[] buffer = aw.GetBinaryFile(Filenames[0], true);
                if (buffer != null)
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                    {
                        GetFileLocations(br);
                        //index, grab, skip
                        StringLocations = new Dictionary<uint, Tuple<uint, uint, uint>> {
                        //working
                        {0, new Tuple<uint, uint, uint>(31,2,4) },
                        {1, new Tuple<uint, uint, uint>(32,2,56) },
                        {2, new Tuple<uint, uint, uint>(33,2,128) },
                        {3, new Tuple<uint, uint, uint>(34,1,18) },//38,58,178, or 78
                        {4, new Tuple<uint, uint, uint>(35,1,10) },
                        {5, new Tuple<uint, uint, uint>(36,2,20) },
                        {6, new Tuple<uint, uint, uint>(37,1,34) },//+1interval 70 //character names here.
                        {7, new Tuple<uint, uint, uint>(38,2,20) },
                        {8, new Tuple<uint, uint, uint>(39,1,0) },
                        {9, new Tuple<uint, uint, uint>(40,1,18) },
                        {11, new Tuple<uint, uint, uint>(41,2,4) },
                        {12, new Tuple<uint, uint, uint>(42,2,4) },
                        {13, new Tuple<uint, uint, uint>(43,2,4) },
                        {14, new Tuple<uint, uint, uint>(44,2,4) },
                        {15, new Tuple<uint, uint, uint>(45,2,4) },
                        {16, new Tuple<uint, uint, uint>(46,2,4) },
                        {17, new Tuple<uint, uint, uint>(47,2,4) },
                        {18, new Tuple<uint, uint, uint>(48,2,20) },
                        {19, new Tuple<uint, uint, uint>(49,2,12) },
                        {21, new Tuple<uint, uint, uint>(50,2,20) },
                        {22, new Tuple<uint, uint, uint>(51,2,28) },
                        {24, new Tuple<uint, uint, uint>(52,2,4) },
                        {25, new Tuple<uint, uint, uint>(53,1,18) },
                        {28, new Tuple<uint, uint, uint>(54,1,10) },
                        {30, new Tuple<uint, uint, uint>(55,1,0) },
                    };

                        for (uint key = 0; key < Files.subPositions.Count; key++)
                        {
                            Loc fpos = Files.subPositions[(int)key];
                            if (StringLocations.ContainsKey(key))
                            {
                                Get_Strings_BinMSG(br, Filenames[0], key, Files.subPositions[(int)(StringLocations[key].Item1)].seek, StringLocations[key].Item2, StringLocations[key].Item3);
                            }
                        }
                        ms = null;
                    }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}