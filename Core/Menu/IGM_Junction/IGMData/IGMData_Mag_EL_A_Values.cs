using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Mag_EL_A_Values : IGMData_Values
        {
            #region Constructors

            static public IGMData_Mag_EL_A_Values Create() => Create<IGMData_Mag_EL_A_Values> (8, 5, new IGMDataItem.Box(title: Icons.ID.Elemental_Attack, pos: new Rectangle(280, 423, 545, 201)), 2, 4);

            #endregion Constructors

            #region Methods

            public Dictionary<Kernel_bin.Element, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel_bin.Element>(out availableFlagsarray, 200, Kernel_bin.Stat.EL_Atk, source.Stat_J[Kernel_bin.Stat.EL_Atk]);

            public override bool Update()
            {
                if (Memory.State.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    Dictionary<Kernel_bin.Element, byte> oldtotal = (prevSetting != null) ? getTotal(prevSetting, out Enum[] availableFlagsarray) : null;
                    Dictionary<Kernel_bin.Element, byte> total = getTotal(c, out availableFlagsarray);
                    FillData(oldtotal, total, availableFlagsarray, Icons.ID.Element_Fire, palette: 9);
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