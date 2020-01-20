using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_GF_Junctioned : IGMData.Base
        {
            #region Methods

            public static IGMData_GF_Junctioned Create() => Create<IGMData_GF_Junctioned>(16, 1, new IGMDataItem.Box { Pos = new Rectangle(0, 141, 440, 282) }, 2, 8);

            public override void Refresh()
            {
                base.Refresh();
                if (Memory.State?.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    int pos = 0;
                    foreach (var gf in c.JunctionedGFs)
                    {
                        ((IGMDataItem.Text)ITEM[pos, 0]).Data = Memory.State.GFs[gf].Name;
                        ITEM[pos, 0].Show();
                        BLANKS[pos++] = false;
                    }
                    for (; pos < Count; pos++)
                    {
                        ITEM[pos, 0].Hide();
                        BLANKS[pos] = true;
                    }
                }
            }

            protected override void Init()
            {
                Table_Options |= Table_Options.FillRows;
                base.Init();

                for (int pos = 0; pos < Count; pos++)
                {
                    ITEM[pos, 0] = new IGMDataItem.Text
                    {
                        Pos = SIZE[pos]
                    };
                    ITEM[pos, 0].Hide();
                    BLANKS[pos] = true;
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-45, -8);
                SIZE[i].Offset((-10 * col), -2 * row);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}