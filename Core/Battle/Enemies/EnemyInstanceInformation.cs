namespace OpenVIII.Battle
{
    public class EnemyInstanceInformation
    {
        #region Fields

        public AnimationSystem animationSystem;
        public bool bIsActive;
        public bool bIsHidden;
        public bool bIsUntargetable;
        public Debug_battleDat Data;

        /// <summary>
        /// bit position of the enemy in encounter data. Use to pair the information with
        /// encounter data
        /// </summary>
        public sbyte partypos;

        #endregion Fields

        #region Properties

        public byte FixedLevel { get; internal set; }
        public bool IsFixedLevel => FixedLevel != 0xFF;
        public Coordinate Location { get; internal set; }

        #endregion Properties
    }
}