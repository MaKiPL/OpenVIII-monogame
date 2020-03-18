using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using OpenVIII.Battle;
using OpenVIII.Battle.Dat;
using OpenVIII.Kernel;
using Abilities = OpenVIII.Battle.Dat.Abilities;

namespace OpenVIII.Dat_Dump
{
    internal static class DumpMonsterAndCharacterDat
    {
        #region Fields

        public static ConcurrentDictionary<int, DatFile> MonsterData = new ConcurrentDictionary<int, DatFile>();
        private static readonly ConcurrentDictionary<int, DatFile> CharacterData = new ConcurrentDictionary<int, DatFile>();

        #endregion Fields

        #region Properties

        private static string Ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        #endregion Properties

        #region Methods

        public static async Task LoadMonsters()
        {
            if (!MonsterData.IsEmpty) return;
            //one issue with this is animations aren't loaded. because it requires all the geometry and skeleton loaded...
            // so the sequence dump is probably less useful or broken.
            Task<bool> addMonster(int i)
            => Task.Run(() => MonsterData.TryAdd(i,
                        DatFile.Load(i, EntityType.Monster,
                            flags: Sections.AnimationSequences | Sections.Information)));

            await Task.WhenAll(Enumerable.Range(0, 200).Select(addMonster));
        }

        public static async Task Process()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t", // note: default is two spaces
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false
            };
            using (StreamWriter csv2File = new StreamWriter(new FileStream("MonsterAttacks.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                using (StreamWriter csvFile = new StreamWriter(new FileStream("SequenceDump.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
                {
                    await LoadMonsters();
                    //header for monster attacks
                    csv2File.WriteLine($"{nameof(Enemy)}{Ls}" +
                        $"{nameof(Enemy.EII.Data.FileName)}{Ls}" +
                        $"{nameof(Abilities)}{Ls}" +
                        $"Number{Ls}" +
                        $"{nameof(Abilities.Animation)}{Ls}" +
                        $"Type{Ls}" +
                        $"BattleID{Ls}" +
                        $"Name{Ls}");
                    //header for animation info
                    csvFile.WriteLine($"Type{Ls}Type BattleID{Ls}Name{Ls}Animation Count{Ls}Sequence Count{Ls}Sequence BattleID{Ls}Offset{Ls}Bytes");
                    using (XmlWriter xmlWriter = XmlWriter.Create("SequenceDump.xml", xmlWriterSettings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("dat");

                        XmlMonsterData(xmlWriter, csvFile, csv2File);
                        XmlCharacterData(xmlWriter, csvFile);

                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                    }
                }
            }

            Console.Write("Press [Enter] key to continue...  ");
            Console.ReadLine();
        }

        private static string XmlAnimations(XmlWriter xmlWriter, DatFile battleDatFile)
        {
            string count = $"{battleDatFile.Animations.Count}";
            xmlWriter.WriteStartElement("animations");
            xmlWriter.WriteAttributeString("Count", count);
            xmlWriter.WriteEndElement();
            return count;
        }

        private static void XmlCharacterData(XmlWriter xmlWriter, TextWriter csvFile)
        {
            xmlWriter.WriteStartElement("characters");
            for (int i = 0; i <= 10; i++)
            {
                DatFile test = DatFile.Load(i, EntityType.Character, 0);
                if (test != null && CharacterData.TryAdd(i, test))
                {
                }

                if (!CharacterData.TryGetValue(i, out DatFile battleDat) || battleDat == null) continue;
                const string type = "character";
                xmlWriter.WriteStartElement(type);
                string id = i.ToString();
                xmlWriter.WriteAttributeString("BattleID", id);
                FF8String name = Memory.Strings.GetName((Characters)i);
                xmlWriter.WriteAttributeString("name", name);
                string prefix0 = $"{type}{Ls}{id}{Ls}";
                string prefix1 = $"{name}";
                prefix1 += $"{Ls}{XmlAnimations(xmlWriter, battleDat)}";
                XmlSequences(xmlWriter, battleDat, csvFile, $"{prefix0}{prefix1}");
                XmlWeaponData(xmlWriter, i, ref battleDat, csvFile, prefix1);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        private static void XmlMonsterData(XmlWriter xmlWriter, StreamWriter csvFile, TextWriter csv2File)
        {
            xmlWriter.WriteStartElement("monsters");
            for (int i = 0; i <= 200; i++)
            {
                if (!MonsterData.TryGetValue(i, out DatFile battleDat) || battleDat == null) continue;
                const string type = "monster";
                string id = i.ToString();
                FF8String name = battleDat.Information.Name ?? new FF8String("");
                string prefix = $"{type}{Ls}{id}{Ls}{name}";
                xmlWriter.WriteStartElement(type);
                xmlWriter.WriteAttributeString("BattleID", id);
                xmlWriter.WriteAttributeString("name", name);
                prefix += $"{Ls}{XmlAnimations(xmlWriter, battleDat)}";
                XmlSequences(xmlWriter, battleDat, csvFile, prefix);
                xmlWriter.WriteEndElement();
                Enemy e = Enemy.Load(new EnemyInstanceInformation { Data = battleDat });
                void addAbility(string fieldName, Abilities a, int number)
                {
                    csv2File.WriteLine($"{name}{Ls}" +
                                       $"{battleDat.FileName}{Ls}" +
                                       $"{fieldName}{Ls}" +
                                       $"{number}{Ls}" +
                                       $"{a.Animation}{Ls}" +
                                       $"{(a.Item != null ? nameof(a.Item) : a.Magic != null ? nameof(a.Magic) : a.Monster != null ? nameof(a.Monster) : "")}{Ls}" +
                                       $"{a.Item?.ID ?? (a.Magic?.MagicDataID ?? (a.Monster?.EnemyAttackID ?? 0))}{Ls}" +
                                       $"\"{(a.Item != null ? a.Item.Value.Name : a.Magic != null ? a.Magic.Name : a.Monster != null ? a.Monster.Name : new FF8String(""))}\"{Ls}");
                }
                void addAbilities(string fieldName, IReadOnlyList<Abilities> abilities)
                {
                    if (abilities == null) return;
                    for (int number = 0; number < e.Info.AbilitiesLow.Length; number++)
                    {
                        Abilities a = abilities[number];
                        addAbility(fieldName, a, number);
                    }
                }
                addAbilities(nameof(e.Info.AbilitiesLow), e.Info.AbilitiesLow);
                addAbilities(nameof(e.Info.AbilitiesMed), e.Info.AbilitiesMed);
                addAbilities(nameof(e.Info.AbilitiesHigh), e.Info.AbilitiesHigh);
            }
            xmlWriter.WriteEndElement();
        }

        private static void XmlSequences(XmlWriter xmlWriter, DatFile battleDatFile, TextWriter csvFile, string prefix)
        {
            xmlWriter.WriteStartElement("sequences");
            string count = $"{battleDatFile.Sequences?.Count ?? 0}";
            xmlWriter.WriteAttributeString("Count", count);
            if (battleDatFile.Sequences != null)
                foreach (AnimationSequence s in battleDatFile.Sequences)
                {
                    xmlWriter.WriteStartElement("sequence");
                    string id = s.ID.ToString();
                    string offset = s.Offset.ToString("X");
                    string bytes = s.Count.ToString();

                    xmlWriter.WriteAttributeString("BattleID", id);
                    xmlWriter.WriteAttributeString("offset", offset);
                    xmlWriter.WriteAttributeString("bytes", bytes);

                    csvFile?.Write($"{prefix ?? ""}{Ls}{count}{Ls}{id}{Ls}{s.Offset}{Ls}{bytes}");
                    foreach (byte b in s)
                    {
                        xmlWriter.WriteString($"{b:X2} ");
                        csvFile?.Write($"{Ls}{b}");
                    }
                    csvFile?.Write(Environment.NewLine);
                    xmlWriter.WriteEndElement();
                }
            csvFile?.Flush();
            xmlWriter.WriteEndElement();
        }

        private static void XmlWeaponData(XmlWriter xmlWriter, int characterID, ref DatFile r, TextWriter csvFile, string prefix1)
        {
            ConcurrentDictionary<int, DatFile> weaponData = new ConcurrentDictionary<int, DatFile>();
            xmlWriter.WriteStartElement("weapons");
            for (int i = 0; i <= 40; i++)
            {
                DatFile test;
                if (characterID == 1 || characterID == 9)
                    test = DatFile.Load(characterID, EntityType.Weapon, i, r);
                else
                    test = DatFile.Load(characterID, EntityType.Weapon, i);
                if (test != null && weaponData.TryAdd(i, test))
                {
                }

                if (!weaponData.TryGetValue(i, out DatFile battleDat) || battleDat == null) continue;
                const string type = "weapon";
                string id = i.ToString();
                xmlWriter.WriteStartElement(type);
                xmlWriter.WriteAttributeString("BattleID", id);
                int index = ModuleBattleDebug.Weapons[(Characters)characterID].FindIndex(v => v == i);
                WeaponsData currentWeaponData = Memory.Kernel_Bin.WeaponsData.FirstOrDefault(v => v.Character == (Characters)characterID &&
                                                                                                  v.AltID == index);

                if (currentWeaponData != default)
                {
                    xmlWriter.WriteAttributeString("name", currentWeaponData.Name);

                    string prefix = $"{type}{Ls}{id}{Ls}{currentWeaponData.Name}/{prefix1}"; //bringing over name from character.
                    //xmlWriter.WriteAttributeString("name", Memory.Strings.GetName((Characters)i));

                    XmlAnimations(xmlWriter, battleDat);
                    XmlSequences(xmlWriter, battleDat, csvFile, prefix);
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        #endregion Methods
    }
}