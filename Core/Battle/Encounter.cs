using System;
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
        public EnemyFlags EnabledEnemy;
        public EnemyCoordinates enemyCoordinates;
        public EnemyFlags HiddenEnemies;
        public byte PrimaryCamera;
        public byte Scenario;
        public EnemyFlags UnloadedEnemy;
        public EnemyFlags UntargetableEnemy;
        private byte[] Enemies;
        public int ID;

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
    /// <summary>
    /// 8 possible enemies. so theres 1 bit per enemy here.
    /// </summary>
    [Flags]
    public enum EnemyFlags : byte
    {
        None,
        Enemy1 = 0x1,
        Enemy2 = 0x2,
        Enemy3 = 0x4,
        Enemy4 = 0x8,
        Enemy5 = 0x10,
        Enemy6 = 0x20,
        Enemy7 = 0x40,
        Enemy8 = 0x80,
    }
}