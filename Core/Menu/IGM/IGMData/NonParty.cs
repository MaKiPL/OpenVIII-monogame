using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private sealed class NonParty : IGMData.Base, IDisposable
        {
            #region Fields

            private const int BarHeight = 4;
            private const int DefaultHPBarWidth = 118;
            private static Texture2D _redPixel;
            private bool _disposedValue;

            #endregion Fields

            #region Destructors

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            ~NonParty()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }

            #endregion Destructors

            #region Properties

            public Damageable[] Contents { get; private set; }

            #endregion Properties

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static NonParty Create() => Create<NonParty>(6, 9, new IGMDataItem.Box { Pos = new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 } }, 2, 3);

            // To detect redundant calls This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                GC.SuppressFinalize(this);
            }

            public override void Draw()
            {
                if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    base.Draw();
            }

            public override void Refresh()
            {
                if (Memory.State?.Characters == null) return;
                sbyte pos = 0;
                Update();
                if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                {
                    for (byte i = 0; Memory.State.Party != null && i < Memory.State.Characters.Count && SIZE != null && pos < SIZE.Length; i++)
                    {
                        if (Memory.State.Party.Contains((Characters)i) ||
                            !Memory.State.Characters[(Characters)i].Available) continue;
                        BLANKS[pos] = false;
                        Refresh(pos++, Memory.State[(Characters)i]);
                    }
                }
                for (; pos < Count; pos++)
                {
                    for (var i = 0; i < Depth; i++)
                    {
                        BLANKS[pos] = true;
                        ITEM[pos, i]?.Hide();
                    }
                }
            }

            protected override void Init()
            {
                Memory.MainThreadOnlyActions.Enqueue(InitRedPixel);
                Table_Options |= Table_Options.FillRows;
                Contents = new Damageable[Count];
                base.Init();

                for (var pos = 0; pos < Count; pos++)
                {
                    const float yoff = 39;
                    var bak = SIZE[pos];
                    ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos] };
                    CURSOR[pos] = new Point(bak.X, (int)(bak.Y + (6 * TextScale.Y)));

                    var r = bak;
                    r.Offset(7, yoff);
                    ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.Lv, Pos = r, Palette = 13 };

                    r = bak;
                    r.Offset((49), yoff);
                    ITEM[pos, 2] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 3 };

                    r = bak;
                    r.Offset(126, yoff);
                    ITEM[pos, 3] = new IGMDataItem.Icon { Data = Icons.ID.HP2, Pos = r, Palette = 13 };

                    r.Offset(0, 28);
                    r.Width = DefaultHPBarWidth;
                    r.Height = BarHeight;
                    ITEM[pos, 4] = new IGMDataItem.Texture { Pos = r, Color = Color.Black };
                    ITEM[pos, 5] = new IGMDataItem.Texture { Pos = r, Color = new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, 100) };

                    //r.Width = DefaultHPBarWidth;
                    //r.Offset(0, 2);
                    //ITEM[pos, 6] = new IGMDataItem.Texture { Data = _red_pixel, Pos = r, Color = Color.Black };
                    //ITEM[pos, 7] = new IGMDataItem.Texture { Data = _red_pixel, Pos = r, Color = color[0] };
                    //TODO red bar resizes based on current/max hp

                    r = bak;
                    r.Offset((166), yoff);
                    ITEM[pos, 8] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 4 };

                    for (var i = 0; i < Depth; i++)
                    {
                        ITEM[pos, i]?.Hide();
                    }
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-26, -8);
                if (row >= 1) SIZE[i].Y -= 4;
                if (row >= 2) SIZE[i].Y -= 4;
            }

            private void Dispose(bool disposing)
            {
                if (_disposedValue) return;
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (_redPixel != null)
                {
                    _redPixel.Dispose();
                    _redPixel = null;
                }
                _disposedValue = true;
            }

            private void InitRedPixel()
            {
                if (_redPixel == null)
                {
                    _redPixel = new Texture2D(Memory.Graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    _redPixel.SetData(new[] { Color.White }, 0, _redPixel.Width * _redPixel.Height);
                }
                for (var pos = 0; pos < Count; pos++)
                {
                    if (ITEM[pos, 4] != null && ITEM[pos, 4].GetType() == typeof(IGMDataItem.Texture))
                        ((IGMDataItem.Texture)ITEM[pos, 4]).Data = _redPixel;
                    if (ITEM[pos, 5] != null && ITEM[pos, 4].GetType() == typeof(IGMDataItem.Texture))
                        ((IGMDataItem.Texture)ITEM[pos, 5]).Data = _redPixel;
                }
            }

            private void Refresh(sbyte pos, Damageable damageable)
            {
                Contents[pos] = damageable;
                if (damageable != null)
                {
                    ITEM[pos, 0] = new IGMDataItem.Text { Data = damageable.Name, Pos = SIZE[pos] };
                    CURSOR[pos] = new Point(SIZE[pos].X, (int)(SIZE[pos].Y + (6 * TextScale.Y)));

                    var r = ITEM[pos, 5].Pos;
                    r.Height = BarHeight;
                    r.Width = (int)(DefaultHPBarWidth * damageable.PercentFullHP());
                    ((IGMDataItem.Texture)ITEM[pos, 5]).Pos = r;

                    //r.Offset(0, 2);
                    //((IGMDataItem.Texture)ITEM[pos, 7]).Pos = r;

                    ((IGMDataItem.Integer)ITEM[pos, 2]).Data = damageable.Level;
                    ((IGMDataItem.Integer)ITEM[pos, 8]).Data = damageable.CurrentHP();
                    for (var i = 0; i < Depth; i++)
                    {
                        ITEM[pos, i]?.Show();
                    }
                }
                else
                    for (var i = 0; i < Depth; i++)
                    {
                        ITEM[pos, i]?.Hide();
                    }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}