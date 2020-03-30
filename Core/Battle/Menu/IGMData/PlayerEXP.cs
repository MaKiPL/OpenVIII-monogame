using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using OpenVIII.AV;
using OpenVIII.IGMData.Dialog.Timed;
using OpenVIII.IGMDataItem;

namespace OpenVIII.IGMData
{
    public class PlayerExp : Base
    {
        #region Fields

        /// <summary>
        /// <para>EXP Acquired</para>
        /// <para>Current EXP</para>
        /// <para>Next LEVEL</para>
        /// </summary>
        private static FF8String _ecn;

        private int _exp;
        private byte _lvl;

        #endregion Fields

        #region Properties

        public int Exp
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

        public static PlayerExp Create(sbyte partyPos, Rectangle? pos = null)
        {
            Debug.Assert(partyPos >= 0 && partyPos <= 2);
            var r = Create<PlayerExp>(1, 12, new Box { Pos = pos ?? new Rectangle(35, 78 + partyPos * 150, 808, 150), Title = Icons.ID.NAME }, 1, 1, partypos: partyPos);
            r._exp = 0;
            return r;
        }

        public override bool Update()
        {
            if (!Enabled || Memory.State == null || !Memory.State.Characters || Memory.State.CharactersCount <= 0 ||
                Memory.State.PartyData == null) return false;
            var c = Memory.State[Memory.State.PartyData[PartyPos]];
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

                if (((Integer) ITEM[0, 4]).Data == _exp) return true;
                ITEM[0, 11].Hide();
                ((Text)ITEM[0, 0]).Data = c.Name;
                ((Integer)ITEM[0, 4]).Data = _exp;
                ((Integer)ITEM[0, 6]).Data = checked((int)c.Experience);
                ((Integer)ITEM[0, 8]).Data = c.ExperienceToNextLevel;
                var lvl = Damageable.Level;

                if (lvl != _lvl && _lvl != 0 && !NoEarnExp)
                {
                    //trigger level up message and sound effect
                    Sound.Play(0x28);
                    ITEM[0, 10].Show();
                }
                ((Integer)ITEM[0, 2]).Data = _lvl = lvl;
            }
            else
            {
                foreach (var i in ITEM)
                {
                    i?.Hide();
                }
            }
            return true;
        }

        protected override void Init()
        {
            base.Init();
            Saves.CharacterData c = null;
            if (Damageable != null && Damageable.GetCharacterData(out c))
                _lvl = Damageable.Level;
            if (_ecn == null)
                _ecn = Strings.Name.EXP_Acquired + "\n" +
                    Strings.Name.CurrentEXP + "\n" +
                    Strings.Name.NextLEVEL;

            ITEM[0, 0] = new Text { Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0) };
            ITEM[0, 1] = new Icon { Data = Icons.ID.Size_16x16_Lv_, Pos = new Rectangle(SIZE[0].X, SIZE[0].Y + 34, 0, 0), Palette = 13 };
            ITEM[0, 2] = new Integer { Data = _lvl, Pos = new Rectangle(SIZE[0].X + 50, SIZE[0].Y + 38, 0, 0), Spaces = 4, NumType = Icons.NumType.SysFntBig };
            ITEM[0, 3] = new Text { Data = _ecn, Pos = new Rectangle(SIZE[0].X + 390, SIZE[0].Y, 0, 0) };
            ITEM[0, 4] = new Integer { Data = _exp, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, SIZE[0].Y, 0, 0), Spaces = 7 };
            ITEM[0, 5] = new Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, SIZE[0].Y, 0, 0) };
            ITEM[0, 6] = new Integer { Data = checked((int)(c?.Experience ?? 0)), Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0), Spaces = 7 };
            ITEM[0, 7] = new Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12), 0, 0) };
            ITEM[0, 8] = new Integer { Data = c?.ExperienceToNextLevel ?? 0, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 160, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0), Spaces = 7 };
            ITEM[0, 9] = new Icon { Data = Icons.ID.P, Pos = new Rectangle(SIZE[0].X + SIZE[0].Width - 20, (int)(SIZE[0].Y + TextScale.Y * 12 * 2), 0, 0) };
            ITEM[0, 10] = Small.Create(Strings.Name.LevelUP_, SIZE[0].X + 190, SIZE[0].Y);
            ITEM[0, 11] = Dialog.Small.Create(Strings.Name.Didnt_receive_EXP, SIZE[0].X + 190, SIZE[0].Y);
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