using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Character BattleMenu
    /// </summary>
    public partial class BattleMenu : Menu
    {
        public bool CrisisLevel => ((IGMData.Commands)Data[SectionName.Commands]).CrisisLevel;

        //private Mode _mode = Mode.Waiting;

        #region Constructors

        public BattleMenu(Damageable damageable) : base(damageable)
        {
        }

        #endregion Constructors

        #region Enums

        public enum SectionName : byte
        {
            Commands,
            HP,
            Renzokeken
        }

        #endregion Enums

        #region Methods

        public override bool Inputs()
        {
            if (Data[SectionName.Renzokeken].Enabled)
                return Data[SectionName.Renzokeken].Inputs();
            return Data[SectionName.Commands].Inputs();
        }

        public IGMData_Renzokeken Renzokeken => (IGMData_Renzokeken)Data[SectionName.Renzokeken];

        protected override void Init()
        {
            Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 880, Y = 636 };
            Data.Add(SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X - 389), 507, 389, 126), Damageable));
            Data.Add(SectionName.Commands, new IGMData.Commands(new Rectangle(50, (int)(Size.Y - 204), 210, 192), Damageable, true));
            Data.Add(SectionName.Renzokeken, new IGMData_Renzokeken(new Rectangle(0, 500, (int)Size.X, 124)));
            Data.ForEach(x => x.Value.AddModeChangeEvent(ref ModeChangeHandler));
            SetMode(Damageable.BattleMode.ATB_Charging);
            base.Init();
        }

        ~BattleMenu()
        {
            Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
        }

        private void ModeChangeEvent(object sender, Enum e)
        {
            switch (e)
            {
                case Damageable.BattleMode.EndTurn:
                    Reset();
                    Refresh();
                    break;
                case Damageable.BattleMode.ATB_Charged:
                    Data[SectionName.Commands].Hide();
                    break;

                case Damageable.BattleMode.YourTurn:
                    Data[SectionName.Commands].Show();
                    Data[SectionName.Commands].Refresh();
                    break;
            }
        }

        public override bool SetMode(Enum mode) => Damageable.SetBattleMode(mode);

        public override Enum GetMode() => Damageable.GetBattleMode();

        public override void Reset() => base.Reset();

        public void DrawData(SectionName v)
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, IGMData.Base> i in Data.Where(a => a.Key.Equals(v)))
                    i.Value.Draw();
        }

        #endregion Methods
    }
}