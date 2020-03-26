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

            public Dictionary<Kernel.JunctionStatuses, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel.JunctionStatuses>(out availableFlagsarray, 100, Kernel.Stat.StAtk, source.StatJ[Kernel.Stat.StAtk]);

            public override bool Update()
            {
                if (Memory.State?.Characters != null && Damageable != null && Damageable.GetCharacterData(out var c))
                {
                    var oldtotal = (prevSetting != null) ? getTotal(prevSetting, out var availableFlagsarray) : null;
                    var total = getTotal(c, out availableFlagsarray);
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
