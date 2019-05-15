using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal static class Init_debugger_battle
    {
        internal struct EncounterFlag
        {
            internal bool CantEspace;
            internal bool NoVictorySequence;
            internal bool ShowTimer;
            internal bool NoEXP;
            internal bool SkipEXPScreen;
            internal bool SurpriseAttack;
            internal bool BackAttacked;
            internal bool isScriptedBattle;

            internal byte Switch { set => SetFlags(value); }

            internal void SetFlags(byte @switch)
            {
                CantEspace = (@switch & 1) == 1;
                NoVictorySequence = (@switch>>1 & 1) == 1;
                ShowTimer = (@switch >> 2 & 1) == 1;
                NoEXP = (@switch >> 3 & 1) == 1;
                SkipEXPScreen = (@switch >> 4 & 1) == 1;
                SurpriseAttack = (@switch >> 5 & 1) == 1;
                BackAttacked = (@switch >> 6 & 1) == 1;
                isScriptedBattle = (@switch >> 7 & 1) == 1;
            }
        }

        internal struct Encounter
        {
            internal byte Scenario;
            internal EncounterFlag BattleFlags;
            internal byte PrimaryCamera;
            internal byte AlternativeCamera;
            internal byte HiddenEnemies;
            internal byte UnloadedEnemy;
            internal byte UntargetableEnemy;
            internal byte EnabledEnemy;
            internal EnemyCoordinates enemyCoordinates;
            private byte[] Enemies; //sizeof 8
            internal byte[] bUnk2; //sizeof 16*3 + 8
            internal byte[] bLevels; //sizeof 8

            internal byte[] BEnemies { get => Enemies.Select(x => (byte)(x - 0x10)).ToArray(); set => Enemies = value; }
            internal int ResolveCameraAnimation(byte cameraPointerValue) => cameraPointerValue & 0b1111;
            internal int ResolveCameraSet(byte cameraPointerValue) => (cameraPointerValue >> 4) & 0b1111;
        }

        internal struct EnemyCoordinates
        {
            internal Coordinate cEnemy1;
            internal Coordinate cEnemy2;
            internal Coordinate cEnemy3;
            internal Coordinate cEnemy4;
            internal Coordinate cEnemy5;
            internal Coordinate cEnemy6;
            internal Coordinate cEnemy7;
            internal Coordinate cEnemy8;

            internal Coordinate GetEnemyCoordinateByIndex(byte index)
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

        internal struct Coordinate
        {
            internal short x;
            internal short y;
            internal short z;

            internal Vector3 GetVector() => new Vector3(
                x /100,
                y /100 ,
                -z /100 );
        }





        internal static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] test = aw.GetListOfFiles();
            string sEncounter = test.First(x => x.ToLower().Contains("scene.out"));
            byte[] sceneOut = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, sEncounter);
            ReadEncounter(sceneOut);
        }

        private static void ReadEncounter(byte[] enc)
        {
            int encounterCount = enc.Length / 128;
            Memory.encounters = new Encounter[encounterCount];

            using (MemoryStream ms = new MemoryStream(enc))
            using (BinaryReader br = new BinaryReader(ms))
                for (int i = 0; i < encounterCount; i++)
                    Memory.encounters[i] = new Encounter()
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
                    };
        }
    }
}