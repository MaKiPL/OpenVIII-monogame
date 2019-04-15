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

                var id = CardValue[pointer];
                uint pos = (uint)id;
                //int i = cards.GetEntry(id).File;
                uint col = (uint)(cards.GetEntry(id).X / cards.GetEntry(id).Width) +1;
                uint row = (uint)(cards.GetEntry(id).Y / cards.GetEntry(id).Width) +1;

                float scale = vp.Height / cards.GetEntry(id).Height;
                Rectangle dst = new Rectangle(new Point(0), (cards.GetEntry(id).Size * scale).ToPoint());
                dst.Offset(vp.Width / 2 - dst.Center.X, 0);
                Memory.SpriteBatchStartStencil();
                Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                cards.Draw(id, dst);
                Memory.font.RenderBasicText(Font.CipherDirty($"{CardValue[pointer].ToString().Replace('_', ' ')}\npos: {pos}\ncol: {col}\nrow: {row}\nx: {cards.GetEntry(id).X}\ny: {cards.GetEntry(id).Y}\nwidth: {cards.GetEntry(id).Width}\nheight: {cards.GetEntry(id).Height}"),
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