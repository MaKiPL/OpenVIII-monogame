using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FF8
{
    internal class Module_card_test
    {
        #region Fields

        private static Mode currentMode;

        private static Cards cards;

        private static Cards.ID[] CardValue;

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
                    if (pointer >= CardValue.Length) pointer = 0;
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
                int pos = (int)CardValue[pointer];
                int i = cards.GetEntry(pos).File;
                int col = (pos % cols);
                int row = (pos / cols) % rows;

                float scale = vp.Height / cards.GetEntry(pos).Height;
                Rectangle dst = new Rectangle(new Point(0), (cards.GetEntry(pos).Size * scale).ToPoint());
                dst.Offset(vp.Width / 2 - dst.Center.X, 0);
                Memory.SpriteBatchStartStencil();
                Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                cards.Draw(pos, dst);
                Memory.font.RenderBasicText(Font.CipherDirty($"{CardValue[pointer].ToString().Replace('_', ' ')}\npos: {pos}\nfile: {i}\ncol: {col}\nrow: {row}\nx: {cards.GetEntry(pos).X}\ny: {cards.GetEntry(pos).Y}\nwidth: {cards.GetEntry(pos).Width}\nheight: {cards.GetEntry(pos).Height}"),
                (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
                Memory.SpriteBatchEnd();
            }
        }

        private static void Initialize()
        {
            cards = new Cards();
            CardValue = (Cards.ID[])Enum.GetValues(typeof(Cards.ID));
            Array.Sort(CardValue);
        }

        #endregion Methods
    }
}