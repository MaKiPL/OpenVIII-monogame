using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.Fields
{
    public class Archive
    {
        #region Constructors

        public Archive()
        {
        }

        #endregion Constructors

        #region Properties

        public Background Background { get; set; }
        public Cameras Cameras { get; set; }
        public EventEngine EventEngine { get; set; }
        public Sections Flags { get; set; }
        public INF inf { get; set; }
        public Field_modes Mod { get; set; } = 0;
        public MrtRat MrtRat { get; set; }
        public MSK msk { get; set; }
        public PMP pmp { get; set; }
        public IServices services { get; set; }
        public SFX sfx { get; set; }
        public TDW tdw { get; set; }
        public WalkMesh WalkMesh { get; set; }
        public ushort ID { get; set; }
        public string FileName { get; set; }
        public string ArchiveName { get; set; }
        public List<Scripts.Jsm.GameObject> jsmObjects { get; set; } = null;

        #endregion Properties

        #region Methods

        public override string ToString() => $"{{{ID}, {FileName}, {Mod}}}";

        public static Archive Load(ushort inputFieldID, Sections flags = Sections.ALL)
        {
            Archive r = new Archive();
            if (!r.Init(inputFieldID, flags))
                return null;
            return r;
        }

        public HashSet<ushort> GetForcedBattleEncounters() => jsmObjects != null && jsmObjects.Count > 0 ?
            (
             from jsmObject in jsmObjects
             from Script in jsmObject.Scripts
             from Instruction in Script.Segment.Flatten()
             where Instruction is BATTLE
             let battle = ((BATTLE)Instruction)
             select battle.Encounter).ToHashSet() : null;

        public HashSet<FF8String> GetAreaNames() => jsmObjects != null && jsmObjects.Count > 0 ?
            (
             from jsmObject in jsmObjects
             from Script in jsmObject.Scripts
             from Instruction in Script.Segment.Flatten()
             where Instruction is SETPLACE
             let setplace = ((SETPLACE)Instruction)
             select setplace.AreaName()).ToHashSet() : null;

        public void Draw()
        {
            switch (Mod)
            {
                case Field_modes.INIT:
                    break; //null
                default:
                    Background.Draw();
                    break;

                case Field_modes.DISABLED:
                    break;
            }
        }

        public bool Init(ushort? inputFieldID = null, Sections flags = Sections.ALL)
        {
            Flags = flags;
            Memory.SuppressDraw = true;
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();
            //TODO fix endless look on FieldID 50.
            ID = inputFieldID ?? Memory.FieldHolder.FieldID;
            int count = (Memory.FieldHolder.fields?.Length ?? 0);
            if (ID >= count ||
                ID < 0)
                return false;
            FileName = Memory.FieldHolder.GetString(ID);
            ArchiveName = test.FirstOrDefault(x => x.IndexOf(FileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(ArchiveName))
            {
                Debug.WriteLine($"FileNotFound :: {ID} - {FileName.ToUpper()}");
                Mod = Field_modes.DISABLED;
                return false;
            }

            ArchiveBase fieldArchive = aw.GetArchive(ArchiveName);
            string[] listOfFiles = fieldArchive.GetListOfFiles();
            string findString(string s) =>
                listOfFiles.FirstOrDefault(x => x.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

            byte[] getFile(string s)
            {
                s = findString(s);
                return !string.IsNullOrWhiteSpace(s) ? fieldArchive.GetBinaryFile(s) : null;
            }
            if (!flags.HasFlag(Sections.MIM | Sections.MAP) ||
                (Background = Background.Load(getFile(".mim"), getFile(".map"))) == null)
            {
                Mod = Field_modes.DISABLED;
            }
            if (flags.HasFlag(Sections.CA | Sections.ID))
            {
                Cameras = Cameras.Load(getFile(".ca"));
                WalkMesh = WalkMesh.Load(getFile(".BattleID"), Cameras);
            }

            //let's start with scripts
            string sJsm = findString(".jsm");
            string sSy = findString(".sy");
            if (flags.HasFlag(Sections.JSM | Sections.SYM) && !string.IsNullOrWhiteSpace(sJsm)&& (FileName != "test3"))
            {
                    jsmObjects = Scripts.Jsm.File.Read(fieldArchive.GetBinaryFile(sJsm));
             
                if (Mod != Field_modes.NOJSM)
                {
                    if (!string.IsNullOrWhiteSpace(sSy))
                    {
                        Sym.GameObjects symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(sSy));

                        services = Initializer.GetServices();
                        EventEngine = ServiceId.Field[services].Engine;
                        EventEngine.Reset();
                        for (int objIndex = 0; objIndex < jsmObjects.Count; objIndex++)
                        {
                            Scripts.Jsm.GameObject obj = jsmObjects[objIndex];
                            FieldObject fieldObject = new FieldObject(obj.Id, symObjects.GetObjectOrDefault(objIndex).Name);

                            foreach (Scripts.Jsm.GameScript script in obj.Scripts)
                                fieldObject.Scripts.Add(script.ScriptId, script.Segment.GetExecuter());

                            EventEngine.RegisterObject(fieldObject);
                        }

                        Mod++;
                    }
                    else
                    {
                        Debug.WriteLine($"FileNotFound :: {FileName.ToUpper()}.sy");
                        //sy file might be optional.
                        //Mod = Field_modes.NOJSM;
                    }
                }
            }
            else
            {
                Mod = Field_modes.NOJSM;
                //goto end;
            }

            //if (flags.HasFlag(Sections.MCH))
            //{
            //    byte[] mchb = getfile(".mch");//Field character models
            //}

            //if (flags.HasFlag(Sections.ONE))
            //{
            //    byte[] oneb = getfile(".one");//Field character models container
            //}
            //if (flags.HasFlag(Sections.MSD))
            //{
            //    byte[] msdb = getfile(".msd");//dialogs
            //}
            if (flags.HasFlag(Sections.INF))
            {
                byte[] infData = getFile(".inf");//gateways
                if (infData != null && infData.Length > 0)
                    inf = INF.Load(infData);
            }

            if (flags.HasFlag(Sections.TDW))
            {
                byte[] tdwData = getFile(".tdw");//extra font
                if (tdwData != null && tdwData.Length > 0)
                    tdw = new TDW(tdwData);
            }

            if (flags.HasFlag(Sections.MSK))
            {
                byte[] mskData = getFile(".msk");//movie cam
                if (mskData != null && mskData.Length > 0)
                    msk = new MSK(mskData);
            }
            if (flags.HasFlag(Sections.RAT | Sections.MRT))
            {
                byte[] ratData = getFile(".rat");//battle on field
                byte[] mrtData = getFile(".mrt");//battle on field
                if (ratData != null && mrtData != null && ratData.Length > 0 && mrtData.Length > 0)
                    MrtRat = new MrtRat(mrtData, ratData);
            }
            //if (flags.HasFlag(Sections.PMD))
            //{
            //    byte[] pmdb = getfile(".pmd");//particle info
            //    if (pmdb != null && pmdb.Length > 4)
            //        using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(),
            //            $"{Memory.FieldHolder.GetString()}.pmd"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            //            fs.Write(pmdb, 0, pmdb.Length);
            //}
            if (flags.HasFlag(Sections.PMP))
            {
                byte[] pmpData = getFile(".pmp");//particle graphic?
                if (pmpData != null && pmpData.Length > 4)
                    pmp = new PMP(pmpData);
            }
            if (flags.HasFlag(Sections.SFX))
            {
                byte[] sfxData = getFile(".sfx");//sound effects
                if (sfxData != null && sfxData.Length > 0)
                    sfx = new SFX(sfxData);
            }

            if (Mod == Field_modes.NOJSM && Background == null)
            {
                Mod = Field_modes.DISABLED;
            }
            return sfx != null || pmp != null || MrtRat != null || msk != null || tdw != null || inf != null ||
                   jsmObjects != null || EventEngine != null || Cameras != null || WalkMesh != null ||
                   Background != null || services != null;
        }

        public void Update()
        {
            switch (Mod)
            {
                case Field_modes.INIT:
                    break;

                case Field_modes.DEBUGRENDER:
                    UpdateScript();
                    Background?.Update();
                    break; //await events here
                case Field_modes.NOJSM://no scripts but has background.
                    Background?.Update();
                    break; //await events here
                case Field_modes.DISABLED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void UpdateScript()
        {
            //We do not know every instruction and it's not possible for now to play field with unknown instruction
            //eventEngine.Update(services);
        }

        #endregion Methods
    }
}