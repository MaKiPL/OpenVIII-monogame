using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Character BattleMenu
    /// </summary>
    public partial class BattleMenu : Menu
    {
        #region Destructors

        ~BattleMenu()
        {
            if (Damageable != null)
                Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
        }

        #endregion Destructors

        #region Enums

        public enum SectionName : byte
        {
            Commands,
            HP,
            Renzokeken,
            Shot
        }

        #endregion Enums

        #region Properties

        public sbyte CrisisLevel => ((IGMData.Commands)Data[SectionName.Commands]).CrisisLevel;

        public IGMData.Limit.Renzokeken Renzokeken
        {
            get
            {
                if (Data.TryGetValue(SectionName.Renzokeken, out Menu_Base val))
                    return (IGMData.Limit.Renzokeken)val;
                return null;
            }
        }

        public IGMData.Limit.Shot Shot
        {
            get
            {
                if (Data.TryGetValue(SectionName.Shot, out Menu_Base val))
                    return (IGMData.Limit.Shot)val;
                return null;
            }
        }

        #endregion Properties

        //private Mode _mode = Mode.Waiting;

        #region Methods

        //public BattleMenu(Damageable damageable) : base(damageable)
        //{
        //}
        public static BattleMenu Create(Damageable damageable) => Create<BattleMenu>(damageable);

        public void DrawData(SectionName v)
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, Menu_Base> i in Data.Where(a => a.Value != null && a.Key.Equals(v)))
                    i.Value.Draw();
        }

        public override Enum GetMode() => Damageable.GetBattleMode();

        public override bool Inputs()
        {
            if (Data[SectionName.Renzokeken].Enabled)
                return Data[SectionName.Renzokeken].Inputs();
            else if (Data[SectionName.Shot].Enabled)
                return Data[SectionName.Shot].Inputs();
            return Data[SectionName.Commands].Inputs();
        }
        public override bool Update()
        {
            //SkipFocus = true;
            return base.Update();
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            switch (e)
            {
                case Damageable.BattleMode.EndTurn:
                    Reset();
                    Refresh();
                    break;
            }
        }

        public override void Refresh(Damageable damageable)
        {
            if (Damageable != damageable)
            {
                if (Damageable != null)
                    Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;

                base.Refresh(damageable);
                if (Damageable != null)
                {
                    Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
                    SetMode(Damageable.BattleMode.ATB_Charging);
                }
            }
            else base.Refresh(Damageable);
        }

        public override void Reset() => base.Reset();

        //public override bool SetMode(Enum mode) => Damageable.SetBattleMode(mode);

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 880, Y = 636 };
            base.Init();
            InitAsync();
        }

        private void InitAsync()
        {
            Memory.MainThreadOnlyActions.Enqueue(() => Data.TryAdd(SectionName.Renzokeken, IGMData.Limit.Renzokeken.Create(new Rectangle(0, (int)Size.Y - 164, (int)Size.X, 124))));
            int width = 100, height = 100;
            Memory.MainThreadOnlyActions.Enqueue(() => Data.TryAdd(SectionName.Shot, IGMData.Limit.Shot.Create(new Rectangle((int)Size.X - width, (int)Size.Y - 164, width, height))));
            Action[] actions = new Action[]
            {
                () => Data.TryAdd(SectionName.Commands, IGMData.Commands.Create(new Rectangle(50, (int)(Size.Y - 224), 210, 186), Damageable, true)),
                () => Data.TryAdd(SectionName.HP, IGMData.NamesHPATB.Create(new Rectangle((int)(Size.X - 389), (int)(Size.Y - 164), 389, 40), Damageable)),
            };
            Memory.ProcessActions(actions);
        }

        #endregion Methods
    }
}