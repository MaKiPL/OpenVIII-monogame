using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF8
{
    public static class Module_icon_test
    {

        #region Fields

        private static Mode currentMode;

        private static int icon;
        private static Icons icons;
        private static int pallet = 2;
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
                    DrawIcons();
                    break;
            }
        }

        public static void Update()
        {
            if (Input.Button(Keys.Up))
            {
                if (pallet == 0)
                    pallet = icons.PalletCount - 1;
                else
                    pallet--;
                currentMode = Mode.Draw;
            }

            if (Input.Button(Keys.Down))
            {
                if (pallet == icons.PalletCount - 1)
                    pallet = 0;
                else
                    pallet++;
                currentMode = Mode.Draw;
            }
            if ((Input.Button(Keys.Up) || Input.Button(Keys.Down)) && icons.GetEntry(icon) != null && (icons.GetEntry(icon).GetLoc().count > 1))
                icon -= (icons.GetEntry(icon).GetLoc().count - 1);
            if (Input.Button(Keys.Right))
            {
                do
                {
                    if (icon == icons.Count - 1)
                        icon = 0;
                    else
                        icon++;
                }
                while (icons.GetEntry(icon) == null);
                currentMode = Mode.Draw;
            }
            if (Input.Button(Keys.Left))
            {
                do
                {
                    if (icon == 0)
                        icon = (int)(icons.Count - 1);
                    else if (icons.GetEntry(icon) != null && icons.GetEntry(icon).GetLoc().count > 1)
                        icon -= icons.GetEntry(icon).GetLoc().count;
                    else
                        icon--;
                }
                while (icons.GetEntry(icon) == null);
                currentMode = Mode.Draw;
            }
            switch (currentMode)
            {
                case Mode.Initialize:
                    Initialize();
                    currentMode++;
                    break;

                case Mode.Draw:
                    currentMode++;
                    break;

                case Mode.Wait:
                    Memory.SuppressDraw = true;
                    break;
            }
        }

        private static void DrawIcons()
        {
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);

            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchEnd();
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

            float scale = 4f;//((float)vp.Height / 480) * 3f;
            do
            {
                Rectangle src = icons.GetEntry(icon).GetRectangle();

                Rectangle dst = new Rectangle()
                {
                    Width = (int)(src.Width * scale),
                    Height = (int)(src.Height * scale)
                };

                dst.X = vp.Width / 2 + (int)(icons.GetEntry(icon).Offset_X * scale);
                dst.Y = vp.Height / 2 + (int)(icons.GetEntry(icon).Offset_Y * scale);
                if (dst.X < 0)
                    dst.X = 0;
                if (dst.Y < 0)
                    dst.Y = 0;
                if (dst.X + dst.Width > vp.Width) dst.X = vp.Width - dst.Width;
                if (dst.Y + dst.Height > vp.Height) dst.Y = vp.Height - dst.Height;
                icons.Draw(icon, pallet, dst);
            }
            while (icons.GetEntry(icon++).Part > 1);
            icon--;
        }

        private static void Initialize() => icons = new Icons();

        #endregion Methods

    }
}