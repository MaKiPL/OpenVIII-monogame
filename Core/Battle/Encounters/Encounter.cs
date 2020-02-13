using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public class Encounter
    {
        #region Fields

        public byte AlternativeCamera;
        public EncounterFlag BattleFlags;
        public byte[] bLevels;
        //public byte[] bUnk1;
        public ushort[] bUnk2;
        public byte[] bUnk3;
        public byte[] bUnk4;
        public BitArray EnabledEnemy;
        public EnemyCoordinates enemyCoordinates;
        public BitArray HiddenEnemies;
        public byte PrimaryCamera;
        public byte Scenario;
        public BitArray UnloadedEnemy;
        public BitArray UntargetableEnemy;
        private byte[] Enemies;
        public int ID;

        #endregion Fields

        #region Properties

        public byte[] BEnemies { get => Enemies.Select(x => (byte)(x - 0x10)).ToArray(); set => Enemies = value; }

        public string Filename => $"a0stg{Scenario.ToString("000")}.x";

        public Vector3 AverageVector { get; private set; }
        public Vector3 MinVector { get; private set; }
        public Vector3 MaxVector { get; private set; }
        public Vector3 MidVector { get; private set; }

        #endregion Properties

        #region Methods

        public int ResolveCameraAnimation(byte cameraPointerValue) => cameraPointerValue & 0b1111;

        public int ResolveCameraSet(byte cameraPointerValue) => (cameraPointerValue >> 4) & 0b1111;

        public override string ToString() => $"{ID} - {Filename}";

        #endregion Methods

        public static Encounter Read(BinaryReader br, int id)
        {
            Encounter e = new Encounter
            {
                Scenario = br.ReadByte(),
                BattleFlags = (EncounterFlag)br.ReadByte(),
                PrimaryCamera = br.ReadByte(),
                AlternativeCamera = br.ReadByte(),
                HiddenEnemies = new BitArray(br.ReadBytes(1)),
                UnloadedEnemy = new BitArray(br.ReadBytes(1)),
                UntargetableEnemy = new BitArray(br.ReadBytes(1)),
                EnabledEnemy = new BitArray(br.ReadBytes(1)),
                enemyCoordinates = EnemyCoordinates.Read(br).Reverse().ToList(), //was thinking if all the bytes are reversed maybe the locations are too.
                BEnemies = br.ReadBytes(8),
                bUnk2 = Enumerable.Range(0, 16).Select(x => br.ReadUInt16()).ToArray(),
                bUnk3 = br.ReadBytes(16),
                bUnk4 = br.ReadBytes(8),
                bLevels = br.ReadBytes(8).Reverse().ToArray(),
                ID = id
            };
            Vector3 total = Vector3.Zero;
            List<Vector3> enabledCoordinates = e.enemyCoordinates.Select((x, i) => new { i, x }).Where(x => e.EnabledEnemy[7 - x.i]).Select(x => new Vector3(x.x.x, x.x.y, x.x.z)).ToList();
            if (enabledCoordinates.Count > 0)
            {
                enabledCoordinates.ForEach(x => total += x);
                e.AverageVector = total / 8f / Memory.EnemyCoordinateScale;
                e.MinVector = new Vector3(enabledCoordinates.Min(x => x.X), enabledCoordinates.Min(x => x.Y), enabledCoordinates.Min(x => x.Z)) / Memory.EnemyCoordinateScale;
                e.MaxVector = new Vector3(enabledCoordinates.Max(x => x.X), enabledCoordinates.Max(x => x.Y), enabledCoordinates.Max(x => x.Z)) / Memory.EnemyCoordinateScale;
                e.MidVector = (e.MinVector + e.MaxVector) / 2f / Memory.EnemyCoordinateScale;
            }
            return e;
        }
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