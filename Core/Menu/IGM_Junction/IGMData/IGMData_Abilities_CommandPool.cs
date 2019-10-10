using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Abilities_CommandPool : IGMData_Pool<IReadOnlyDictionary<Kernel_bin.Abilities, Kernel_bin.Command_abilities>, Kernel_bin.Abilities>
        {
            #region Constructors

            public IGMData_Abilities_CommandPool() : base(11, 1, new IGMDataItem.Box(pos: new Rectangle(435, 150, 405, 480), title: Icons.ID.COMMAND), 11, Kernel_bin.Commandabilities.Count / 11 + (Kernel_bin.Commandabilities.Count % 11 > 0 ? 1 : 0)) => Source = Kernel_bin.Commandabilities;

            #endregion Constructors

            #region Methods

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.SetMode(Mode.Abilities);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                if (Contents[CURSOR_SELECT] != Kernel_bin.Abilities.None && !BLANKS[CURSOR_SELECT] && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(31);
                    base.Inputs_OKAY();
                    int target = IGM_Junction.Data[SectionName.TopMenu_Abilities].CURSOR_SELECT - 1;
                    c.Commands[target] = Contents[CURSOR_SELECT];
                    IGM_Junction.SetMode(Mode.Abilities);
                    IGM_Junction.Data[SectionName.TopMenu_Abilities].Refresh();
                    IGM_Junction.Data[SectionName.Commands].Refresh();
                    return true;
                }
                return false;
            }

            public override bool Update()
            {
                if (IGM_Junction != null && !IGM_Junction.GetMode().Equals(Mode.Abilities_Commands))
                    Cursor_Status &= ~Cursor_Status.Enabled;
                else
                {
                    Cursor_Status |= Cursor_Status.Enabled;
                }
                int pos = 0;
                int skip = Page * Rows;
                if(Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                for (int i = 0;
                    Memory.State.Characters != null &&
                    i < c.UnlockedGFAbilities.Count &&
                    pos < Rows; i++)
                {
                    if (c.UnlockedGFAbilities[i] != Kernel_bin.Abilities.None)
                    {
                        Kernel_bin.Abilities j = (c.UnlockedGFAbilities[i]);
                        if (Source.ContainsKey(j) && skip-- <= 0)
                        {
                            Font.ColorID cid = c.Commands.Contains(j) ? Font.ColorID.Grey : Font.ColorID.White;
                            BLANKS[pos] = cid == Font.ColorID.Grey ? true : false;
                            ITEM[pos, 0] = new IGMDataItem.Text(
                                Icons.ID.Ability_Command, 9,
                            Source[j].Name,
                            new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), cid);
                            Contents[pos] = j;
                            pos++;
                        }
                    }
                }
                for (; pos < Rows; pos++)
                {
                    ITEM[pos, 0] = null;
                    BLANKS[pos] = true;
                    Contents[pos] = Kernel_bin.Abilities.None;
                }

                if (Contents[CURSOR_SELECT] != Kernel_bin.Abilities.None && IGM_Junction.GetMode().Equals(Mode.Abilities_Commands))
                    IGM_Junction.ChangeHelp(Source[Contents[CURSOR_SELECT]].Description);
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
                    else if (Contents[Rows - 1] == Kernel_bin.Abilities.None)
                        Pages = Page + 1;
                }
                return base.Update();
            }

            public override void UpdateTitle()
            {
                base.UpdateTitle();
                if (Pages == 1)
                {
                    ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.COMMAND;
                    ITEM[11, 0] = ITEM[12, 0] = null;
                }
                else
                    switch (Page)
                    {
                        case 0:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.COMMAND_PG1;
                            break;

                        case 1:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.COMMAND_PG2;
                            break;
                    }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-22, -8);
                SIZE[i].Offset(60, 12 + (-4 * row));
            }

            #endregion Methods
        }

        #endregion Classes
    }
}