using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings
    {
        #region Fields

        private static ConcurrentDictionary<FileID, StringsBase> Files;

        private static object Filesloc = new object();

        #endregion Fields

        #region Constructors

        public Strings() => Init();

        #endregion Constructors

        #region Enums

        /// <summary>
        /// filenames of files with strings and id's for structs that hold the data.
        /// </summary>
        public enum FileID : uint
        {
            MNGRP = 0,
            AREAMES = 2,
            NAMEDIC = 3,
            KERNEL = 4,
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
                    return d.Squallsname ?? Read(FileID.MNGRP, 2, 92);

                case Faces.ID.Rinoa_Heartilly:
                    return d.Rinoasname ?? Read(FileID.MNGRP, 2, 93);

                case Faces.ID.Angelo:
                    return d.Angelosname ?? Read(FileID.MNGRP, 2, 94);

                case Faces.ID.Boko:
                    return d.Bokosname ?? Read(FileID.MNGRP, 2, 135);

                case Faces.ID.Zell_Dincht:
                case Faces.ID.Irvine_Kinneas:
                case Faces.ID.Quistis_Trepe:
                case Faces.ID.Selphie_Tilmitt:
                case Faces.ID.Seifer_Almasy:
                case Faces.ID.Edea_Kramer:
                case Faces.ID.Laguna_Loire:
                case Faces.ID.Kiros_Seagill:
                case Faces.ID.Ward_Zabac:
                    return Read(FileID.KERNEL, 6, (int)id);

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
                    return d.GFs[id.ToGFs()].Name ?? Read(FileID.MNGRP, 2, 95 + (int)(id.ToGFs()));

                case Faces.ID.Griever:
                    return d.Grieversname ?? Read(FileID.MNGRP, 2, 135);

                case Faces.ID.MiniMog:
                    return Read(FileID.KERNEL, 0, 72); // also in KERNEL, 12, 36
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
        public FF8StringReference Read(FileID fileID, int sectionID, int stringID) => Files[fileID][(uint)sectionID, stringID];

        public StringsBase this[FileID id] => Files[id];

        private void Init()
        {
            lock (Filesloc)
                if (Files == null)
                {
                    Memory.Log.WriteLine($"{nameof(Strings)} :: {nameof(Init)}");
                    var tasks = new List<Task<bool>>();
                    Files = new ConcurrentDictionary<FileID, StringsBase>();
                    tasks.Add(Task.Run(() => Files.TryAdd(FileID.NAMEDIC, Namedic.Load()))); // areames and kernel require namedic
                    //though there shouldn't be anything reading the strings till this is done processing
                    //Task.WaitAll(tasks.ToArray());
                    tasks.Add(Task.Run(() => Files.TryAdd(FileID.MNGRP, Mngrp.Load())));
                    tasks.Add(Task.Run(() => Files.TryAdd(FileID.AREAMES, Areames.Load())));
                    tasks.Add(Task.Run(() => Files.TryAdd(FileID.KERNEL, Kernel.Load())));
                    Task.WaitAll(tasks.ToArray());
                    if (tasks.Any(x => !x.Result))
                        throw new ArgumentException($"{this}::Failed to add to dictionary...");
                }
        }


        #endregion Methods
    }
}