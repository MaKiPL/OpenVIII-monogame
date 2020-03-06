using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using OpenVIII.Kernel;

namespace OpenVIII.IGMData.Pool
{
    public class GF : IGMData.Pool.Base<Saves.Data, GFs>
    {
        #region Properties

        public Dictionary<GFs, Characters> JunctionedGFs { get; private set; }

        public IEnumerable<GFs> UnlockedGFs { get; private set; }

        #endregion Properties

        #region Methods

        public static GF Create(Rectangle? pos = null, Damageable damageable = null, bool battle = false)
        {
            GF r = new GF
            {
                Count = 5,
                Depth = 3,
                CONTAINER = new IGMDataItem.Box
                {
                    Pos = pos ?? new Rectangle(440, 149, 385, 193),
                    Title = Icons.ID.GF
                },
                Rows = 4,
                DefaultPages = 4,
                Battle = battle
            };
            r.SetDamageable(damageable, null);
            r.Init();
            r.Refresh();
            r.Update();
            return r;
        }

        public override bool Inputs_CANCEL()
        {
            if (Battle) Hide();
            else
            {
                base.Inputs_CANCEL();
                Menu.IGM_Junction.Data[IGM_Junction.SectionName.TopMenu_GF_Group].Hide();
                Menu.IGM_Junction.SetMode(IGM_Junction.Mode.TopMenu_Junction);
            }
            return true;
        }

        public override bool Inputs_OKAY()
        {
            if (Battle)
            {
                base.Inputs_OKAY();
                Damageable.SetSummon(Contents[CURSOR_SELECT]);
                Hide();
                return true;
            }
            else
            {
                skipsnd = true;
                AV.Sound.Play(31);
                base.Inputs_OKAY();
                GFs select = Contents[CURSOR_SELECT];
                Characters characterid = Damageable.GetCharacterData(out Saves.CharacterData characterdata) && JunctionedGFs.ContainsKey(select) ?
                    JunctionedGFs[select] : characterdata?.ID ?? Characters.Blank;

                if (characterid != Characters.Blank)
                {
                    if (characterid != characterdata.ID)
                    {
                        //show error msg
                    }
                    else
                    {
                        //Purge everything that you can't have anymore. Because the GF provided for you.
                        List<Kernel.Abilities> a = (characterdata).UnlockedGFAbilities;
                        characterdata.RemoveJunctionedGF(select);
                        List<Kernel.Abilities> b = (characterdata).UnlockedGFAbilities;
                        foreach (Kernel.Abilities r in a.Except(b).Where(v => !Memory.Kernel_Bin.JunctionAbilities.ContainsKey(v)))
                        {
                            if (Memory.Kernel_Bin.CommandAbilities.ContainsKey(r))
                            {
                                characterdata.Commands.Remove(r);
                                characterdata.Commands.Add(Kernel.Abilities.None);
                            }
                            else if (Memory.Kernel_Bin.EquippableAbilities.ContainsKey(r))
                            {
                                characterdata.Abilities.Remove(r);
                                characterdata.Abilities.Add(Kernel.Abilities.None);
                            }
                        }
                        foreach (Kernel.Abilities r in a.Except(b).Where(v => Memory.Kernel_Bin.JunctionAbilities.ContainsKey(v)))
                        {
                            if (KernelBin.Stat2Ability.Any(item => item.Value == r))
                                switch (r)
                                {
                                    case Kernel.Abilities.StAtkJ:
                                        characterdata.StatJ[Kernel.Stat.ST_Atk] = 0;
                                        break;

                                    case Kernel.Abilities.ElAtkJ:
                                        characterdata.StatJ[Kernel.Stat.EL_Atk] = 0;
                                        break;

                                    case Kernel.Abilities.ElDefJ:
                                    case Kernel.Abilities.ElDefJ2:
                                    case Kernel.Abilities.ElDefJ4:
                                        byte count = 0;
                                        if (b.Contains(Kernel.Abilities.ElDefJ4))
                                            count = 4;
                                        else if (b.Contains(Kernel.Abilities.ElDefJ2))
                                            count = 2;
                                        else if (b.Contains(Kernel.Abilities.ElDefJ))
                                            count = 1;
                                        for (; count < 4; count++)
                                            characterdata.StatJ[Kernel.Stat.EL_Def_1 + count] = 0;
                                        break;

                                    case Kernel.Abilities.StDefJ:
                                    case Kernel.Abilities.StDefJ2:
                                    case Kernel.Abilities.StDefJ4:
                                        count = 0;
                                        if (b.Contains(Kernel.Abilities.StDefJ4))
                                            count = 4;
                                        else if (b.Contains(Kernel.Abilities.StDefJ2))
                                            count = 2;
                                        else if (b.Contains(Kernel.Abilities.StDefJ))
                                            count = 1;
                                        for (; count < 4; count++)
                                            characterdata.StatJ[Kernel.Stat.ST_Def_1 + count] = 0;
                                        break;

                                    case Kernel.Abilities.Ability3:
                                    case Kernel.Abilities.Ability4:
                                        count = 2;
                                        if (b.Contains(Kernel.Abilities.Ability4))
                                            count = 4;
                                        else if (b.Contains(Kernel.Abilities.Ability3))
                                            count = 3;
                                        for (; count < characterdata.Abilities.Count; count++)
                                            characterdata.Abilities[count] = Kernel.Abilities.None;
                                        break;

                                    default:
                                        characterdata.StatJ[KernelBin.Stat2Ability.FirstOrDefault(x => x.Value == r).Key] = 0;
                                        break;
                                }
                        }
                        Menu.IGM_Junction.Refresh();
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Refresh()
        {
            if (Memory.State == null) return;
            Source = Memory.State;
            JunctionedGFs = Source.JunctionedGFs();
            UnlockedGFs = Source.UnlockedGFs;
            ((IGMDataItem.Icon)ITEM[Rows, 2]).Data = Battle ? Icons.ID.HP : Icons.ID.Size_16x08_Lv_;
            if (Damageable != null)
            {
                int pos = 0;
                int skip = Page * Rows;
                if (Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    if (Battle)
                    {
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] == c.ID && !Source[g].IsDead, Font.ColorID.White);
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] != c.ID && Source[g].IsDead, Font.ColorID.Red, true);
                    }
                    else
                    {
                        AddGFs(ref pos, ref skip, g => !JunctionedGFs.ContainsKey(g), Font.ColorID.White);
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] == c.ID, Font.ColorID.Grey);
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] != c.ID, Font.ColorID.Dark_Grey);
                        UpdateCharacter();
                    }
                }
                else if (Damageable.GetEnemy(out Enemy e))
                {
                    var gfs = e.JunctionedGFs;
                    foreach (GFs g in gfs)
                    {
                        if(!AddGF(ref pos, ref skip, g, Source[g].IsDead ? Font.ColorID.Red : Font.ColorID.White, Source[g].IsDead)) break;
                    }
                }
                for (; pos < Rows; pos++)
                    HideChild(pos);
            }
            base.Refresh();
            UpdateTitle();
        }
        
        public override void UpdateTitle()
        {
            base.UpdateTitle();
            if (Pages == 1)
            {
                ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.GF;
                ITEM[Count - 1, 0].Hide();
                ITEM[Count - 2, 0].Hide();
            }
            else
            {
                ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.GF_PG1 + checked((byte)Page);
                ITEM[Count - 1, 0].Show();
                ITEM[Count - 2, 0].Show();
            }
        }

        protected override void Init()
        {
            base.Init();
            SIZE[Rows] = SIZE[0];
            SIZE[Rows].Y = Y;
            ITEM[Rows, 2] = new IGMDataItem.Icon
            {
                Pos = new Rectangle(SIZE[Rows].X + SIZE[Rows].Width - (Battle ? 50 : 30), SIZE[Rows].Y, 0, 0),
                Scale = new Vector2(2.5f)
            };
            for (int i = 0; i < Rows;)
                AddGF(ref i, GFs.Blank);
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        protected override void PAGE_NEXT()
        {
            do
            {
                base.PAGE_NEXT();
                Refresh();
            }
            while (!ITEM[0, 0].Enabled && Page != 0);
        }

        protected override void PAGE_PREV()
        {
            do
            {
                base.PAGE_PREV();
                Refresh();
            }
            while (!ITEM[0, 0].Enabled && Page != 0);
        }

        protected override void SetCursor_select(int value)
        {
            if (value != GetCursor_select())
            {
                base.SetCursor_select(value);
                UpdateCharacter();
            }
        }

        private void AddGF(ref int pos, GFs g, Font.ColorID color = Font.ColorID.White)
        {
            Contents[pos] = g;
            if (g != GFs.Blank)
            {
                ((IGMDataItem.Text)ITEM[pos, 0]).Data = Memory.Strings.GetName(g);
                ((IGMDataItem.Text)ITEM[pos, 0]).FontColor = color;
                ((IGMDataItem.Integer)ITEM[pos, 2]).Data = Battle ? Source.GFs[g].CurrentHP() : Source.GFs[g].Level;
                ShowChild(pos, g);
                if (Battle) ITEM[pos, 1].Hide();
            }
            else
            {
                if (ITEM[pos, 0] == null)
                    ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos], FontColor = color };
                if (ITEM[pos, 1] == null)
                    ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.JunctionSYM, Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 100, SIZE[pos].Y, 0, 0) };
                if (ITEM[pos, 2] == null)
                    ITEM[pos, 2] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), Spaces = 3 };
                HideChild(pos);
            }
            pos++;
        }

        private void AddGFs(ref int pos, ref int skip, System.Func<GFs, bool> predicate, Font.ColorID colorid = Font.ColorID.White, bool blank = false)
        {
            foreach (GFs g in UnlockedGFs.Where(predicate))
            {
                if (!AddGF(ref pos, ref skip, g, colorid, blank)) break;
            }
        }

        private bool AddGF(ref int pos, ref int skip, GFs g, Font.ColorID colorid, bool blank)
        {
            if (pos >= Rows) return false;
            if (skip-- <= 0)
            {
                BLANKS[pos] = blank;
                AddGF(ref pos, g, colorid);
            }
            return true;
        }

        private void HideChild(int pos)
        {
            BLANKS[pos] = true;
            ITEM[pos, 0].Hide();
            ITEM[pos, 1].Hide();
            ITEM[pos, 2].Hide();
        }

        private void ShowChild(int pos, GFs g = GFs.Blank)
        {
            BLANKS[pos] = false;
            ITEM[pos, 0].Show();
            if (JunctionedGFs?.ContainsKey(g) ?? false && !Battle)
                ITEM[pos, 1].Show();
            else
                ITEM[pos, 1].Hide();
            ITEM[pos, 2].Show();
        }

        private void UpdateCharacter()
        {
            if (!Battle && Menu.IGM_Junction != null)
            {
                GFs g = Contents[CURSOR_SELECT];
                IGMDataItem.Box i =
                    (IGMDataItem.Box)((IGM_Junction.IGMData_GF_Group)Menu.IGM_Junction.Data[IGM_Junction.SectionName.TopMenu_GF_Group]).ITEM[2, 0];
                i.Data = JunctionedGFs.Count > 0 && JunctionedGFs.ContainsKey(g) ? Memory.Strings.GetName(JunctionedGFs[g]) : null;
            }
        }

        #endregion Methods
    }
}