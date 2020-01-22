using System.Linq;

namespace OpenVIII.Battle
{
    public class Encounter
    {
        #region Fields

        public byte AlternativeCamera;
        public EncounterFlag BattleFlags;
        public byte[] bLevels;
        public byte[] bUnk2;
        public byte EnabledEnemy;
        public EnemyCoordinates enemyCoordinates;
        public byte HiddenEnemies;
        public byte PrimaryCamera;
        public byte Scenario;
        public byte UnloadedEnemy;
        public byte UntargetableEnemy;
        private byte[] Enemies;

        #endregion Fields


        #region Properties

        public byte[] BEnemies { get => Enemies.Select(x => (byte)(x - 0x10)).ToArray(); set => Enemies = value; }

        public string Filename => $"a0stg{Scenario.ToString("000")}.x";

        #endregion Properties

        #region Methods

        public int ResolveCameraAnimation(byte cameraPointerValue) => cameraPointerValue & 0b1111;

        public int ResolveCameraSet(byte cameraPointerValue) => (cameraPointerValue >> 4) & 0b1111;

        #endregion Methods
    }
}