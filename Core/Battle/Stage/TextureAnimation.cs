using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes
        /// <summary>
        /// The fountain doesn't have a UV for just the water animation.
        /// So am forced to copy the pixels from the frames to the drawn from location.
        /// </summary>
        private class TextureAnimation
        {
            #region Fields

            private int cols;
            private int height;
            private int rows;
            private int skip;
            private int texturePage;
            private TextureHandler tex;
            private int width;
            private int x;
            private int y;
            public TimeSpan TotalFrameTime = TimeSpan.FromMilliseconds(1000 / 10);
            public TimeSpan time;
            private int FrameNumber;
            private int Frames;

            #endregion Fields

            #region Constructors

            //public Animation(int width, int height, byte clut, byte texturePage, byte cols, byte rows, ModelGroups _mg, int Count = 0, int x = 0, int y = 0, int skip = 1)
            public TextureAnimation(TextureHandler tex, int width, int height, int texturePage, int cols, int rows, int count = 0, int x = 0, int y = 0, int skip = 1)
            {
                this.tex = tex;
                this.width = width;
                this.height = height;
                this.texturePage = texturePage;
                this.cols = cols;
                this.rows = rows;
                this.skip = skip;
                FrameNumber = skip;
                this.x = x;
                this.y = y;
                Frames = count > 0 ? count : rows * cols;
            }

            #endregion Constructors

            #region Methods

            public void Update()
            {
                time += Memory.ElapsedGameTime;
                if (time > TotalFrameTime)
                {
                    FrameNumber++;
                    if (FrameNumber >= Frames)
                    {
                        FrameNumber = skip;
                    }
                    int row = (FrameNumber / cols) % rows;
                    int col = FrameNumber % cols;
                    time = TimeSpan.Zero;
                    Vector2 scale = tex.ScaleFactor;
                    Texture2D t = (Texture2D)tex;
                    Rectangle dst = new Rectangle(x + (texturePage * 128), y, width, height);
                    Rectangle src = new Rectangle(x + (texturePage * 128) + (col * width), y + (row * height), width, height);
                    src.Scale(scale);
                    dst.Scale(scale);
                    Color[] colors = new Color[width * height];
                    t.GetData(0, src, colors, 0, colors.Length);
                    t.SetData(0, dst, colors, 0, colors.Length);
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}