using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenVIII
{
    public partial class IGM_Items : Menu
    {
        #region Fields

        protected Dictionary<Mode, Func<bool>> InputsDict;
        public EventHandler<KeyValuePair<byte, FF8String>> ChoiceChangeHandler;
        public EventHandler<KeyValuePair<Item_In_Menu, FF8String>> ItemChangeHandler;
        public EventHandler ReInitCompletedHandler;
        public EventHandler<Faces.ID> TargetChangeHandler;

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

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            base.Init();
            List<Task> tasks = new List<Task>
            {
                Task.Run(() => Data.TryAdd(SectionName.Help, new IGMDataItem.HelpBox { Pos = new Rectangle(15, 69, 810, 78), Title = Icons.ID.HELP, Options = Box_Options.Middle})),
                Task.Run(() => Data.TryAdd(SectionName.TopMenu, IGMData_TopMenu.Create(new Dictionary<FF8String, FF8String>() {
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 179),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 180)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 183),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 184)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 202),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 203)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 181),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 182)}}))),
                Task.Run(() => Data.TryAdd(SectionName.Title, new IGMDataItem.Box { Data = Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), Pos = new Rectangle(615, 0, 225, 66)})),
                Task.Run(() => Data.TryAdd(SectionName.UseItemGroup, IGMData.Group.Base.Create(IGMData_Statuses.Create(),IGMData.Pool.Item.Create(),IGMData_TargetPool.Create())))
            };
            Task.WaitAll(tasks.ToArray());
            IGMDataItem.HelpBox help = (IGMDataItem.HelpBox)Data[SectionName.Help];
            help.AddTextChangeEvent(ref ChoiceChangeHandler);
            help.AddTextChangeEvent(ref ItemChangeHandler);
            help.AddModeChangeEvent(ref ModeChangeHandler);
            InputsDict = new Dictionary<Mode, Func<bool>>() {
                {Mode.TopMenu, Data[SectionName.TopMenu].Inputs},
                {Mode.SelectItem, ((IGMData.Base)((IGMData.Group.Base)Data[SectionName.UseItemGroup]).ITEM[1,0]).Inputs},
                {Mode.UseItemOnTarget, ((IGMData.Base)((IGMData.Group.Base)Data[SectionName.UseItemGroup]).ITEM[2,0]).Inputs}
                };
            SetMode(Mode.SelectItem);
        }

        public override bool Inputs() => InputsDict[(Mode)GetMode()]();

        public override void Refresh() => Refresh(false);

        public new void Refresh(bool skipmode)
        {
            if (!skipmode)
                SetMode(Mode.SelectItem);
            base.Refresh();
            ReInitCompletedHandler?.Invoke(this, null);
        }

        #endregion Methods
    }
}