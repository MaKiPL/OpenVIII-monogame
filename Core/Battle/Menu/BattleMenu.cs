using System;
using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.IGMData;
using OpenVIII.IGMData.Limit;

namespace OpenVIII
{
    /// <summary>
    /// Character BattleMenu
    /// </summary>
    public class BattleMenu : Menu
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
            Renzokuken,
            Shot
        }

        #endregion Enums

        #region Properties

        public sbyte CrisisLevel => ((Commands)Data[SectionName.Commands]).CrisisLevel;

        public Renzokuken Renzokuken
        {
            get
            {
                if (Data.TryGetValue(SectionName.Renzokuken, out var val))
                    return (Renzokuken)val;
                return null;
            }
        }

        public Shot Shot
        {
            get
            {
                if (Data.TryGetValue(SectionName.Shot, out var val))
                    return (Shot)val;
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
                foreach (var i in Data.Where(a => a.Value != null && a.Key.Equals(v)))
                    i.Value.Draw();
        }

        public override Enum GetMode() => Damageable.GetBattleMode();

        public override bool Inputs()
        {
            if (Data[SectionName.Renzokuken].Enabled)
                return Data[SectionName.Renzokuken].Inputs();
            if (Data[SectionName.Shot].Enabled)
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
            Memory.MainThreadOnlyActions.Enqueue(() => Data.TryAdd(SectionName.Renzokuken, Renzokuken.Create(new Rectangle(0, (int)Size.Y - 164, (int)Size.X, 124))));
            int width = 100, height = 100;
            Memory.MainThreadOnlyActions.Enqueue(() => Data.TryAdd(SectionName.Shot, Shot.Create(new Rectangle((int)Size.X - width, (int)Size.Y - 164, width, height))));
            var actions = new Action[]
            {
                () => Data.TryAdd(SectionName.Commands, Commands.Create(new Rectangle(50, (int)(Size.Y - 224), 210, 186), Damageable, true)),
                () => Data.TryAdd(SectionName.HP, NamesHPATB.Create(new Rectangle((int)(Size.X - 389), (int)(Size.Y - 164), 389, 40), Damageable))
            };
            Memory.ProcessActions(actions);
        }

        #endregion Methods
    }
}
