using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Mag_ST_D_Values : IGMData_Values
        {
            #region Constructors

            // public new Saves.CharacterData PrevSetting { get; private set; } public new
            // Saves.CharacterData Setting { get; private set; }
            public IGMData_Mag_ST_D_Values() : base(14, 5, new IGMDataItem_Box(title: Icons.ID.Status_Defense, pos: new Rectangle(280, 342, 545, 288)), 2, 7)
            {
            }

            #endregion Constructors

            #region Methods

            public Dictionary<Kernel_bin.J_Statuses, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel_bin.J_Statuses>(out availableFlagsarray, 100, Kernel_bin.Stat.ST_Def_1,
                            source.Stat_J[Kernel_bin.Stat.ST_Def_1],
                            source.Stat_J[Kernel_bin.Stat.ST_Def_2],
                            source.Stat_J[Kernel_bin.Stat.ST_Def_3],
                            source.Stat_J[Kernel_bin.Stat.ST_Def_4]);

            public override bool Update()
            {
                if (Memory.State.Characters != null)
                {
                    Dictionary<Kernel_bin.J_Statuses, byte> oldtotal = (prevSetting != null) ? getTotal(prevSetting, out Enum[] availableFlagsarray) : null;
                    Dictionary<Kernel_bin.J_Statuses, byte> total = getTotal(Memory.State.Characters[Character], out availableFlagsarray);
                    FillData(oldtotal, total, availableFlagsarray, Icons.ID.Status_Death, 1, palette: 10);
                }
                return base.Update();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-25, -10);
                SIZE[i].Y -= 3 * row;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}