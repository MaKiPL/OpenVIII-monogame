using OpenVIII.Fields.Scripts;
using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenVIII.Fields.Scripts.Jsm;

namespace OpenVIII.Dat_Dump
{
    internal class DumpEncounterInfo
    {
        #region Fields

        public static ConcurrentDictionary<int, Fields.Archive> FieldData;
        private static HashSet<KeyValuePair<string, ushort>> FieldsWithBattleScripts;
        private static HashSet<ushort> WorldEncounters;

        #endregion Fields

        #region Properties

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

        #endregion Properties

        #region Methods

        internal static void Process()
        {
            LoadWorld();
            LoadFields();
            if (DumpMonsterAndCharacterDat.MonsterData?.IsEmpty ?? true)
                DumpMonsterAndCharacterDat.LoadMonsters(); //load all the monsters.
            using (StreamWriter csvFile = new StreamWriter(new FileStream("BattleEncounters.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                string Header =
                $"{nameof(Battle.Encounter.ID)}{ls}" +
                $"{nameof(Battle.Encounter.Filename)}{ls}" +
                $"{nameof(BattleStageNames)}{ls}" +
                $"{nameof(Battle.Encounter.BEnemies)}{ls}" +
                $"{nameof(Fields)}{ls}";
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
                    enemies = enemies.TrimEnd(ls[0], ' ') + "\"";
                    Data += $"{enemies}{ls}";
                    //check encounters in fields and confirm encounter rate is above 0.
                    IEnumerable<string> fieldmatchs = FieldData.Where(x => x.Value.MrtRat != null && (x.Value.MrtRat.Any(y => y.Key == e.ID && y.Value > 0))).Select(x => x.Value.FileName);
                    IEnumerable<string> second = FieldsWithBattleScripts.Where(x => x.Value == e.ID).Select(x => x.Key);
                    if (second.Any())
                    {
                        if (fieldmatchs.Any())
                            fieldmatchs.Concat(second).Distinct();
                        else
                            fieldmatchs = second;
                    }
                    if (fieldmatchs.Any())
                        Data += $"\"{string.Join($"{ls} ", fieldmatchs).TrimEnd(ls[0], ' ')}\"{ls}";
                    else if (WorldEncounters.Any(x => x == e.ID))
                    {
                        Data += $"WorldMap{ls}";
                    }
                    else
                        Data += ls;
                    csvFile.WriteLine(Data);
                }
            }
        }

        private static void LoadFields()
        {
            if (FieldData == null)
            {
                FieldData = new ConcurrentDictionary<int, Fields.Archive>();

                Task[] tasks = new Task[Memory.FieldHolder.fields.Length];
                foreach (ushort j in Enumerable.Range(0, Memory.FieldHolder.fields.Length))
                {
                    void process(ushort i)
                    {
                        if (!FieldData.ContainsKey(i))
                        {
                            Fields.Archive archive = Fields.Archive.Load(i, Fields.Sections.MRT | Fields.Sections.RAT | Fields.Sections.JSM | Fields.Sections.SYM);

                            if (archive != null)
                                FieldData.TryAdd(i, archive);
                        }
                    }
                    tasks[j] = (Task.Run(() => process(j)));
                }
                Task.WaitAll(tasks);
            }
            //https://stackoverflow.com/questions/22988115/selectmany-to-flatten-a-nested-structure
            //Func<IJsmInstruction, IEnumerable<IJsmInstruction>> flattener = null;
            //flattener = n => new[] { n }
            //    .Concat(n == null
            //        ? Enumerable.Empty<JsmInstruction>()
            //        : n.SelectMany(flattener));
            IEnumerable<IJsmInstruction> flatten(IJsmInstruction e)
            {
                bool checktype(IJsmInstruction inst)
                {
                    return
                        typeof(Control.While.WhileSegment) == inst.GetType() ||
                    typeof(Control.If.ElseIfSegment) == inst.GetType() ||
                    typeof(Control.If.ElseSegment) == inst.GetType() ||
                    typeof(Control.If.IfSegment) == inst.GetType() ||
                    typeof(Jsm.ExecutableSegment) == inst.GetType();
                }

                if (checktype(e))
                {
                    ExecutableSegment es = ((ExecutableSegment)e);
                    foreach (ExecutableSegment child in es.Where(x => checktype(x)).Select(x => (ExecutableSegment)x))
                    {
                        foreach (IJsmInstruction i in flatten(child))
                            yield return i;
                    }
                    foreach (IJsmInstruction child in es.Where(x => !checktype(x)))
                    {
                        yield return child;
                    }
                }
                else
                    yield return e;
            }
            FieldsWithBattleScripts =
            (from FieldArchive in FieldData
             where FieldArchive.Value.jsmObjects != null && FieldArchive.Value.jsmObjects.Count > 0
             from jsmObject in FieldArchive.Value.jsmObjects
             from Script in jsmObject.Scripts
             from Instruction in flatten(Script.Segment)
             where Instruction.GetType() == typeof(Fields.Scripts.Instructions.BATTLE)
             select (new KeyValuePair<string, ushort>(FieldArchive.Value.FileName, ((Fields.Scripts.Instructions.BATTLE)Instruction).Encounter))).ToHashSet();
        }

        private static void LoadWorld()
        {
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_WORLD);
            ArchiveBase awMain = ArchiveWorker.Load(Memory.Archives.A_MAIN);

            //string wmxPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("wmx.obj")).Select(x => x).First();
            //string texlPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("texl.obj")).Select(x => x).First();
            string wmPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains($"wmset{Extended.GetLanguageShort(true)}.obj")).Select(x => x).First();
            //string charaOne = aw.GetListOfFiles().Where(x => x.ToLower().Contains("chara.one")).Select(x => x).First();
            //string railFile = aw.GetListOfFiles().Where(x => x.ToLower().Contains("rail.obj")).Select(x => x).First();

            //wmx = aw.GetBinaryFile(wmxPath);
            //texl = new texl(aw.GetBinaryFile(texlPath));
            //chara = new CharaOne(aw.GetBinaryFile(charaOne));
            using (World.Wmset Wmset = new World.Wmset(aw.GetBinaryFile(wmPath)))
            {
                WorldEncounters = Wmset.Encounters.SelectMany(x => x.Select(y => y)).Distinct().ToHashSet();
            }
            //rail = new rail(aw.GetBinaryFile(railFile));
        }

        #endregion Methods
    }
}