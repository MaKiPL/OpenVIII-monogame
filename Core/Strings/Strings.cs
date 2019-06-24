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
                    return d.GFs[id.ToGFs()].Name ?? Read(FileID.MNGRP, 2, 95 - 16 + (int)id);

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
        public FF8StringReference Read(FileID fileID, int sectionID, int stringID) => Files[fileID][(uint)sectionID,stringID];



        //public FF8String Read(BinaryReader br, FileID fid, uint pos)
        //{
        //    if (pos == 0)
        //        return new FF8String("");
        //    if (pos < br.BaseStream.Length)
        //        using (MemoryStream os = new MemoryStream(50))
        //        {
        //            br.BaseStream.Seek(pos, SeekOrigin.Begin);
        //            int c = 0;
        //            byte b = 0;
        //            do
        //            {
        //                if (br.BaseStream.Position > br.BaseStream.Length) break;
        //                //sometimes strings start with 00 or 01. But there is another 00 at the end.
        //                //I think it's for SeeD test like 1 is right and 0 is wrong. for now i skip them.
        //                b = br.ReadByte();
        //                if (b != 0 && b != 1)
        //                {
        //                    os.WriteByte(b);
        //                }
        //                c++;
        //            }
        //            while (b != 0 || c == 0);
        //            if (os.Length > 0)
        //                return os.ToArray();
        //        }
        //    return null;
        //}

        private Dictionary<FileID, StringsBase> Files;
        private void Init() => Files = new Dictionary<FileID, StringsBase>{
                { FileID.MNGRP, new Mngrp()},
                { FileID.KERNEL, new Kernel() },
                { FileID.AREAMES, new Areames() },
                { FileID.NAMEDIC, new Namedic() }
                };











        #endregion Methods
    }
}