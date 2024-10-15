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
                        MonsterDatFile.CreateInstance(i, 
                            Sections.AnimationSequences | Sections.Information)));

            await Task.WhenAll(Enumerable.Range(0, 200).Select(addMonster));
        }

        public static async Task Process()
        {
            var xmlWriterSettings = new XmlWriterSettings
            {
                Async = true, // Enable async operations
                Indent = true,
                IndentChars = "\t", // note: default is two spaces
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false
            };
            using (var csv2File = new StreamWriter(new FileStream("MonsterAttacks.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                using (var csvFile = new StreamWriter(new FileStream("SequenceDump.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
                {
                    await LoadMonsters();
                    //header for monster attacks
                    await csv2File.WriteLineAsync($"{nameof(Enemy)}{Ls}" +
                        $"{nameof(Enemy.EII.Data.FileName)}{Ls}" +
                        $"{nameof(Abilities)}{Ls}" +
                        $"Number{Ls}" +
                        $"{nameof(Abilities.Animation)}{Ls}" +
                        $"Type{Ls}" +
                        $"ID{Ls}" +
                        $"Name{Ls}");
                    //header for animation info
                    await csvFile.WriteLineAsync($"Type{Ls}Type ID{Ls}Name{Ls}Animation Count{Ls}Sequence Count{Ls}Sequence ID{Ls}Offset{Ls}Bytes");
                    using (var xmlWriter = XmlWriter.Create("SequenceDump.xml", xmlWriterSettings))
                    {
                        await xmlWriter.WriteStartDocumentAsync();
                        await xmlWriter.WriteStartElementAsync(null, "dat", null);

                        await XmlMonsterDataAsync(xmlWriter, csvFile, csv2File);
                        await XmlCharacterDataAsync(xmlWriter, csvFile);

                        await xmlWriter.WriteEndElementAsync();
                        await xmlWriter.WriteEndDocumentAsync();
                    }
                }
            }
        }

        private static async Task<string> XmlAnimationsAsync(XmlWriter xmlWriter, DatFile battleDatFile)
        {
            var count = $"{battleDatFile.Animations.Count}";
            await xmlWriter.WriteStartElementAsync(null, "animations", null);
            await xmlWriter.WriteAttributeStringAsync(null, "Count", null, count);
            await xmlWriter.WriteEndElementAsync();
            return count;
        }

        private static async Task XmlCharacterDataAsync(XmlWriter xmlWriter, TextWriter csvFile)
        {
            await xmlWriter.WriteStartElementAsync(null, "characters", null);
            for (var i = 0; i <= 10; i++)
            {
                DatFile test = CharacterDatFile.CreateInstance(i, 0);
                if (test != null && CharacterData.TryAdd(i, test))
                {
                    // Character data added successfully
                }

                if (!CharacterData.TryGetValue(i, out var battleDat) || battleDat == null) continue;

                const string type = "character";
                await xmlWriter.WriteStartElementAsync(null, type, null);

                var id = i.ToString();
                await xmlWriter.WriteAttributeStringAsync(null, "ID", null, id);

                var name = Memory.Strings.GetName((Characters)i);
                await xmlWriter.WriteAttributeStringAsync(null, "name", null, name);

                var prefix0 = $"{type}{Ls}{id}{Ls}";
                var prefix1 = $"{name}";
                prefix1 += $"{Ls}{await XmlAnimationsAsync(xmlWriter, battleDat)}"; // Assuming XmlAnimations is async-safe

                await XmlSequencesAsync(xmlWriter, battleDat, csvFile, $"{prefix0}{prefix1}"); // Assuming XmlSequences is now async
                await XmlWeaponDataAsync(xmlWriter, i, battleDat, csvFile, prefix1); // Assuming XmlWeaponData is async

                await xmlWriter.WriteEndElementAsync(); // End of "character"
            }
            await xmlWriter.WriteEndElementAsync(); // End of "characters"
        }


        private static async Task XmlMonsterDataAsync(XmlWriter xmlWriter, StreamWriter csvFile, TextWriter csv2File)
        {
            await xmlWriter.WriteStartElementAsync(null, "monsters", null);
            for (var i = 0; i <= 200; i++)
            {
                if (!MonsterData.TryGetValue(i, out var battleDat) || battleDat == null) continue;

                const string type = "monster";
                var id = i.ToString();
                var name = battleDat.Information.Name ?? new FF8String("");
                var prefix = $"{type}{Ls}{id}{Ls}{name}";

                await xmlWriter.WriteStartElementAsync(null, type, null);
                await xmlWriter.WriteAttributeStringAsync(null, "ID", null, id);
                await xmlWriter.WriteAttributeStringAsync(null, "name", null, name);

                prefix += $"{Ls}{await XmlAnimationsAsync(xmlWriter, battleDat)}";  // Assuming XmlAnimations is async-safe

                await XmlSequencesAsync(xmlWriter, battleDat, csvFile, prefix); // Assuming XmlSequences is now async

                await xmlWriter.WriteEndElementAsync(); // End of "monster"

                var e = Enemy.Load(new EnemyInstanceInformation { Data = battleDat });

                async Task addAbilityAsync(string fieldName, Abilities a, int number)
                {
                    await csv2File.WriteLineAsync($"{name}{Ls}" +
                                                  $"{battleDat.FileName}{Ls}" +
                                                  $"{fieldName}{Ls}" +
                                                  $"{number}{Ls}" +
                                                  $"{a.Animation}{Ls}" +
                                                  $"{(a.Item != null ? nameof(a.Item) : a.Magic != null ? nameof(a.Magic) : a.Monster != null ? nameof(a.Monster) : "")}{Ls}" +
                                                  $"{a.Item?.ID ?? (a.Magic?.MagicDataID ?? (a.Monster?.EnemyAttackID ?? 0))}{Ls}" +
                                                  $"\"{(a.Item != null ? a.Item.Value.Name : a.Magic != null ? a.Magic.Name : a.Monster != null ? a.Monster.Name : new FF8String(""))}\"{Ls}");
                }

                async Task addAbilitiesAsync(string fieldName, IReadOnlyList<Abilities> abilities)
                {
                    if (abilities == null) return;
                    for (var number = 0; number < e.Info.AbilitiesLow.Length; number++)
                    {
                        var a = abilities[number];
                        await addAbilityAsync(fieldName, a, number);
                    }
                }

                await addAbilitiesAsync(nameof(e.Info.AbilitiesLow), e.Info.AbilitiesLow);
                await addAbilitiesAsync(nameof(e.Info.AbilitiesMed), e.Info.AbilitiesMed);
                await addAbilitiesAsync(nameof(e.Info.AbilitiesHigh), e.Info.AbilitiesHigh);
            }
            await xmlWriter.WriteEndElementAsync(); // End of "monsters"
        }


        private static async Task XmlSequencesAsync(XmlWriter xmlWriter, DatFile battleDatFile, TextWriter csvFile, string prefix)
        {
            await xmlWriter.WriteStartElementAsync(null, "sequences", null);
            var count = $"{battleDatFile.Sequences?.Count ?? 0}";
            await xmlWriter.WriteAttributeStringAsync(null, "Count", null, count);

            if (battleDatFile.Sequences != null)
            {
                foreach (var s in battleDatFile.Sequences)
                {
                    await xmlWriter.WriteStartElementAsync(null, "sequence", null);
                    var id = s.ID.ToString();
                    var offset = s.Offset.ToString("X");
                    var bytes = s.Count.ToString();

                    await xmlWriter.WriteAttributeStringAsync(null, "ID", null, id);
                    await xmlWriter.WriteAttributeStringAsync(null, "offset", null, offset);
                    await xmlWriter.WriteAttributeStringAsync(null, "bytes", null, bytes);

                    if (csvFile != null)
                    {
                        await csvFile.WriteAsync($"{prefix ?? ""}{Ls}{count}{Ls}{id}{Ls}{s.Offset}{Ls}{bytes}");
                    }

                    foreach (var b in s)
                    {
                        await xmlWriter.WriteStringAsync($"{b:X2} ");
                        if (csvFile != null)
                        {
                            await csvFile.WriteAsync($"{Ls}{b}");
                        }
                    }

                    if (csvFile != null)
                    {
                        await csvFile.WriteLineAsync();
                    }

                    await xmlWriter.WriteEndElementAsync(); // End of "sequence"
                }
            }

            if (csvFile != null)
            {
                await csvFile.FlushAsync(); // Ensure async flush
            }

            await xmlWriter.WriteEndElementAsync(); // End of "sequences"
        }

        private static async Task XmlWeaponDataAsync(XmlWriter xmlWriter, int characterID, DatFile r, TextWriter csvFile, string prefix1)
        {
            var weaponData = new ConcurrentDictionary<int, DatFile>();
            await xmlWriter.WriteStartElementAsync(null, "weapons", null);

            for (var i = 0; i <= 40; i++)
            {
                DatFile test;
                if (characterID == 1 || characterID == 9)
                    test = WeaponDatFile.CreateInstance(characterID, i, r);
                else
                    test = WeaponDatFile.CreateInstance(characterID, i);

                if (test != null && weaponData.TryAdd(i, test))
                {
                    // Weapon data added successfully
                }

                if (!weaponData.TryGetValue(i, out var battleDat) || battleDat == null) continue;

                const string type = "weapon";
                var id = i.ToString();
                await xmlWriter.WriteStartElementAsync(null, type, null);
                await xmlWriter.WriteAttributeStringAsync(null, "ID", null, id);

                var index = ModuleBattleDebug.Weapons[(Characters)characterID]?.Select(((b, i1) => new { i, b }))
                    .FirstOrDefault(v => v.b == i)?.i;

                if (!index.HasValue) continue;

                var currentWeaponData = Memory.KernelBin.WeaponsData.FirstOrDefault(v =>
                    v.Character == (Characters)characterID && v.AltID == checked((byte)index.Value));

                if (currentWeaponData != default)
                {
                    await xmlWriter.WriteAttributeStringAsync(null, "name", null, currentWeaponData.Name);

                    var prefix = $"{type}{Ls}{id}{Ls}{currentWeaponData.Name}/{prefix1}"; // Bringing over name from character.

                    await XmlAnimationsAsync(xmlWriter, battleDat); // Assuming XmlAnimations is now async
                    await XmlSequencesAsync(xmlWriter, battleDat, csvFile, prefix); // Assuming XmlSequences is async
                }

                await xmlWriter.WriteEndElementAsync(); // End of "weapon"
            }

            await xmlWriter.WriteEndElementAsync(); // End of "weapons"
        }


        #endregion Methods
    }
}