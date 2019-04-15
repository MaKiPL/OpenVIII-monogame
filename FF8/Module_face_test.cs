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
                        var id = FaceValue[pointer];
                        int pos = (int)id;
                        int i = faces.GetEntry(id).File;
                        int col = (pos % cols);
                        int row = (pos / cols) % rows;

                        float scale = vp.Height/ faces.GetEntry(id).Height;
                        Rectangle dst = new Rectangle(new Point(0), (faces.GetEntry(id).Size*scale).ToPoint());
                        dst.Offset(vp.Width/2 - dst.Center.X, 0);
                        Memory.SpriteBatchStartStencil();
                        Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                        faces.Draw(id, dst);
                        Memory.font.RenderBasicText(Font.CipherDirty($"{FaceValue[pointer].ToString().Replace('_', ' ')}\npos: {pos}\nfile: {i}\ncol: {col}\nrow: {row}\nx: {faces.GetEntry(id).X}\ny: {faces.GetEntry(id).Y}\nwidth: {faces.GetEntry(id).Width}\nheight: {faces.GetEntry(id).Height}"),
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