using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.Fields
{
    public class Archive
    {
        public Archive()
        {
        }
        #region Properties

        public Background Background { get; private set; }
        public Cameras Cameras { get; private set; }
        public EventEngine EventEngine { get; private set; }
        public INF inf { get; private set; }
        public Field_modes Mod { get; set; } = 0;
        public MrtRat MrtRat { get; private set; }
        public MSK msk { get; private set; }
        public PMP pmp { get; private set; }
        public IServices services { get; private set; }
        public SFX sfx { get; private set; }
        public TDW tdw { get; private set; }
        public WalkMesh WalkMesh { get; private set; }
        public Sections Flags { get; private set; }

        #endregion Properties

        #region Methods
        public static Archive Load(ushort inputFieldID, Sections flags = Sections.ALL)
        {
            var r = new Archive();
            r.Init(inputFieldID, flags);
            return r;
        }

        public void Init(ushort? inputFieldID = null, Sections flags = Sections.ALL)
        {
            Flags = flags;
            Memory.SuppressDraw = true;
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();
            //TODO fix endless look on FieldID 50.
            ushort fieldID = inputFieldID ?? Memory.FieldHolder.FieldID;
            int count = (Memory.FieldHolder.fields?.Length ?? 0);
            if (fieldID >= count ||
                fieldID < 0)
                goto end;
            string fieldFileName = Memory.FieldHolder.GetString(fieldID);
            string fieldArchiveName = test.FirstOrDefault(x => x.IndexOf(fieldFileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fieldArchiveName))
            {
                Debug.WriteLine($"FileNotFound :: {fieldID} - {fieldFileName.ToUpper()}");
                Mod = Field_modes.DISABLED;
                goto end;
            }

            ArchiveBase fieldArchive = aw.GetArchive(fieldArchiveName);
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
            string s_jsm = findstr(".jsm");
            string s_sy = findstr(".sy");
            if ((Background = Background.Load(getfile(".mim"), getfile(".map"))) == null)
            {
                Mod = Field_modes.DISABLED;
                goto end;
            }
            Cameras = Cameras.Load(getfile(".ca"));
            WalkMesh = WalkMesh.Load(getfile(".id"),Cameras);
            //let's start with scripts
            List<Scripts.Jsm.GameObject> jsmObjects;

            if (!string.IsNullOrWhiteSpace(s_jsm))
            {
                try
                {
                    jsmObjects = Scripts.Jsm.File.Read(fieldArchive.GetBinaryFile(s_jsm));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Mod = Field_modes.NOJSM;
                    goto end;
                }

                Sym.GameObjects symObjects;
                if (!string.IsNullOrWhiteSpace(s_sy))
                {
                    symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(s_sy));
                }
                else
                {
                    Debug.WriteLine($"FileNotFound :: {fieldFileName.ToUpper()}.sy");
                    Mod = Field_modes.NOJSM;
                    goto end;
                }
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
            }
            else
            {
                Mod = Field_modes.NOJSM;
                goto end;
            }

            //byte[] mchb = getfile(".mch");//Field character models
            //byte[] oneb = getfile(".one");//Field character models container
            //byte[] msdb = getfile(".msd");//dialogs
            byte[] infb = getfile(".inf");//gateways
            inf = INF.Load(infb);
            //byte[] idb = getfile(".id");//walkmesh
            //byte[] cab = getfile(".ca");//camera
            byte[] tdwb = getfile(".tdw");//extra font
            tdw = new TDW(tdwb);
            byte[] mskb = getfile(".msk");//movie cam
            msk = new MSK(mskb);
            byte[] ratb = getfile(".rat");//battle on field
            byte[] mrtb = getfile(".mrt");//battle on field
            MrtRat = new MrtRat(mrtb, ratb);
            //byte[] pmdb = getfile(".pmd");//particle info
            //if (pmdb != null && pmdb.Length > 4)
            //    using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(),
            //        $"{Memory.FieldHolder.GetString()}.pmd"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            //        fs.Write(pmdb, 0, pmdb.Length);
            byte[] pmpb = getfile(".pmp");//particle graphic?
            if (pmpb != null && pmpb.Length > 4)
                pmp = new PMP(pmpb);
            byte[] sfxb = getfile(".sfx");//sound effects
            sfx = new SFX(sfxb);

            Mod++;
        end:
            return;
        }

        public void Update()
        {
            switch (Mod)
            {
                case Field_modes.INIT:
                    break;

                case Field_modes.DEBUGRENDER:
                    UpdateScript();
                    Background.Update();
                    break; //await events here
                case Field_modes.NOJSM://no scripts but has background.
                    Background.Update();
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
        #endregion Methods
    }
}