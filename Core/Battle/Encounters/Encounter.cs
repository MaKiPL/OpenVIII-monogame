using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    /// <summary>
    /// 8 possible enemies. so there's 1 bit per enemy here.
    /// </summary>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

    public class Encounter
    {
        #region Fields

        public byte AlternativeCamera;
        public EncounterFlag BattleFlags;
        public byte[] BLevels;

        //public byte[] bUnk1;
        public ushort[] BUnk2;

        public byte[] BUnk3;
        public byte[] BUnk4;
        public BitArray EnabledEnemy;
        public EnemyCoordinates EnemyCoordinates;
        public BitArray HiddenEnemies;
        public int ID;
        public byte PrimaryCamera;
        public byte Scenario;
        public BitArray UnloadedEnemy;
        public BitArray UntargetableEnemy;
        private byte[] _enemies;

        #endregion Fields

        #region Properties

        public Vector3 AverageVector { get; set; }
        public byte[] BEnemies { get => _enemies.Select(x => (byte)(x - 0x10)).ToArray(); set => _enemies = value; }

        public IEnumerable<KeyValuePair<int, byte>> EnabledMonsters => BEnemies.Select((x, i) => new KeyValuePair<int, byte>(i, x)).Where(x => EnabledEnemy.Cast<bool>().Reverse().ElementAt(x.Key));
        public string Filename => $"a0stg{Scenario:000}.x";
        public Vector3 MaxVector { get; set; }
        public Vector3 MidVector { get; set; }
        public Vector3 MinVector { get; set; }
        public IEnumerable<byte> UniqueMonstersList => EnabledMonsters.Select(x => x.Value).Distinct();

        #endregion Properties

        #region Methods

        public static Encounter Read(BinaryReader br, int id)
        {
            var e = new Encounter
            {
                Scenario = br.ReadByte(),
                BattleFlags = (EncounterFlag)br.ReadByte(),
                PrimaryCamera = br.ReadByte(),
                AlternativeCamera = br.ReadByte(),
                HiddenEnemies = new BitArray(br.ReadBytes(1)),
                UnloadedEnemy = new BitArray(br.ReadBytes(1)),
                UntargetableEnemy = new BitArray(br.ReadBytes(1)),
                EnabledEnemy = new BitArray(br.ReadBytes(1)),
                EnemyCoordinates = EnemyCoordinates.Read(br).Reverse().ToList(), //was thinking if all the bytes are reversed maybe the locations are too.
                BEnemies = br.ReadBytes(8),
                BUnk2 = Enumerable.Range(0, 16).Select(x => br.ReadUInt16()).ToArray(),
                BUnk3 = br.ReadBytes(16),
                BUnk4 = br.ReadBytes(8),
                BLevels = br.ReadBytes(8).Reverse().ToArray(),
                ID = id
            };

            var total = Vector3.Zero;
            var enabledCoordinates = e.EnemyCoordinates.Select((x, i) => new {i, x})
                .Where(x => e.EnabledEnemy[7 - x.i]).Select(x => new Vector3(x.x.X, x.x.Y, x.x.Z)).ToList();
            if (enabledCoordinates.Count <= 0) return e;
            {
                enabledCoordinates.ForEach(x => total += x);
                e.AverageVector = total / 8f / Memory.EnemyCoordinateScale;
                e.MinVector =
                    new Vector3(enabledCoordinates.Min(x => x.X), enabledCoordinates.Min(x => x.Y),
                        enabledCoordinates.Min(x => x.Z)) / Memory.EnemyCoordinateScale;
                e.MaxVector =
                    new Vector3(enabledCoordinates.Max(x => x.X), enabledCoordinates.Max(x => x.Y),
                        enabledCoordinates.Max(x => x.Z)) / Memory.EnemyCoordinateScale;
                e.MidVector = (e.MinVector + e.MaxVector) / 2f / Memory.EnemyCoordinateScale;
            }
            return e;
        }

        public int ResolveCameraAnimation(byte cameraPointerValue) => cameraPointerValue & 0b1111;

        public int ResolveCameraSet(byte cameraPointerValue) => (cameraPointerValue >> 4) & 0b1111;

        public override string ToString() => $"{ID} - {Filename}";

        #endregion Methods
    }
}