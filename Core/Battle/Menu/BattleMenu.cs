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
        public bool CrisisLevel { get => ((IGMData_Commands)Data[SectionName.Commands]).CrisisLevel; }

        //private Mode _mode = Mode.Waiting;

        #region Constructors

        public BattleMenu(Characters character, Characters? visablecharacter = null) : base(character, visablecharacter)
        {
        }

        #endregion Constructors

        #region Enums

        public enum Mode : byte
        {
            /// <summary>
            /// ATB bar filling
            /// </summary>
            /// <remarks>Orange Bar precent filled. ATB/Full ATB</remarks>
            ATB_Charging,

            /// <summary>
            /// ATB bar charged, waiting for your turn
            /// </summary>
            /// <remarks>Yellow Bar</remarks>
            ATB_Charged,

            /// <summary>
            /// Your turn
            /// </summary>
            /// <remarks>Yellow Bar/Name/HP Blinking</remarks>
            YourTurn,

            /// <summary>
            /// GF Cast
            /// </summary>
            /// <remarks>Show GF name/hp and blueish bar.</remarks>
            GF_Charging,
        }

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
            if(Data[SectionName.Renzokeken].Enabled)
               return Data[SectionName.Renzokeken].Inputs();            
            return Data[SectionName.Commands].Inputs();
        }

        public IGMData_Renzokeken Renzokeken => (IGMData_Renzokeken)Data[SectionName.Renzokeken];

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 880, Y = 636 };
            Data.Add(SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X - 389), 507, 389, 126), Character, VisableCharacter));
            Data.Add(SectionName.Commands, new IGMData_Commands(new Rectangle(50, (int)(Size.Y - 204), 210, 192), Character, VisableCharacter, true));
            Data.Add(SectionName.Renzokeken, new IGMData_Renzokeken(new Rectangle(0, 500, (int)Size.X, 124)));
            Data.ForEach(x => x.Value.SetModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.ATB_Charging);
            base.Init();
        }
        public void DrawData(SectionName v)
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, IGMData> i in Data.Where(a=>a.Key.Equals(v)))
                    i.Value.Draw();
        }
        #endregion Methods

    }
}