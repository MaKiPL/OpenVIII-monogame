using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace OpenVIII.Dat_Dump
{
    internal static class DumpMonsterAndCharacterDat
    {
        #region Fields

        public static ConcurrentDictionary<int, Debug_battleDat> MonsterData = new ConcurrentDictionary<int, Debug_battleDat>();
        private static ConcurrentDictionary<int, Debug_battleDat> CharacterData = new ConcurrentDictionary<int, Debug_battleDat>();

        #endregion Fields

        #region Properties

        private static string ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        #endregion Properties

        #region Methods

        public static void LoadMonsters()
        {
            for (int i = 0; i <= 200; i++)
            {
                //one issue with this is animations aren't loaded. because it requires all the geometry and skeleton loaded...
                // so the sequence dump is probably less useful or broken.
                MonsterData.TryAdd(i, Debug_battleDat.Load(i, Debug_battleDat.EntityType.Monster, flags: Sections.Animation_Sequences | Sections.Information));
            }
        }

        public static void Process()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t", // note: default is two spaces
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false,
            };
            using (StreamWriter csv2File = new StreamWriter(new FileStream("MonsterAttacks.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                using (StreamWriter csvFile = new StreamWriter(new FileStream("SequenceDump.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
                {
                    LoadMonsters();
                    //header for monster attacks
                    csv2File.WriteLine($"{nameof(Enemy)}{ls}" +
                        $"{nameof(Enemy.EII.Data.fileName)}{ls}" +
                        $"{nameof(Debug_battleDat.Abilities)}{ls}" +
                        $"Number{ls}" +
                        $"{nameof(Debug_battleDat.Abilities.animation)}{ls}" +
                        $"Type{ls}" +
                        $"BattleID{ls}" +
                        $"Name{ls}");
                    //header for animation info
                    csvFile.WriteLine($"Type{ls}Type BattleID{ls}Name{ls}Animation Count{ls}Sequence Count{ls}Sequence BattleID{ls}Offset{ls}Bytes");
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
            FF8String sval = Console.ReadLine().Trim((Environment.NewLine + " _").ToCharArray());
        }

        private static string XMLAnimations(XmlWriter xmlWriter, Debug_battleDat _BattleDat)
        {
            string count = $"{_BattleDat.animHeader.animations?.Length ?? 0}";
            xmlWriter.WriteStartElement("animations");
            xmlWriter.WriteAttributeString("Count", count);
            xmlWriter.WriteEndElement();
            return count;
        }

        private static void XmlCharacterData(XmlWriter xmlWriter, StreamWriter csvFile)
        {
            xmlWriter.WriteStartElement("characters");
            for (int i = 0; i <= 10; i++)
            {
                Debug_battleDat test = Debug_battleDat.Load(i, Debug_battleDat.EntityType.Character, 0);
                if (test != null && CharacterData.TryAdd(i, test))
                {
                }
                if (CharacterData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {
                    const string type = "character";
                    xmlWriter.WriteStartElement(type);
                    string id = i.ToString();
                    xmlWriter.WriteAttributeString("BattleID", id);
                    FF8String name = Memory.Strings.GetName((Characters)i);
                    xmlWriter.WriteAttributeString("name", name);
                    string prefix0 = $"{type}{ls}{id}{ls}";
                    string prefix1 = $"{name}";
                    prefix1 += $"{ls}{XMLAnimations(xmlWriter, _BattleDat)}";
                    XMLSequences(xmlWriter, _BattleDat, csvFile, $"{prefix0}{prefix1}");
                    XmlWeaponData(xmlWriter, i, ref _BattleDat, csvFile, prefix1);
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        private static void XmlMonsterData(XmlWriter xmlWriter, StreamWriter csvFile, StreamWriter csv2File)
        {
            xmlWriter.WriteStartElement("monsters");
            for (int i = 0; i <= 200; i++)
            {
                if (MonsterData.TryGetValue(i, out Debug_battleDat _BattleDat) && _BattleDat != null)
                {
                    const string type = "monster";
                    string id = i.ToString();
                    FF8String name = _BattleDat.information.name ?? new FF8String("");
                    string prefix = $"{type}{ls}{id}{ls}{name}";
                    xmlWriter.WriteStartElement(type);
                    xmlWriter.WriteAttributeString("BattleID", id);
                    xmlWriter.WriteAttributeString("name", name);
                    prefix += $"{ls}{XMLAnimations(xmlWriter, _BattleDat)}";
                    XMLSequences(xmlWriter, _BattleDat, csvFile, prefix);
                    xmlWriter.WriteEndElement();
                    Enemy e = Enemy.Load(new Battle.EnemyInstanceInformation { Data = _BattleDat });
                    void addability(string fieldname, Debug_battleDat.Abilities a, int number)
                    {
                        csv2File.WriteLine($"{name}{ls}" +
                            $"{_BattleDat.fileName}{ls}" +
                            $"{fieldname}{ls}" +
                            $"{number}{ls}" +
                            $"{a.animation}{ls}" +
                            $"{(a.ITEM != null ? nameof(a.ITEM) : a.MAGIC != null ? nameof(a.MAGIC) : a.MONSTER != null ? nameof(a.MONSTER) : "")}{ls}" +
                            $"{(a.ITEM != null ? a.ITEM.Value.ID : a.MAGIC != null ? a.MAGIC.ID : a.MONSTER != null ? a.MONSTER.ID : 0)}{ls}" +
                        $"\"{(a.ITEM != null ? a.ITEM.Value.Name : a.MAGIC != null ? a.MAGIC.Name : a.MONSTER != null ? a.MONSTER.Name : new FF8String(""))}\"{ls}");
                    }
                    void addabilities(string fieldname, Debug_battleDat.Abilities[] abilites)
                    {
                        if (abilites != null)
                            for (int number = 0; number < e.Info.abilitiesLow.Length; number++)
                            {
                                Debug_battleDat.Abilities a = abilites[number];
                                addability(fieldname, a, number);
                            }
                    }
                    addabilities(nameof(e.Info.abilitiesLow), e.Info.abilitiesLow);
                    addabilities(nameof(e.Info.abilitiesMed), e.Info.abilitiesMed);
                    addabilities(nameof(e.Info.abilitiesHigh), e.Info.abilitiesHigh);
                }
            }
            xmlWriter.WriteEndElement();
        }

        private static void XMLSequences(XmlWriter xmlWriter, Debug_battleDat _BattleDat, StreamWriter csvFile, string prefix)
        {
            xmlWriter.WriteStartElement("sequences");
            string count = $"{_BattleDat.Sequences?.Count ?? 0}";
            xmlWriter.WriteAttributeString("Count", count);
            if (_BattleDat.Sequences != null)
                foreach (Debug_battleDat.AnimationSequence s in _BattleDat.Sequences)
                {
                    xmlWriter.WriteStartElement("sequence");
                    string id = s.id.ToString();
                    string offset = s.offset.ToString("X");
                    string bytes = s.data.Length.ToString();

                    xmlWriter.WriteAttributeString("BattleID", id);
                    xmlWriter.WriteAttributeString("offset", offset);
                    xmlWriter.WriteAttributeString("bytes", bytes);

                    csvFile?.Write($"{prefix ?? ""}{ls}{count}{ls}{id}{ls}{s.offset}{ls}{bytes}");
                    foreach (byte b in s.data)
                    {
                        xmlWriter.WriteString($"{b.ToString("X2")} ");
                        csvFile?.Write($"{ls}{b}");
                    }
                    csvFile?.Write(Environment.NewLine);
                    xmlWriter.WriteEndElement();
                }
            csvFile?.Flush();
            xmlWriter.WriteEndElement();
        }

        private static void XmlWeaponData(XmlWriter xmlWriter, int character_id, ref Debug_battleDat r, StreamWriter csvFile, string prefix1)
        {
            ConcurrentDictionary<int, Debug_battleDat> WeaponData = new ConcurrentDictionary<int, Debug_battleDat>();
            xmlWriter.WriteStartElement("weapons");
            for (int i = 0; i <= 40; i++)
            {
                Debug_battleDat test;
                if (character_id == 1 || character_id == 9)
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon, i, r);
                else
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon, i);
                if (test != null && WeaponData.TryAdd(i, test))
                {
                }
                if (WeaponData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {
                    const string type = "weapon";
                    string id = i.ToString();
                    xmlWriter.WriteStartElement(type);
                    xmlWriter.WriteAttributeString("BattleID", id);
                    int index = Module_battle_debug.Weapons[(Characters)character_id].FindIndex(v => v == i);
                    Kernel.Weapons_Data weapondata = Memory.Kernel_Bin.WeaponsData.FirstOrDefault(v => v.Character == (Characters)character_id &&
                    v.AltID == index);

                    if (weapondata != default)
                    {
                        xmlWriter.WriteAttributeString("name", weapondata.Name);

                        string prefix = $"{type}{ls}{id}{ls}{weapondata.Name}/{prefix1}"; //bringing over name from character.
                                                                                          //xmlWriter.WriteAttributeString("name", Memory.Strings.GetName((Characters)i));

                        XMLAnimations(xmlWriter, _BattleDat);
                        XMLSequences(xmlWriter, _BattleDat, csvFile, prefix);
                    }
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        #endregion Methods
    }
}