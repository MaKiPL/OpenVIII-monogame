using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class IGM_Lobby : Menu
    {
        #region Fields

        private Dictionary<Enum, Menu_Base> Data0;

        #endregion Fields

        #region Enums

        public enum SectionName
        {
            BG,
            Selections,
        }

        #endregion Enums

        #region Methods

        public static IGM_Lobby Create() => Create<IGM_Lobby>();

        public override bool Inputs() => Data[SectionName.Selections].Inputs();

        public override void StartDraw()
        {
            var backupfocus = Focus;
            GenerateFocus(new Vector2(1280, 720), Box_Options.Top);
            base.StartDraw();
            Data0.Where(m => m.Value != null).ForEach(m => m.Value.Draw());
            base.EndDraw();
            Focus = backupfocus;
            base.StartDraw();
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            //Data0 is scaled from 1280x720
            Data0 = new Dictionary<Enum, Menu_Base> {
                { SectionName.BG, new IGMDataItem.Texture_Handler(TextureHandler.Create("start{0:00}", 2), new Rectangle(0,-25, 1280, 0)) } //new Rectangle(-45,-25, 1280+100, 0)
            };
            //Data is scaled from Size
            Data.TryAdd(SectionName.Selections, IGMData_Selections.Create());
            base.Init();
        }

        #endregion Methods
    }
}