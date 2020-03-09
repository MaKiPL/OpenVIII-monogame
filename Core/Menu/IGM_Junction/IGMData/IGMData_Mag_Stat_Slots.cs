using Microsoft.Xna.Framework;
using OpenVIII.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenVIII
{
    // ReSharper disable once InconsistentNaming
    public partial class IGM_Junction
    {
        #region Classes

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private class IgmDataMagStatSlots : IGMData_Slots<Saves.CharacterData>
        {
            #region Properties

            /// <summary>
            /// Convert stat to correct icon BattleID.
            /// </summary>
            private static IReadOnlyDictionary<Stat, Icons.ID> Stat2Icon { get; } = new Dictionary<Stat, Icons.ID>
                {
                    { Stat.HP, Icons.ID.Stats_Hit_Points },
                    { Stat.STR, Icons.ID.Stats_Strength },
                    { Stat.VIT, Icons.ID.Stats_Vitality },
                    { Stat.MAG, Icons.ID.Stats_Magic },
                    { Stat.SPR, Icons.ID.Stats_Spirit },
                    { Stat.SPD, Icons.ID.Stats_Speed },
                    { Stat.EVA, Icons.ID.Stats_Evade },
                    { Stat.LUCK, Icons.ID.Stats_Luck },
                    { Stat.HIT, Icons.ID.Stats_Hit_Percent },
                };

            #endregion Properties

            #region Methods

            public static IgmDataMagStatSlots Create() => Create<IgmDataMagStatSlots>(10, 5, new IGMDataItem.Box { Pos = new Rectangle(0, 414, 840, 216) }, 2, 5);

            public override void BackupSetting()
            {
                if (Damageable.GetCharacterData(out Saves.CharacterData c))
                    SetPrevSetting((Saves.CharacterData)c.Clone());
            }

            public override void CheckMode(bool cursor = true) =>
               CheckMode(-1, Mode.None, Mode.Mag_Stat,
                   IGM_Junction != null && (IGM_Junction.GetMode().Equals(Mode.Mag_Stat)),
                   IGM_Junction != null && (IGM_Junction.GetMode().Equals(Mode.Mag_Pool_Stat)),
                   IGM_Junction != null && ((IGM_Junction.GetMode().Equals(Mode.Mag_Stat) || IGM_Junction.GetMode().Equals(Mode.Mag_Pool_Stat)) && cursor));

            public override bool Inputs()
            {
                if (Enabled) Cursor_Status |= Cursor_Status.Enabled;
                return base.Inputs();
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.SetMode(Mode.TopMenu_Junction);
                IGM_Junction.Data[SectionName.Mag_Group].Hide();
                return true;
            }

            public override void Inputs_Left()
            {
                base.Inputs_Left();
                if (CURSOR_SELECT < Count / Cols || BLANKS[CURSOR_SELECT - Count / Cols])
                {
                    PageLeft();
                }
                else
                {
                    CURSOR_SELECT -= Count / Cols;
                }
            }

            public override void Inputs_Menu()
            {
                skipdata = true;
                base.Inputs_Menu();
                skipdata = false;
                if (Contents[CURSOR_SELECT] == Stat.None && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    c.StatJ[Contents[CURSOR_SELECT]] = 0;
                    IGM_Junction.Refresh();
                }
            }

            public override bool Inputs_OKAY()
            {
                if (!BLANKS[CURSOR_SELECT])
                {
                    base.Inputs_OKAY();
                    BackupSetting();
                    IGM_Junction.SetMode(Mode.Mag_Pool_Stat);
                    return true;
                }
                return false;
            }

            public override void Inputs_Right()
            {
                base.Inputs_Right();
                if (CURSOR_SELECT < Count / Cols && !BLANKS[CURSOR_SELECT + Count / Cols])
                {
                    //if (CURSOR_SELECT == 0) CURSOR_SELECT++;
                    CURSOR_SELECT += Count / Cols;
                }
                else
                {
                    PageRight();
                }
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (e.Equals(Mode.Mag_Stat))
                    base.ModeChangeEvent(sender, e);
            }

            /// <summary>
            /// Things that may of changed before screen loads or junction is changed.
            /// </summary>
            public override void Refresh()
            {
                if (Memory.State?.Characters == null) return;
                Contents = Array.ConvertAll(Contents, c =>
                {
                    if (!Enum.IsDefined(typeof(Stat), c))
                        throw new InvalidEnumArgumentException(nameof(c), (int)c, typeof(Stat));
                    return Stat.None;
                });
                base.Refresh();

                if (unlocked == null) return;
                {
                    ((IGMDataItem.Icon)ITEM[5, 0]).Palette =
                        (byte)(unlocked.Contains(Abilities.StAtkJ) ? 2 : 7);
                    ((IGMDataItem.Icon)ITEM[5, 1]).Palette =
                        (byte)(unlocked.Contains(Abilities.StDefJ) ||
                               unlocked.Contains(Abilities.StDefJ2) ||
                               unlocked.Contains(Abilities.StDefJ4) ? 2 : 7);
                    ((IGMDataItem.Icon)ITEM[5, 2]).Palette =
                        (byte)(unlocked.Contains(Abilities.ElAtkJ) ? 2 : 7);
                    ((IGMDataItem.Icon)ITEM[5, 3]).Palette =
                        (byte)(unlocked.Contains(Abilities.ElDefJ) ||
                               unlocked.Contains(Abilities.ElDefJ2) ||
                               unlocked.Contains(Abilities.ElDefJ4) ? 2 : 7);
                    BLANKS[5] = true;
                    foreach (Stat stat in Stat2Icon.Keys.OrderBy(o => (byte)o))
                    {
                        if (Damageable.GetCharacterData(out Saves.CharacterData c))
                        {
                            bool isUnlocked = unlocked.Contains(Kernel.KernelBin.Stat2Ability[stat]);
                            int pos = (int)stat;
                            if (pos >= 5) pos++;
                            Contents[pos] = stat;
                            FF8String name = Memory.Kernel_Bin.MagicData[c.StatJ[stat]].Name;
                            if (name == null || name.Length == 0) name = Strings.Name._;
                            ushort currentValue = Damageable.TotalStat(stat);
                            ushort previousValue = GetPrevSetting()?.TotalStat(stat) ?? currentValue;
                            ((IGMDataItem.Text)ITEM[pos, 1]).Data = name;
                            ((IGMDataItem.Integer)ITEM[pos, 2]).Data = currentValue;

                            if (previousValue == currentValue)
                            {
                                ITEM[pos, 4].Hide();
                                if (isUnlocked)
                                {
                                    SetPalettes(pos, 2);
                                    SetFontColor(pos, Font.ColorID.White);
                                }
                                else
                                {
                                    SetPalettes(pos, 7);
                                    SetFontColor(pos, Font.ColorID.Grey);
                                }
                            }
                            else if (previousValue > currentValue)
                            {
                                SetPalettes(pos, 5, 16, Icons.ID.Arrow_Down);
                                SetFontColor(pos, Font.ColorID.Red);
                            }
                            else
                            {
                                SetPalettes(pos, 6, 17, Icons.ID.Arrow_Up);
                                SetFontColor(pos, Font.ColorID.Yellow);
                            }
                            BLANKS[pos] = !isUnlocked;
                        }
                    }
                }
            }

            public override void UndoChange()
            {
                //override this use it to take value of prevSetting and restore the setting unless default method works
                if (GetPrevSetting() == null || !Damageable.GetCharacterData(out Saves.CharacterData c) ||
                    !GetPrevSetting().GetCharacterData(out Saves.CharacterData prev)) return;
                c.Magics = prev.CloneMagic();
                c.StatJ = prev.CloneMagicJunction();
            }

            protected override void AddEventListener()
            {
                if (!eventAdded)
                {
                    IGMData.Pool.Magic.SlotConfirmListener += ConfirmChangeEvent;
                    IGMData.Pool.Magic.SlotRefreshListener += ReInitEvent;
                    IGMData.Pool.Magic.SlotUndoListener += UndoChangeEvent;
                }
                base.AddEventListener();
            }

            /// <summary>
            /// Things fixed at startup.
            /// </summary>
            protected override void Init()
            {
                Contents = new Stat[Count];
                base.Init();

                ITEM[5, 0] = new IGMDataItem.Icon { Data = Icons.ID.Icon_Status_Attack, Pos = new Rectangle(SIZE[5].X + 200, SIZE[5].Y, 0, 0) };
                ITEM[5, 1] = new IGMDataItem.Icon { Data = Icons.ID.Icon_Status_Defense, Pos = new Rectangle(SIZE[5].X + 240, SIZE[5].Y, 0, 0) };
                ITEM[5, 2] = new IGMDataItem.Icon { Data = Icons.ID.Icon_Elemental_Attack, Pos = new Rectangle(SIZE[5].X + 280, SIZE[5].Y, 0, 0) };
                ITEM[5, 3] = new IGMDataItem.Icon { Data = Icons.ID.Icon_Elemental_Defense, Pos = new Rectangle(SIZE[5].X + 320, SIZE[5].Y, 0, 0) };

                foreach (Stat stat in Stat2Icon.Keys.OrderBy(o => (byte)o))
                {
                    int pos = (int)stat;
                    if (pos >= 5) pos++;
                    ITEM[pos, 0] = new IGMDataItem.Icon { Data = Stat2Icon[stat], Pos = new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0) };
                    ITEM[pos, 1] = new IGMDataItem.Text { Pos = new Rectangle(SIZE[pos].X + 80, SIZE[pos].Y, 0, 0) };
                    ITEM[pos, 2] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[pos].X + 152, SIZE[pos].Y, 0, 0), NumType = Icons.NumType.SysFntBig, Spaces = 10 };
                    ITEM[pos, 3] = stat == Stat.HIT || stat == Stat.EVA
                                    ? new IGMDataItem.Text { Data = Strings.Name.Percent, Pos = new Rectangle(SIZE[pos].X + 350, SIZE[pos].Y, 0, 0) }
                                    : null;
                    ITEM[pos, 4] = new IGMDataItem.Icon { Pos = new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), Palette = 16 };
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-22, -8);
                SIZE[i].Offset(0, 4 + (-2 * row));
            }

            protected override void PageLeft() => IGM_Junction.SetMode(Mode.Mag_EL_A);

            protected override void PageRight() => IGM_Junction.SetMode(Mode.Mag_ST_A);

            protected override void SetCursor_select(int value)
            {
                if (value != GetCursor_select())
                {
                    base.SetCursor_select(value);
                    IGMData.Pool.Magic.ChangeStat(Contents[CURSOR_SELECT]);
                }
            }

            private void ConfirmChangeEvent(object sender, Mode e) => ConfirmChange();

            private void ReInitEvent(object sender, Damageable e) => Refresh(e);

            private void SetFontColor(int pos, Font.ColorID color)
            {
                ((IGMDataItem.Text)ITEM[pos, 1]).FontColor = color;
                ((IGMDataItem.Integer)ITEM[pos, 2]).FontColor = color;
                if (ITEM[pos, 3] != null)
                    ((IGMDataItem.Text)ITEM[pos, 3]).FontColor = color;
            }

            private void SetPalettes(int pos, byte statIconPalette, byte arrowPalette = 2, Icons.ID arrowIconId = Icons.ID.None)
            {
                ((IGMDataItem.Icon)ITEM[pos, 0]).Palette = ((IGMDataItem.Icon)ITEM[pos, 0]).Faded_Palette = statIconPalette;
                ((IGMDataItem.Icon)ITEM[pos, 4]).Data = arrowIconId;
                ((IGMDataItem.Icon)ITEM[pos, 4]).Palette = arrowPalette;
            }

            private void UndoChangeEvent(object sender, Mode e) => UndoChange();

            #endregion Methods
        }

        #endregion Classes
    }
}