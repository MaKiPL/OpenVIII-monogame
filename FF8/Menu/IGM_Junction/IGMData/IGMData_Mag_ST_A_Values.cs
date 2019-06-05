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
            private class IGMData_Mag_ST_A_Values : IGMData
            {
                public IGMData_Mag_ST_A_D_Slots ST_A_D_Slots { get; }

                public IGMData_Mag_ST_A_Values(IGMData_Mag_ST_A_D_Slots mag_ST_A_D_Slots) : base( 12, 5, new IGMDataItem_Box(title: Icons.ID.Status_Attack, pos: new Rectangle(280, 363, 545, 267)), 2, 6)
                {
                    ST_A_D_Slots = mag_ST_A_D_Slots;
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
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Atk]
                        };
                        Dictionary<Kernel_bin.J_Statuses, byte> total = new Dictionary<Kernel_bin.J_Statuses, byte>(8);

                        IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(Kernel_bin.J_Statuses)).Cast<Enum>();
                        foreach (Enum flag in availableFlags.Where(d=>!total.ContainsKey((Kernel_bin.J_Statuses)d)))
                            total.Add((Kernel_bin.J_Statuses)flag, 0);
                        for (int i = 0; i < spell.Length; i++)
                            foreach (Enum flag in availableFlags.Where(Kernel_bin.MagicData[spell[i]].Stat_J_atk.HasFlag))
                            {
                                total[(Kernel_bin.J_Statuses)flag] += (byte)((Kernel_bin.MagicData[spell[i]].Stat_J_atk_val * Memory.State.Characters[Character].Magics[spell[i]]) / 100);
                                if (total[(Kernel_bin.J_Statuses)flag] > 100) total[(Kernel_bin.J_Statuses)flag] = 100;
                            }

                        Enum[] availableFlagsarray = availableFlags.ToArray();
                        for (short pos = 0; pos < Count; pos++)
                        {
                            short offset = (short)(Icons.ID.Status_Death + pos >= Icons.ID.Status_Curse ? 1 : 0);
                            ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Status_Death + pos+offset, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 10);
                            ITEM[pos, 1] = null;
                            //ITEM[pos, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 105, SIZE[pos].Y, 0, 0), 17);
                            ITEM[pos, 2] = null;
                            ITEM[pos, 3] = new IGMDataItem_Int(total[(Kernel_bin.J_Statuses)availableFlagsarray[pos + 1]], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 80, SIZE[pos].Y, 0, 0), 17, numtype: Icons.NumType.sysFntBig, spaces: 3);
                            ITEM[pos, 4] = new IGMDataItem_String("%", new Rectangle(SIZE[pos].X + SIZE[pos].Width - 20, SIZE[pos].Y, 0, 0));
                        }
                    }
                    return base.Update();
                }
            }
        }
    }
}