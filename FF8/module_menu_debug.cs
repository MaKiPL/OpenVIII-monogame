using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal static class Module_menu_debug
    {
        private enum Mode
        {
            Initialize,
            Draw,
            Wait
        }

        /// <summary>
        /// First half in faces1.tex,
        /// second half in faces2.tex,
        /// 8 cols 2 rows per file.
        /// </summary>
        private enum Faces
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
        private static readonly Texture2D[] faces = new Texture2D[2];
        //private static Texture2D[] textest;
        private static int pointer = -1;
        private static Mode currentMode;
        private static Faces[] FaceValue;
        private static int time;
        private static Entry[] entries;

        private struct Entry //Rectangle + File
        {
            public byte File { get; set; }
            private Rectangle src;
            public byte X { get => (byte)src.X; set => src.X = value; }
            public byte Y { get => (byte)src.Y; set => src.Y = value; }
            public UInt16 Width { get => (UInt16)src.Width; set => src.Width = value; }
            public UInt16 Height { get => (UInt16)src.Height; set => src.Height = value; }
            public Rectangle GetRectangle() => src;
        }

        private static void Initialize()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                aw.GetListOfFiles().First(x => x.ToLower().Contains("face.sp2")));
            using (MemoryStream ms = new MemoryStream(test))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(4, SeekOrigin.Begin);
                UInt32[] locs = new UInt32[32];//br.ReadUInt32(); 32 valid values in face.sp2 rest is unknown
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
            //textest = new Texture2D[tex.TextureData.NumOfPalettes];
            //for (int i = 0; i < textest.Length; i++)
            //    textest[i] = tex.GetTexture(i);
            FaceValue = (Faces[])Enum.GetValues(typeof(Faces));
            Array.Sort(FaceValue);
        }
        public static void Update()
        {
            switch (currentMode)
            {
                case Mode.Initialize:
                    Initialize();
                    currentMode++;
                    break;
                case Mode.Draw:
                    pointer++;
                    if (pointer >= FaceValue.Length) pointer = 0;
                    currentMode++;
                    break;
                case Mode.Wait:
                    time += Memory.gameTime.ElapsedGameTime.Milliseconds;
                    if (time > 2000)
                    {
                        currentMode--;
                        time = 0;
                    }
                    //else
                    //    Memory.SuppressDraw = true;
                    break;
            }
        }
        public static void Draw()
        {
            switch (currentMode)
            {
                case Mode.Initialize:
                    break;
                case Mode.Wait:
                case Mode.Draw:
                    if (pointer >= 0)
                    {
                        Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

                        int rows = 2;
                        int cols = 8;
                        int totalitems = rows * cols;
                        int pos = (int)FaceValue[pointer];
                        //int i = pos / totalitems;
                        int i = entries[pos].File;
                        int col = (pos % cols);
                        int row = (pos / cols) % rows;
                        //int texWidth = faces[i].Width;
                        //int texHeight = (int)(faces[i].Height * .75); //bottom 25% of pixels is blank

                        Rectangle dst = new Rectangle();
                        //Rectangle src = new Rectangle
                        //{
                        //    Width = texWidth / 8,
                        //    Height = texHeight / 2
                        //};
                        //src.X = src.Width * col;
                        //src.Y = src.Height * row;
                        Rectangle src = entries[pos].GetRectangle();
                        dst.Height = vp.Height;
                        dst.Width = src.Width * (dst.Height / src.Height);
                        dst.X = vp.Width / 2 - dst.Width / 2;
                        dst.Y = 0;
                        Memory.SpriteBatchStartAlpha();
                        Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                        Memory.spriteBatch.Draw(faces[i], dst, src,
                            Color.White);
                        Memory.font.RenderBasicText(Font.CipherDirty($"{FaceValue[pointer].ToString().Replace('_', ' ')}\npos: {pos}\nfile: {i}\ncol: {col}\nrow: {row}\nx: {src.X}\ny: {src.Y}\nwidth: {src.Width}\nheight: {src.Height}"),
                        (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
                        Memory.SpriteBatchEnd();
                        //System.Threading.Thread.Sleep(500);

                    }
                    break;
            }
        }
    }
}
