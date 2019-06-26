using Microsoft.Xna.Framework;
using System;
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

            /// <summary>
            /// </summary>
            /// <remarks>
            /// Using Faces.ID because it contains characters and gfs. Can cast to Characters or
            /// subtract 16 and cast to GFs
            /// </remarks>
            private class IGMData_TargetPool : IGMData_Pool<Saves.Data, Faces.ID>
            {
                #region Fields

                private bool eventSet;

                #endregion Fields

                #region Constructors

                public IGMData_TargetPool() : base(9, 3, new IGMDataItem_Box(pos: new Rectangle(420, 150, 420, 360), title: Icons.ID.TARGET), 9, 1) => Cursor_Status &= ~Cursor_Status.Enabled;

                #endregion Constructors

                #region Properties

                public Item_In_Menu Item { get; private set; }
                public byte TopMenuChoice { get; private set; }

                private bool All => (Item.Target & (Item_In_Menu._Target.All | Item_In_Menu._Target.All2)) != 0;

                private bool IsMe => InGameMenu_Items.GetMode().Equals(Mode.UseItemOnTarget);

                #endregion Properties

                #region Methods

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-18, -20);
                    SIZE[i].Y -= 3 * row;
                    //SIZE[i].X += 2;
                    SIZE[i].Height = (int)(12 * TextScale.Y);
                }

                public override void Draw()
                {
                    base.Draw();
                    if (All && IsMe)
                    {
                        // if all draw blinking pointers on everyone.
                        byte i = 0;
                        foreach (Point c in CURSOR)
                        {
                            if (!BLANKS[i++])
                                DrawPointer(c, blink: true);
                        }
                    }
                }

                public override bool Inputs()
                {
                    // if ((Item.Target & (Item_In_Menu._Target.All|Item_In_Menu._Target.All2)) == 0)
                    Cursor_Status |= Cursor_Status.Enabled;
                    return base.Inputs();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Items.SetMode(Mode.SelectItem);
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        InGameMenu_Items.ItemChangeHandler += ItemTypeChangeEvent;
                        eventSet = true;
                    }
                    ////List won't populate unless theres a valid item set.
                    //if (Item.Type == Item_In_Menu._Type.None && Memory.State.Items != null)
                    //    Item = Memory.MItems[Memory.State.Items.FirstOrDefault(m => m.ID > 0 && m.QTY > 0).ID];
                    //Fill();
                    base.ReInit();
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => TopMenuChoice = e.Key;

                private void ItemTypeChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e)
                {
                    if (!Item.Equals(e.Key))
                    {
                        bool sameTargets = Item.Target != e.Key.Target || Item.Type != e.Key.Type;

                        Item = e.Key;
                        if (sameTargets)
                        {
                            Fill();
                            base.ReInit();
                        }
                    }
                }

                private bool TestCharacter(ref Faces.ID id)
                {
                    Characters character = id.ToCharacters();
                    if (Item.Type == Item_In_Menu._Type.Blue_Magic & character != Characters.Quistis_Trepe)
                        return false;
                    if (character == Characters.Blank || (Item.Target & Item_In_Menu._Target.Character) == 0)
                        return false;
                    if (Memory.State.Characters.ContainsKey(character) && Memory.State.Characters[character].VisibleInMenu)
                        return true;
                    return false;
                }

                private bool TestGF(ref Faces.ID id)
                {
                    GFs gf = id.ToGFs();
                    if (gf == GFs.Blank || gf == GFs.All || (Item.Target & Item_In_Menu._Target.GF) == 0)
                        return false;
                    if (Memory.State.GFs.ContainsKey(gf))// && Memory.State.GFs[gf].VisibleInMenu)
                        return true;
                    return false;
                }

                public override void ResetPages()
                {
                }

                private void Fill()
                {
                    Faces.ID id = 0;
                    int skip = Page * rows;
                    for (int i = 0; i < rows; i++)
                    {
                        while (!Enum.IsDefined(typeof(Faces.ID), id)
                            || !(TestCharacter(ref id) || TestGF(ref id))
                            || skip-- > 0)
                            if ((byte)++id > 32)
                            {
                                for (; i < rows; i++)
                                    ITEM[i, 0] = null;
                                Pages = Page+1;
                                return;
                            }
                        ITEM[i, 0] = new IGMDataItem_String(Memory.Strings.GetName(id), pos: SIZE[i]);
                        id++;                          
                    }
                    Pages = Page + 2;
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                    if (!IsMe)
                        Cursor_Status &= ~Cursor_Status.Enabled;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}