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

        #endregion Constructors

        #region Properties

        public Background Background { get; set; }
        public Cameras Cameras { get; set; }
        public EventEngine EventEngine { get; set; }
        public Sections Flags { get; set; }
        public INF INF { get; set; }
        public FieldModes Mod { get; set; } = 0;
        public MrtRat MrtRat { get; set; }
        public MSK MSK { get; set; }
        public PMP PMP { get; set; }
        public IServices Services { get; set; }
        public SFX SFX { get; set; }
        public TDW TDW { get; set; }
        public WalkMesh WalkMesh { get; set; }
        public ushort ID { get; set; }
        public string FileName { get; set; }
        public string ArchiveName { get; set; }
        public List<Scripts.Jsm.GameObject> JSMObjects { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString() => $"{{{ID}, {FileName}, {Mod}}}";

        public static Archive Load(ushort inputFieldID, Sections flags = Sections.ALL)
        {
            Archive r = new Archive();
            return !r.Init(inputFieldID, flags) ? null : r;
        }

/*
        public HashSet<ushort> GetForcedBattleEncounters() => JSMObjects != null && JSMObjects.Count > 0 ?
            (
             from jsmObject in JSMObjects
             from script in jsmObject.Scripts
             from instruction in script.Segment.Flatten()
             where instruction is BATTLE
             let battle = ((BATTLE)instruction)
             select battle.Encounter).ToHashSet() : null;
*/

        public HashSet<FF8String> GetAreaNames() => JSMObjects != null && JSMObjects.Count > 0 ?
            (
             from jsmObject in JSMObjects
             from script in jsmObject.Scripts
             from instruction in script.Segment.Flatten()
             where instruction is SETPLACE
             let setPlace = ((SETPLACE)instruction)
             select setPlace.AreaName()).ToHashSet() : null;

        public void Draw()
        {
            switch (Mod)
            {
                case FieldModes.Init:
                    break;
                case FieldModes.Disabled:
                    break;
                case FieldModes.DebugRender:
                case FieldModes.NoJSM:
                    Background.Draw();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            if (ID >= count)
                return false;
            FileName = Memory.FieldHolder.GetString(ID);
            ArchiveName = test.FirstOrDefault(x => x.IndexOf(FileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(ArchiveName))
            {
                Memory.Log.WriteLine($"FileNotFound :: {ID} - {FileName.ToUpper()}");
                Mod = FieldModes.Disabled;
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
                Mod = FieldModes.Disabled;
            }
            if (flags.HasFlag(Sections.CA | Sections.ID))
            {
                Cameras = Cameras.CreateInstance(getFile(".ca"));
                WalkMesh = WalkMesh.Load(getFile(".ID"), Cameras);
            }

            //let's start with scripts
            string sJsm = findString(".jsm");
            string sSy = findString(".sy");
            if (flags.HasFlag(Sections.JSM | Sections.SYM) && !string.IsNullOrWhiteSpace(sJsm)&& (FileName != "test3"))
            {
                    JSMObjects = Scripts.Jsm.File.Read(fieldArchive.GetBinaryFile(sJsm));
             
                if (Mod != FieldModes.NoJSM)
                {
                    if (!string.IsNullOrWhiteSpace(sSy))
                    {
                        Sym.GameObjects symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(sSy));

                        Services = Initializer.GetServices();
                        EventEngine = ServiceId.Field[Services].Engine;
                        EventEngine.Reset();
                        for (int objIndex = 0; objIndex < JSMObjects.Count; objIndex++)
                        {
                            Scripts.Jsm.GameObject obj = JSMObjects[objIndex];
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
                        //Mod = Field_modes.NoJSM;
                    }
                }
            }
            else
            {
                Mod = FieldModes.NoJSM;
                //goto end;
            }

            //if (flags.HasFlag(Sections.MCH))
            //{
            //    byte[] mchB = getFile(".mch");//Field character models
            //}

            //if (flags.HasFlag(Sections.ONE))
            //{
            //    byte[] oneB = getFile(".one");//Field character models container
            //}
            //if (flags.HasFlag(Sections.MSD))
            //{
            //    byte[] msdB = getFile(".msd");//dialogs
            //}
            if (flags.HasFlag(Sections.INF))
            {
                byte[] infData = getFile(".inf");//gateways
                if (infData != null && infData.Length > 0)
                    INF = INF.Load(infData);
            }

            if (flags.HasFlag(Sections.TDW))
            {
                byte[] tdwData = getFile(".tdw");//extra font
                if (tdwData != null && tdwData.Length > 0)
                    TDW = new TDW(tdwData);
            }

            if (flags.HasFlag(Sections.MSK))
            {
                byte[] mskData = getFile(".msk");//movie cam
                if (mskData != null && mskData.Length > 0)
                    MSK = new MSK(mskData);
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
            //    byte[] pmdB = getFile(".pmd");//particle info
            //    if (pmdB != null && pmdB.Length > 4)
            //        using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(),
            //            $"{Memory.FieldHolder.GetString()}.pmd"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            //            fs.Write(pmdB, 0, pmdB.Length);
            //}
            if (flags.HasFlag(Sections.PMP))
            {
                byte[] pmpData = getFile(".pmp");//particle graphic?
                if (pmpData != null && pmpData.Length > 4)
                    PMP = new PMP(pmpData);
            }
            if (flags.HasFlag(Sections.SFX))
            {
                byte[] sfxData = getFile(".sfx");//sound effects
                if (sfxData != null && sfxData.Length > 0)
                    SFX = new SFX(sfxData);
            }

            if (Mod == FieldModes.NoJSM && Background == null)
            {
                Mod = FieldModes.Disabled;
            }
            return SFX != null || PMP != null || MrtRat != null || MSK != null || TDW != null || INF != null ||
                   JSMObjects != null || EventEngine != null || Cameras != null || WalkMesh != null ||
                   Background != null || Services != null;
        }

        public void Update()
        {
            switch (Mod)
            {
                case FieldModes.Init:
                    break;

                case FieldModes.DebugRender:
                    UpdateScript();
                    Background?.Update();
                    break; //await events here
                case FieldModes.NoJSM://no scripts but has background.
                    Background?.Update();
                    break; //await events here
                case FieldModes.Disabled:
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