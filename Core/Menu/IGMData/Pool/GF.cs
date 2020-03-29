using Microsoft.Xna.Framework;
using OpenVIII.Kernel;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public class GF : Base<Saves.Data, GFs>
    {
        #region Properties

        public Dictionary<GFs, Characters> JunctionedGFs { get; private set; }

        public IEnumerable<GFs> UnlockedGFs { get; private set; }

        #endregion Properties

        #region Methods

        public static GF Create(Rectangle? pos = null, Damageable damageable = null, bool battle = false)
        {
            var r = new GF
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
            r.SetDamageable(damageable);
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
                var select = Contents[CURSOR_SELECT];
                var characterID = Damageable.GetCharacterData(out var characterData) && JunctionedGFs.ContainsKey(select) ?
                    JunctionedGFs[select] : characterData?.ID ?? Characters.Blank;

                if (characterID == Characters.Blank) return false;
                if (characterData != null && characterID == characterData.ID)
                {
                    //Purge everything that you can't have anymore. Because the GF provided for you.
                    var a = (characterData).UnlockedGFAbilities;
                    characterData.RemoveJunctionedGF(select);
                    var b = (characterData).UnlockedGFAbilities;
                    foreach (var r in a.Except(b).Where(v => !Memory.KernelBin.JunctionAbilities.ContainsKey(v)))
                    {
                        if (Memory.KernelBin.CommandAbilities.ContainsKey(r))
                        {
                            characterData.Commands.Remove(r);
                            characterData.Commands.Add(Abilities.None);
                        }
                        else if (Memory.KernelBin.EquippableAbilities.ContainsKey(r))
                        {
                            characterData.Abilities.Remove(r);
                            characterData.Abilities.Add(Abilities.None);
                        }
                    }

                    foreach (var r in a.Except(b).Where(v => Memory.KernelBin.JunctionAbilities.ContainsKey(v)))
                    {
                        if (Kernel.KernelBin.Stat2Ability.Any(item => item.Value == r))
                            switch (r)
                            {
                                case Abilities.StAtkJ:
                                    characterData.StatJ[Stat.StAtk] = 0;
                                    break;

                                case Abilities.ElAtkJ:
                                    characterData.StatJ[Stat.ElAtk] = 0;
                                    break;

                                case Abilities.ElDefJ:
                                case Abilities.ElDefJ2:
                                case Abilities.ElDefJ4:
                                    byte count = 0;
                                    if (b.Contains(Abilities.ElDefJ4))
                                        count = 4;
                                    else if (b.Contains(Abilities.ElDefJ2))
                                        count = 2;
                                    else if (b.Contains(Abilities.ElDefJ))
                                        count = 1;
                                    for (; count < 4; count++)
                                        characterData.StatJ[Stat.ElDef1 + count] = 0;
                                    break;

                                case Abilities.StDefJ:
                                case Abilities.StDefJ2:
                                case Abilities.StDefJ4:
                                    count = 0;
                                    if (b.Contains(Abilities.StDefJ4))
                                        count = 4;
                                    else if (b.Contains(Abilities.StDefJ2))
                                        count = 2;
                                    else if (b.Contains(Abilities.StDefJ))
                                        count = 1;
                                    for (; count < 4; count++)
                                        characterData.StatJ[Stat.StDef1 + count] = 0;
                                    break;

                                case Abilities.Ability3:
                                case Abilities.Ability4:
                                    count = 2;
                                    if (b.Contains(Abilities.Ability4))
                                        count = 4;
                                    else if (b.Contains(Abilities.Ability3))
                                        count = 3;
                                    for (; count < characterData.Abilities.Count; count++)
                                        characterData.Abilities[count] = Abilities.None;
                                    break;

                                case Abilities.None:
                                    break;

                                case Abilities.HPJ:
                                    break;

                                case Abilities.StrJ:
                                    break;

                                case Abilities.VitJ:
                                    break;

                                case Abilities.MagJ:
                                    break;

                                case Abilities.SprJ:
                                    break;

                                case Abilities.SpdJ:
                                    break;

                                case Abilities.EvaJ:
                                    break;

                                case Abilities.HitJ:
                                    break;

                                case Abilities.LuckJ:
                                    break;

                                case Abilities.Magic:
                                    break;

                                case Abilities.GF:
                                    break;

                                case Abilities.Draw:
                                    break;

                                case Abilities.Item:
                                    break;

                                case Abilities.Empty:
                                    break;

                                case Abilities.Card:
                                    break;

                                case Abilities.Doom:
                                    break;

                                case Abilities.MadRush:
                                    break;

                                case Abilities.Treatment:
                                    break;

                                case Abilities.Defend:
                                    break;

                                case Abilities.Darkside:
                                    break;

                                case Abilities.Recover:
                                    break;

                                case Abilities.Absorb:
                                    break;

                                case Abilities.Revive:
                                    break;

                                case Abilities.LvDown:
                                    break;

                                case Abilities.LvUp:
                                    break;

                                case Abilities.Kamikaze:
                                    break;

                                case Abilities.Devour:
                                    break;

                                case Abilities.MiniMog:
                                    break;

                                case Abilities.HP20:
                                    break;

                                case Abilities.HP40:
                                    break;

                                case Abilities.HP80:
                                    break;

                                case Abilities.Str20:
                                    break;

                                case Abilities.Str40:
                                    break;

                                case Abilities.Str60:
                                    break;

                                case Abilities.Vit20:
                                    break;

                                case Abilities.Vit40:
                                    break;

                                case Abilities.Vit60:
                                    break;

                                case Abilities.Mag20:
                                    break;

                                case Abilities.Mag40:
                                    break;

                                case Abilities.Mag60:
                                    break;

                                case Abilities.Spr20:
                                    break;

                                case Abilities.Spr40:
                                    break;

                                case Abilities.Spr60:
                                    break;

                                case Abilities.Spd20:
                                    break;

                                case Abilities.Spd40:
                                    break;

                                case Abilities.Eva30:
                                    break;

                                case Abilities.Luck50:
                                    break;

                                case Abilities.Mug:
                                    break;

                                case Abilities.MedData:
                                    break;

                                case Abilities.Counter:
                                    break;

                                case Abilities.ReturnDamage:
                                    break;

                                case Abilities.Cover:
                                    break;

                                case Abilities.Initiative:
                                    break;

                                case Abilities.MoveHPUp:
                                    break;

                                case Abilities.HPBonus:
                                    break;

                                case Abilities.StrBonus:
                                    break;

                                case Abilities.VitBonus:
                                    break;

                                case Abilities.MagBonus:
                                    break;

                                case Abilities.SprBonus:
                                    break;

                                case Abilities.AutoProtect:
                                    break;

                                case Abilities.AutoShell:
                                    break;

                                case Abilities.AutoReflect:
                                    break;

                                case Abilities.AutoHaste:
                                    break;

                                case Abilities.AutoPotion:
                                    break;

                                case Abilities.Expend2:
                                    break;

                                case Abilities.Expend3:
                                    break;

                                case Abilities.Ribbon:
                                    break;

                                case Abilities.Alert:
                                    break;

                                case Abilities.MoveFind:
                                    break;

                                case Abilities.EncHalf:
                                    break;

                                case Abilities.EncNone:
                                    break;

                                case Abilities.RareItem:
                                    break;

                                case Abilities.SumMag10:
                                    break;

                                case Abilities.SumMag20:
                                    break;

                                case Abilities.SumMag30:
                                    break;

                                case Abilities.SumMag40:
                                    break;

                                case Abilities.GFHP10:
                                    break;

                                case Abilities.GFHP20:
                                    break;

                                case Abilities.GFHP30:
                                    break;

                                case Abilities.GFHP40:
                                    break;

                                case Abilities.Boost:
                                    break;

                                case Abilities.Haggle:
                                    break;

                                case Abilities.SellHigh:
                                    break;

                                case Abilities.Familiar:
                                    break;

                                case Abilities.CallShop:
                                    break;

                                case Abilities.JunkShop:
                                    break;

                                case Abilities.ThunderMagRF:
                                    break;

                                case Abilities.IceMagRF:
                                    break;

                                case Abilities.FireMagRF:
                                    break;

                                case Abilities.LifeMagRF:
                                    break;

                                case Abilities.TimeMagRF:
                                    break;

                                case Abilities.StatusMagRF:
                                    break;

                                case Abilities.SuptMagRF:
                                    break;

                                case Abilities.ForbidMagRF:
                                    break;

                                case Abilities.RecoveryMedRF:
                                    break;

                                case Abilities.StatusMedRF:
                                    break;

                                case Abilities.AmmoRF:
                                    break;

                                case Abilities.ToolRF:
                                    break;

                                case Abilities.ForbidMedRF:
                                    break;

                                case Abilities.GFRecoveryMedRF:
                                    break;

                                case Abilities.GFAblMedRF:
                                    break;

                                case Abilities.MidMagRF:
                                    break;

                                case Abilities.HighMagRF:
                                    break;

                                case Abilities.MedLvUp:
                                    break;

                                case Abilities.CardMod:
                                    break;

                                default:
                                    characterData.StatJ[
                                        Kernel.KernelBin.Stat2Ability.FirstOrDefault(x => x.Value == r).Key] = 0;
                                    break;
                            }
                    }

                    Menu.IGM_Junction.Refresh();
                    return true;
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
                var pos = 0;
                var skip = Page * Rows;
                if (Damageable.GetCharacterData(out var c))
                {
                    if (Battle)
                    {
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] == c.ID && !Source[g].IsDead);
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] != c.ID && Source[g].IsDead, Font.ColorID.Red, true);
                    }
                    else
                    {
                        AddGFs(ref pos, ref skip, g => !JunctionedGFs.ContainsKey(g));
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] == c.ID, Font.ColorID.Grey);
                        AddGFs(ref pos, ref skip, g => JunctionedGFs.ContainsKey(g) && JunctionedGFs[g] != c.ID, Font.ColorID.Dark_Grey);
                        UpdateCharacter();
                    }
                }
                else if (Damageable.GetEnemy(out var e))
                {
                    var gfs = e.JunctionedGFs;
                    foreach (var g in gfs)
                    {
                        if (!AddGF(ref pos, ref skip, g, Source[g].IsDead ? Font.ColorID.Red : Font.ColorID.White, Source[g].IsDead)) break;
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
            for (var i = 0; i < Rows;)
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

        private bool AddGF(ref int pos, ref int skip, GFs g, Font.ColorID colorID, bool blank)
        {
            if (pos >= Rows) return false;
            if (skip-- <= 0)
            {
                BLANKS[pos] = blank;
                AddGF(ref pos, g, colorID);
            }
            return true;
        }

        private void AddGFs(ref int pos, ref int skip, System.Func<GFs, bool> predicate, Font.ColorID colorID = Font.ColorID.White, bool blank = false)
        {
            foreach (var g in UnlockedGFs.Where(predicate))
            {
                if (!AddGF(ref pos, ref skip, g, colorID, blank)) break;
            }
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
            if (JunctionedGFs?.ContainsKey(g) ?? false)
                ITEM[pos, 1].Show();
            else
                ITEM[pos, 1].Hide();
            ITEM[pos, 2].Show();
        }

        private void UpdateCharacter()
        {
            if (!Battle && Menu.IGM_Junction != null)
            {
                var g = Contents[CURSOR_SELECT];
                var i =
                    (IGMDataItem.Box)((IGM_Junction.IGMData_GF_Group)Menu.IGM_Junction.Data[IGM_Junction.SectionName.TopMenu_GF_Group]).ITEM[2, 0];
                i.Data = JunctionedGFs.Count > 0 && JunctionedGFs.ContainsKey(g) ? Memory.Strings.GetName(JunctionedGFs[g]) : null;
            }
        }

        #endregion Methods
    }
}