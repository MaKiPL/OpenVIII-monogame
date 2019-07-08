using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    #region Classes

    public partial class IGM : Menu
    {

        #region Fields

        public EventHandler<KeyValuePair<Items, FF8String>> ChoiceChangeHandler;
        //public EventHandler<Mode> ModeChangeHandler;
        protected Dictionary<Mode, Func<bool>> InputDict;
        //private Mode _mode = 0;

        #endregion Fields

        #region Enums

        public enum Items
        {
            Junction,
            Item,
            Magic,
            Status,
            GF,
            Ability,
            Switch,
            Card,
            Config,
            Tutorial,
            Save,
            Battle,
            CurrentEXP,
            NextLEVEL,
        }

        public enum Mode
        {
            ChooseItem,
            ChooseChar,
        }

        public enum SectionName
        {
            Header,
            Footer,
            SideMenu,
            Clock,
            PartyGroup,
        }

        #endregion Enums

        #region Methods

        protected override void Init()
        {
            Size = new Vector2 { X = 843, Y = 630 };
            //TextScale = new Vector2(2.545455f, 3.0375f);
            Data.Add(SectionName.Header, new IGMData_Header());
            Data.Add(SectionName.Footer, new IGMData_Footer());
            Data.Add(SectionName.Clock, new IGMData_Clock());
            Data.Add(SectionName.PartyGroup, new IGMData_PartyGroup(new IGMData_Party(), new IGMData_NonParty()));
            Data.Add(SectionName.SideMenu, new IGMData_SideMenu(new Dictionary<FF8String, FF8String>() {
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 0), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 1)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 3)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 4), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 5)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 8), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 9)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 6), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 7)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 62), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 63)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 64), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 65)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 10), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 11)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 16), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 17)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 67), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 68)},
                    { Memory.Strings.Read(Strings.FileID.MNGRP, 0, 14), Memory.Strings.Read(Strings.FileID.MNGRP, 0, 15)},
                    { "Battle", "Test Battle Menu"}
                }));
            InputDict = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.ChooseItem, Data[SectionName.SideMenu].Inputs },
                    { Mode.ChooseChar, Data[SectionName.PartyGroup].Inputs },
                };
            SetMode((Mode)0);
            base.Init();
        }

        public override bool Inputs() => InputDict[(Mode)GetMode()]();


        #endregion Methods
    }

    #endregion Classes

}