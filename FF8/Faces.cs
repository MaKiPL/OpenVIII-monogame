using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal partial class Faces : I_SP2
    {

        #region Fields

        private static readonly Texture2D[] faces = new Texture2D[2];

        private static Dictionary<ID, Entry> entries;

        #endregion Fields

        #region Constructors

        public Faces()
        {
            if (entries == null)
            {
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("face.sp2")));
                using (MemoryStream ms = new MemoryStream(test))
                {
                    ushort[] locs;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        ms.Seek(4, SeekOrigin.Begin);
                        locs = new UInt16[Count];//br.ReadUInt32(); 32 valid values in face.sp2 rest is invalid
                        entries = new Dictionary<ID, Entry>((int)Count);
                        for (uint i = 0; i < Count; i++)
                        {
                            locs[i] = br.ReadUInt16();
                            ms.Seek(2, SeekOrigin.Current);
                        }
                        byte fid = 0;
                        Entry Last = null;
                        for (uint i = 0; i < Count; i++)
                        {
                            entries[(ID)i] = new Entry();
                            fid = entries[(ID)i].LoadfromStreamSP2(br, locs[i],(byte)(Last!=null?Last.Y:0), fid);

                            Last = entries[(ID)i];


                        }
                    }
                }
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "face.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                TEX tex;
                tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("face1.tex"))));
                faces[0] = tex.GetTexture();
                tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("face2.tex"))));
                faces[1] = tex.GetTexture();
            }
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// First half in faces1.tex, second half in faces2.tex, 8 cols 2 rows per file.
        /// </summary>
        public enum ID
        {
            Squall_Leonhart = 0,
            Zell_Dincht,
            Irvine_Kinneas,
            Quistis_Trepe,
            Rinoa_Heartilly,
            Selphie_Tilmitt,
            Seifer_Almasy,
            Edea_Kramer,
            Laguna_Loire,
            Kiros_Seagill,
            Ward_Zabac,
            Lion = Ward_Zabac + 2, //skipped blank
            MiniMog,
            Boko,
            Angelo,
            Quezacotl,
            Shiva,
            Ifrit,
            Siren,
            Brothers,
            Diablos,
            Carbuncle,
            Leviathan,
            Pandemona,
            Cerberus,
            Alexander,
            Doomtrain,
            Bahamut,
            Cactuar,
            Tonberry,
            Eden
        }

        #endregion Enums

        #region Properties

        public uint Count => 32;

        public uint PalletCount => 1;

        #endregion Properties

        #region Methods

        public Entry GetEntry(Enum id) => entries[(ID)id];

        public Entry GetEntry(int id) => entries[(ID)id];

        public void Draw(Enum id, Rectangle dst, float fade = 1f) => Memory.spriteBatch.Draw(faces[entries[(ID)id].File], dst, entries[(ID)id].GetRectangle, Color.White * fade);
        public void Draw(int id, Rectangle dst, float fade = 1f) => Draw((ID)id, dst, fade); 

        #endregion Methods

    }
}