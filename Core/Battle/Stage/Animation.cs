using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        private class Animation
        {
            public int FrameNumber = 0;
            public TimeSpan TotalFrameTime = TimeSpan.FromMilliseconds(1000 / 10);
            public TimeSpan PauseAtStart = TimeSpan.Zero;
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
            private int skip;

            public Animation(int width, int height, byte clut, byte texturePage, byte cols, byte rows, ModelGroups _mg, int count = 0, int x = 0, int y = 0, int skip = 1)
            {
                Width = width;
                Height = height;
                Clut = clut;
                TexturePage = texturePage;
                Cols = cols;
                Rows = rows;
                Frames = count > 0 && count <= Cols * Rows ? count : Cols * Rows;
                this.skip = skip;
                Rectangle = new Rectangle(x, y, width, height);
                IEnumerable<Model> temp = (from modelgroups in _mg
                                           from model in modelgroups
                                           select model).Where(i => i.quads != null && i.triangles != null && i.vertices != null);

                qs = (from model in temp
                      from q in model.quads
                      select q).Where(q => Rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
                ts = (from model in temp
                      from q in model.triangles
                      select q).Where(q => Rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
            }

            public int Frames { get; private set; }

            private sbyte step = 1;
            public bool TopDown;
            public bool Reverseable;

            public void Update()
            {
                time += Memory.gameTime.ElapsedGameTime;
                if (time >= TotalFrameTime + (FrameNumber <= skip ? PauseAtStart : TimeSpan.Zero))
                {
                    time = TimeSpan.Zero;
                    int Last = FrameNumber;
                    if (step != 1 && step != -1) step = 1;
                    FrameNumber += step;
                    if (Reverseable)
                    {
                        if (FrameNumber >= Frames)
                        {
                            FrameNumber = Frames - 1;
                            step *= -1;
                        }
                        if (FrameNumber < skip)
                        {
                            FrameNumber = skip + 1;
                            step *= -1;
                        }
                    }
                    else
                        if (FrameNumber >= Frames)
                    {
                        FrameNumber = skip;
                    }
                    int lastrow, lastcol, col, row;
                    if (!TopDown)
                    {
                        lastcol = Last % Cols;
                        lastrow = (Last / Cols) % Rows;
                        col = FrameNumber % Cols;
                        row = (FrameNumber / Cols) % Rows;
                    }
                    else
                    {
                        lastcol = (Last / Rows) % Cols;
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
        }
    }
}