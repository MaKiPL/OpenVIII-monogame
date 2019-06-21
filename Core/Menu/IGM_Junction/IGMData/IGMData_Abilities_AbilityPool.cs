using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Abilities_AbilityPool : IGMData_Pool<IReadOnlyDictionary<Kernel_bin.Abilities, Kernel_bin.Equipable_Ability>, Kernel_bin.Abilities>
            {
                public IGMData_Abilities_AbilityPool() : base( 11, 1, new IGMDataItem_Box(pos: new Rectangle(435, 150, 405, 480), title: Icons.ID.ABILITY), 11, Kernel_bin.EquipableAbilities.Count / 11 + (Kernel_bin.EquipableAbilities.Count % 11 > 0 ? 1 : 0)) => Source = Kernel_bin.EquipableAbilities;

                protected override void Init()
                {
                    base.Init();
                    Hide();
                }

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
                        int target = InGameMenu_Junction.Data[SectionName.TopMenu_Abilities].CURSOR_SELECT - 4;
                        Memory.State.Characters[Character].Abilities[target] = Contents[CURSOR_SELECT];
                        InGameMenu_Junction.SetMode(Mode.Abilities);
                        InGameMenu_Junction.ReInit(); // can be more specific if you want to find what is being changed.
                    }
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.SetMode(Mode.Abilities);
                }

                public override void UpdateTitle()
                {
                    base.UpdateTitle();
                    if (Pages == 1)
                    {
                        ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.ABILITY;
                        ITEM[11, 0] = ITEM[12, 0] = null;
                    }
                    else
                        switch (Page)
                        {
                            case 0:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.ABILITY_PG1;
                                break;

                            case 1:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.ABILITY_PG2;
                                break;

                            case 2:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.ABILITY_PG3;
                                break;

                            case 3:
                                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.ABILITY_PG4;
                                break;
                        }
                }

                public override bool Update()
                {
                    if (InGameMenu_Junction != null &&InGameMenu_Junction.GetMode() != Mode.Abilities_Abilities)
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    else
                        Cursor_Status |= Cursor_Status.Enabled;
                    int pos = 0;
                    int skip = Page * rows;
                    for (int i = 0;
                        Memory.State.Characters != null &&
                        i < Memory.State.Characters[Character].UnlockedGFAbilities.Count &&
                        pos < rows; i++)
                    {
                        if (Memory.State.Characters[Character].UnlockedGFAbilities[i] != Kernel_bin.Abilities.None)
                        {
                            Kernel_bin.Abilities j = Memory.State.Characters[Character].UnlockedGFAbilities[i];
                            if (Source.ContainsKey(j))
                            {
                                if (skip > 0)
                                {
                                    skip--;
                                    continue;
                                }
                                Font.ColorID cid = Memory.State.Characters[Character].Abilities.Contains(j) ? Font.ColorID.Grey : Font.ColorID.White;
                                BLANKS[pos] = cid == Font.ColorID.Grey ? true : false;

                                ITEM[pos, 0] = new IGMDataItem_String(
                                    Source[j].Icon, 9,
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
                    if (Contents[CURSOR_SELECT] != Kernel_bin.Abilities.None &&InGameMenu_Junction.GetMode() == Mode.Abilities_Abilities)
                        InGameMenu_Junction.ChangeHelp(Source[Contents[CURSOR_SELECT]].Description.ReplaceRegion());
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