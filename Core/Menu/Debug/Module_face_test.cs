using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public static class Module_face_test
    {
        #region Fields

        private static Mode currentMode;

        private static Faces.ID[] FaceValue;

        private static int pointer = -1;

        private static TimeSpan time;

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

        /// <summary>
        /// Make sure the next frame will draw.
        /// </summary>
        public static void Show()
        {
            if (currentMode == Mode.Wait)
                currentMode = Mode.Draw;
            Memory.SuppressDraw = false;
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
                    time += Memory.ElapsedGameTime;
                    if (time > TimeSpan.FromMilliseconds(2000))
                    {
                        currentMode--;
                        time = TimeSpan.Zero;
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
                var vp = Memory.graphics.GraphicsDevice.Viewport;

                var rows = 2;
                var cols = 8;
                var totalitems = rows * cols;
                var id = FaceValue[pointer];
                var pos = (int)id;
                int i = Memory.Faces.GetEntry(id).File;
                var col = (pos % cols);
                var row = (pos / cols) % rows;

                var scale = vp.Height / Memory.Faces.GetEntry(id).Height;
                var dst = new Rectangle(new Point(0), (Memory.Faces.GetEntry(id).Size * scale).ToPoint());
                dst.Offset(vp.Width / 2 - dst.Center.X, 0);
                Memory.SpriteBatchStartAlpha();
                Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                Memory.Faces.Draw(id, dst);
                Memory.font.RenderBasicText($"{FaceValue[pointer].ToString().Replace('_', ' ')}\n" +
                    $"pos: {pos}\n" +
                    $"file: {i}\n" +
                    $"col: {col}\n" +
                    $"row: {row}\n" +
                    $"x: {Memory.Faces.GetEntry(id).X}\n" +
                    $"y: {Memory.Faces.GetEntry(id).Y}\n" +
                    $"width: {Memory.Faces.GetEntry(id).Width}\n" +
                    $"height: {Memory.Faces.GetEntry(id).Height}",
                    (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), lineSpacing: 1);
                Memory.SpriteBatchEnd();
            }
        }

        private static void Initialize()
        {
            FaceValue = (Faces.ID[])Enum.GetValues(typeof(Faces.ID));
            Array.Sort(FaceValue);
        }

        #endregion Methods
    }
}