using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public class Module_card_test
    {
        #region Fields

        private static Cards.ID[] CardValue;
        private static Mode currentMode;
        private static int pointer = -1;

        private static TimeSpan time;
        private static TimeSpan totaltime = TimeSpan.FromMilliseconds(2000);
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
                    if (pointer >= CardValue.Length) pointer = 0;
                    currentMode++;
                    break;

                case Mode.Wait:
                    time += Memory.gameTime.ElapsedGameTime;
                    if (time > totaltime)
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
                Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

                Cards.ID id = CardValue[pointer];
                uint pos = (uint)id;
                //int i = cards.GetEntry(id).File;
                uint col = (uint)(Memory.Cards.GetEntry(id).X / Memory.Cards.GetEntry(id).Width) + 1;
                uint row = (uint)(Memory.Cards.GetEntry(id).Y / Memory.Cards.GetEntry(id).Width) + 1;

                Rectangle dst = new Rectangle(new Point(0), (Memory.Cards.GetEntry(id).Size).ToPoint());
                dst.Height = (int)Math.Round(dst.Width * (1+Cards.AspectRatio));

                float scale = vp.Height / dst.Height;
                dst = dst.Scale(new Vector2(scale));
                dst.Offset(vp.Width / 2 - dst.Center.X, vp.Height / 2 - dst.Center.Y);
                Memory.SpriteBatchStartAlpha();
                Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
                Memory.Cards.Draw(id, dst);
                Memory.font.RenderBasicText(
                    $"{CardValue[pointer].ToString().Replace('_', ' ')}\n" +
                    $"pos: {pos}\n" +
                    $"col: {col}\n" +
                    $"row: {row}\n" +
                    $"x: {Memory.Cards.GetEntry(id).X}\n" +
                    $"y: {Memory.Cards.GetEntry(id).Y}\n" +
                    $"width: {Memory.Cards.GetEntry(id).Width}\n" +
                    $"height: {Memory.Cards.GetEntry(id).Height}",
                    (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), lineSpacing: 1);
                Memory.SpriteBatchEnd();
            }
        }

        private static void Initialize()
        {
            CardValue = (Cards.ID[])Enum.GetValues(typeof(Cards.ID));
            Array.Sort(CardValue);
        }

        #endregion Methods
    }
}