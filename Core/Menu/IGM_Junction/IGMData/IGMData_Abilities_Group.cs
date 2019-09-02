using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Abilities_Group : IGMData_Group
        {
            #region Constructors

            public IGMData_Abilities_Group(params IGMData[] d) : base(d)
            {
            }

            #endregion Constructors

            #region Methods

            public override bool Inputs()
            {
                skipdata = true;
                bool ret = base.Inputs();
                skipdata = false;
                if (Commands != null)
                {
                    if (CURSOR_SELECT >= Commands.Count)
                    {
                        AbilityPool?.Show();
                        CommandsPool?.Hide();
                    }
                    else
                    {
                        AbilityPool?.Hide();
                        CommandsPool?.Show();
                    }
                }
                return ret;
            }

            public override bool Inputs_CANCEL()
            {
                skipdata = true;
                base.Inputs_CANCEL();
                skipdata = false;
                IGM_Junction.Data[SectionName.TopMenu_Abilities].Hide();
                IGM_Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                if (Commands != null)
                {
                    if (CURSOR_SELECT >= Commands.Count)
                        IGM_Junction.SetMode(Mode.Abilities_Abilities);
                    else
                        IGM_Junction.SetMode(Mode.Abilities_Commands);
                    return true;
                }
                return false;
            }

            public override void Inputs_Menu()
            {
                skipdata = true;
                base.Inputs_Menu();
                skipdata = false;

                if (Commands!=null)
                {
                    if (CURSOR_SELECT < Commands.Count)
                    {
                        Memory.State.Characters[Character].Commands[CURSOR_SELECT - 1] = Kernel_bin.Abilities.None;
                        IGM_Junction.Data[SectionName.TopMenu_Abilities].Refresh();
                        IGM_Junction.Data[SectionName.Commands].Refresh();
                    }
                    else
                    {
                        Memory.State.Characters[Character].Abilities[CURSOR_SELECT - Commands.Count] = Kernel_bin.Abilities.None;
                        IGM_Junction.Refresh();
                    }
                }
            }
            private IGMData_Abilities_CommandSlots Commands => ((IGMData_Abilities_CommandSlots)ITEM[0, 0]);

            private IGMData_Abilities_AbilitySlots Ability => ((IGMData_Abilities_AbilitySlots)ITEM[1, 0]);

            private IGMData_Abilities_CommandPool CommandsPool => ((IGMData_Abilities_CommandPool)ITEM[2, 0]);

            private IGMData_Abilities_AbilityPool AbilityPool => ((IGMData_Abilities_AbilityPool)ITEM[3, 0]);

            public override void Refresh()
            {
                base.Refresh();

                int total_Count = (Commands?.Count ?? 0) + (Ability?.Count ?? 0);
                if (Memory.State.Characters != null)
                {
                    SIZE = new Rectangle[total_Count];
                    CURSOR = new Point[total_Count];
                    BLANKS = new bool[total_Count];
                    int i = 0;
                    test(Commands, ref i);
                    test(Ability, ref i);
                }

                void test(IGMData t, ref int i)
                {
                    int pos = 0;
                    for (; pos < t.Count && i < total_Count; i++)
                    {
                        SIZE[i] = t.SIZE[pos];
                        CURSOR[i] = t.CURSOR[pos];
                        BLANKS[i] = t.BLANKS[pos];
                        pos++;
                    }
                }
                if (CURSOR_SELECT == 0)
                    CURSOR_SELECT = 1;
            }
            

            public override bool Update()
            {
                bool ret = base.Update();

                if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.Abilities))
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;

                    if (Commands!= null && Ability != null)
                    {
                        if (CURSOR_SELECT >= Commands.Count)
                        {
                            if (Ability.Descriptions != null && Ability.Descriptions.TryGetValue(CURSOR_SELECT - Commands.Count, out FF8String v))
                            {
                                IGM_Junction.ChangeHelp(v);
                            }
                        }
                        else
                        {
                            if (Commands.Descriptions != null && Commands.Descriptions.TryGetValue(CURSOR_SELECT, out FF8String v))
                            {
                                IGM_Junction.ChangeHelp(v);
                            }
                        }
                    }
                }
                else
                    Cursor_Status |= Cursor_Status.Blinking;

                return ret;
            }

            protected override void Init()
            {
                base.Init();
                Cursor_Status |= Cursor_Status.Enabled;
                Hide();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}