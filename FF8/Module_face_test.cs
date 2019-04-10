using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FF8
{
    internal static class Module_face_test
    {

        #region Fields

        private static Mode currentMode;

        private static Faces faces;

        private static Faces.ID[] FaceValue;

        private static int pointer = -1;

        private static int time;

        #endregion Fields

        #region Enums

        private enum Mode
        {
            Initialize,
            Draw,
            Wait
        }

        #endregion Enums

        #region Methods

        public static void Draw()
        {
            switch (currentMode)
            {
                case Mode.Initialize:
                    break;

                case Mode.Wait:
                case Mode.Draw:
                    DrawFace();
                    break;
            }
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
                    else
                        Memory.SuppressDraw = true;
                    break;
            }
        }

        private static void DrawFace()
        {
                                if (pointer >= 0)
                    {
                        Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

                        int rows = 2;
                        int cols = 8;
                        int totalitems = rows * cols;
                        int pos = (int)FaceValue[pointer];
                        int i = faces.GetEntry(pos).File;
                        int col = (pos % cols);
                        int row = (pos / cols) % rows;

                        Rectangle dst = new Rectangle
                        {
                            Height = vp.Height
                        };
                        dst.Width = faces.GetEntry(pos).Width * (dst.Height / faces.GetEntry(pos).Height);
                        dst.X = vp.Width / 2 - dst.Width / 2;
                        dst.Y = 0;
                        Memory.SpriteBatchStartStencil();
                        Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                        faces.Draw(pos, dst);
                        Memory.font.RenderBasicText(Font.CipherDirty($"{FaceValue[pointer].ToString().Replace('_', ' ')}\npos: {pos}\nfile: {i}\ncol: {col}\nrow: {row}\nx: {faces.GetEntry(pos).X}\ny: {faces.GetEntry(pos).Y}\nwidth: {faces.GetEntry(pos).Width}\nheight: {faces.GetEntry(pos).Height}"),
                        (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
                        Memory.SpriteBatchEnd();
                    }
        }
        private static void Initialize()
        {
            faces = new Faces();
            FaceValue = (Faces.ID[])Enum.GetValues(typeof(Faces.ID));
            Array.Sort(FaceValue);
        }

        #endregion Methods

    }
}