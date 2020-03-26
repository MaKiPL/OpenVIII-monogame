using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    public partial class Background
    {
        #region Classes

        public class Tiles : IReadOnlyList<Tile>
        {
            #region Fields
            /// <summary>
            /// Use before deswizzling.
            /// </summary>
            public void UniquePupuIDs()
            {
                var duplicateIDs = GetOverLapTiles().ToList();
                foreach (var i in duplicateIDs)
                {
                    i[1].PupuID = i[0].PupuID + 1;
                }
                Debug.Assert(!GetOverLapTiles().Any());
            }

            private IEnumerable<Tile[]> GetOverLapTiles() => (from t1 in _tiles
                                                        from t2 in _tiles
                                                        where t1.PupuID == t2.PupuID && t1.TileID < t2.TileID && t1.Intersect(t2)
                                                        select new[] { t1, t2 }).OrderBy(x=>x[0].TileID);

            private readonly IReadOnlyList<Tile> _tiles;

            #endregion Fields

            #region Constructors

            public Tiles() => _tiles = new List<Tile>();

            public Tiles(IReadOnlyList<Tile> tiles) => _tiles = tiles;

            #endregion Constructors

            #region Properties

            public Point BottomRight => new Point(_tiles.Max(tile => tile.X) + Tile.Size, _tiles.Max(tile => tile.Y) + Tile.Size);
            

            public int Height => Math.Abs(TopLeft.Y) + BottomRight.Y;// + (int)Origin.Y;

            public Point TopLeft => new Point(_tiles.Min(tile => tile.X), _tiles.Min(tile => tile.Y));
            public int Width => Math.Abs(TopLeft.X) + BottomRight.X;//+ (int)Origin.X;
            public Point Size => new Point(Width, Height);
            public Vector2 Origin => BottomRight.ToVector2() + TopLeft.ToVector2();

            #endregion Properties

            #region Indexers


            #endregion Indexers

            #region Methods

            public static Tiles Load(byte[] mapB, byte textureType)
            {
                var tiles = new List<Tile>();
                //128x256
                using (var br = new BinaryReader(new MemoryStream(mapB)))
                    while (br.BaseStream.Position + 16 < br.BaseStream.Length)
                    {
                        var tile = Tile.Load(br, tiles.Count, textureType);
                        if (tile != null)
                            tiles.Add(tile);
                    }
                return new Tiles(tiles.AsReadOnly());
            }

            #endregion Methods

            public IEnumerator<Tile> GetEnumerator()
            {
                return _tiles.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _tiles).GetEnumerator();
            }

            public int Count => _tiles.Count;

            public Tile this[int index] => _tiles[index];
        }

        #endregion Classes
    }
}