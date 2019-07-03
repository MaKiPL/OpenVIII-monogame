using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    class IGM_Lobby : Menu
    {
        public enum SectionName
        {
            BG,
            Selections,            
        }
        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            Data.Add(SectionName.BG,new IGMData_Container(new IGMDataItem_TextureHandler(new TextureHandler("start{0:00}", 2),new Rectangle(-280,0,1120,630))));
            //Data.Add(SectionName.Selections, new IGMData_Selections());
            base.Init();
        }


        protected override bool Inputs()
        { return false; }

        private class IGMData_Selections : IGMData
        {
            public IGMData_Selections() : base(count:3,depth: 1, container: new IGMDataItem_Empty(new Rectangle(300,408,250,250)), cols:1, rows:3)
            {
            }
            protected override void Init()
            {
                ITEM[0, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 1, 105), SIZE[0]);
                ITEM[1, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 1, 106), SIZE[1]);
                ITEM[2, 0] = new IGMDataItem_String("OpenVIII debug tools", SIZE[2]);
                base.Init();
            }
        }
    }
}
