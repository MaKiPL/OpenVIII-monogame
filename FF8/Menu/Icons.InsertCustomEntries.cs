using Microsoft.Xna.Framework;

namespace FF8
{
    internal partial class Icons
    {
        protected override void InsertCustomEntries()
        {
            Entry BG = new Entry
            {
                X = 0,
                Y = 48,
                Width = 256,
                Height = 16,
                CustomPallet = 1,
                Fill = Vector2.UnitX,
                Tile = Vector2.UnitY,
            };
            Entry Border_TopLeft = new Entry
            {
                X = 16,
                Y = 0,
                Width = 8,
                Height = 8,
                CustomPallet = 0,
            };
            Entry Border_Top = new Entry
            {
                X = 24,
                Y = 0,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                Offset = new Vector2(8, 0),
                End = new Vector2(-8, 0),
                CustomPallet = 0
            };
            Entry Border_Bottom = new Entry
            {
                X = 24,
                Y = 16,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                Snap_Bottom = true,
                Offset = new Vector2(8, -8),
                End = new Vector2(-8, 0),
                CustomPallet = 0
            };
            Entry Border_TopRight = new Entry
            {
                X = 32,
                Y = 0,
                Width = 8,
                Height = 8,
                Snap_Right = true,
                Offset = new Vector2(-8, 0),
                CustomPallet = 0
            };
            Entry Border_Left = new Entry
            {
                X = 16,
                Y = 8,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitY,
                Offset = new Vector2(0, 8),
                End = new Vector2(0, -8),
                CustomPallet = 0
            };
            Entry Border_Right = new Entry
            {
                X = 32,
                Y = 8,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitY,
                Snap_Right = true,
                Offset = new Vector2(-8, 8),
                End = new Vector2(0, -8),
                CustomPallet = 0
            };
            Entry Border_BottomLeft = new Entry
            {
                X = 16,
                Y = 16,
                Width = 8,
                Height = 8,
                Snap_Bottom = true,
                Offset = new Vector2(0, -8),
                CustomPallet = 0
            };
            Entry Border_BottomRight = new Entry
            {
                X = 32,
                Y = 16,
                Width = 8,
                Height = 8,
                Snap_Bottom = true,
                Snap_Right = true,
                Offset = new Vector2(-8, -8),
                CustomPallet = 0
            };

            Entries[ID.Bar_BG] = new EntryGroup(new Entry
            {
                X = 16,
                Y = 24,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                CustomPallet = 0
            });
            Entries[ID.Bar_Fill] = new EntryGroup(new Entry
            {
                X = 0,
                Y = 16,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                Offset = new Vector2(0, 1),
                CustomPallet = 5
            });
            Entries[ID.Menu_BG_256] = new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
            Entries[ID.Menu_BG_368] = new EntryGroup(BG, new Entry
            {
                X = 0,
                Y = 64,
                Offset = new Vector2(256, 0),
                Width = 112,
                Height = 16,
                CustomPallet = 1,
                Fill = Vector2.UnitX,
                Tile = Vector2.UnitY
            }, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
            
            Entries[ID.DEBUG] = new EntryGroup(
                new Entry { X = 128, Y = 24, Width = 7, Height = 8 },
                new Entry { X = 65, Y = 8, Width = 6, Height = 8, Offset = new Vector2(7, 0) },
                new Entry { X = 147, Y = 24, Width = 6, Height = 8, Offset = new Vector2(13, 0) },
                new Entry { X = 141, Y = 24, Width = 6, Height = 8, Offset = new Vector2(19, 0) },
                new Entry { X = 104, Y = 16, Width = 6, Height = 8, Offset = new Vector2(25, 0) }
                );
        }
    }
}