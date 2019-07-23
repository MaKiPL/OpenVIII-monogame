using System;
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    /// <summary>
    /// Character BattleMenu
    /// </summary>
    public partial class BattleMenu : Menu
    {
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
            HP
        }

        #endregion Enums

        #region Methods
        
        public override bool Inputs() => Data[SectionName.Commands].Inputs();

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Pos = new Rectangle ( 0,0,881, 636 );
            Data.Add(SectionName.Commands, new IGMData_Commands(new Rectangle(50, (int)(Pos.Height - 204), 210, 192), Character, VisableCharacter, true));
            Data.Add(SectionName.HP, new IGMData_HP(new Rectangle((int)(Pos.Width - 389), 507, 389, 126), Character, VisableCharacter));
            Data.ForEach(x => x.Value.SetModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.ATB_Charging);
            base.Init();
        }

        public override void SetModeChangeEvent(ref EventHandler<Enum> modeChangeHandler) => throw new NotImplementedException();
        public override void Refresh(Characters character, Characters? visableCharacter) => throw new NotImplementedException();

        #endregion Methods

    }
}