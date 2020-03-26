using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Mag_ST_A_D_Slots : IGMData_Slots<Saves.CharacterData>
        {
            #region Methods

            public static IGMData_Mag_ST_A_D_Slots Create() => Create<IGMData_Mag_ST_A_D_Slots>(5, 2, new IGMDataItem.Box { Pos = new Rectangle(0, 414, 840, 216) }, 1, 5);

            public override void BackupSetting() => SetPrevSetting((Saves.CharacterData)Damageable.Clone());

            public override void CheckMode(bool cursor = true) =>
                CheckMode(0, Mode.Mag_ST_A, Mode.Mag_ST_D,
                    IGM_Junction != null && (IGM_Junction.GetMode().Equals(Mode.Mag_ST_A) || IGM_Junction.GetMode().Equals(Mode.Mag_ST_D)),
                    IGM_Junction != null && (IGM_Junction.GetMode().Equals(Mode.Mag_Pool_ST_A) || IGM_Junction.GetMode().Equals(Mode.Mag_Pool_ST_D)),
                    cursor);

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.SetMode(Mode.TopMenu_Junction);
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
                    IGM_Junction.Refresh();
                }
            }

            public override bool Inputs_OKAY()
            {
                if (!BLANKS[CURSOR_SELECT])
                {
                    base.Inputs_OKAY();
                    BackupSetting();
                    IGM_Junction.SetMode(CURSOR_SELECT == 0 ? Mode.Mag_Pool_ST_A : Mode.Mag_Pool_ST_D);
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
                    FillData(Icons.ID.Icon_Status_Attack, Kernel.Stat.StAtk, Kernel.Stat.StDef1);
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

            protected override void Init() => base.Init();

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-30, -6);
                SIZE[i].Y -= row * 2;
            }

            protected override void PageLeft() => IGM_Junction.SetMode(Mode.Mag_Stat);

            protected override void PageRight() => IGM_Junction.SetMode(Mode.Mag_EL_A);

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
                            return unlocked.Contains(Kernel.Abilities.StAtkJ);

                        case 1:
                            return unlocked.Contains(Kernel.Abilities.StDefJ) ||
                                unlocked.Contains(Kernel.Abilities.StDefJ2) ||
                                unlocked.Contains(Kernel.Abilities.StDefJ4);

                        case 2:
                            return unlocked.Contains(Kernel.Abilities.StDefJ2) ||
                                unlocked.Contains(Kernel.Abilities.StDefJ4);

                        case 3:
                        case 4:
                            return unlocked.Contains(Kernel.Abilities.StDefJ4);
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