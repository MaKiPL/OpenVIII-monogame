using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8
{
    public static class Module_icon_test
    {
        #region Fields

        private static Mode currentMode;

        private static Icons.ID icon = Icons.ID.Arrow_Down;
        private const int DefaultPallet = 2;
        private static int pallet = DefaultPallet;

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
                if (pallet <= 0)
                    pallet = (int)Memory.Icons.PalletCount - 1;
                else
                    pallet--;
                currentMode = Mode.Draw;
            }

            if (Input.Button(Keys.Down))
            {
                if (pallet >= Memory.Icons.PalletCount - 1)
                    pallet = 0;
                else
                    pallet++;
                currentMode = Mode.Draw;
            }
            //if ((Input.Button(Keys.Up) || Input.Button(Keys.Down)) && Memory.Icons.GetEntry(icon) != null && (Memory.Icons.GetEntry(icon).GetLoc.count > 1))
            //    icon -= (Memory.Icons.GetEntry(icon).GetLoc.count - 1);
            if (Input.Button(Keys.Right))
            {
                do
                {
                    if (icon >= Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max())
                        icon = 0;
                    else
                        icon++;
                }
                while (Memory.Icons.GetEntry(icon) == null);
                currentMode = Mode.Draw;
            }
            if (Input.Button(Keys.Left))
            {
                do
                {
                    if (icon <= 0)
                        icon = Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max();
                    //else if (Memory.Icons.GetEntry(icon) != null && Memory.Icons.GetEntry(icon).GetLoc.count > 1)
                    //    icon -= Memory.Icons.GetEntry(icon).GetLoc.count;
                    else
                        icon--;
                }
                while (Memory.Icons.GetEntry(icon) == null);
                currentMode = Mode.Draw;
            }
            switch (currentMode)
            {
                case Mode.Initialize:
                    //SaveStringToFile();
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
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Gray);
            Memory.SpriteBatchEnd();
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

            Vector2 scale = new Vector2(4f);
            Rectangle dst = new Rectangle()
            {
                Width = (int)(Memory.Icons.GetEntryGroup(icon).Width * scale.X),
                Height = (int)(Memory.Icons.GetEntryGroup(icon).Height * scale.Y)
            };
            if (icon == Icons.ID.Menu_BG_368)
            {
                dst.Width = vp.Width;
                dst.Height = vp.Height - 50;
                scale = Vector2.Zero;
            }
            else
            {
                dst.X = vp.Width / 2 - dst.Width / 2;
                dst.Y = vp.Height / 2 - dst.Height / 2;
            }

            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            Memory.Icons.Draw(icon, pallet, dst, scale);
            Memory.font.RenderBasicText(
                $"{(icon).ToString().Replace('_', ' ')}\n" +
                $"id: {(ushort)icon}\n\n" +
                $"pallet: {pallet}\n\n" +
                $"width: {Memory.Icons[icon].Width}\n" +
                $"height: {Memory.Icons[icon].Height}",
                (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f),lineSpacing: 0);
            Memory.SpriteBatchEnd();
        }
        static public void SaveStringToFile()
        {
            using (FileStream fs = File.Create("D:\\iconsdatadump.csv"))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(System.Text.Encoding.UTF8.GetBytes(ToString()));
            }
        }
        static public new string ToString()
        {
            string output = "{Enum Name},{Enum ID}," + Memory.Icons.GetEntry(Icons.ID.Finger_Right).ToStringHeader;
            for(uint i = 0; i < Memory.Icons.Count; i++)
            {
                EntryGroup eg = Memory.Icons.GetEntryGroup((Icons.ID)i);
                foreach(Entry e in eg)
                {
                    output += $"{((Icons.ID)i).ToString().Replace('_', ' ')},{i}," + e.ToString();
                }
            }
            return output;
        }


        #endregion Methods
    }
}