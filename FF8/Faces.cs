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

        private const int TextureCount = 2;
        private const string TextureFilename = "face{0:0}.tex";
        private const int TextureStartOffset = 1;
        private const string IndexFilename = "face.sp2";

        private static Texture2D[] textures;

        private static Dictionary<Enum, Entry> entries;

        #endregion Fields

        #region Constructors

        public Faces() => Process(TextureCount, TextureFilename, TextureStartOffset, IndexFilename, ref entries, ref textures);

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

        public uint Count {get;set;}

        public uint PalletCount => 1;

        #endregion Properties

        #region Methods

        protected void Process(int tc, string fn, int tso, string ind, ref Dictionary<Enum, Entry> dict, ref Texture2D[] texs)
        {
            if (dict == null)
            {
                texs = new Texture2D[tc];
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains(ind)));
                using (MemoryStream ms = new MemoryStream(test))
                {
                    ushort[] locs;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        //ms.Seek(4, SeekOrigin.Begin);
                        Count = br.ReadUInt32();
                        locs = new ushort[Count];//br.ReadUInt32(); 32 valid values in face.sp2 rest is invalid
                        dict = new Dictionary<Enum, Entry>((int)Count);
                        for (uint i = 0; i < Count; i++)
                        {
                            locs[i] = br.ReadUInt16();
                            ms.Seek(2, SeekOrigin.Current);
                        }
                        byte fid = 0;
                        Entry Last = null;
                        for (uint i = 0; i < Count; i++)
                        {
                            ms.Seek(locs[i] + 6, SeekOrigin.Begin);
                            byte t = br.ReadByte();
                            if (t == 0 || t == 96) continue;
                            dict[(ID)i] = new Entry();
                            fid = dict[(ID)i].LoadfromStreamSP2(br, locs[i], (byte)(Last != null ? Last.Y : 0), fid);

                            Last = dict[(ID)i];
                        }
                    }
                }
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "face.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                TEX tex;
                for (int i = 0; i < tc; i++)
                {
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                        aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(fn, i + tso)))));
                    texs[i] = tex.GetTexture();
                }
            }
        }

        public Entry GetEntry(Enum id) => entries[id];

        public Entry GetEntry(int id) => entries[(ID)id];

        public void Draw(Enum id, Rectangle dst, float fade = 1f) => Memory.spriteBatch.Draw(textures[entries[id].File], dst, entries[id].GetRectangle, Color.White * fade);

        public void Draw(int id, Rectangle dst, float fade = 1f) => Draw((ID)id, dst, fade);

        #endregion Methods
    }
}