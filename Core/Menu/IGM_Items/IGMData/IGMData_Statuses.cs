using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Items
    {
        #region Enums

        public enum Items
        {
            HP,
            LV,
            ForwardSlash
        }

        #endregion Enums

        #region Classes

        private class IGMData_Statuses : IGMData
        {
            #region Fields

            private bool eventSet;
            private Dictionary<Items, FF8String> Misc;

            #endregion Fields

            #region Constructors

            public IGMData_Statuses() : base(2, 4, new IGMDataItem.Box(title: Icons.ID.STATUS, pos: new Rectangle(420, 510, 420, 120)), 1, 2)
            {
            }

            #endregion Constructors

            #region Properties

            public Item_In_Menu Item { get; private set; }
            public Faces.ID Target { get; private set; } = Faces.ID.Blank;
            public byte TopMenuChoice { get; private set; }

            private bool All => (Item.Target & (Item_In_Menu._Target.All | Item_In_Menu._Target.All2)) != 0;

            #endregion Properties

            #region Methods

            public override void Refresh()
            {
                if (IGM_Items != null)
                {
                    if (!eventSet)
                    {
                        IGM_Items.ModeChangeHandler += ModeChangeEvent;
                        IGM_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        IGM_Items.ItemChangeHandler += ItemChangeEvent;
                        IGM_Items.TargetChangeHandler += TargetChangeEvent;
                        eventSet = true;
                    }
                    else
                        Fill(Target); // refresh the screen.
                }
            }

            protected override void Init()
            {
                Misc = new Dictionary<Items, FF8String> {
                { Items.HP,Memory.Strings.Read(Strings.FileID.MNGRP,0,26)},
                { Items.LV,Memory.Strings.Read(Strings.FileID.MNGRP,0,27)},
                { Items.ForwardSlash,Memory.Strings.Read(Strings.FileID.MNGRP,0,25)},
                };
                base.Init();
                Target = Faces.ID.Blank;
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 5 * row;
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.UseItemOnTarget))
                    TargetChangeEvent(this, Faces.ID.Blank);
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => TopMenuChoice = e.Key;

            private void Fill(Faces.ID e)
            {
                if ((e == Faces.ID.Blank && Target != Faces.ID.Blank) || All)
                {
                    Target = e;
                    foreach (IGMDataItem i in ITEM)
                        i?.Hide();
                }
                else
                {
                    if (Target == Faces.ID.Blank)
                        foreach (IGMDataItem i in ITEM)
                            i?.Show();
                    Target = e;
                    Characters character = e.ToCharacters();
                    GFs gf = e.ToGFs();
                    if (character != Characters.Blank || (gf != GFs.Blank && gf != GFs.All))
                    {
                        ITEM[0, 0] = new IGMDataItem_String(Misc[Items.LV], new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0));
                        ITEM[1, 0] = new IGMDataItem_String(Misc[Items.HP], new Rectangle(SIZE[1].X, SIZE[1].Y, 0, 0));
                        ITEM[1, 2] = new IGMDataItem_String(Misc[Items.ForwardSlash], new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0));
                    }
                    if (Memory.State.Characters != null && character != Characters.Blank)
                    {
                        ITEM[0, 1] = new IGMDataItem_Int(Memory.State.Characters[character].Level, new Rectangle(SIZE[0].X + 35, SIZE[0].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 2] = Memory.State.Party != null && Memory.State.Party.Contains(character)
                            ? new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(SIZE[0].X + 155, SIZE[0].Y, 0, 0), 6)
                            : null;
                        ITEM[1, 1] = new IGMDataItem_Int(Memory.State.Characters[character].CurrentHP(character), new Rectangle(SIZE[1].X + 35, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[1, 3] = new IGMDataItem_Int(Memory.State.Characters[character].MaxHP(character), new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 5);
                    }
                    if (Memory.State.GFs != null && (gf != GFs.Blank && gf != GFs.All))
                    {
                        ITEM[0, 1] = new IGMDataItem_Int(Memory.State.GFs[gf].Level, new Rectangle(SIZE[0].X + 35, SIZE[0].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 2] = null;
                        ITEM[1, 1] = new IGMDataItem_Int(Memory.State.GFs[gf].CurrentHP(), new Rectangle(SIZE[1].X + 35, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[1, 3] = new IGMDataItem_Int(Memory.State.GFs[gf].MaxHP(), new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 5);
                    }
                }
            }

            private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => Item = e.Key;

            private void TargetChangeEvent(object sender, Faces.ID e) => Fill(e);

            #endregion Methods
        }

        #endregion Classes
    }
}