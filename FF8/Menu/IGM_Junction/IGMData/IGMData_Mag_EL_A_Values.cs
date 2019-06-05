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
            private class IGMData_Mag_EL_A_Values : IGMData
            {
                public IGMData_Mag_EL_A_Values() : base( 8, 5, new IGMDataItem_Box(title: Icons.ID.Elemental_Attack, pos: new Rectangle(280, 423, 545, 201)), 2, 4)
                {
                }
                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-25, -25);
                    SIZE[i].Y -= 6 * row;
                }

                public override bool Update()
                {
                    if (Memory.State.Characters != null)
                    {
                        byte[] spell = new byte[] {
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.Elem_Atk]
                        };
                        Dictionary<Kernel_bin.Element, byte> total = new Dictionary<Kernel_bin.Element, byte>(8);

                        IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(Kernel_bin.Element)).Cast<Enum>();
                        foreach (Enum flag in availableFlags)
                            total.Add((Kernel_bin.Element)flag, 0);
                        for (int i = 0; i < spell.Length; i++)
                            foreach (Enum flag in availableFlags.Where(Kernel_bin.MagicData[spell[i]].Elem_J_atk.HasFlag))
                            {
                                total[(Kernel_bin.Element)flag] += (byte)((Kernel_bin.MagicData[spell[i]].Elem_J_atk_val * Memory.State.Characters[Character].Magics[spell[i]]) / 100);
                                if (total[(Kernel_bin.Element)flag] > 200) total[(Kernel_bin.Element)flag] = 200;
                            }

                        Enum[] availableFlagsarray = availableFlags.ToArray();
                        for (short pos = 0; pos < Count; pos++)
                        {
                            ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Element_Fire + pos, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 9);
                            ITEM[pos, 1] = total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] > 100 ? new IGMDataItem_Icon(Icons.ID.Star, new Rectangle(SIZE[pos].X + 45, SIZE[pos].Y, 0, 0), 4) : null;
                            //ITEM[pos, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 105, SIZE[pos].Y, 0, 0), 17);
                            ITEM[pos, 2] = null;
                            if (total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] > 100) total[(Kernel_bin.Element)availableFlagsarray[pos + 1]] -= 100;
                            ITEM[pos, 3] = new IGMDataItem_Int(total[(Kernel_bin.Element)availableFlagsarray[pos + 1]], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 80, SIZE[pos].Y, 0, 0), 17, numtype: Icons.NumType.sysFntBig, spaces: 3);
                            ITEM[pos, 4] = new IGMDataItem_String("%", new Rectangle(SIZE[pos].X + SIZE[pos].Width - 20, SIZE[pos].Y, 0, 0));
                        }
                    }
                    return base.Update();
                }
            }
        }
    }
}