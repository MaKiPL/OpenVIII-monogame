using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
        #region Classes

        public partial class IGM_Junction
        {
            #region Classes

            private class IGMData_Mag_ST_A_Values : IGMData_Values
            {

                #region Constructors

                public IGMData_Mag_ST_A_Values() : base( 12, 5, new IGMDataItem_Box(title: Icons.ID.Status_Attack, pos: new Rectangle(280, 363, 545, 267)), 2, 6)
                {
                }

                #endregion Constructors

                #region Methods

                public Dictionary<Kernel_bin.J_Statuses, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel_bin.J_Statuses>(out availableFlagsarray, 100, Kernel_bin.Stat.ST_Atk, source.Stat_J[Kernel_bin.Stat.ST_Atk]);

                public override bool Update()
                {
                    if (Memory.State.Characters != null)
                    {
                        Dictionary<Kernel_bin.J_Statuses, byte> oldtotal = (prevSetting != null) ? getTotal(prevSetting, out Enum[] availableFlagsarray) : null;
                        Dictionary<Kernel_bin.J_Statuses, byte> total = getTotal(Memory.State.Characters[Character], out availableFlagsarray);
                        FillData(oldtotal, total, availableFlagsarray, Icons.ID.Status_Death, palette: 10, skip: new Icons.ID[] { Icons.ID.Status_Curse });
                    }
                    return base.Update();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-25, -25);
                    SIZE[i].Y -= 6 * row;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    
}