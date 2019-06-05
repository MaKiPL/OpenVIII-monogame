using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Abilities_CommandPool : IGMData_Pool<Dictionary<Kernel_bin.Abilities, Kernel_bin.Command_abilities>, Kernel_bin.Abilities>
            {
                public IGMData_Abilities_CommandPool() : base(11, 1, new IGMDataItem_Box(pos: new Rectangle(435, 150, 405, 480), title: Icons.ID.COMMAND), 11, Kernel_bin.Commandabilities.Count / 11 + (Kernel_bin.Commandabilities.Count % 11 > 0 ? 1 : 0)) => Source = Kernel_bin.Commandabilities;

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(60, 12 + (-4 * row));
                }

                public override void Inputs_OKAY()
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(31);
                    base.Inputs_OKAY();
                    if (Contents[CURSOR_SELECT] != Kernel_bin.Abilities.None && !BLANKS[CURSOR_SELECT])
                    {
                        int target = InGameMenu_Junction.Data[SectionName.TopMenu_Abilities].CURSOR_SELECT - 1;
                        Memory.State.Characters[Character].Commands[target] = Contents[CURSOR_SELECT];
                        InGameMenu_Junction.mode = Mode.Abilities;
                        InGameMenu_Junction.Data[SectionName.TopMenu_Abilities].ReInit();
                        InGameMenu_Junction.Data[SectionName.Commands].ReInit();
                    }
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.mode = Mode.Abilities;
                }

                public override void UpdateTitle()
                {
                    base.UpdateTitle();
                    if (Pages == 1)
                    {
                        ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.COMMAND;
                        ITEM[11, 0] = ITEM[12, 0] = null;
                    }
                    else
                        switch (Page)
                        {
                            case 0:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.COMMAND_PG1;
                                break;

                            case 1:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.COMMAND_PG2;
                                break;
                        }
                }

                public override bool Update()
                {
                    if (InGameMenu_Junction != null && InGameMenu_Junction.mode != Mode.Abilities_Commands)
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    else
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                    }
                    int pos = 0;
                    int skip = Page * rows;
                    for (int i = 0;
                        Memory.State.Characters != null &&
                        i < Memory.State.Characters[Character].UnlockedGFAbilities.Count &&
                        pos < rows; i++)
                    {
                        if (Memory.State.Characters[Character].UnlockedGFAbilities[i] != Kernel_bin.Abilities.None)
                        {
                            Kernel_bin.Abilities j = (Memory.State.Characters[Character].UnlockedGFAbilities[i]);
                            if (Source.ContainsKey(j) && skip-- <= 0)
                            {
                                Font.ColorID cid = Memory.State.Characters[Character].Commands.Contains(j) ? Font.ColorID.Grey : Font.ColorID.White;
                                BLANKS[pos] = cid == Font.ColorID.Grey ? true : false;
                                ITEM[pos, 0] = new IGMDataItem_String(
                                    Icons.ID.Ability_Command, 9,
                                Source[j].Name,
                                new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), cid);
                                Contents[pos] = j;
                                pos++;
                            }
                        }
                    }
                    for (; pos < rows; pos++)
                    {
                        ITEM[pos, 0] = null;
                        BLANKS[pos] = true;
                        Contents[pos] = Kernel_bin.Abilities.None;
                    }

                    if (Contents[CURSOR_SELECT] != Kernel_bin.Abilities.None && InGameMenu_Junction.mode == Mode.Abilities_Commands)
                        ((IGMDataItem_Box)InGameMenu_Junction.Data[SectionName.Help].CONTAINER).Data = Source[Contents[CURSOR_SELECT]].Description.ReplaceRegion();
                    UpdateTitle();
                    if (Contents[CURSOR_SELECT] == Kernel_bin.Abilities.None)
                        CURSOR_NEXT();
                    if (Pages > 1)
                    {
                        if (Contents[0] == Kernel_bin.Abilities.None)
                        {
                            Pages = Page;
                            PAGE_NEXT();
                            return Update();
                        }
                        else if (Contents[rows - 1] == Kernel_bin.Abilities.None)
                            Pages = Page + 1;
                    }
                    return base.Update();
                }
            }
        }
    }
}