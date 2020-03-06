using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Mag_ST_A_Values : IGMData_Values
        {
            #region Methods

            public static IGMData_Mag_ST_A_Values Create() => Create<IGMData_Mag_ST_A_Values>(12, 5, new IGMDataItem.Box { Title = Icons.ID.Status_Attack, Pos = new Rectangle(280, 363, 545, 267) }, 2, 6);

            public Dictionary<Kernel.J_Statuses, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel.J_Statuses>(out availableFlagsarray, 100, Kernel.Stat.ST_Atk, source.StatJ[Kernel.Stat.ST_Atk]);

            public override bool Update()
            {
                if (Memory.State?.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    Dictionary<Kernel.J_Statuses, byte> oldtotal = (prevSetting != null) ? getTotal(prevSetting, out Enum[] availableFlagsarray) : null;
                    Dictionary<Kernel.J_Statuses, byte> total = getTotal(c, out availableFlagsarray);
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
}