using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Items : Menu
        {
            #region Fields

            public EventHandler<KeyValuePair<byte, FF8String>> ChoiceChangeHandler;

            public EventHandler<KeyValuePair<Item_In_Menu, FF8String>> ItemChangeHandler;

            //public EventHandler<Mode> ModeChangeHandler;

            public EventHandler<Faces.ID> TargetChangeHandler;
            public EventHandler ReInitCompletedHandler;

            protected Dictionary<Mode, Func<bool>> InputsDict;
            
            #endregion Fields

            #region Enums

            public enum Mode : byte
            {
                /// <summary>
                /// Select one of the 4 top options to do
                /// </summary>
                TopMenu,

                /// <summary>
                /// Choose an item to use
                /// </summary>
                SelectItem,


                /// <summary>
                /// Choose a character or gf to use item on
                /// </summary>
                UseItemOnTarget,
            }

            public enum SectionName : byte
            {
                TopMenu,
                UseItemGroup,
                Help,
                Title,
            }

            #endregion Enums

            #region Methods


            public override void ReInit()
            {
                ReInit(false);
            }
            public void ReInit(bool skipmode)
            {
                if(!skipmode)
                    SetMode(Mode.SelectItem);
                base.ReInit();
                ReInitCompletedHandler?.Invoke(this, null);
            }



            protected override void Init()
            {
                Size = new Vector2 { X = 840, Y = 630 };
                //TextScale = new Vector2(2.545455f, 3.0375f);

                Data.Add(SectionName.Help, new IGMData_Help(
                    new IGMDataItem_Box(null, pos: new Rectangle(15, 69, 810, 78), Icons.ID.HELP, options: Box_Options.Middle)));
                Data.Add(SectionName.TopMenu, new IGMData_TopMenu(new Dictionary<FF8String, FF8String>() {
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 179),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 180)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 183),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 184)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 202),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 203)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 181),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 182)},
                            }));
                Data.Add(SectionName.Title, new IGMData_Container(
                    new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), pos: new Rectangle(615, 0, 225, 66))));
                Data.Add(SectionName.UseItemGroup, new IGMData_Group(
                    new IGMData_Statuses(),
                    new IGMData_ItemPool(),
                    new IGMData_TargetPool()
                    ));
                InputsDict = new Dictionary<Mode, Func<bool>>() {
                {Mode.TopMenu, Data[SectionName.TopMenu].Inputs},
                {Mode.SelectItem, ((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[1,0]).Inputs},
                {Mode.UseItemOnTarget, ((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[2,0]).Inputs}
                };
                SetMode(Mode.SelectItem);
                base.Init();
            }
            protected override bool Inputs() => InputsDict[(Mode)GetMode()]();

            #endregion Methods
        }

        #endregion Classes
    }
}