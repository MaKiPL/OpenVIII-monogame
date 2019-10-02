using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public partial class VictoryMenu
        {
            #region Classes

            private class IGMData_PlayerEXP : IGMData
            {
                #region Fields

                private int _exp;
                private byte _lvl;

                #endregion Fields

                #region Constructors

                public IGMData_PlayerEXP(sbyte partypos) : base(1, 12, new IGMDataItem_Box(pos: new Rectangle(35, 78 + partypos * 150, 808, 150)), 1, 1, partypos: partypos)
                {
                    _exp = 0;
                    Debug.Assert(partypos >= 0 && partypos <= 2);
                }

                #endregion Constructors

                #region Properties

                public int EXP
                {
                    get => _exp; set
                    {
                        if (Damageable != null)
                        {
                            if (_exp == 0 || !Damageable.IsGameOver)
                            {
                                if (value < 0) value = 0;
                                if (_exp != 0 && Damageable.GetCharacterData(out Saves.CharacterData c))
                                    c.Experience += (uint)Math.Abs((MathHelper.Distance(_exp, value)));
                                _exp = value;
                            }
                            else if (Damageable.IsGameOver)
                                ITEM[0, 11].Show();
                        }
                    }
                }

                #endregion Properties

                #region Methods

                public override void Refresh() => base.Refresh();

                public override bool Update()
                {
                    if (Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                    {
                        ((IGMDataItem_Int)ITEM[0, 4]).Data = _exp;
                        ((IGMDataItem_Int)ITEM[0, 6]).Data = checked((int)c.Experience);
                        ((IGMDataItem_Int)ITEM[0, 8]).Data = c.ExperienceToNextLevel;
                        byte lvl = Damageable.Level;
                        if (lvl != _lvl)
                        {
                            _lvl = lvl;
                            //trigger level up message and sound effect
                            init_debugger_Audio.PlaySound(0x28);
                            ITEM[0, 10].Show();
                        }
                        ((IGMDataItem_Int)ITEM[0, 2]).Data = _lvl;

                        return base.Update();
                    }
                    return false;
                }

                protected override void Init()
                {
                    if (Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                    {

                        Hide();
                        if (ECN == null)
                            ECN = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 29) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 30) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 31);
                        base.Init();
                        uint exp = c.Experience;
                        ushort expTNL = c.ExperienceToNextLevel;
                        _lvl = Damageable.Level;
                        FF8String name = Damageable.Name;

                        ITEM[0, 0] = new IGMDataItem_String(name, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0));
                        ITEM[0, 1] = new IGMDataItem_Icon(Icons.ID.Size_16x16_Lv_, new Rectangle(SIZE[0].X, SIZE[0].Y + 34, 0, 0), 13);
                        ITEM[0, 2] = new IGMDataItem_Int(_lvl, new Rectangle(SIZE[0].X + 50, SIZE[0].Y + 38, 0, 0), spaces: 4, numtype: Icons.NumType.sysFntBig);
                        ITEM[0, 3] = new IGMDataItem_String(ECN, new Rectangle(SIZE[0].X + 390, SIZE[0].Y, 0, 0));
                        ITEM[0, 4] = new IGMDataItem_Int(0, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, SIZE[0].Y, 0, 0), spaces: 7);
                        ITEM[0, 5] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, SIZE[0].Y, 0, 0));
                        ITEM[0, 6] = new IGMDataItem_Int((int)exp, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0), spaces: 7);
                        ITEM[0, 7] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0));
                        ITEM[0, 8] = new IGMDataItem_Int(expTNL, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0), spaces: 7);
                        ITEM[0, 9] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0));
                        ITEM[0, 10] = new IGMData_TimedMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 32), SIZE[0].X + 190, SIZE[0].Y);
                        ITEM[0, 10].Hide();
                        ITEM[0, 11] = new IGMData_SmallMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 49), SIZE[0].X + 190, SIZE[0].Y);
                        ITEM[0, 11].Hide();
                    }
                }

                protected override void InitShift(int i, int col, int row)
                {
                    SIZE[i].Inflate(-25, -20);
                    base.InitShift(i, col, row);
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}