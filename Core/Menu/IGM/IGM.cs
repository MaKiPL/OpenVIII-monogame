using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#pragma warning disable 67

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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            CurrentExp,
            NextLevel,
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

            var actions = new Action[]
            {
                () => Data.TryAdd(SectionName.Header, Header.Create()),
                () => Data.TryAdd(SectionName.Footer, Footer.Create()),
                () => Data.TryAdd(SectionName.Clock, Clock.Create()),
                () => Data.TryAdd(SectionName.PartyGroup, PartyGroup.Create(Party.Create(), NonParty.Create())),
                () => {
                    var keys = new[] {
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

                    var values = new[] {
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
                        Data.TryAdd(SectionName.SideMenu, SideMenu.Create((from i in Enumerable.Range(0,keys.Length)
                            select i).ToDictionary(x=>keys[x],x=>values[x])));
                    else Data.TryAdd(SectionName.SideMenu,null);
                }
            };
            Memory.ProcessActions(actions);
            Func<bool> sideMenuInputs = null;
            if (Data[SectionName.SideMenu] != null) sideMenuInputs = Data[SectionName.SideMenu].Inputs;
            InputDict = new Dictionary<Mode, Func<bool>>
                    {
                        { Mode.ChooseItem, sideMenuInputs },
                        { Mode.ChooseChar, Data[SectionName.PartyGroup].Inputs },
                    };
            SetMode((Mode)0);
        }

        #endregion Methods

        /*
                protected virtual void OnChoiceChangeHandler(KeyValuePair<Items, FF8String> e)
                {
                    ChoiceChangeHandler?.Invoke(this, e);
                }
        */
    }
}