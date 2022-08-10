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
            struct pupu_key
            {
                private uint m_pupu_common;

                public uint pupu_common { get => m_pupu_common; set => m_pupu_common = value & 0xFFFFFF00; }
                private int x { get; set; }
                private int y { get; set; }
                public pupu_key(Tile tile)
                {
                    m_pupu_common = 0;
                    x = tile.X / Tile.Size;
                    y = tile.Y / Tile.Size;
                    pupu_common = tile.PupuID;
                }
            }
            #region Fields
            /// <summary>
            /// Use before deswizzling.
            /// </summary>
            public void UniquePupuIDs()
            {
                
                var offsets = new Dictionary<pupu_key, byte>();
               
                foreach(var item in _tiles.Select(t => new { key = new pupu_key(t), tile = t }))
                {
                    if(offsets.ContainsKey(item.key))
                    {
                        ++offsets[item.key];
                    }
                    else
                    {
                        offsets.Add(item.key, default);
                    }
                    
                    item.tile.PupuID = item.key.pupu_common + offsets[item.key];
                    if (item.tile.X % Tile.Size != 0 && item.tile.Y % Tile.Size != 0)
                    {
                        item.tile.PupuID += 0x30;
                    }
                    else if (item.tile.X % Tile.Size != 0)
                    {
                        item.tile.PupuID += 0x10;
                    }
                    else if (item.tile.Y % Tile.Size != 0)
                    {
                        item.tile.PupuID += 0x20;
                    }
                }
                //var duplicateIDs = GetOverLapTiles().ToList();
                //foreach (var i in duplicateIDs)
                //{
                //    i[1].PupuID = i[0].PupuID + 1;
                //}
                //Debug.Assert(!GetOverLapTiles().Any());
            }

            //private IEnumerable<Tile[]> GetOverLapTiles() => (from t1 in _tiles.Take(_tiles.Count - 1)
            //                                                  from t2 in _tiles.Skip(1)
            //                                            where t1.PupuID == t2.PupuID && t1.TileID < t2.TileID && t1.Intersect(t2)
            //                                            select new[] { t1, t2 }).OrderBy(x=>x[0].TileID);

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