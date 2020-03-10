using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings : IReadOnlyDictionary<Strings.FileID, Strings.StringsBase>
    {
        #region Fields

        private static ConcurrentDictionary<FileID, StringsBase> _files;

        private static readonly object FilesLoc = new object();

        #endregion Fields

        #region Constructors

        public Strings() => Init();

        #endregion Constructors

        #region Enums

        /// <summary>
        /// file names of files with strings and BattleID's for structs that hold the data.
        /// </summary>
        public enum FileID : uint
        {
            MenuGroup = 0,
            AreaNames = 2,
            Namedic = 3,
            Kernel = 4,
        }

        #endregion Enums

        #region Methods

        public FF8String GetName(Characters c, Saves.Data d = null) => GetName(c.ToFacesID(), d);

        public FF8String GetName(GFs gf, Saves.Data d = null) => GetName(gf.ToFacesID(), d);

        public FF8String GetName(Faces.ID id, Saves.Data d = null)
        {
            if (d == null)
                d = Memory.State;
            switch (id)
            {
                case Faces.ID.Squall_Leonhart:
                    return d.SquallName ?? Read(FileID.MenuGroup, 2, 92);

                case Faces.ID.Rinoa_Heartilly:
                    return d.RinoaName ?? Read(FileID.MenuGroup, 2, 93);

                case Faces.ID.Angelo:
                    return d.AngeloName ?? Read(FileID.MenuGroup, 2, 94);

                case Faces.ID.Boko:
                    return d.BokoName ?? Read(FileID.MenuGroup, 2, 135);

                case Faces.ID.Zell_Dincht:
                case Faces.ID.Irvine_Kinneas:
                case Faces.ID.Quistis_Trepe:
                case Faces.ID.Selphie_Tilmitt:
                case Faces.ID.Seifer_Almasy:
                case Faces.ID.Edea_Kramer:
                case Faces.ID.Laguna_Loire:
                case Faces.ID.Kiros_Seagill:
                case Faces.ID.Ward_Zabac:
                    return Read(FileID.Kernel, 6, (int)id);

                case Faces.ID.Quezacotl:
                case Faces.ID.Shiva:
                case Faces.ID.Ifrit:
                case Faces.ID.Siren:
                case Faces.ID.Brothers:
                case Faces.ID.Diablos:
                case Faces.ID.Carbuncle:
                case Faces.ID.Leviathan:
                case Faces.ID.Pandemona:
                case Faces.ID.Cerberus:
                case Faces.ID.Alexander:
                case Faces.ID.Doomtrain:
                case Faces.ID.Bahamut:
                case Faces.ID.Cactuar:
                case Faces.ID.Tonberry:
                case Faces.ID.Eden:
                    if (d.GFs != null && d.GFs.TryGetValue(id.ToGFs(), out Saves.GFData value) && value.Name != null &&
                        value.Name.Length > 0)
                        return value.Name;

                    return Read(FileID.MenuGroup, 2, 95 + (int)(id.ToGFs()));

                case Faces.ID.Griever:
                    return d.GrieverName ?? Read(FileID.MenuGroup, 2, 135);

                case Faces.ID.MiniMog:
                    return Read(FileID.Kernel, 0, 72); // also in KERNEL, 12, 36
                default:
                    return new FF8String();
            }
        }

        /// <summary>
        /// Remember to Close() if done using
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="sectionID"></param>
        /// <param name="stringID"></param>
        /// <returns></returns>
        public FF8StringReference Read(FileID fileID, int sectionID, int stringID) => _files[fileID][sectionID, stringID];

        public FF8StringReference ReadByOffset(FileID fileID, int sectionID, int offset)
        {
            FF8StringReference r = new FF8StringReference(Memory.Strings[FileID.Kernel].GetArchive(),
                Memory.Strings[FileID.Kernel].GetFileNames()[0],
                Memory.Strings[FileID.Kernel].GetFiles()
                    .SubPositions[(int) ((Kernel) Memory.Strings[FileID.Kernel]).StringLocations[0].StringLocation] +
                offset, settings: (FF8StringReference.Settings.Namedic | FF8StringReference.Settings.MultiCharByte));
            return r.Length>0 ? r : null;
        }

        public bool ContainsKey(FileID key)
        {
            return _files.ContainsKey(key);
        }

        public bool TryGetValue(FileID key, out StringsBase value)
        {
            return _files.TryGetValue(key, out value);
        }

        public StringsBase this[FileID id] => _files[id];
        public IEnumerable<FileID> Keys => ((IReadOnlyDictionary<Strings.FileID, Strings.StringsBase>) _files).Keys;

        public IEnumerable<StringsBase> Values => ((IReadOnlyDictionary<Strings.FileID, Strings.StringsBase>) _files).Values;

        private void Init()
        {
            lock (FilesLoc)
                if (_files == null)
                {
                    Memory.Log.WriteLine($"{nameof(Strings)} :: {nameof(Init)}");
                    _files = new ConcurrentDictionary<FileID, StringsBase>();
                    Func<bool>[] func = {
                    () => _files.TryAdd(FileID.Namedic, Namedic.Load()), // area names and kernel require namedic
                    //though there shouldn't be anything reading the strings till this is done processing
                    //Task.WaitAll(tasks.ToArray());
                    () => _files.TryAdd(FileID.MenuGroup, MenuGroup.Load()),
                    () => _files.TryAdd(FileID.AreaNames, Areames.Load()),
                    () => _files.TryAdd(FileID.Kernel, Kernel.Load()),
                };
                    IEnumerable<bool> tasks = Memory.ProcessFuncs(func);
                    if (tasks.Any(x => !x))
                        throw new ArgumentException($"{this}::Failed to add to dictionary...");
                }
        }

        #endregion Methods

        public IEnumerator<KeyValuePair<FileID, StringsBase>> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _files).GetEnumerator();
        }

        public int Count => _files.Count;
    }
}
