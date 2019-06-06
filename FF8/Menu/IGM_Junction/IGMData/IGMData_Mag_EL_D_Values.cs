using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_EL_D_Values : IGMData
            {
                public IGMData_Mag_EL_A_D_Slots Slots { get; private set; }

                public IGMData_Mag_EL_D_Values() : base(8, 5, new IGMDataItem_Box(title: Icons.ID.Elemental_Defense, pos: new Rectangle(280, 423, 545, 201)), 2, 4)
                {
                }

                public override void ReInit()
                {
                    if (InGameMenu_Junction != null)
                        //Slots = (IGMData_Mag_Stat_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[0, 0]).Data;
                        Slots = (IGMData_Mag_EL_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[3, 0]).Data;
                    
                    //Slots = (IGMData_Mag_ST_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[6, 0]).Data;
                    base.ReInit();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-25, -25);
                    SIZE[i].Y -= 6 * row;
                }

                public Dictionary<Kernel_bin.Element, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                {
                    byte[] spell = new byte[] {
                            source.Stat_J[Kernel_bin.Stat.Elem_Def_1],
                            source.Stat_J[Kernel_bin.Stat.Elem_Def_2],
                            source.Stat_J[Kernel_bin.Stat.Elem_Def_3],
                            source.Stat_J[Kernel_bin.Stat.Elem_Def_4]
                        };
                    Dictionary<Kernel_bin.Element, byte> total = new Dictionary<Kernel_bin.Element, byte>(8);

                    IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(Kernel_bin.Element)).Cast<Enum>();
                    foreach (Enum flag in availableFlags.Where(d => !total.ContainsKey((Kernel_bin.Element)d)))
                        total.Add((Kernel_bin.Element)flag, 0);
                    for (int i = 0; i < spell.Length; i++)
                        foreach (Enum flag in availableFlags.Where(Kernel_bin.MagicData[spell[i]].Elem_J_def.HasFlag))
                        {
                            int t = total[(Kernel_bin.Element)flag] + ((Kernel_bin.MagicData[spell[i]].Elem_J_def_val * Memory.State.Characters[Character].Magics[spell[i]]) / 100);
                            total[(Kernel_bin.Element)flag] = (byte)(t >200? 200:t);
                        }

                    availableFlagsarray = availableFlags.ToArray();
                    return total;
                }

                public override bool Update()
                {
                    if (Memory.State.Characters != null)
                    {
                        int _nag = 0;
                        int _pos = 0;
                        Dictionary<Kernel_bin.Element, byte> oldtotal = (Slots.PrevSetting != null) ? getTotal(Slots.PrevSetting, out Enum[] availableFlagsarray) : null;
                        Dictionary<Kernel_bin.Element, byte> total = getTotal(Memory.State.Characters[Character], out availableFlagsarray);
                        for (short pos = 0; pos < Count; pos++)
                        {
                            ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Element_Fire + pos, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 9);
                            ITEM[pos, 1] = total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] > 100 ? new IGMDataItem_Icon(Icons.ID.Star, new Rectangle(SIZE[pos].X + 45, SIZE[pos].Y, 0, 0), 4) : null;
                            ITEM[pos, 2] = null;
                            ITEM[pos, 3] = new IGMDataItem_Int(
                                total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] > 100 ?
                                total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] - 100 :
                                total[(Kernel_bin.Element)availableFlagsarray[pos + 1]], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 80, SIZE[pos].Y, 0, 0), 17, numtype: Icons.NumType.sysFntBig, spaces: 3);
                            ITEM[pos, 4] = new IGMDataItem_String("%", new Rectangle(SIZE[pos].X + SIZE[pos].Width - 20, SIZE[pos].Y, 0, 0));
                            if (oldtotal != null)
                            {
                                if (oldtotal[(Kernel_bin.Element)availableFlagsarray[pos + 1]] > total[(Kernel_bin.Element)availableFlagsarray[pos + 1]])
                                {
                                    ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 5;
                                    ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 5;
                                    ITEM[pos, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Down, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 105, SIZE[pos].Y, 0, 0), 16);
                                    ((IGMDataItem_Int)ITEM[pos, 3]).Colorid = Font.ColorID.Red;
                                    ((IGMDataItem_String)ITEM[pos, 4]).Colorid = Font.ColorID.Red;
                                    if (++_nag > _pos)
                                    {
                                        ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Pallet = 5;
                                        ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Faded_Pallet = 5;
                                        ((IGMDataItem_String)Slots.ITEM[Slots.CURSOR_SELECT, 1]).Colorid = Font.ColorID.Red;
                                    }
                                }
                                else if (oldtotal[(Kernel_bin.Element)availableFlagsarray[pos + 1]] < total[(Kernel_bin.Element)availableFlagsarray[pos + 1]])
                                {
                                    ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 6;
                                    ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 6;
                                    ITEM[pos, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 105, SIZE[pos].Y, 0, 0), 17);
                                    ((IGMDataItem_Int)ITEM[pos, 3]).Colorid = Font.ColorID.Yellow;
                                    ((IGMDataItem_String)ITEM[pos, 4]).Colorid = Font.ColorID.Yellow;
                                    if (_nag <= ++_pos)
                                    {
                                        ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Pallet = 6;
                                        ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Faded_Pallet = 6;
                                        ((IGMDataItem_String)Slots.ITEM[Slots.CURSOR_SELECT, 1]).Colorid = Font.ColorID.Yellow;
                                    }
                                }
                            }
                        }
                    }
                    return base.Update();
                }
            }
        }
    }
}