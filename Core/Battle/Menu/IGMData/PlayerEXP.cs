using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace OpenVIII.IGMData
{
    public class PlayerEXP : IGMData.Base
    {
        #region Fields

        /// <summary>
        /// <para>EXP Acquired</para>
        /// <para>Current EXP</para>
        /// <para>Next LEVEL</para>
        /// </summary>
        private static FF8String ECN;

        private int _exp;
        private byte _lvl;

        #endregion Fields

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
                        if (_exp != 0 && Damageable.GetCharacterData(out var c) && !NoEarnExp)
                            c.Experience += (uint)Math.Abs((MathHelper.Distance(_exp, value)));
                        _exp = value;
                    }
                    else if (Damageable.IsGameOver)
                        ITEM[0, 11].Show();
                }
            }
        }

        public bool NoEarnExp { get; internal set; }

        #endregion Properties

        #region Methods

        public static PlayerEXP Create(sbyte partypos, Rectangle? pos = null)
        {
            Debug.Assert(partypos >= 0 && partypos <= 2);
            var r = Create<PlayerEXP>(1, 12, new IGMDataItem.Box { Pos = pos ?? new Rectangle(35, 78 + partypos * 150, 808, 150), Title = Icons.ID.NAME }, 1, 1, partypos: partypos);
            r._exp = 0;
            return r;
        }

        public override bool Update()
        {
            if (Enabled && Memory.State?.Characters != null && Memory.State.Characters.Count>0 && Memory.State.PartyData != null)
            {
                if (Memory.State.Characters.TryGetValue(Memory.State.PartyData[PartyPos], out var c))
                { }
                base.Update();
                if ((Damageable = c) != null)
                {
                    for (var i = 0; i < Count; i++)
                    {
                        for (var k = 0; k < 10 && k < Depth; k++)
                        {
                            ITEM[i, k]?.Show();
                        }
                    }
                    if (((IGMDataItem.Integer)ITEM[0, 4]).Data != _exp)
                    {
                        ITEM[0, 11].Hide();
                        ((IGMDataItem.Text)ITEM[0, 0]).Data = c.Name;
                        ((IGMDataItem.Integer)ITEM[0, 4]).Data = _exp;
                        ((IGMDataItem.Integer)ITEM[0, 6]).Data = checked((int)c.Experience);
                        ((IGMDataItem.Integer)ITEM[0, 8]).Data = c.ExperienceToNextLevel;
                        var lvl = Damageable.Level;

                        if (lvl != _lvl && _lvl != 0 && !NoEarnExp)
                        {
                            //trigger level up message and sound effect
                            AV.Sound.Play(0x28);
                            ITEM[0, 10].Show();
                        }
                    ((IGMDataItem.Integer)ITEM[0, 2]).Data = _lvl = lvl;
                    }
                }
                else
                {
                    foreach (var i in ITEM)
                        if (i != null)
                            i.Hide();
                }
                return true;
            }
            return false;
        }

        protected override void Init()
        {
            base.Init();
            Saves.CharacterData c = null;
            if (Damageable != null && Damageable.GetCharacterData(out c))
                _lvl = Damageable.Level;
            if (ECN == null)
                ECN = Strings.Name.EXP_Acquired + "\n" +
                    Strings.Name.CurrentEXP + "\n" +
                    Strings.Name.NextLEVEL;

            ITEM[0, 0] = new IGMDataItem.Text { Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0) };
            ITEM[0, 1] = new IGMDataItem.Icon { Data = Icons.ID.Size_16x16_Lv_, Pos = new Rectangle(SIZE[0].X, SIZE[0].Y + 34, 0, 0), Palette = 13 };
            ITEM[0, 2] = new IGMDataItem.Integer { Data = _lvl, Pos = new Rectangle(SIZE[0].X + 50, SIZE[0].Y + 38, 0, 0), Spaces = 4, NumType = Icons.NumType.SysFntBig };
            ITEM[0, 3] = new IGMDataItem.Text { Data = ECN, Pos = new Rectangle(SIZE[0].X + 390, SIZE[0].Y, 0, 0) };
            ITEM[0, 4] = new IGMDataItem.Integer { Data = _exp, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, SIZE[0].Y, 0, 0), Spaces = 7 };
            ITEM[0, 5] = new IGMDataItem.Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, SIZE[0].Y, 0, 0) };
            ITEM[0, 6] = new IGMDataItem.Integer { Data = checked((int)(c?.Experience ?? 0)), Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0), Spaces = 7 };
            ITEM[0, 7] = new IGMDataItem.Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0) };
            ITEM[0, 8] = new IGMDataItem.Integer { Data = c?.ExperienceToNextLevel ?? 0, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0), Spaces = 7 };
            ITEM[0, 9] = new IGMDataItem.Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0) };
            ITEM[0, 10] = IGMData.Dialog.Timed.Small.Create(Strings.Name.LevelUP_, SIZE[0].X + 190, SIZE[0].Y);
            ITEM[0, 11] = IGMData.Dialog.Small.Create(Strings.Name.Didnt_receive_EXP, SIZE[0].X + 190, SIZE[0].Y);
            ITEM[0, 10].Hide();
            ITEM[0, 11].Hide();
        }

        protected override void InitShift(int i, int col, int row)
        {
            SIZE[i].Inflate(-25, -20);
            base.InitShift(i, col, row);
        }

        #endregion Methods
    }
}