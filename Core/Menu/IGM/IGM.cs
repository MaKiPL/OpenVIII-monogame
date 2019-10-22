using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenVIII
{
    public partial class IGM : Menu
    {
        #region Fields

        public EventHandler<KeyValuePair<Items, FF8String>> ChoiceChangeHandler;

        //public EventHandler<Mode> ModeChangeHandler;
        protected Dictionary<Mode, Func<bool>> InputDict;

        #endregion Fields

        //private Mode _mode = 0;

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
            Clock,
            PartyGroup,
            SideMenu,
        }

        #endregion Enums

        #region Methods

        public override bool Inputs() => InputDict[(Mode)GetMode()]();

        protected override void Init()
        {
            Size = new Vector2 { X = 843, Y = 630 };
            base.Init();
            //TextScale = new Vector2(2.545455f, 3.0375f);

            List<Task> tasks = new List<Task>
            {
                Task.Run(() => Data.TryAdd(SectionName.Header, IGMData_Header.Create())),
                Task.Run(() => Data.TryAdd(SectionName.Footer, IGMData_Footer.Create())),
                Task.Run(() => Data.TryAdd(SectionName.Clock, IGMData_Clock.Create())),
                Task.Run(() => Data.TryAdd(SectionName.PartyGroup, IGMData_PartyGroup.Create(IGMData_Party.Create(), IGMData_NonParty.Create()))),
                Task.Run(() => Data.TryAdd(SectionName.SideMenu, IGMData_SideMenu.Create(new Dictionary<FF8String, FF8String>() {
                    { Strings.Name.SideMenu.Junction, Strings.Description.SideMenu.Junction},
                    { Strings.Name.SideMenu.Item, Strings.Description.SideMenu.Item},
                    { Strings.Name.SideMenu.Magic, Strings.Description.SideMenu.Magic},
                    { Strings.Name.SideMenu.GF, Strings.Description.SideMenu.GF},
                    { Strings.Name.SideMenu.Status, Strings.Description.SideMenu.Status},
                    { Strings.Name.SideMenu.Ability, Strings.Description.SideMenu.Ability},
                    { Strings.Name.SideMenu.Switch, Strings.Description.SideMenu.Switch},
                    { Strings.Name.SideMenu.Card, Strings.Description.SideMenu.Card},
                    { Strings.Name.SideMenu.Config, Strings.Description.SideMenu.Config},
                    { Strings.Name.SideMenu.Tutorial, Strings.Description.SideMenu.Tutorial},
                    { Strings.Name.SideMenu.Save, Strings.Description.SideMenu.Save},
                    { Strings.Name.SideMenu.Battle, Strings.Description.SideMenu.Battle}})))
            };
            Task.WaitAll(tasks.ToArray());
            InputDict = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.ChooseItem, Data[SectionName.SideMenu].Inputs },
                    { Mode.ChooseChar, Data[SectionName.PartyGroup].Inputs },
                };
            SetMode((Mode)0);
        }

        #endregion Methods
    }
}