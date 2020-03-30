using Microsoft.Xna.Framework;
using System.Collections;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_Abilities_Group : IGMData.Group.Base
        {
            #region Properties

            private IGMData.Slots.Abilities Ability => ((IGMData.Slots.Abilities)ITEM[1, 0]);

            private IGMData_Abilities_AbilityPool AbilityPool => ((IGMData_Abilities_AbilityPool)ITEM[3, 0]);

            private IGMData.Slots.Command Commands => ((IGMData.Slots.Command)ITEM[0, 0]);

            private IGMData_Abilities_CommandPool CommandsPool => ((IGMData_Abilities_CommandPool)ITEM[2, 0]);

            #endregion Properties

            #region Methods

            public static new IGMData_Abilities_Group Create(params Menu_Base[] d) => Create<IGMData_Abilities_Group>(d);

            public override bool Inputs()
            {
                skipdata = true;
                var ret = base.Inputs();
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
                Junction.Data[SectionName.TopMenu_Abilities].Hide();
                Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override void Inputs_Menu()
            {
                skipdata = true;
                base.Inputs_Menu();
                skipdata = false;

                if (Commands != null && Damageable.GetCharacterData(out var c))
                {
                    if (CURSOR_SELECT < Commands.Count)
                    {
                        c.Commands[CURSOR_SELECT - 1] = Kernel.Abilities.None;
                        Junction.Data[SectionName.TopMenu_Abilities].Refresh();
                        Junction.Data[SectionName.Commands].Refresh();
                    }
                    else
                    {
                        c.Abilities[CURSOR_SELECT - Commands.Count] = Kernel.Abilities.None;
                        Junction.Refresh();
                    }
                }
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                if (Commands != null)
                {
                    if (CURSOR_SELECT >= Commands.Count)
                        Junction.SetMode(Mode.Abilities_Abilities);
                    else
                        Junction.SetMode(Mode.Abilities_Commands);
                    return true;
                }
                return false;
            }

            public override void Refresh()
            {
                base.Refresh();

                var total_Count = (Commands?.Count ?? 0) + (Ability?.Count ?? 0);
                if (Memory.State?.Characters != null)
                {//TODO fix this. these values should be set in init() not refresh...
                    SIZE = new Rectangle[total_Count];
                    CURSOR = new Point[total_Count];
                    BLANKS = new BitArray(total_Count,false);
                    var i = 0;
                    test(Commands, ref i);
                    test(Ability, ref i);
                }

                void test(IGMData.Base t, ref int i)
                {
                    var pos = 0;
                    for (; t != null && pos < t.Count && i < total_Count; i++)
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
                var ret = base.Update();

                if (Junction != null && Junction.GetMode().Equals(Mode.Abilities))
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;

                    if (Commands != null && Ability != null)
                    {
                        if (CURSOR_SELECT >= Commands.Count)
                        {
                            if (Ability.Descriptions != null && Ability.Descriptions.TryGetValue(CURSOR_SELECT - Commands.Count, out var v))
                            {
                                Junction.ChangeHelp(v);
                            }
                        }
                        else
                        {
                            if (Commands.Descriptions != null && Commands.Descriptions.TryGetValue(CURSOR_SELECT, out var v))
                            {
                                Junction.ChangeHelp(v);
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