using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_GF_Junctioned : IGMData
        {
            #region Constructors

            public IGMData_GF_Junctioned() : base(16, 1, new IGMDataItem_Box(pos: new Rectangle(0, 141, 440, 282)), 2, 8)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                base.Refresh();
                if (Memory.State.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(Saves.GFflags)).Cast<Enum>();
                    int pos = 0;
                    foreach (Enum flag in availableFlags.Where(c.JunctionnedGFs.HasFlag))
                    {
                        if ((Saves.GFflags)flag == Saves.GFflags.None) continue;
                        ITEM[pos, 0] = new IGMDataItem_String(
                        Memory.State.GFs[Saves.ConvertGFEnum[(Saves.GFflags)flag]].Name, SIZE[pos]);
                        pos++;
                    }
                    for (; pos < Count; pos++)
                        ITEM[pos, 0] = null;
                }
            }

            protected override void Init()
            {
                Table_Options |= Table_Options.FillRows;
                base.Init();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-45, -8);
                SIZE[i].Offset((-10 * col), 0);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}