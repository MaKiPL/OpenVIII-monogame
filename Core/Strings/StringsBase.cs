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

        public abstract class StringsBase
        {
            #region Fields

            protected Memory.Archive Archive;
            protected string[] FileNames;
            protected StringFile StringFiles;
            protected FF8StringReference.Settings Settings;

            #endregion Fields

            #region Constructors

            protected void SetValues(Memory.Archive archive, params string[] fileNames)
            {
                Memory.Log.WriteLine($"{nameof(StringsBase)}::{nameof(SetValues)} Archive= \"{archive}\", Files= {{{string.Join(", ", fileNames)}}}");

                Archive = archive;
                FileNames = fileNames;
            }

            #endregion Constructors

            #region Indexers

            public FF8StringReference this[int sectionID, int stringID] => StringFiles?[sectionID, stringID];

            #endregion Indexers

            #region Methods

            public Memory.Archive GetArchive() => Archive;

            public IReadOnlyList<string> GetFileNames() => FileNames;

            public StringFile GetFiles() => StringFiles;

            /// <summary>
            /// <para>
            /// So you read the pointers at location, you get so many pointers then skip so many
            /// bytes before getting more pointers. Do this till start of next section.
            /// </para>
            /// </summary>
            /// <param name="br">BinaryReader where data is.</param>
            /// <param name="filename">file you are reading from</param>
            /// <param name="pointerStart">Section where pointers are.</param>
            /// <param name="stringStart">Section where strings are</param>
            /// <param name="grab">Get so many pointers</param>
            /// <param name="skip">Then skip so many bytes</param>
            protected void Get_Strings_BinMSG(BinaryReader br, string filename, int pointerStart, uint stringStart, uint grab = 0, uint skip = 0)
            {
                Loc fPos = StringFiles.SubPositions[pointerStart];
                if (fPos.Seek > br.BaseStream.Length) return;
                br.BaseStream.Seek(fPos.Seek, SeekOrigin.Begin);
                
                if (StringFiles.SPositions.ContainsKey(pointerStart))
                {
                }
                else
                {
                    ushort b = 0;
                    ushort last = b;
                    List<FF8StringReference> tmp = new List<FF8StringReference>();
                    uint g = 1;
                    while (br.BaseStream.Position < fPos.Max)
                    {
                        b = br.ReadUInt16();
                        if (last > b)
                            break;
                        else
                        {
                            if (b != 0xFFFF)
                            {
                                tmp.Add(new FF8StringReference(Archive, filename, b + stringStart, settings: Settings));
                                last = b;
                            }
                            else
                                tmp.Add(null);

                            if (grab <= 0 || ++g <= grab) continue;
                            br.BaseStream.Seek(skip, SeekOrigin.Current);
                            g = 1;
                        }
                    }

                    StringFiles.SPositions.Add(pointerStart, tmp);
                }
            }

            protected void Get_Strings_ComplexStr(BinaryReader br, string filename, int key, IReadOnlyList<int> list)
            {
                uint[] fPaddings = MenuGroupReadPadding(br, StringFiles.SubPositions[key], 1);
                StringFiles.SPositions.Add(key, new List<FF8StringReference>());
                if (fPaddings == null) return;
                for (uint p = 0; p < fPaddings.Length; p += 2)
                {
                    key = list[(int)fPaddings[(int)p + 1]];
                    Loc fPos = StringFiles.SubPositions[(int)key];
                    uint fPad = fPaddings[p] + fPos.Seek;
                    br.BaseStream.Seek(fPad, SeekOrigin.Begin);
                    if (!StringFiles.SPositions.ContainsKey(key))
                        StringFiles.SPositions.Add(key, new List<FF8StringReference>());
                    br.BaseStream.Seek(fPad + 6, SeekOrigin.Begin);
                    //byte[] UNK = br.ReadBytes(6);
                    ushort len = br.ReadUInt16();
                    uint stop = (uint)(br.BaseStream.Position + len - 9); //6 for UNK, 2 for len 1, for end null
                    StringFiles.SPositions[key].Add(new FF8StringReference(Archive, filename, (uint)br.BaseStream.Position, settings: Settings));
                    //entry contains possible more than one string so I am scanning for null
                    while (br.BaseStream.Position + 1 < stop)
                    {
                        byte b = br.ReadByte();
                        if (b == 0) StringFiles.SPositions[key].Add(new FF8StringReference(Archive, filename, (uint)br.BaseStream.Position, settings: Settings));
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
            protected void Get_Strings_Offsets(BinaryReader br, string filename, int key, bool pad = false)
            {
                Loc fPos = StringFiles.SubPositions[key];
                uint[] fPaddings = pad ? MenuGroupReadPadding(br, fPos) : (new uint[] { 1 });
                if (fPaddings == null) return;
                StringFiles.SPositions.Add(key, new List<FF8StringReference>());
                for (uint p = 0; p < fPaddings.Length; p++)
                {
                    if (fPaddings[p] <= 0) continue;
                    uint fPad = pad ? fPaddings[p] + fPos.Seek : fPos.Seek;
                    if (fPad > br.BaseStream.Length) return;
                    br.BaseStream.Seek(fPad, SeekOrigin.Begin);
                    if (br.BaseStream.Position + 4 >= br.BaseStream.Length) continue;
                    int count = br.ReadUInt16();
                    for (int i = 0; i < count && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                    {
                        uint c = br.ReadUInt16();
                        if (c >= br.BaseStream.Length || c == 0) continue;
                        c += fPad;
                        StringFiles.SPositions[key].Add(new FF8StringReference(Archive, filename, c, settings: Settings));
                    }
                }
            }

            protected abstract void LoadArchiveFiles();

            protected uint[] MenuGroupReadPadding(BinaryReader br, Loc fPos, int type = 0)
            {
                if (fPos.Seek > br.BaseStream.Length) return null;
                br.BaseStream.Seek(fPos.Seek, SeekOrigin.Begin);
                uint size = type == 0 ? br.ReadUInt16() : br.ReadUInt32();
                uint[] fPaddings = new uint[type == 0 ? size : size * type * 2];
                for (int i = 0; i < fPaddings.Length; i += 1 + type)
                {
                    fPaddings[i] = br.ReadUInt16();
                    if (type == 0 && fPaddings[i] + fPos.Seek >= fPos.Max)
                        fPaddings[i] = 0;
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
                byte[] buffer = aw.GetBinaryFile(FileNames[0],true);
                if (buffer == null) return;
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer, true)))
                {
                    StringFiles = new StringFile(1);
                    StringFiles.SubPositions.Add(Loc.CreateInstance(0, uint.MaxValue));
                    Get_Strings_Offsets(br, FileNames[0], 0);
                }
            }
        }

        #endregion Methods
    }

    #endregion Classes
}