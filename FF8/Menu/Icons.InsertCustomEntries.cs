using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Icons
    {
        /// <summary>
        /// Add Manual hardcoded entries / settings here.
        /// </summary>
        /// <remarks>Maybe one day we can export these to an xml file and read it in on launch.</remarks>
        protected override void InsertCustomEntries()
        {
            Entry BG = new Entry
            {
                X = 0,
                Y = 48,
                Width = 256,
                Height = 16,
                CustomPalette = 1,
                Fill = Vector2.UnitX,
                Tile = Vector2.UnitY,
            };
            Entry Border_TopLeft = new Entry
            {
                X = 16,
                Y = 0,
                Width = 8,
                Height = 8,
                CustomPalette = 0,
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
                CustomPalette = 0
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
                CustomPalette = 0
            };
            Entry Border_TopRight = new Entry
            {
                X = 32,
                Y = 0,
                Width = 8,
                Height = 8,
                Snap_Right = true,
                Offset = new Vector2(-8, 0),
                CustomPalette = 0
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
                CustomPalette = 0
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
                CustomPalette = 0
            };
            Entry Border_BottomLeft = new Entry
            {
                X = 16,
                Y = 16,
                Width = 8,
                Height = 8,
                Snap_Bottom = true,
                Offset = new Vector2(0, -8),
                CustomPalette = 0
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
                CustomPalette = 0
            };

            Entries[ID.Bar_BG] = new EntryGroup(new Entry
            {
                X = 16,
                Y = 24,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                Fill = Vector2.UnitY,
                CustomPalette = 0
            });
            Entries[ID.Bar_Fill] = new EntryGroup(new Entry
            {
                X = 0,
                Y = 16,
                Width = 8,
                Height = 8,
                Tile = Vector2.UnitX,
                Fill = Vector2.UnitY,
                Offset = new Vector2(2, 2),
                End = new Vector2(-2, 0),
                CustomPalette = 5
            });
            Entries[ID.MenuBorder] = new EntryGroup(Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
            Entries[ID.Menu_BG_256] = new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
            Entries[ID.Menu_BG_368] = new EntryGroup(BG, new Entry
            {
                X = 0,
                Y = 64,
                Offset = new Vector2(256, 0), //offset should be 256 but i had issue with 1 pixel gap should be able to get away with losing one pixel.
                Width = 112,
                Height = 16,
                CustomPalette = 1,
                Fill = Vector2.UnitX,
                Tile = Vector2.UnitY
            }, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);

            Entries[ID.DEBUG] = new EntryGroup(
                new Entry { X = 128, Y = 24, Width = 7, Height = 8, Offset = new Vector2(4, 0) },
                new Entry { X = 65, Y = 8, Width = 6, Height = 8, Offset = new Vector2(7 + 4, 0) },
                new Entry { X = 147, Y = 24, Width = 6, Height = 8, Offset = new Vector2(13 + 4, 0) },
                new Entry { X = 141, Y = 24, Width = 6, Height = 8, Offset = new Vector2(19 + 4, 0) },
                new Entry { X = 104, Y = 16, Width = 6, Height = 8, Offset = new Vector2(25 + 4, 0) }
                );

            Entry P_ = Entries[ID.Size_08x08_P_][0].Clone();
            P_.Offset.X += Entries[ID.COMMAND][0].Width + 4;
            P_.CustomPalette = 2;
            Entry _1 = Entries[ID.Num_8x8_1_1][0].Clone();
            _1.Offset.X += P_.Offset.X + P_.Width + 2;
            _1.CustomPalette = 7;
            Entry _2 = Entries[ID.Num_8x8_1_2][0].Clone();
            _2.Offset.X += P_.Offset.X + P_.Width + 2;
            _2.CustomPalette = 7;

            Entries[ID.COMMAND_PG1] = new EntryGroup(Entries[ID.COMMAND][0], P_, _1);
            Entries[ID.COMMAND_PG2] = new EntryGroup(Entries[ID.COMMAND][0], P_, _2);

            P_ = Entries[ID.Size_08x08_P_][0].Clone();
            P_.Offset.X += Entries[ID.ABILITY][0].Width + 8;
            P_.CustomPalette = 2;
            _1 = Entries[ID.Num_8x8_1_1][0].Clone();
            _1.Offset.X += P_.Offset.X + P_.Width + 2;
            _1.CustomPalette = 7;
            _2 = Entries[ID.Num_8x8_1_2][0].Clone();
            _2.Offset.X += P_.Offset.X + P_.Width + 2;
            _2.CustomPalette = 7;
            Entry _3 = Entries[ID.Num_8x8_1_3][0].Clone();
            _3.Offset.X += P_.Offset.X + P_.Width + 2;
            _3.CustomPalette = 7;
            Entry _4 = Entries[ID.Num_8x8_1_4][0].Clone();
            _4.Offset.X += P_.Offset.X + P_.Width + 2;
            _4.CustomPalette = 7;
            Entries[ID.ABILITY_PG1] = new EntryGroup(Entries[ID.ABILITY][0], P_, _1);
            Entries[ID.ABILITY_PG2] = new EntryGroup(Entries[ID.ABILITY][0], P_, _2);
            Entries[ID.ABILITY_PG3] = new EntryGroup(Entries[ID.ABILITY][0], P_, _3);
            Entries[ID.ABILITY_PG4] = new EntryGroup(Entries[ID.ABILITY][0], P_, _4);

            P_ = Entries[ID.Size_08x08_P_][0].Clone();
            P_.Offset.X += Entries[ID.GF][0].Width + 8;
            P_.CustomPalette = 2;
            _1 = Entries[ID.Num_8x8_1_1][0].Clone();
            _1.Offset.X += P_.Offset.X + P_.Width + 2;
            _1.CustomPalette = 7;
            _2 = Entries[ID.Num_8x8_1_2][0].Clone();
            _2.Offset.X += P_.Offset.X + P_.Width + 2;
            _2.CustomPalette = 7;
            _3 = Entries[ID.Num_8x8_1_3][0].Clone();
            _3.Offset.X += P_.Offset.X + P_.Width + 2;
            _3.CustomPalette = 7;
            _4 = Entries[ID.Num_8x8_1_4][0].Clone();
            _4.Offset.X += P_.Offset.X + P_.Width + 2;
            _4.CustomPalette = 7;
            Entries[ID.GF_PG1] = new EntryGroup(Entries[ID.GF][0], P_, _1);
            Entries[ID.GF_PG2] = new EntryGroup(Entries[ID.GF][0], P_, _2);
            Entries[ID.GF_PG3] = new EntryGroup(Entries[ID.GF][0], P_, _3);
            Entries[ID.GF_PG4] = new EntryGroup(Entries[ID.GF][0], P_, _4);

            //13 pages for 50 spells
            P_ = Entries[ID.Size_08x08_P_][0].Clone();
            P_.Offset.X += Entries[ID.MAGIC][0].Width + 8;
            P_.CustomPalette = 2;
            _1 = Entries[ID.Num_8x8_1_1][0].Clone();
            _1.Offset.X += P_.Offset.X + P_.Width + 2;
            _1.CustomPalette = 7;
            _2 = Entries[ID.Num_8x8_1_2][0].Clone();
            _2.Offset.X += P_.Offset.X + P_.Width + 2;
            _2.CustomPalette = 7;
            _3 = Entries[ID.Num_8x8_1_3][0].Clone();
            _3.Offset.X += P_.Offset.X + P_.Width + 2;
            _3.CustomPalette = 7;
            _4 = Entries[ID.Num_8x8_1_4][0].Clone();
            _4.Offset.X += P_.Offset.X + P_.Width + 2;
            _4.CustomPalette = 7;
            Entry _5 = Entries[ID.Num_8x8_1_5][0].Clone();
            _5.Offset.X += P_.Offset.X + P_.Width + 2;
            _5.CustomPalette = 7;
            Entry _6 = Entries[ID.Num_8x8_1_6][0].Clone();
            _6.Offset.X += P_.Offset.X + P_.Width + 2;
            _6.CustomPalette = 7;
            Entry _7 = Entries[ID.Num_8x8_1_7][0].Clone();
            _7.Offset.X += P_.Offset.X + P_.Width + 2;
            _7.CustomPalette = 7;
            Entry _8 = Entries[ID.Num_8x8_1_8][0].Clone();
            _8.Offset.X += P_.Offset.X + P_.Width + 2;
            _8.CustomPalette = 7;
            Entry _9 = Entries[ID.Num_8x8_1_9][0].Clone();
            _9.Offset.X += P_.Offset.X + P_.Width + 2;
            _9.CustomPalette = 7;
            Entry __0 = Entries[ID.Num_8x8_1_0][0].Clone();
            __0.Offset.X += P_.Offset.X + P_.Width + 2 + _1.Width;
            __0.CustomPalette = 7;
            Entry __1 = _1.Clone();
            __1.Offset.X = __0.Offset.X;
            Entry __2 = _2.Clone();
            __2.Offset.X = __0.Offset.X;
            Entry __3 = _3.Clone();
            __3.Offset.X = __0.Offset.X;
            Entries[ID.MAGIC_PG1] = new EntryGroup(Entries[ID.MAGIC][0], P_, _1);
            Entries[ID.MAGIC_PG2] = new EntryGroup(Entries[ID.MAGIC][0], P_, _2);
            Entries[ID.MAGIC_PG3] = new EntryGroup(Entries[ID.MAGIC][0], P_, _3);
            Entries[ID.MAGIC_PG4] = new EntryGroup(Entries[ID.MAGIC][0], P_, _4);
            Entries[ID.MAGIC_PG5] = new EntryGroup(Entries[ID.MAGIC][0], P_, _5);
            Entries[ID.MAGIC_PG6] = new EntryGroup(Entries[ID.MAGIC][0], P_, _6);
            Entries[ID.MAGIC_PG7] = new EntryGroup(Entries[ID.MAGIC][0], P_, _7);
            Entries[ID.MAGIC_PG8] = new EntryGroup(Entries[ID.MAGIC][0], P_, _8);
            Entries[ID.MAGIC_PG9] = new EntryGroup(Entries[ID.MAGIC][0], P_, _9);
            Entries[ID.MAGIC_PG10] = new EntryGroup(Entries[ID.MAGIC][0], P_, _1, __0);
            Entries[ID.MAGIC_PG11] = new EntryGroup(Entries[ID.MAGIC][0], P_, _1, __1);
            Entries[ID.MAGIC_PG12] = new EntryGroup(Entries[ID.MAGIC][0], P_, _1, __2);
            Entries[ID.MAGIC_PG13] = new EntryGroup(Entries[ID.MAGIC][0], P_, _1, __3);


            //18 pages for 198 Items
            P_ = Entries[ID.Size_08x08_P_][0].Clone();
            P_.Offset.X += Entries[ID.ITEM][0].Width + 8;
            P_.CustomPalette = 2;
            _1 = Entries[ID.Num_8x8_1_1][0].Clone();
            _1.Offset.X += P_.Offset.X + P_.Width + 2;
            _1.CustomPalette = 7;
            _2 = Entries[ID.Num_8x8_1_2][0].Clone();
            _2.Offset.X += P_.Offset.X + P_.Width + 2;
            _2.CustomPalette = 7;
            _3 = Entries[ID.Num_8x8_1_3][0].Clone();
            _3.Offset.X += P_.Offset.X + P_.Width + 2;
            _3.CustomPalette = 7;
            _4 = Entries[ID.Num_8x8_1_4][0].Clone();
            _4.Offset.X += P_.Offset.X + P_.Width + 2;
            _4.CustomPalette = 7;
            _5 = Entries[ID.Num_8x8_1_5][0].Clone();
            _5.Offset.X += P_.Offset.X + P_.Width + 2;
            _5.CustomPalette = 7;
            _6 = Entries[ID.Num_8x8_1_6][0].Clone();
            _6.Offset.X += P_.Offset.X + P_.Width + 2;
            _6.CustomPalette = 7;
            _7 = Entries[ID.Num_8x8_1_7][0].Clone();
            _7.Offset.X += P_.Offset.X + P_.Width + 2;
            _7.CustomPalette = 7;
            _8 = Entries[ID.Num_8x8_1_8][0].Clone();
            _8.Offset.X += P_.Offset.X + P_.Width + 2;
            _8.CustomPalette = 7;
            _9 = Entries[ID.Num_8x8_1_9][0].Clone();
            _9.Offset.X += P_.Offset.X + P_.Width + 2;
            _9.CustomPalette = 7;
            __0 = Entries[ID.Num_8x8_1_0][0].Clone();
            __0.Offset.X += P_.Offset.X + P_.Width + 2 + _1.Width;
            __0.CustomPalette = 7;
            __1 = _1.Clone();
            __1.Offset.X = __0.Offset.X;
            __2 = _2.Clone();
            __2.Offset.X = __0.Offset.X;
            __3 = _3.Clone();
            __3.Offset.X = __0.Offset.X;
            Entry __4 = _4.Clone();
            __4.Offset.X = __0.Offset.X;
            Entry __5 = _5.Clone();
            __5.Offset.X = __0.Offset.X;
            Entry __6 = _6.Clone();
            __6.Offset.X = __0.Offset.X;
            Entry __7 = _7.Clone();
            __7.Offset.X = __0.Offset.X;
            Entry __8 = _8.Clone();
            __8.Offset.X = __0.Offset.X;
            Entries[ID.ITEM_PG1] = new EntryGroup(Entries[ID.ITEM][0], P_, _1);
            Entries[ID.ITEM_PG2] = new EntryGroup(Entries[ID.ITEM][0], P_, _2);
            Entries[ID.ITEM_PG3] = new EntryGroup(Entries[ID.ITEM][0], P_, _3);
            Entries[ID.ITEM_PG4] = new EntryGroup(Entries[ID.ITEM][0], P_, _4);
            Entries[ID.ITEM_PG5] = new EntryGroup(Entries[ID.ITEM][0], P_, _5);
            Entries[ID.ITEM_PG6] = new EntryGroup(Entries[ID.ITEM][0], P_, _6);
            Entries[ID.ITEM_PG7] = new EntryGroup(Entries[ID.ITEM][0], P_, _7);
            Entries[ID.ITEM_PG8] = new EntryGroup(Entries[ID.ITEM][0], P_, _8);
            Entries[ID.ITEM_PG9] = new EntryGroup(Entries[ID.ITEM][0], P_, _9);
            Entries[ID.ITEM_PG10] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __0);
            Entries[ID.ITEM_PG11] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __1);
            Entries[ID.ITEM_PG12] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __2);
            Entries[ID.ITEM_PG13] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __3);
            Entries[ID.ITEM_PG14] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __4);
            Entries[ID.ITEM_PG15] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __5);
            Entries[ID.ITEM_PG16] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __6);
            Entries[ID.ITEM_PG17] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __7);
            Entries[ID.ITEM_PG18] = new EntryGroup(Entries[ID.ITEM][0], P_, _1, __8);

            //revese order of rewind so arrows draw correctly
            Entry _RR_0 = Entries[ID.Rewind_Fast][0].Clone();
            Entry _RR_1 = Entries[ID.Rewind_Fast][1].Clone();
            Entries[ID.Rewind_Fast] = new EntryGroup(_RR_1, _RR_0);
        }
    }
}