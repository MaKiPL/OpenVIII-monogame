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
            Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
        }

        #endregion Destructors

        #region Enums

        public enum SectionName : byte
        {
            Commands,
            HP,
            Renzokeken
        }

        #endregion Enums

        #region Properties

        public bool CrisisLevel => ((IGMData.Commands)Data[SectionName.Commands]).CrisisLevel;

        public IGMData.Renzokeken Renzokeken
        {
            get
            {
                if (Data.TryGetValue(SectionName.Renzokeken, out Menu_Base val))
                    return (IGMData.Renzokeken)val;
                return null;
            }
        }

        #endregion Properties

        #region Methods

        //private Mode _mode = Mode.Waiting;

        //public BattleMenu(Damageable damageable) : base(damageable)
        //{
        //}
        public static BattleMenu Create(Damageable damageable) => Create<BattleMenu>(damageable);

        public void DrawData(SectionName v)
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, Menu_Base> i in Data.Where(a => a.Key.Equals(v)))
                    i.Value.Draw();
        }

        public override Enum GetMode() => Damageable.GetBattleMode();

        public override bool Inputs()
        {
            if (Data[SectionName.Renzokeken].Enabled)
                return Data[SectionName.Renzokeken].Inputs();
            return Data[SectionName.Commands].Inputs();
        }

        public override void Reset() => base.Reset();

        public override bool SetMode(Enum mode) => Damageable.SetBattleMode(mode);

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 880, Y = 636 };
            base.Init();
            Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
            InitAsync();

            Data.ForEach(x => x.Value.AddModeChangeEvent(ref ModeChangeHandler));
            SetMode(Damageable.BattleMode.ATB_Charging);
        }

        protected override void ModeChangeEvent(object sender, Enum e)
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

        private void InitAsync()
        {
            IGMData_HP.ThreadUnsafeOperations();
            IGMData.Renzokeken.ThreadUnsafeOperations();
            List<Task> tasks = new List<Task>
            {
                Task.Run(() => Data.TryAdd(SectionName.Commands, IGMData.Commands.Create(new Rectangle(50, (int)(Size.Y - 204), 210, 192), Damageable, true))),
                Task.Run(() => Data.TryAdd(SectionName.HP, IGMData_HP.Create(new Rectangle((int)(Size.X - 389), 507, 389, 126), Damageable))),
                //Task.Run(() => Data.TryAdd(SectionName.Renzokeken, IGMData.Renzokeken.Create(new Rectangle(0, 500, (int)Size.X, 124))))
            };
            //Some code that cannot be threaded on init.
            //Data.TryAdd(SectionName.HP, IGMData_HP.Create(new Rectangle((int)(Size.X - 389), 507, 389, 126), Damageable));
            Data.TryAdd(SectionName.Renzokeken, IGMData.Renzokeken.Create(new Rectangle(0, 500, (int)Size.X, 124)));
            if (!Task.WaitAll(tasks.ToArray(), 10000))
                throw new TimeoutException("Task took too long!");
            //var t = Task.WhenAll(tasks);
            //try
            //{
            //    await t;
            //}
            //catch { }

            //if (t.Status == TaskStatus.RanToCompletion)
            //    Console.WriteLine("All attempts succeeded.");
            //else if (t.Status == TaskStatus.Faulted)
            //    Console.WriteLine(t.Exception);
        }

        #endregion Methods
    }
}