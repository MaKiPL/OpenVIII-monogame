using System;
using System.Collections.Concurrent;
using System.Xml;

namespace OpenVIII.Dat_Dump
{
    internal class Program
    {
        #region Methods

        static ConcurrentDictionary<int, Debug_battleDat> MonsterData = new ConcurrentDictionary<int, Debug_battleDat>();
        static ConcurrentDictionary<int, Debug_battleDat> CharacterData = new ConcurrentDictionary<int, Debug_battleDat>();
        
        private static void Main(string[] args)
        {
        start:
            Memory.Init(null, null, null);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t", // note: default is two spaces
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false,
            };

            XmlWriter xmlWriter = XmlWriter.Create("test.xml", xmlWriterSettings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("dat");
            XmlMonsterData(xmlWriter);
            XmlCharacterData(xmlWriter);

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            Console.Write("Press [Enter] key to continue...  ");
            FF8String sval = Console.ReadLine().Trim((Environment.NewLine + " _").ToCharArray());
            goto start;
        }

        private static void XmlMonsterData(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("monsters");
            for (int i = 0; i < 200; i++)
            {
                MonsterData.TryAdd(i, Debug_battleDat.Load(i, Debug_battleDat.EntityType.Monster));
                if (MonsterData.TryGetValue(i, out Debug_battleDat _BattleDat) && _BattleDat != null)
                {

                    xmlWriter.WriteStartElement("monster");
                    xmlWriter.WriteAttributeString("id", i.ToString());
                    xmlWriter.WriteAttributeString("name", _BattleDat.information.name ?? new FF8String(""));
                    XMLAnimations(xmlWriter, _BattleDat);
                    XMLSequences(xmlWriter, _BattleDat);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }
        private static void XmlCharacterData(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("characters");
            for (int i = 0; i < 10; i++)
            {
                Debug_battleDat test = Debug_battleDat.Load(i, Debug_battleDat.EntityType.Character,0);
                if (test!=null && CharacterData.TryAdd(i, test))
                {
                }
                if (CharacterData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {

                    xmlWriter.WriteStartElement("character");
                    xmlWriter.WriteAttributeString("id", i.ToString());
                    xmlWriter.WriteAttributeString("name", Memory.Strings.GetName((Characters)i));
                    XMLAnimations(xmlWriter, _BattleDat);
                    XMLSequences(xmlWriter, _BattleDat);
                    XmlWeaponData(xmlWriter,i,ref _BattleDat);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }
        private static void XmlWeaponData(XmlWriter xmlWriter, int character_id, ref Debug_battleDat r)
        {
            ConcurrentDictionary<int, Debug_battleDat> WeaponData = new ConcurrentDictionary<int, Debug_battleDat>();
            xmlWriter.WriteStartElement("weapons");
            for (int i = 0; i < 40; i++)
            {
                Debug_battleDat test;
                if (character_id == 1 || character_id == 9)
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon,i,r);
                else
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon, i);
                if (test != null && WeaponData.TryAdd(i, test))
                {
                }
                if (WeaponData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {

                    xmlWriter.WriteStartElement("weapon");
                    xmlWriter.WriteAttributeString("id", i.ToString());
                    //xmlWriter.WriteAttributeString("name", Memory.Strings.GetName((Characters)i));
                    XMLAnimations(xmlWriter, _BattleDat);
                    XMLSequences(xmlWriter, _BattleDat);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }

        private static void XMLSequences(XmlWriter xmlWriter, Debug_battleDat _BattleDat)
        {
            xmlWriter.WriteStartElement("sequences");
            xmlWriter.WriteAttributeString("count", $"{_BattleDat.Sequences?.Count ?? 0}");
            if (_BattleDat.Sequences != null)
                foreach (Debug_battleDat.Section5 s in _BattleDat.Sequences)
                {
                    xmlWriter.WriteStartElement("sequence");
                    xmlWriter.WriteAttributeString("id", s.id.ToString());
                    xmlWriter.WriteAttributeString("offset", s.offset.ToString("X"));
                    xmlWriter.WriteAttributeString("bytes", s.data.Length.ToString());
                    foreach (byte b in s.data)
                        xmlWriter.WriteString($"{b.ToString("X2")} ");
                    xmlWriter.WriteEndElement();
                }
            xmlWriter.WriteEndElement();
        }

        private static void XMLAnimations(XmlWriter xmlWriter, Debug_battleDat _BattleDat)
        {
            xmlWriter.WriteStartElement("animations");
            xmlWriter.WriteAttributeString("count", $"{_BattleDat.animHeader.animations?.Length ?? 0}");
            xmlWriter.WriteEndElement();
        }

        #endregion Methods
    }
}