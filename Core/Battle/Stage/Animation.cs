using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class Animation
        {
            #region Fields

            public int FrameNumber = 0;
            public TimeSpan frametime = TimeSpan.FromMilliseconds(1000 / 10);
            public TimeSpan time = TimeSpan.Zero;
            private byte Clut = 4;
            private byte Cols = 4;
            private int Height = 64;
            private List<Quad> qs;
            private Rectangle Rectangle;
            private byte Rows = 2;
            //private Vector2 start;
            private byte TexturePage = 4;
            private List<Triangle> ts;
            private int Width = 64;

            #endregion Fields

            #region Constructors

            public Animation(int width, int height, byte clut, byte texturePage, byte cols, byte rows, int texWidth, ModelGroups _mg)
            {
                Width = width;
                Height = height;
                Clut = clut;
                TexturePage = texturePage;
                Cols = cols;
                Rows = rows;
                //start = CalculateUV(Vector2.Zero, texturePage);
                Rectangle = new Rectangle(0, 0, width, height);

                qs = (from modelgroups in _mg
                      from mg in modelgroups
                      from q in mg.quads
                      select q).Where(q => Rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
                ts = (from modelgroups in _mg
                      from mg in modelgroups
                      from q in mg.triangles
                      select q).Where(q => Rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
            }

            #endregion Constructors

            #region Properties

            public int Frames => Rows * Cols;

            #endregion Properties

            #region Methods

            public void Update()
            {
                time += Memory.gameTime.ElapsedGameTime;
                if (time >= frametime)
                {
                    time = TimeSpan.Zero;
                    int Last = FrameNumber;
                    FrameNumber += 1;
                    if (FrameNumber >= Frames)
                        FrameNumber = 0;
                    int lastrow, lastcol, col, row;
                    if (true)
                    {
                        lastcol = Last % Cols;
                        lastrow = (Last / Cols)%Rows;
                        col = FrameNumber % Cols;
                        row = (FrameNumber / Cols)%Rows;
                    }
                    else
                    {

                        lastcol = (Last /Rows) % Cols;
                        lastrow = Last % Rows;
                        col = (FrameNumber / Rows) % Cols;
                        row = FrameNumber % Rows;
                    }

                    foreach (Quad q in qs)
                    {
                        for (int i = 0; i < q.UVs.Count; i++)
                        {
                            Vector2 uv = q.UVs[i];
                            uv.X += Width * (col - lastcol);
                            uv.Y += Height * (row - lastrow);
                            q.UVs[i] = uv;
                        }
                    }
                    foreach (Triangle q in ts)
                    {
                        for (int i = 0; i < q.UVs.Count; i++)
                        {
                            Vector2 uv = q.UVs[i];
                            uv.X += Width * (col - lastcol);
                            uv.Y += Height * (row - lastrow);
                            q.UVs[i] = uv;
                        }
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}