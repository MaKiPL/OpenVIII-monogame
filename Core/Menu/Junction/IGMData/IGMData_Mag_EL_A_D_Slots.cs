using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_Mag_EL_A_D_Slots : IGMData_Slots<Saves.CharacterData>
        {
            #region Properties

            public IGMData.Pool.Magic Pool { get; private set; }

            #endregion Properties

            #region Methods

            public static IGMData_Mag_EL_A_D_Slots Create() =>
                                                                                                                                        Create<IGMData_Mag_EL_A_D_Slots>(5, 2, new IGMDataItem.Box { Pos = new Rectangle(0, 414, 840, 216) }, 1, 5);

            public override void BackupSetting() => SetPrevSetting((Saves.CharacterData)Damageable.Clone());

            public override void CheckMode(bool cursor = true) =>
                CheckMode(0, Mode.Mag_EL_A, Mode.Mag_EL_D,
                    Junction != null && (Junction.GetMode().Equals(Mode.Mag_EL_A) || Junction.GetMode().Equals(Mode.Mag_EL_D)),
                    Junction != null && (Junction.GetMode().Equals(Mode.Mag_Pool_EL_A) || Junction.GetMode().Equals(Mode.Mag_Pool_EL_D)),
                    cursor);

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.SetMode(Mode.TopMenu_Junction);
                return true;
            }

            public override void Inputs_Left()
            {
                base.Inputs_Left();
                PageLeft();
            }

            public override void Inputs_Menu()
            {
                skipdata = true;
                base.Inputs_Menu();
                skipdata = false;
                if (Contents[CURSOR_SELECT] == Kernel.Stat.None && Damageable.GetCharacterData(out var c))
                {
                    c.StatJ[Contents[CURSOR_SELECT]] = 0;
                    Junction.Refresh();
                }
            }

            public override bool Inputs_OKAY()
            {
                if (!BLANKS[CURSOR_SELECT])
                {
                    base.Inputs_OKAY();
                    BackupSetting();
                    Junction.SetMode(CURSOR_SELECT == 0 ? Mode.Mag_Pool_EL_A : Mode.Mag_Pool_EL_D);
                    return true;
                }
                return false;
            }

            public override void Inputs_Right()
            {
                base.Inputs_Right();
                PageRight();
            }

            public override void Refresh()
            {
                if (Memory.State?.Characters != null && Damageable != null)
                {
                    base.Refresh();
                    FillData(Icons.ID.Icon_Elemental_Attack, Kernel.Stat.ElAtk, Kernel.Stat.ElDef1);
                }
            }

            public override void UndoChange()
            {
                //override this use it to take value of prevSetting and restore the setting unless default method works
                if (GetPrevSetting() != null && Damageable.GetCharacterData(out var c))
                {
                    c.Magics = GetPrevSetting().CloneMagic();
                    c.StatJ = GetPrevSetting().CloneMagicJunction();
                }
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

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-30, -6);
                SIZE[i].Y -= row * 2;
            }

            protected override void PageLeft() => Junction.SetMode(Mode.Mag_ST_A);

            protected override void PageRight() => Junction.SetMode(Mode.Mag_Stat);

            protected override void SetCursor_select(int value)
            {
                if (value != GetCursor_select())
                {
                    base.SetCursor_select(value);
                    CheckMode();
                    IGMData.Pool.Magic.ChangeStat(Contents[CURSOR_SELECT]);
                }
            }

            protected override bool Unlocked(byte pos)
            {
                if (unlocked != null)
                    switch (pos)
                    {
                        case 0:
                            return unlocked.Contains(Kernel.Abilities.ElAtkJ);

                        case 1:
                            return unlocked.Contains(Kernel.Abilities.ElDefJ) ||
                                unlocked.Contains(Kernel.Abilities.ElDefJ2) ||
                                unlocked.Contains(Kernel.Abilities.ElDefJ4);

                        case 2:
                            return unlocked.Contains(Kernel.Abilities.ElDefJ2) ||
                                unlocked.Contains(Kernel.Abilities.ElDefJ4);

                        case 3:
                        case 4:
                            return unlocked.Contains(Kernel.Abilities.ElDefJ4);
                    }
                return false;
            }

            private void ConfirmChangeEvent(object sender, Mode e) => ConfirmChange();

            private void ReInitEvent(object sender, Damageable e) => Refresh(e);

            private void UndoChangeEvent(object sender, Mode e) => UndoChange();

            #endregion Methods
        }

        #endregion Classes
    }
}