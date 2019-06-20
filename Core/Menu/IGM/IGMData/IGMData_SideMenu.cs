using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM
        {
            private class IGMData_SideMenu : IGMData
            {
                public IGMData_SideMenu() : base(11, 1, new IGMDataItem_Box(pos: new Rectangle { Width = 226, Height = 492, X = 843 - 226 }),1,11)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-26, -12);
                    ITEM[i, 0].Pos = SIZE[i];
                }

                protected override void Init()
                {
                    ITEM[0, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 0));
                    ITEM[1, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2));
                    ITEM[2, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 4));
                    ITEM[3, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 8));
                    ITEM[4, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 6));
                    ITEM[5, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 62));
                    ITEM[6, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 64));
                    ITEM[7, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 10));
                    ITEM[8, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 16));
                    ITEM[9, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 67));
                    ITEM[10, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 14));
                    base.Init();
                }

                public override bool Inputs() => base.Inputs();

                public override bool Update() => base.Update();
            }

        }
    }
}