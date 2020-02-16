using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenVIII.Dat_Dump
{
    internal class DumpEncounterInfo
    {
        public static Fields.Archive[] FieldData;
        public static string[] BattleStageNames { get; } = new string[] {
            "Balamb Garden Quad",
            "Dollet Bridge",
            "Dollet Trasmission Tower path",
            "Dollet Transmission Tower (Top)",
            "Dollet Transmission Tower (Elevator)",
            "Dollet Transmission Tower (Elevator 2 ?)",
            "Dollet City ? (Spice Spice Shop)",
            "Balamb Garden entrance gate",
            "Balamb Garden 1st Floor Hall",
            "Balamb Garden 2nd Floor Corridor",
            "Balamb Garden(Flyng Form) Quad",
            "Balamb Garden Outer Corridor",
            "Balamb Garden Training Center (elevator zone)",
            "Balamb Garden Norg's Floor",
            "Balamb Garden Underground Levels (Tube)",
            "Balamb Garden Underground levels (falling ladder zone?)",
            "Balamb Garden Underground levels (OilBoil Zone?)",
            "Timber Pub Area",
            "Timber Maniacs square",
            "Train (Deling Presidential Vagon)",
            "Deling City Sewers",
            "Deling City (Caraway Residence secret exit path?)",
            "Balamb Garden Class Room",
            "Galbadia Garden Corridor ?",
            "Galbadia Garden Corridor 2 ?",
            "Galbadia Missile Base",
            "Deep Sea Research Center (Entrance?)",
            "Balamb Town (Balamb Hotel road)",
            "Balamb Town (Balamb Hotel Hall)",
            "? Diabolous Lair?",
            "Fire Cavern (path)",
            "Fire Cavern (Ifrit Lair)",
            "Galbadia Garden Hall",
            "Galbadia Garden Auditorium (Edea's battle?)",
            "Galbadia Garden Auditorium 2? (Edea's battle?)",
            "Galbadia Garden Corridor",
            "Galbadia Garden (Ice Hockey Field)",
            "?? Some broken wall place..Ultimecia Castle?",
            "StarField?",
            "Desert Prison? (elevator?)",
            "Desert Prison? (Floor?)",
            "Esthar City (road)",
            "Desert Prison? (Top?)",
            "Esthar City (road2 ?)",
            "Missile Base? Hangar?",
            "Missile Base? Hangar2?",
            "Missile Base? Control room?",
            "Winhill Village main square",
            "Tomb of the Unknown King (Corridor)?",
            "Esthar City (road 3 ?)",
            "Tomb of the Unknown King (Boss Fight room)?",
            "Fisherman Horizon (Road)",
            "Fisherman Horizon (Train Station Square)",
            "Desert Prison? (Floor?)",
            "Salt Lake?",
            "Ultima Weapon Stage",
            "Salt Lake 2?",
            "Esthar Road",
            "Ultimecia's Castle (bridge)",
            "Esthar (square?)",
            "Esthar (?)",
            "Esthar (cave?)",
            "Esthar (cave2?)",
            "Esthar (Centra excavation site)",
            "Esthar (Centra excavation site)",
            "Esthar (Centra excavation site)",
            "Esthar (Centra excavation site)",
            "Lunatic Pandora?",
            "Lunatic Pandora",
            "Lunatic Pandora(Adel?)",
            "(Centra excavation site)",
            "(Centra excavation site)",
            "(Centra excavation site)",
            "(Centra excavation site)",
            "? ?",
            "(Centra excavation site)",
            "Centra Ruins (Lower Level)",
            "Centra Ruins (Tower Level)",
            "Centra Ruins (Tower Level)",
            "Centra Ruins (Odin Room)",
            "Centra excavation site (Entrance)",
            "Trabia Canyon",
            "Ragnarok?",
            "Ragnarok?",
            "? Diabolous Lair?",
            "Deep Sea Research Center (Entrance)",
            "Deep Sea Research Center",
            "Deep Sea Research Center",
            "Deep Sea Research Center",
            "Deep Sea Research Center",
            "Deep Sea Research Center",
            "Deep Sea Research Center",
            "? ?",
            "? Esthar shops?",
            "Tear's Point",
            "Esthar",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Deling City (Edea's Room)",
            "Balamb Plains?",
            "Desert Canyon?",
            "Desert?",
            "Snow-Covered Plains? (Trabia Region?)",
            "Wood",
            "Snow-Covered Wood",
            "Balamb Isle? (Beach zone?)",
            "?Snow Beach?",
            "Esthar City",
            "Esthar City",
            "Generic Landscape? Dirt Ground",
            "Generic Landscape? Grass Ground",
            "Generic Landscape? Dirt Ground",
            "Generic Landscape? Snow Covered Mountains",
            "Esthar City",
            "Esthar City",
            "Generic Landscape?",
            "Esthar City",
            "Esthar City",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Generic Landscape?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Esthar City",
            "Generic Landscape?",
            "Generic Landscape? (Beach at night?)",
            "Commencement Room",
            "Ultimecia's Castle",
            "Ultimecia's Castle (Tiamat)",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Ultimecia's Castle",
            "Esthar City",
            "Lunatic Pandora Lab",
            "Lunatic Pandora Lab",
            "Edea's Parade Vehicle",
            "Tomb of the Unknown King (Boss Fight room)?",
            "Desert Prison?",
            "Galbadian something?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Balamb Garden (External Corridor?)",
            "Balamb Garden (External Corridor?)",
            "Balamb Garden (External Corridor?)",
            "Balamb Garden (External Corridor?)",
            "Balamb Garden (External Corridor?)",
            "Generic Landscape?",
            "Generic Landscape?",
            "Generic Landscape?",
            "Test Environment? (UV tile texture)",
            "Generic Landscape?",
            "Generic Landscape?" };

        private static string ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        internal static void Process()
        {
            if(FieldData == null)
            {
                FieldData = new Fields.Archive[Memory.FieldHolder.fields.Length];
                foreach (ushort i in Enumerable.Range(0, Memory.FieldHolder.fields.Length))
                {
                    FieldData[i] = Fields.Archive.Load(i);
                }
            }
            if (DumpMonsterAndCharacterDat.MonsterData?.IsEmpty ?? true)
                DumpMonsterAndCharacterDat.LoadMonsters(); //load all the monsters.
            using (StreamWriter csvFile = new StreamWriter(new FileStream("BattleEncounters.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite),System.Text.Encoding.UTF8))
            {
                string Header =
                $"{nameof(Battle.Encounter.ID)}{ls}" +
                $"{nameof(Battle.Encounter.Filename)}{ls}" +
                $"{nameof(BattleStageNames)}{ls}" +
                $"{nameof(Battle.Encounter.BEnemies)}{ls}";
                csvFile.WriteLine(Header);
                foreach (Battle.Encounter e in Memory.Encounters)
                {
                    //wip
                    string Data =
                        $"{e.ID}{ls}" +
                        $"{e.Filename}{ls}" +
                        $"\"{BattleStageNames[e.Scenario]}\"{ls}";
                    string enemies = "\"";
                    IEnumerable<byte> unique = e.UniqueMonstersList;
                    IEnumerable<KeyValuePair<byte, int>> counts = e.UniqueMonstersList.Select(x => new KeyValuePair<byte, int>(x, e.EnabledMonsters.Count(y => y.Value == x)));
                    unique.ForEach(x =>
                    {
                        string name = "<unknown>";
                        if (DumpMonsterAndCharacterDat.MonsterData.TryGetValue(x, out Debug_battleDat _BattleDat) && _BattleDat != null)
                        {
                            name = _BattleDat.information.name.Value_str.Trim();
                            name += $" ({_BattleDat.fileName})";
                        }

                        enemies += $"{counts.First(y => y.Key == x).Value} × {name}{ls} ";
                    });
                    enemies = enemies.TrimEnd() + "\"";
                    Data += $"{enemies}{ls}";
                    csvFile.WriteLine(Data);
                }
            }
        }
    }
}