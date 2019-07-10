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
            Size = new Vector2 { X = 881, Y = 636 };
            Data.Add(SectionName.Commands, new IGMData_Commands(new Rectangle(50, (int)(Size.Y - 204), 210, 192), Character, VisableCharacter, true));
            Data.Add(SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X - 389), 507, 389, 126), Character, VisableCharacter));
            Data.ForEach(x => x.Value.SetModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.ATB_Charging);
            base.Init();
        }

        #endregion Methods

    }
}