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

            private readonly TimeSpan _pauseAtStart;
            private readonly bool _reversible;
            private readonly bool _topDown;
            private readonly TimeSpan _totalFrameTime;
            private readonly byte _cols;
            private readonly int _frames;
            private readonly int _height;
            private readonly IReadOnlyList<Quad> _qs;
            private readonly byte _rows;
            private readonly int _skip;
            private readonly IReadOnlyList<Triangle> _ts;
            private readonly int _width;
            private int _frameNumber;
            private sbyte _step = 1;
            private TimeSpan _time;

            #endregion Fields

            #region Constructors

            public Animation(int width, int height, byte texturePage, byte cols, byte rows, ModelGroups mg,
                int count = 0, int x = 0, int y = 0, int skip = 1, bool topDown = false, bool reversible = false,
                TimeSpan? totalFrameTime = null, TimeSpan? pauseAtStart = null)
            {
                _totalFrameTime = totalFrameTime ?? TimeSpan.FromMilliseconds(100);
                _pauseAtStart = pauseAtStart ?? TimeSpan.Zero;
                _topDown = topDown;
                _reversible = reversible;
                _width = width;
                _height = height;
                _cols = cols;
                _rows = rows;
                _frames = count > 0 && count <= _cols * _rows ? count : _cols * _rows;
                _skip = skip;
                Rectangle rectangle = new Rectangle(x, y, width, height);
                IEnumerable<Model> temp = (from modelGroup in mg
                                           from model in modelGroup
                                           select model).Where(i => i.Quads != null && i.Triangles != null && i.Vertices != null).ToArray();

                _qs = (from model in temp
                       from q in model.Quads
                       select q).Where(q => rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
                _ts = (from model in temp
                       from q in model.Triangles
                       select q).Where(q => rectangle.Contains(q.Rectangle) && q.TexturePage == texturePage).ToList();
            }

            #endregion Constructors

            #region Methods

            public void Update()
            {
                _time += Memory.ElapsedGameTime;
                if (_time < _totalFrameTime + (_frameNumber <= _skip ? _pauseAtStart : TimeSpan.Zero)) return;
                _time = TimeSpan.Zero;
                int last = _frameNumber;
                if (_step != 1 && _step != -1) _step = 1;
                _frameNumber += _step;
                if (_reversible)
                {
                    if (_frameNumber >= _frames)
                    {
                        _frameNumber = _frames - 1;
                        _step *= -1;
                    }
                    if (_frameNumber < _skip)
                    {
                        _frameNumber = _skip + 1;
                        _step *= -1;
                    }
                }
                else if (_frameNumber >= _frames)
                {
                    _frameNumber = _skip;
                }
                int lastRow, lastCol, col, row;
                if (!_topDown)
                {
                    lastCol = last % _cols;
                    lastRow = (last / _cols) % _rows;
                    col = _frameNumber % _cols;
                    row = (_frameNumber / _cols) % _rows;
                }
                else
                {
                    lastCol = (last / _rows) % _cols;
                    lastRow = last % _rows;
                    col = (_frameNumber / _rows) % _cols;
                    row = _frameNumber % _rows;
                }

                foreach (Quad q in _qs)
                {
                    for (int i = 0; i < q.UVs.Count; i++)
                    {
                        Vector2 uv = q.UVs[i];
                        uv.X += _width * (col - lastCol);
                        uv.Y += _height * (row - lastRow);
                        q.UVs[i] = uv;
                    }
                }
                foreach (Triangle q in _ts)
                {
                    for (int i = 0; i < q.UVs.Count; i++)
                    {
                        Vector2 uv = q.UVs[i];
                        uv.X += _width * (col - lastCol);
                        uv.Y += _height * (row - lastRow);
                        q.UVs[i] = uv;
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}