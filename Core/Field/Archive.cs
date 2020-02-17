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

        public Background Background { get; private set; }
        public Cameras Cameras { get; private set; }
        public EventEngine EventEngine { get; private set; }
        public Sections Flags { get; private set; }
        public INF inf { get; private set; }
        public Field_modes Mod { get; set; } = 0;
        public MrtRat MrtRat { get; private set; }
        public MSK msk { get; private set; }
        public PMP pmp { get; private set; }
        public IServices services { get; private set; }
        public SFX sfx { get; private set; }
        public TDW tdw { get; private set; }
        public WalkMesh WalkMesh { get; private set; }
        public ushort ID { get; private set; }
        public string FileName { get; private set; }
        public string ArchiveName { get; private set; }

        #endregion Properties

        #region Methods

        public static Archive Load(ushort inputFieldID, Sections flags = Sections.ALL)
        {
            Archive r = new Archive();
            r.Init(inputFieldID, flags);
            return r;
        }

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

        public void Init(ushort? inputFieldID = null, Sections flags = Sections.ALL)
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
                return;
            FileName = Memory.FieldHolder.GetString(ID);
            ArchiveName = test.FirstOrDefault(x => x.IndexOf(FileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(ArchiveName))
            {
                Debug.WriteLine($"FileNotFound :: {ID} - {FileName.ToUpper()}");
                Mod = Field_modes.DISABLED;
                return;
            }

            ArchiveBase fieldArchive = aw.GetArchive(ArchiveName);
            string[] filelist = fieldArchive.GetListOfFiles();
            string findstr(string s) =>
                filelist.FirstOrDefault(x => x.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

            byte[] getfile(string s)
            {
                s = findstr(s);
                if (!string.IsNullOrWhiteSpace(s))
                    return fieldArchive.GetBinaryFile(s);
                else
                    return null;
            }
            if (!flags.HasFlag(Sections.MIM | Sections.MAP) || (Background = Background.Load(getfile(".mim"), getfile(".map"))) == null)
            {
                Mod = Field_modes.DISABLED;
            }
            if (flags.HasFlag(Sections.CA | Sections.ID))
            {
                Cameras = Cameras.Load(getfile(".ca"));
                WalkMesh = WalkMesh.Load(getfile(".id"), Cameras);
            }

            //let's start with scripts
            string s_jsm = findstr(".jsm");
            string s_sy = findstr(".sy");
            List<Scripts.Jsm.GameObject> jsmObjects = null;
            if (flags.HasFlag(Sections.JSM | Sections.SYM) && !string.IsNullOrWhiteSpace(s_jsm))
            {
                try
                {
                    jsmObjects = Scripts.Jsm.File.Read(fieldArchive.GetBinaryFile(s_jsm));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Mod = Field_modes.NOJSM;
                }
                if (Mod != Field_modes.NOJSM)
                {
                    Sym.GameObjects symObjects;
                    if (!string.IsNullOrWhiteSpace(s_sy))
                    {
                        symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(s_sy));

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
                        Mod = Field_modes.NOJSM;
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
                byte[] infb = getfile(".inf");//gateways
                if (infb != null && infb.Length > 0)
                    inf = INF.Load(infb);
            }

            if (flags.HasFlag(Sections.TDW))
            {
                byte[] tdwb = getfile(".tdw");//extra font
                if(tdwb != null && tdwb.Length >0)
                tdw = new TDW(tdwb);
            }

            if (flags.HasFlag(Sections.MSK))
            {
                byte[] mskb = getfile(".msk");//movie cam
                if (mskb != null && mskb.Length > 0)
                    msk = new MSK(mskb);
            }
            if (flags.HasFlag(Sections.RAT | Sections.MRT))
            {
                byte[] ratb = getfile(".rat");//battle on field
                byte[] mrtb = getfile(".mrt");//battle on field
                if(ratb != null && mrtb != null && ratb.Length >0 && mrtb.Length>0)
                    MrtRat = new MrtRat(mrtb, ratb);
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
                byte[] pmpb = getfile(".pmp");//particle graphic?
                if (pmpb != null && pmpb.Length > 4)
                    pmp = new PMP(pmpb);
            }
            if(flags.HasFlag(Sections.SFX))
            {
                byte[] sfxb = getfile(".sfx");//sound effects
                if (sfxb != null && sfxb.Length > 0)
                    sfx = new SFX(sfxb);
            }

            if (Mod == Field_modes.NOJSM && Background == null)
            {
                Mod = Field_modes.DISABLED;
            }
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
            }
        }

        private void UpdateScript()
        {
            //We do not know every instruction and it's not possible for now to play field with unknown instruction
            //eventEngine.Update(services);
        }

        #endregion Methods
    }
}