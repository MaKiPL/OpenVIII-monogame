using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static class Module_icon_test
    {
        #region Fields

        private const int DefaultPalette = 2;
        private static Mode currentMode;

        private static Icons.ID icon = Icons.ID.Arrow_Down;
        private static int palette = DefaultPalette;

        private static float zoom = 40f;

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

        public static void SaveStringToFile()
        {
            FileStream fs = null;
            //fs is disposed by binary writer
            using (BinaryWriter bw =
                new BinaryWriter(fs = File.Open("D:\\iconsdatadump.csv",
                FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                bw.Write(System.Text.Encoding.UTF8.GetBytes(ToString()));
                fs = null;
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

        public static new string ToString()
        {
            string output = "{Enum Name},{Enum ID}," + Memory.Icons.GetEntry(Icons.ID.Finger_Right).ToStringHeader;
            for (uint i = 0; i < Memory.Icons.Count; i++)
            {
                EntryGroup eg = Memory.Icons.GetEntryGroup((Icons.ID)i);
                foreach (Entry e in eg)
                {
                    output += $"{((Icons.ID)i).ToString().Replace('_', ' ')},{i}," + e.ToString();
                }
            }
            return output;
        }

        public static void Update()
        {
            if (Input2.DelayedButton(new InputButton() { Key = Keys.OemMinus, Trigger = ButtonTrigger.Press }) || Input2.DelayedButton(new InputButton() { Key = Keys.Subtract, Trigger = ButtonTrigger.Press }))
            {
                if (zoom - 1 < 1f)
                    zoom = 1f;
                else
                    zoom--;
                Show();
            }

            if (Input2.DelayedButton(new InputButton() { Key = Keys.OemPlus, Trigger = ButtonTrigger.Press }) || Input2.DelayedButton(new InputButton() { Key = Keys.Add, Trigger = ButtonTrigger.Press }))
            {
                if (zoom + 1 > 100f)
                    zoom = 100f;
                else
                    zoom++;
                Show();
            }

            if (Input2.DelayedButton(FF8TextTagKey.Up))
            {
                if (palette <= 0)
                    palette = (int)Memory.Icons.PaletteCount - 1;
                else
                    palette--;
                Show();
            }

            if (Input2.DelayedButton(FF8TextTagKey.Down))
            {
                if (palette >= Memory.Icons.PaletteCount - 1)
                    palette = 0;
                else
                    palette++;
                Show();
            }
            if (Input2.DelayedButton(FF8TextTagKey.Right) || Input2.Button(Keys.PageDown))
            {
                do
                {
                    if (icon >= Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max())
                        icon = 0;
                    else
                        icon++;
                }
                while (Memory.Icons.GetEntry(icon) == null);
                Show();
            }
            if (Input2.DelayedButton(FF8TextTagKey.Left) || Input2.Button(Keys.PageUp))
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
                Show();
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
            Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp);
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Gray);
            Memory.SpriteBatchEnd();
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

            Vector2 scale = new Vector2(zoom);
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
            dst.Size = Point.Zero;
            Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp);
            Memory.Icons.Draw(icon, palette, dst, scale);
            Memory.font.RenderBasicText(
                $"{(icon).ToString().Replace('_', ' ')}\n" +
                $"id: {(ushort)icon}\n\n" +
                $"palette: {palette}\n\n" +
                $"width: {Memory.Icons[icon].Width}\n" +
                $"height: {Memory.Icons[icon].Height}",
                (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), lineSpacing: 0);
            Memory.SpriteBatchEnd();
        }

        #endregion Methods
    }
}