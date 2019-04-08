using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
            Lion = Ward_Zabac+2, //skipped blank
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
        private static void Initialize()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);

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
            FaceValue=(Faces[])Enum.GetValues(typeof(Faces));
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
                    if ((int)pointer >= FaceValue.Length) pointer = 0;
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
                        int totalitems = rows*cols;
                        int pos = (int)FaceValue[pointer];
                        int i = (int)pos / totalitems;
                        int col = ((int)pos % cols);
                        int row = ((int)pos / cols) % rows;
                        int texWidth = faces[i].Width;
                        int texHeight = (int)(faces[i].Height*.75); //bottom 25% of pixels is blank

                        Rectangle dst = new Rectangle();
                        Rectangle src = new Rectangle();
                        src.Width = texWidth / 8;
                        src.Height = texHeight / 2;
                        src.X = src.Width * col;
                        src.Y = src.Height * row;
                        dst.Height = vp.Height;
                        dst.Width = src.Width * (dst.Height / src.Height);
                        dst.X = vp.Width/2 - dst.Width / 2;
                        dst.Y = 0;
                        Memory.SpriteBatchStartAlpha();
                        Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                        Memory.spriteBatch.Draw(faces[i],dst, src,
                            Color.White);
                        Memory.font.RenderBasicText(Font.CipherDirty($"{FaceValue[pointer].ToString().Replace('_',' ')}\npos: {pos}\nfile: {i}\ncol: {col}\nrow: {row}"),
                        (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
                        Memory.SpriteBatchEnd();
                        //System.Threading.Thread.Sleep(500);

                    }
                    break;
            }
        }
    }
}
