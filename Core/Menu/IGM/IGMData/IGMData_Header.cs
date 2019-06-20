using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM
        {
            protected class IGMData_Header : IGMData
            {
                private bool eventSet= false;

                //private Dictionary<Enum, Item> strHeaderText;

                public IGMData_Header() : base(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75 }, title: Icons.ID.HELP))
                { }

                //protected override void Init()
                //{
                //    strHeaderText = new Dictionary<Enum, Item>()
                //    {
                //    { Items.Junction, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,1) } },
                //    { Items.Item, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,3) } },
                //    { Items.Magic, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,5) } },
                //    { Items.Status, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,9) } },
                //    { Items.GF, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,7) } },
                //    { Items.Ability, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,63) } },
                //    { Items.Switch, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,65) } },
                //    { Items.Card, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,11) } },
                //    { Items.Config, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,17) } },
                //    { Items.Tutorial, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,68) } },
                //    { Items.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,15) } },
                //    };
                //    base.Init();
                //}


                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu.ChoiceChangeHandler += ChoiceChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e)
                { 
                    ((IGMDataItem_Box)CONTAINER).Data = e.Value;
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                }

                public bool Update(FF8String selection)
                {
                    ((IGMDataItem_Box)CONTAINER).Data = selection;
                    return true;
                }

                //private new bool Update() => base.Update();
            }

        }
    }
}