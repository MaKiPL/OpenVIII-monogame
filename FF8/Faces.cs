using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal class Faces
    {
        #region Fields

        private static readonly Texture2D[] faces = new Texture2D[2];

        private static Entry[] entries;

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
                using (BinaryReader br = new BinaryReader(ms))
                {
                    ms.Seek(4, SeekOrigin.Begin);
                    UInt32[] locs = new UInt32[32];//br.ReadUInt32(); 32 valid values in face.sp2 rest is invalid
                    entries = new Entry[locs.Length];
                    for (int i = 0; i < locs.Length; i++)
                    {
                        locs[i] = br.ReadUInt32();
                    }
                    byte fid = 0;
                    for (int i = 0; i < locs.Length; i++)
                    {
                        ms.Seek(locs[i] + 4, SeekOrigin.Begin);
                        entries[i].X = br.ReadByte();
                        entries[i].Y = br.ReadByte();
                        ms.Seek(2, SeekOrigin.Current);
                        entries[i].Width = br.ReadUInt16();
                        entries[i].Height = br.ReadUInt16();
                        if (i - 1 > 0 && entries[i].Y < entries[i - 1].Y)
                            fid++;
                        entries[i].File = fid;
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

        #region Methods

        public Entry GetEntry(ID id) => entries[(int)id];

        public Entry GetEntry(int id) => entries[id];

        internal void Draw(ID id, Rectangle dst, float fade = 1f) => Draw((int)id, dst, fade);

        internal void Draw(int id, Rectangle dst, float fade = 1f) => Memory.spriteBatch.Draw(faces[entries[id].File], dst, entries[id].GetRectangle(), Color.White * fade);

        #endregion Methods

        #region Structs

        public struct Entry //Rectangle + File
        {
            #region Fields

            private Rectangle src;

            #endregion Fields

            #region Properties

            public byte File { get; set; }
            public UInt16 Height { get => (UInt16)src.Height; set => src.Height = value; }
            public UInt16 Width { get => (UInt16)src.Width; set => src.Width = value; }
            public byte X { get => (byte)src.X; set => src.X = value; }
            public byte Y { get => (byte)src.Y; set => src.Y = value; }

            #endregion Properties

            #region Methods

            public Rectangle GetRectangle() => src;

            #endregion Methods
        }

        #endregion Structs
    }
}