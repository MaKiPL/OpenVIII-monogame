using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        public partial class VictoryMenu
        {
            private class IGMData_PlayerEXP : IGMData
            {
                private int _exp;
                private byte _lvl;

                public IGMData_PlayerEXP(int exp, sbyte partypos) : base(1, 12, new IGMDataItem_Box(pos: new Rectangle(35, 78 + partypos * 150, 808, 150)), 1, 1, partypos: partypos)
                {
                    _exp = exp;
                    Debug.Assert(partypos >= 0 && partypos <= 2);
                }

                protected override void Init()
                {
                    if (Character != Characters.Blank)
                    {
                        if (ECN == null)
                            ECN = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 29) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 30) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 31);
                        base.Init();
                        uint exp = Memory.State[Character].Experience;
                        ushort expTNL = Memory.State[Character].ExperienceToNextLevel;
                        _lvl = Memory.State[Character].Level;
                        FF8String name = Memory.State[Character].Name;

                        ITEM[0, 0] = new IGMDataItem_String(name, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0));
                        ITEM[0, 1] = new IGMDataItem_Icon(Icons.ID.Size_16x16_Lv_, new Rectangle(SIZE[0].X, SIZE[0].Y + 34, 0, 0), 13);
                        ITEM[0, 2] = new IGMDataItem_Int(_lvl, new Rectangle(SIZE[0].X + 50, SIZE[0].Y + 38, 0, 0), spaces: 4, numtype: Icons.NumType.sysFntBig);
                        ITEM[0, 3] = new IGMDataItem_String(ECN, new Rectangle(SIZE[0].X + 390, SIZE[0].Y, 0, 0));
                        ITEM[0, 4] = new IGMDataItem_Int(0, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, SIZE[0].Y, 0, 0), spaces: 7);
                        ITEM[0, 5] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, SIZE[0].Y, 0, 0));
                        ITEM[0, 6] = new IGMDataItem_Int((int)exp, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0), spaces: 7);
                        ITEM[0, 7] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0));
                        ITEM[0, 8] = new IGMDataItem_Int((int)expTNL, new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0), spaces: 7);
                        ITEM[0, 9] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0));
                        ITEM[0, 10] = new IGMDataItem_IGMData(new IGMData_TimedMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 32),SIZE[0].X+190,SIZE[0].Y));
                        ITEM[0, 10].Hide();
                        ITEM[0, 11] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 49), SIZE[0].X + 190, SIZE[0].Y));
                        ITEM[0, 11].Hide();
                    }
                }

                protected override void InitShift(int i, int col, int row)
                {
                    SIZE[i].Inflate(-25, -20);
                    base.InitShift(i, col, row);
                }

                public override void Refresh()
                {
                    base.Refresh();
                }
                public int EXP
                {
                    get => _exp; set
                    {
                        if (_exp == 0 || !Memory.State[Character].IsGameOver)
                        {
                            if (value < 0) value = 0;
                            if(_exp != 0)
                                Memory.State[Character].Experience += (uint)Math.Abs((MathHelper.Distance(_exp, value)));
                            _exp = value;
                        }
                        else if (Memory.State[Character].IsGameOver)
                            ITEM[0, 11].Show();
                    }
                }
                public override bool Update()
                {
                    if (Character != Characters.Blank)
                    {
                        ((IGMDataItem_Int)ITEM[0, 4]).Data = _exp;
                        ((IGMDataItem_Int)ITEM[0, 6]).Data = (int)Memory.State[Character].Experience;
                        ((IGMDataItem_Int)ITEM[0, 8]).Data = Memory.State[Character].ExperienceToNextLevel;
                        byte lvl = Memory.State[Character].Level;
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
            }
        }
    }
}