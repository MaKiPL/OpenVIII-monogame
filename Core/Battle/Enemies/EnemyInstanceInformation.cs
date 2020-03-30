using OpenVIII.Battle.Dat;

namespace OpenVIII.Battle
{
    public class EnemyInstanceInformation
    {
        #region Fields

        public AnimationSystem AnimationSystem;
        public bool BIsActive;
        public bool BIsHidden;
        public bool BIsUntargetable;
        public DatFile Data;

        /// <summary>
        /// bit position of the enemy in encounter data. Use to pair the information with
        /// encounter data
        /// </summary>
        public sbyte PartyPos;

        #endregion Fields

        #region Properties

        public byte FixedLevel { get; internal set; }
        public bool IsFixedLevel => FixedLevel != 0xFF;
        public Coordinate Location { get; internal set; }

        #endregion Properties
    }
}