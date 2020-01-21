using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static class Init_debugger_battle
    {
        public struct EncounterFlag
        {
            public bool CantEspace;
            public bool NoVictorySequence;
            public bool ShowTimer;
            public bool NoEXP;
            public bool SkipEXPScreen;
            public bool SurpriseAttack;
            public bool BackAttacked;
            public bool isScriptedBattle;

            public byte Switch { set => SetFlags(value); }

            public void SetFlags(byte @switch)
            {
                CantEspace = (@switch & 1) == 1;
                NoVictorySequence = (@switch >> 1 & 1) == 1;
                ShowTimer = (@switch >> 2 & 1) == 1;
                NoEXP = (@switch >> 3 & 1) == 1;
                SkipEXPScreen = (@switch >> 4 & 1) == 1;
                SurpriseAttack = (@switch >> 5 & 1) == 1;
                BackAttacked = (@switch >> 6 & 1) == 1;
                isScriptedBattle = (@switch >> 7 & 1) == 1;
            }
        }

        public class Encounters : IList
        {
            private List<Encounter> encounters;

            public Encounters(int count = 0) => encounters = new List<Encounter>(count);

            public object this[int index] { get => ((IList)encounters)[index]; set => ((IList)encounters)[index] = value; }

            public bool IsReadOnly => ((IList)encounters).IsReadOnly;

            public bool IsFixedSize => ((IList)encounters).IsFixedSize;

            public int Count => ((IList)encounters).Count;

            public object SyncRoot => ((IList)encounters).SyncRoot;

            public bool IsSynchronized => ((IList)encounters).IsSynchronized;

            public int Add(object value) => ((IList)encounters).Add(value);

            public void Clear() => ((IList)encounters).Clear();

            public bool Contains(object value) => ((IList)encounters).Contains(value);

            public void CopyTo(Array array, int index) => ((IList)encounters).CopyTo(array, index);

            public IEnumerator GetEnumerator() => ((IList)encounters).GetEnumerator();

            public int IndexOf(object value) => ((IList)encounters).IndexOf(value);

            public void Insert(int index, object value) => ((IList)encounters).Insert(index, value);

            public void Remove(object value) => ((IList)encounters).Remove(value);

            public void RemoveAt(int index) => ((IList)encounters).RemoveAt(index);

            public int CurrentIndex { get; set; }

            public Encounter Current(int? index = null)
            {
                if (index.HasValue)
                    CurrentIndex = index.Value;
                return encounters[CurrentIndex];
            }

            public Encounter Previous()
            {
                CurrentIndex--;
                if (CurrentIndex < 0)
                    CurrentIndex = encounters.Count - 1;
                return Current();
            }

            public Encounter Next()
            {
                CurrentIndex++;
                if (CurrentIndex >= encounters.Count)
                    CurrentIndex = 0;
                return Current();
            }
        }

        public class Encounter
        {
            public byte Scenario;
            public EncounterFlag BattleFlags;
            public byte PrimaryCamera;
            public byte AlternativeCamera;
            public byte HiddenEnemies;
            public byte UnloadedEnemy;
            public byte UntargetableEnemy;
            public byte EnabledEnemy;
            public EnemyCoordinates enemyCoordinates;
            private byte[] Enemies; //sizeof 8
            public byte[] bUnk2; //sizeof 16*3 + 8
            public byte[] bLevels; //sizeof 8

            public byte[] BEnemies { get => Enemies.Select(x => (byte)(x - 0x10)).ToArray(); set => Enemies = value; }

            public int ResolveCameraAnimation(byte cameraPointerValue) => cameraPointerValue & 0b1111;

            public int ResolveCameraSet(byte cameraPointerValue) => (cameraPointerValue >> 4) & 0b1111;
            public string Filename => $"a0stg{Scenario.ToString("000")}.x";
        }

        public struct EnemyCoordinates
        {
            public Coordinate cEnemy1;
            public Coordinate cEnemy2;
            public Coordinate cEnemy3;
            public Coordinate cEnemy4;
            public Coordinate cEnemy5;
            public Coordinate cEnemy6;
            public Coordinate cEnemy7;
            public Coordinate cEnemy8;

            public Coordinate GetEnemyCoordinateByIndex(byte index)
            {
                switch (index)
                {
                    case 0:
                        return cEnemy8;

                    case 1:
                        return cEnemy7;

                    case 2:
                        return cEnemy6;

                    case 3:
                        return cEnemy5;

                    case 4:
                        return cEnemy4;

                    case 5:
                        return cEnemy3;

                    case 6:
                        return cEnemy2;

                    case 7:
                        return cEnemy1;

                    default:
                        return cEnemy1;
                }
            }
        }

        public struct Coordinate
        {
            public short x;
            public short y;
            public short z;

            public Vector3 GetVector() => new Vector3(
                x,
                y,
                -z) / 100f;
        }

        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(Init_debugger_battle)} :: {nameof(Init)}");
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            byte[] sceneOut = aw.GetBinaryFile("scene.out");
            ReadEncounter(sceneOut);
            Memory.Encounters.CurrentIndex = 87;
        }

        private static void ReadEncounter(byte[] enc)
        {
            Memory.Log.WriteLine($"{nameof(Init_debugger_battle)} :: {nameof(ReadEncounter)}");
            if (enc == null || enc.Length == 0) return;
            int encounterCount = enc.Length / 128;
            Memory.Encounters = new Encounters(encounterCount);

            MemoryStream ms = null;

            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(enc)))
            {
                for (int i = 0; i < encounterCount; i++)
                    Memory.Encounters.Add(new Encounter()
                    {
                        Scenario = br.ReadByte(),
                        BattleFlags = new EncounterFlag() { Switch = br.ReadByte() },
                        PrimaryCamera = br.ReadByte(),
                        AlternativeCamera = br.ReadByte(),
                        HiddenEnemies = br.ReadByte(),
                        UnloadedEnemy = br.ReadByte(),
                        UntargetableEnemy = br.ReadByte(),
                        EnabledEnemy = br.ReadByte(),
                        enemyCoordinates = new EnemyCoordinates()
                        {
                            cEnemy1 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy2 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy3 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy4 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy5 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy6 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy7 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            },
                            cEnemy8 = new Coordinate()
                            {
                                x = br.ReadInt16(),
                                y = br.ReadInt16(),
                                z = br.ReadInt16()
                            }
                        },
                        BEnemies = br.ReadBytes(8),
                        bUnk2 = br.ReadBytes(16 * 3 + 8),
                        bLevels = br.ReadBytes(8)
                    });
            ms = null;
        }
    }
}
}