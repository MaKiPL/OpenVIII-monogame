using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVIII
{
    public partial class IGM : Menu
    {
        #region Fields

        protected Dictionary<Mode, Func<bool>> InputDict;

        #endregion Fields

        #region Events

        public event EventHandler<KeyValuePair<Items, FF8String>> ChoiceChangeHandler;

        #endregion Events

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

        //private Mode _mode = 0;
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

        public static IGM Create() => Create<IGM>();

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
                Task.Run(() => {
                    FF8String[] keys = new FF8String[] {
                        Strings.Name.SideMenu.Junction,
                        Strings.Name.SideMenu.Item,
                        Strings.Name.SideMenu.Magic,
                        Strings.Name.SideMenu.GF,
                        Strings.Name.SideMenu.Status,
                        Strings.Name.SideMenu.Ability,
                        Strings.Name.SideMenu.Switch,
                        Strings.Name.SideMenu.Card,
                        Strings.Name.SideMenu.Config,
                        Strings.Name.SideMenu.Tutorial,
                        Strings.Name.SideMenu.Save,
                        Strings.Name.SideMenu.Battle};

                    FF8String[] values = new FF8String[] {
                        Strings.Description.SideMenu.Junction,
                        Strings.Description.SideMenu.Item,
                        Strings.Description.SideMenu.Magic,
                        Strings.Description.SideMenu.GF,
                        Strings.Description.SideMenu.Status,
                        Strings.Description.SideMenu.Ability,
                        Strings.Description.SideMenu.Switch,
                        Strings.Description.SideMenu.Card,
                        Strings.Description.SideMenu.Config,
                        Strings.Description.SideMenu.Tutorial,
                        Strings.Description.SideMenu.Save,
                        Strings.Description.SideMenu.Battle};
                    if(keys.Distinct().Count() == keys.Length && values.Length == keys.Length)
                    Data.TryAdd(SectionName.SideMenu, IGMData_SideMenu.Create((from i in Enumerable.Range(0,keys.Length)
                                                                               select i).ToDictionary(x=>keys[x],x=>values[x])));
                    else Data.TryAdd(SectionName.SideMenu,null); })
                                                       

            };
            Task.WaitAll(tasks.ToArray());
            Func<bool> SideMenuInputs = null;
            if(Data[SectionName.SideMenu] != null) SideMenuInputs = Data[SectionName.SideMenu].Inputs;
            InputDict = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.ChooseItem, SideMenuInputs },
                    { Mode.ChooseChar, Data[SectionName.PartyGroup].Inputs },
                };
            SetMode((Mode)0);
        }

        #endregion Methods
    }
}