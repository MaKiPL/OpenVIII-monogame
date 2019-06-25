using Microsoft.Xna.Framework;
using OpenVIII.Core.Menu.IGM_Items.IGMData;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Items
        {
            #region Classes

            private class IGMData_Statuses : IGMData
            {
                #region Fields

                private bool eventSet;
                private Dictionary<Items, FF8String> Misc;

                #endregion Fields

                #region Constructors

                public IGMData_Statuses() : base(2, 4, new IGMDataItem_Box(pos: new Rectangle(420, 510, 420, 120)),2,1)
                {
                }

                #endregion Constructors

                #region Properties

                public Item_In_Menu Item { get; private set; }
                public Faces.ID Target { get; private set; }
                public byte TopMenuChoice { get; private set; }

                #endregion Properties

                #region Methods
                protected override void Init()
                {
                    Misc = new Dictionary<Items, FF8String> {
                { Items.HP,Memory.Strings.Read(Strings.FileID.MNGRP,0,26)},
                { Items.LV,Memory.Strings.Read(Strings.FileID.MNGRP,0,27)},
                { Items.ForwardSlash,Memory.Strings.Read(Strings.FileID.MNGRP,0,25)},
                };
                    base.Init();
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        InGameMenu_Items.ItemChangeHandler += ItemChangeEvent;
                        InGameMenu_Items.TargetChangeHandler += TargetChangeEvent;
                        eventSet = true;
                    }
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => TopMenuChoice = e.Key;

                private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => Item = e.Key;

                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                private bool All => (Item.Target & (Item_In_Menu._Target.All | Item_In_Menu._Target.All2)) !=0;

                private void TargetChangeEvent(object sender, Faces.ID e)
                {
                    if (!All)
                    {
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
                            ITEM[0, 2] = Memory.State.Party != null && Memory.State.Party.Contains(character)
                                ? new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(SIZE[0].X + 155, SIZE[0].Y, 0, 0), 6)
                                : null;
                            ITEM[1, 1] = new IGMDataItem_Int(Memory.State.GFs[gf].CurrentHP, new Rectangle(SIZE[1].X + 35, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                            ITEM[1, 3] = new IGMDataItem_Int(Memory.State.GFs[gf].MaxHP, new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 5);
                        }
                    }
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}

namespace OpenVIII.Core.Menu.IGM_Items.IGMData
{
    public enum Items
    {
        HP,
        LV,
        ForwardSlash
    }
}