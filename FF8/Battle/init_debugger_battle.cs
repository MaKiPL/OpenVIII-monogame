using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal static class Init_debugger_battle
    {
        public struct Encounter
        {
            public byte bScenario;
            public byte bSwitch;
            public byte bCamera;
            public byte bUnk;
            public byte bVisibleEnemy;
            public byte bLoadedEnemy;
            public byte bTargetableEnemy;
            public byte bNumOfEnemies;
            public EnemyCoordinates enemyCoordinates;
            private byte[] bEnemies; //sizeof 8
            public byte[] bUnk2; //sizeof 16*3+8
            public byte[] bLevels; //sizeof 8

            public byte[] BEnemies { get => bEnemies.Select(x => (byte)(x - 0x10)).ToArray(); set => bEnemies = value; }
        }

        internal struct EnemyCoordinates
        {
            public Coordinate cEnemy1;
            public Coordinate cEnemy2;
            public Coordinate cEnemy3;
            public Coordinate cEnemy4;
            public Coordinate cEnemy5;
            public Coordinate cEnemy6;
            public Coordinate cEnemy7;
            public Coordinate cEnemy8;
        }

        internal struct Coordinate
        {
            public short x;
            public short y;
            public short z;
        }





        internal static void DEBUG()
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
                        bScenario = br.ReadByte(),
                        bSwitch = br.ReadByte(),
                        bCamera = br.ReadByte(),
                        bUnk = br.ReadByte(),
                        bVisibleEnemy = br.ReadByte(),
                        bLoadedEnemy = br.ReadByte(),
                        bTargetableEnemy = br.ReadByte(),
                        bNumOfEnemies = br.ReadByte(),
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