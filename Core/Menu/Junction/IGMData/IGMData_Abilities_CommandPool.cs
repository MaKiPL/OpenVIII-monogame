using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_Abilities_CommandPool : IGMData.Pool.Base<IReadOnlyDictionary<Kernel.Abilities, Kernel.CommandAbility>, Kernel.Abilities>
        {
            #region Methods

            public static IGMData_Abilities_CommandPool Create()
            {
                IGMData_Abilities_CommandPool r = null;
                if (Memory.KernelBin.CommandAbilities != null)
                {
                    r = Create<IGMData_Abilities_CommandPool>(11, 1, new IGMDataItem.Box { Pos = new Rectangle(435, 150, 405, 480), Title = Icons.ID.COMMAND }, 11, (Memory.KernelBin.CommandAbilities.Count / 11) + (Memory.KernelBin.CommandAbilities.Count % 11 > 0 ? 1 : 0));
                    r.Source = Memory.KernelBin.CommandAbilities;
                }
                return r;
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.SetMode(Mode.Abilities);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                if (Contents[CURSOR_SELECT] != Kernel.Abilities.None && !BLANKS[CURSOR_SELECT] && Damageable.GetCharacterData(out var c))
                {
                    skipsnd = true;
                    AV.Sound.Play(31);
                    base.Inputs_OKAY();
                    var target = ((IGMData.Base)Junction.Data[SectionName.TopMenu_Abilities]).CURSOR_SELECT - 1;
                    c.Commands[target] = Contents[CURSOR_SELECT];
                    Junction.SetMode(Mode.Abilities);
                    Junction.Data[SectionName.TopMenu_Abilities].Refresh();
                    Junction.Data[SectionName.Commands].Refresh();
                    return true;
                }
                return false;
            }

            public override bool Update()
            {
                if (Junction != null && !Junction.GetMode().Equals(Mode.Abilities_Commands))
                    Cursor_Status &= ~Cursor_Status.Enabled;
                else
                {
                    Cursor_Status |= Cursor_Status.Enabled;
                }
                var pos = 0;
                var skip = Page * Rows;
                if (Damageable != null && Damageable.GetCharacterData(out var c))
                    for (var i = 0;
                        Memory.State.Characters &&
                        i < c.UnlockedGFAbilities.Count &&
                        pos < Rows; i++)
                    {
                        if (c.UnlockedGFAbilities[i] == Kernel.Abilities.None) continue;
                        var j = (c.UnlockedGFAbilities[i]);
                        if (!Source.ContainsKey(j) || skip-- > 0) continue;
                        Font.ColorID cid;
                        if (c.Commands.Contains(j))
                        {
                            cid = Font.ColorID.Grey;
                            BLANKS[pos] = true;
                        }
                        else
                        {
                            cid = Font.ColorID.White;
                            BLANKS[pos] = false;
                        }
                        ((IGMDataItem.Text)ITEM[pos, 0]).Data = Source[j].Name;
                        ((IGMDataItem.Text)ITEM[pos, 0]).FontColor = cid;
                        ITEM[pos, 0].Show();
                        Contents[pos] = j;
                        pos++;
                    }
                for (; pos < Rows; pos++)
                {
                    ITEM[pos, 0].Hide();
                    BLANKS[pos] = true;
                    Contents[pos] = Kernel.Abilities.None;
                }

                if (Contents[CURSOR_SELECT] != Kernel.Abilities.None && Junction.GetMode().Equals(Mode.Abilities_Commands))
                    Junction.ChangeHelp(Source[Contents[CURSOR_SELECT]].Description);
                UpdateTitle();
                if (Contents[CURSOR_SELECT] == Kernel.Abilities.None)
                    CURSOR_NEXT();
                if (Pages > 1)
                {
                    if (Contents[0] == Kernel.Abilities.None)
                    {
                        //Pages = Page;
                        PAGE_NEXT();
                        return Update();
                    }
                    //else if (Contents[Rows - 1] == Kernel_bin.Abilities.None)
                    //    Pages = Page + 1;
                }
                return base.Update();
            }

            public override void UpdateTitle()
            {
                base.UpdateTitle();
                if (Pages == 1)
                {
                    ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.COMMAND;
                    ITEM[11, 0].Hide();
                    ITEM[12, 0].Hide();
                }
                else
                {
                    ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.COMMAND_PG1 + checked((byte)Page);
                    ITEM[11, 0].Show();
                    ITEM[12, 0].Show();
                }
            }

            protected override void Init()
            {
                base.Init();
                for (var pos = 0; pos < Rows; pos++)
                    ITEM[pos, 0] = new IGMDataItem.Text
                    {
                        Icon = Icons.ID.Ability_Command,
                        Palette = 9,
                        Pos = new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0)
                    };
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
