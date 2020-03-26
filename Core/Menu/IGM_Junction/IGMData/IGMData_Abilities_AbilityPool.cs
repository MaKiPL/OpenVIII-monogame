using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Abilities_AbilityPool : IGMData.Pool.Base<IReadOnlyDictionary<Kernel.Abilities, Kernel.IEquippableAbility>, Kernel.Abilities>
        {
            #region Methods

            public static IGMData_Abilities_AbilityPool Create()
            {
                IGMData_Abilities_AbilityPool r = null;
                if (Memory.Kernel_Bin.EquippableAbilities != null)
                {
                    r = Create<IGMData_Abilities_AbilityPool>(11, 1, new IGMDataItem.Box { Pos = new Rectangle(435, 150, 405, 480), Title = Icons.ID.ABILITY }, 11, Memory.Kernel_Bin.EquippableAbilities.Count / 11 + (Memory.Kernel_Bin.EquippableAbilities.Count % 11 > 0 ? 1 : 0));
                    r.Source = Memory.Kernel_Bin.EquippableAbilities;
                }
                return r;
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.SetMode(Mode.Abilities);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                if (Contents[CURSOR_SELECT] != Kernel.Abilities.None && !BLANKS[CURSOR_SELECT] && Damageable.GetCharacterData(out var c))
                {
                    skipsnd = true;
                    AV.Sound.Play(31);
                    base.Inputs_OKAY();
                    var target = ((IGMData.Base)IGM_Junction.Data[SectionName.TopMenu_Abilities]).CURSOR_SELECT - 4;
                    c.Abilities[target] = Contents[CURSOR_SELECT];
                    IGM_Junction.SetMode(Mode.Abilities);
                    IGM_Junction.Refresh(); // can be more specific if you want to find what is being changed.
                    return true;
                }
                return false;
            }

            public override bool Update()
            {
                if (IGM_Junction != null && !IGM_Junction.GetMode().Equals(Mode.Abilities_Abilities))
                    Cursor_Status &= ~Cursor_Status.Enabled;
                else
                    Cursor_Status |= Cursor_Status.Enabled;
                var pos = 0;
                var skip = Page * Rows;
                if (Damageable != null && Damageable.GetCharacterData(out var c))
                    for (var i = 0;
                        Memory.State.Characters != null &&
                        i < c.UnlockedGFAbilities.Count &&
                        pos < Rows; i++)
                    {
                        if (c.UnlockedGFAbilities[i] != Kernel.Abilities.None)
                        {
                            var j = c.UnlockedGFAbilities[i];
                            if (Source.ContainsKey(j))
                            {
                                if (skip > 0)
                                {
                                    skip--;
                                    continue;
                                }

                                Font.ColorID cid;
                                if (c.Abilities.Contains(j))
                                {
                                    cid = Font.ColorID.Grey;
                                    BLANKS[pos] = true;
                                }
                                else
                                {
                                    cid = Font.ColorID.White;
                                    BLANKS[pos] = false;
                                }

                                ((IGMDataItem.Text)ITEM[pos, 0]).Icon = Source[j].Icon;
                                ((IGMDataItem.Text)ITEM[pos, 0]).Data = Source[j].Name;
                                ((IGMDataItem.Text)ITEM[pos, 0]).FontColor = cid;
                                ITEM[pos, 0].Show();
                                Contents[pos] = j;
                                pos++;
                            }
                        }
                    }
                for (; pos < Rows; pos++)
                {
                    ITEM[pos, 0].Hide();
                    BLANKS[pos] = true;
                    Contents[pos] = Kernel.Abilities.None;
                }
                if (Contents[CURSOR_SELECT] != Kernel.Abilities.None && IGM_Junction.GetMode().Equals(Mode.Abilities_Abilities))
                    IGM_Junction.ChangeHelp(Source[Contents[CURSOR_SELECT]].Description);
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
                    //    //Pages = Page + 1;
                }
                return base.Update();
            }

            public override void UpdateTitle()
            {
                base.UpdateTitle();
                if (Pages == 1)
                {
                    ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.ABILITY;
                    ITEM[11, 0] = ITEM[12, 0] = null;
                }
                else
                    switch (Page)
                    {
                        case 0:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.ABILITY_PG1;
                            break;

                        case 1:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.ABILITY_PG2;
                            break;

                        case 2:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.ABILITY_PG3;
                            break;

                        case 3:
                            ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.ABILITY_PG4;
                            break;
                    }
            }

            protected override void Init()
            {
                base.Init();
                Hide();
                for (var pos = 0; pos < Rows; pos++)
                    ITEM[pos, 0] = new IGMDataItem.Text
                    {
                        Palette = 9,
                        Pos =
                        new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0)
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
