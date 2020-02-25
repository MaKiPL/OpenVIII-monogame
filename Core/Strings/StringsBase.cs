using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings
    {
        #region Classes

        public abstract class StringsBase
        {
            #region Fields

            protected Memory.Archive Archive;
            protected string[] Filenames;
            protected StringFile Files;
            protected FF8StringReference.Settings Settings;

            #endregion Fields

            #region Constructors

            protected StringsBase()
            {
            }

            protected void SetValues(Memory.Archive archive, params string[] filenames)
            {
                Memory.Log.WriteLine($"{nameof(StringsBase)}::{nameof(SetValues)} Archive= \"{archive}\", Files= {{{string.Join(", ", filenames)}}}");

                Archive = archive;
                Filenames = filenames;
            }

            #endregion Constructors

            #region Indexers

            public FF8StringReference this[uint sectionid, int stringid] => Files?[sectionid, stringid];

            #endregion Indexers

            #region Methods

            public Memory.Archive GetArchive() => Archive;

            public IReadOnlyList<string> GetFilenames() => Filenames;

            public StringFile GetFiles() => Files;

            /// <summary>
            /// <para>
            /// So you read the pointers at location, you get so many pointers then skip so many
            /// bytes before getting more pointers. Do this till start of next section.
            /// </para>
            /// </summary>
            /// <param name="br">BinaryReader where data is.</param>
            /// <param name="filename">file you are reading from</param>
            /// <param name="PointerStart">Section where pointers are.</param>
            /// <param name="StringStart">Section where strings are</param>
            /// <param name="grab">Get so many pointers</param>
            /// <param name="skip">Then skip so many bytes</param>
            protected void Get_Strings_BinMSG(BinaryReader br, string filename, uint PointerStart, uint StringStart, uint grab = 0, uint skip = 0)
            {
                Loc fpos = Files.subPositions[(int)PointerStart];
                if (fpos.seek > br.BaseStream.Length || fpos.seek < 0) return;
                br.BaseStream.Seek(fpos.seek, SeekOrigin.Begin);
                if (Files.sPositions.ContainsKey(PointerStart))
                {
                }
                else
                {
                    ushort b = 0;
                    ushort last = b;
                    if (!Files.sPositions.ContainsKey(PointerStart))
                    {
                        Files.sPositions.Add(PointerStart, new List<FF8StringReference>());
                        uint g = 1;
                        while (br.BaseStream.Position < fpos.max)
                        {
                            b = br.ReadUInt16();
                            if (last > b)
                                break;
                            else
                            {
                                if (b != 0xFFFF)
                                {
                                    Files.sPositions[PointerStart].Add(new FF8StringReference(Archive, filename, b + StringStart, settings: Settings));
                                    last = b;
                                }
                                else
                                    Files.sPositions[PointerStart].Add(null);
                                if (grab > 0 && ++g > grab)
                                {
                                    br.BaseStream.Seek(skip, SeekOrigin.Current);
                                    g = 1;
                                }
                            }
                        }
                    }
                }
            }

            protected void Get_Strings_ComplexStr(BinaryReader br, string filename, uint key, List<uint> list)
            {
                uint[] fPaddings;
                fPaddings = mngrp_read_padding(br, Files.subPositions[(int)key], 1);
                Files.sPositions.Add(key, new List<FF8StringReference>());
                if (fPaddings == null) return;
                for (uint p = 0; p < fPaddings.Length; p += 2)
                {
                    key = list[(int)fPaddings[(int)p + 1]];
                    Loc fpos = Files.subPositions[(int)key];
                    uint fpad = fPaddings[p] + fpos.seek;
                    br.BaseStream.Seek(fpad, SeekOrigin.Begin);
                    if (!Files.sPositions.ContainsKey(key))
                        Files.sPositions.Add(key, new List<FF8StringReference>());
                    br.BaseStream.Seek(fpad + 6, SeekOrigin.Begin);
                    //byte[] UNK = br.ReadBytes(6);
                    ushort len = br.ReadUInt16();
                    uint stop = (uint)(br.BaseStream.Position + len - 9); //6 for UNK, 2 for len 1, for end null
                    Files.sPositions[key].Add(new FF8StringReference(Archive, filename, (uint)br.BaseStream.Position, settings: Settings));
                    //entry contains possible more than one string so I am scanning for null
                    while (br.BaseStream.Position + 1 < stop)
                    {
                        byte b = br.ReadByte();
                        if (b == 0) Files.sPositions[key].Add(new FF8StringReference(Archive, filename, (uint)br.BaseStream.Position, settings: Settings));
                    }
                }
            }

            /// <summary>
            /// TODO: make this work with more than one file.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="spos"></param>
            /// <param name="key"></param>
            /// <param name="pad"></param>
            protected void Get_Strings_Offsets(BinaryReader br, string filename, uint key, bool pad = false)
            {
                Loc fpos = Files.subPositions[(int)key];
                uint[] fPaddings = pad ? mngrp_read_padding(br, fpos) : (new uint[] { 1 });
                if (fPaddings == null) return;
                Files.sPositions.Add(key, new List<FF8StringReference>());
                for (uint p = 0; p < fPaddings.Length; p++)
                {
                    if (fPaddings[p] <= 0) continue;
                    uint fpad = pad ? fPaddings[p] + fpos.seek : fpos.seek;
                    if (fpad > br.BaseStream.Length || fpad < 0) return;
                    br.BaseStream.Seek(fpad, SeekOrigin.Begin);
                    if (br.BaseStream.Position + 4 < br.BaseStream.Length)
                    {
                        int count = br.ReadUInt16();
                        for (int i = 0; i < count && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                        {
                            uint c = br.ReadUInt16();
                            if (c < br.BaseStream.Length && c != 0)
                            {
                                c += fpad;
                                Files.sPositions[key].Add(new FF8StringReference(Archive, filename, c, settings: Settings));
                            }
                        }
                    }
                }
            }

            protected abstract void GetFileLocations(BinaryReader br);

            protected abstract void LoadArchiveFiles();

            protected uint[] mngrp_read_padding(BinaryReader br, Loc fpos, int type = 0)
            {
                uint[] fPaddings = null;
                if (fpos.seek < 0 || fpos.seek > br.BaseStream.Length) return null;
                br.BaseStream.Seek(fpos.seek, SeekOrigin.Begin);
                uint size = type == 0 ? br.ReadUInt16() : br.ReadUInt32();
                fPaddings = new uint[type == 0 ? size : size * type * 2];
                for (int i = 0; i < fPaddings.Length; i += 1 + type)
                {
                    fPaddings[i] = br.ReadUInt16();
                    if (type == 0 && fPaddings[i] + fpos.seek >= fpos.max)
                        fPaddings[i] = 0;
                    //if (fPaddings[i] != 0)
                    //    fPaddings[i] += fpos.seek;
                    for (int j = 1; j < type + 1; j++)
                    {
                        fPaddings[i + j] = br.ReadUInt16();
                    }
                }
                return fPaddings;
            }

            protected abstract void DefaultValues();

            public static T Load<T>() where T : StringsBase, new()
            {
                Memory.Log.WriteLine($"{nameof(StringsBase)} :: {nameof(Load)} :: {typeof(T)}");
                T r = new T();
                r.DefaultValues();
                r.LoadArchiveFiles();
                return r;
            }

            protected void LoadArchiveFiles_Simple()
            {
                ArchiveBase aw = ArchiveWorker.Load(Archive, true);
                MemoryStream ms;
                byte[] buffer = aw.GetBinaryFile(Filenames[0],true);
                if (buffer != null)
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer, true)))
                    {
                        Files = new StringFile(1);
                        Files.subPositions.Add(new Loc { seek = 0, length = uint.MaxValue });
                        Get_Strings_Offsets(br, Filenames[0], 0);
                        ms = null;
                    }
            }
        }

        #endregion Methods
    }

    #endregion Classes
}