using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.IO;

namespace FF8
{
    internal static class Module_field_debug
    {
        private static Field_mods mod = 0;
        private static Texture2D tex;
        private static Texture2D texOverlap;

        private static List<Tile> tiles;

        private struct Tile
        {
            public short x, y;
            public ushort z;
            public byte texID; // 4 bits
            public byte pallID; //[6-10]
            public byte srcx, srcy;
            public byte layId;
            public byte blendType;
            public byte state;
            public byte parameter;
            public byte blend1;
            public byte blend2;
        }

        private struct SJSM
        {
            public byte cDoorEntity;
            public byte cWalkmeshEntity;
            public byte cBackgroundEntity;
            public byte cOtherEntity;
            public ushort offsetSecOne;
            public ushort offsetScriptData;
            public EntryPointEntity[] EntityEntryPoints;
            public EntryPointScript[] entryPointScripts;
            public int[] ScriptData;
        }

        private static List<ScriptEntry> ScriptSystem;
        private static string[] symbolNames;

        private static List<int> Stack;

        private struct EntryPointEntity
        {
            public byte scriptCount;
            public byte label;
            public string labelASM;
        }

        private struct EntryPointScript
        {
            public ushort position;
            public byte flag;
        }

        private struct ScriptOpcode
        {
            public ushort opcodeBinary;
            public string opcodeASM;
            public JSMopcodes opcode;
            public ushort parameter;
        }

        private struct ScriptEntry //final struct
        {
            public ushort Entity;
            public string ScriptName;
            public ushort ID;
            public ushort localID;
            public ScriptOpcode[] Scripts;
        }

        private static SJSM jsm;

        private static int width, height;

        private enum JSMopcodes
        {
            NOP,
            CAL,
            JMP,
            JPF,
            GJMP,
            LBL,
            RET,
            PSHN_L,
            PSHI_L,
            POPI_L,
            PSHM_B,
            POPM_B,
            PSHM_W,
            POPM_W,
            PSHM_L,
            POPM_L,
            PSHSM_B,
            PSHSM_W,
            PSHSM_L,
            PSHAC,
            REQ,
            REQSW,
            REQEW,
            PREQ,
            PREQSW,
            PREQEW,
            UNUSE,
            DEBUG,
            HALT,
            SET,
            SET3,
            IDLOCK,
            IDUNLOCK,
            EFFECTPLAY2,
            FOOTSTEP,
            JUMP,
            JUMP3,
            LADDERUP,
            LADDERDOWN,
            LADDERUP2,
            LADDERDOWN2,
            MAPJUMP,
            MAPJUMP3,
            SETMODEL,
            BASEANIME,
            ANIME,
            ANIMEKEEP,
            CANIME,
            CANIMEKEEP,
            RANIME,
            RANIMEKEEP,
            RCANIME,
            RCANIMEKEEP,
            RANIMELOOP,
            RCANIMELOOP,
            LADDERANIME,
            DISCJUMP,
            SETLINE,
            LINEON,
            LINEOFF,
            WAIT,
            MSPEED,
            MOVE,
            MOVEA,
            PMOVEA,
            CMOVE,
            FMOVE,
            PJUMPA,
            ANIMESYNC,
            ANIMESTOP,
            MESW,
            MES,
            MESSYNC,
            MESVAR,
            ASK,
            WINSIZE,
            WINCLOSE,
            UCON,
            UCOFF,
            MOVIE,
            MOVIESYNC,
            SETPC,
            DIR,
            DIRP,
            DIRA,
            PDIRA,
            SPUREADY,
            TALKON,
            TALKOFF,
            PUSHON,
            PUSHOFF,
            ISTOUCH,
            MAPJUMPO,
            MAPJUMPON,
            MAPJUMPOFF,
            SETMESSPEED,
            SHOW,
            HIDE,
            TALKRADIUS,
            PUSHRADIUS,
            AMESW,
            AMES,
            GETINFO,
            THROUGHON,
            THROUGHOFF,
            BATTLE,
            BATTLERESULT,
            BATTLEON,
            BATTLEOFF,
            KEYSCAN,
            KEYON,
            AASK,
            PGETINFO,
            DSCROLL,
            LSCROLL,
            CSCROLL,
            DSCROLLA,
            LSCROLLA,
            CSCROLLA,
            SCROLLSYNC,
            RMOVE,
            RMOVEA,
            RPMOVEA,
            RCMOVE,
            RFMOVE,
            MOVESYNC,
            CLEAR,
            DSCROLLP,
            LSCROLLP,
            CSCROLLP,
            LTURNR,
            LTURNL,
            CTURNR,
            CTURNL,
            ADDPARTY,
            SUBPARTY,
            CHANGEPARTY,
            REFRESHPARTY,
            SETPARTY,
            ISPARTY,
            ADDMEMBER,
            SUBMEMBER,
            ISMEMBER,
            LTURN,
            CTURN,
            PLTURN,
            PCTURN,
            JOIN,
            MESFORCUS,
            BGANIME,
            RBGANIME,
            RBGANIMELOOP,
            BGANIMESYNC,
            BGDRAW,
            BGOFF,
            BGANIMESPEED,
            SETTIMER,
            DISPTIMER,
            SHADETIMER,
            SETGETA,
            SETROOTTRANS,
            SETVIBRATE,
            STOPVIBRATE,
            MOVIEREADY,
            GETTIMER,
            FADEIN,
            FADEOUT,
            FADESYNC,
            SHAKE,
            SHAKEOFF,
            FADEBLACK,
            FOLLOWOFF,
            FOLLOWON,
            GAMEOVER,
            ENDING,
            SHADELEVEL,
            SHADEFORM,
            FMOVEA,
            FMOVEP,
            SHADESET,
            MUSICCHANGE,
            MUSICLOAD,
            FADENONE,
            POLYCOLOR,
            POLYCOLORALL,
            KILLTIMER,
            CROSSMUSIC,
            DUALMUSIC,
            EFFECTPLAY,
            EFFECTLOAD,
            LOADSYNC,
            MUSICSTOP,
            MUSICVOL,
            MUSICVOLTRANS,
            MUSICVOLFADE,
            ALLSEVOL,
            ALLSEVOLTRANS,
            ALLSEPOS,
            ALLSEPOSTRANS,
            SEVOL,
            SEVOLTRANS,
            SEPOS,
            SEPOSTRANS,
            SETBATTLEMUSIC,
            BATTLEMODE,
            SESTOP,
            BGANIMEFLAG,
            INITSOUND,
            BGSHADE,
            BGSHADESTOP,
            RBGSHADELOOP,
            DSCROLL2,
            LSCROLL2,
            CSCROLL2,
            DSCROLLA2,
            LSCROLLA2,
            CSCROLLA2,
            DSCROLLP2,
            LSCROLLP2,
            CSCROLLP2,
            SCROLLSYNC2,
            SCROLLMODE2,
            MENUENABLE,
            MENUDISABLE,
            FOOTSTEPON,
            FOOTSTEPOFF,
            FOOTSTEPOFFALL,
            FOOTSTEPCUT,
            PREMAPJUMP,
            USE,
            SPLIT,
            ANIMESPEED,
            RND,
            DCOLADD,
            DCOLSUB,
            TCOLADD,
            TCOLSUB,
            FCOLADD,
            FCOLSUB,
            COLSYNC,
            DOFFSET,
            LOFFSETS,
            COFFSETS,
            LOFFSET,
            COFFSET,
            OFFSETSYNC,
            RUNENABLE,
            RUNDISABLE,
            MAPFADEOFF,
            MAPFADEON,
            INITTRACE,
            SETDRESS,
            GETDRESS,
            FACEDIR,
            FACEDIRA,
            FACEDIRP,
            FACEDIRLIMIT,
            FACEDIROFF,
            SARALYOFF,
            SARALYON,
            SARALYDISPOFF,
            SARALYDISPON,
            MESMODE,
            FACEDIRINIT,
            FACEDIRI,
            JUNCTION,
            SETCAMERA,
            BATTLECUT,
            FOOTSTEPCOPY,
            WORLDMAPJUMP,
            RFACEDIRI,
            RFACEDIR,
            RFACEDIRA,
            RFACEDIRP,
            RFACEDIROFF,
            FACEDIRSYNC,
            COPYINFO,
            PCOPYINFO,
            RAMESW,
            BGSHADEOFF,
            AXIS,
            AXISSYNC,
            MENUNORMAL,
            MENUPHS,
            BGCLEAR,
            GETPARTY,
            MENUSHOP,
            DISC,
            DSCROLL3,
            LSCROLL3,
            CSCROLL3,
            MACCEL,
            MLIMIT,
            ADDITEM,
            SETWITCH,
            SETODIN,
            RESETGF,
            MENUNAME,
            REST,
            MOVECANCEL,
            PMOVECANCEL,
            ACTORMODE,
            MENUSAVE,
            SAVEENABLE,
            PHSENABLE,
            HOLD,
            MOVIECUT,
            SETPLACE,
            SETDCAMERA,
            CHOICEMUSIC,
            GETCARD,
            DRAWPOINT,
            PHSPOWER,
            KEY,
            CARDGAME,
            SETBAR,
            DISPBAR,
            KILLBAR,
            SCROLLRATIO2,
            WHOAMI,
            MUSICSTATUS,
            MUSICREPLAY,
            DOORLINEOFF,
            DOORLINEON,
            MUSICSKIP,
            DYING,
            SETHP,
            GETHP,
            MOVEFLUSH,
            MUSICVOLSYNC,
            PUSHANIME,
            POPANIME,
            KEYSCAN2,
            KEYON2,
            PARTICLEON,
            PARTICLEOFF,
            KEYSIGHNCHANGE,
            ADDGIL,
            ADDPASTGIL,
            ADDSEEDLEVEL,
            PARTICLESET,
            SETDRAWPOINT,
            MENUTIPS,
            LASTIN,
            LASTOUT,
            SEALEDOFF,
            MENUTUTO,
            OPENEYES,
            CLOSEEYES,
            BLINKEYES,
            SETCARD,
            HOWMANYCARD,
            WHERECARD,
            ADDMAGIC,
            SWAP,
            SETPARTY2,
            SPUSYNC,
            BROKEN,
            UNKNOWN1,
            UNKNOWN2,
            UNKNOWN3,
            UNKNOWN4,
            UNKNOWN5,
            UNKNOWN6,
            UNKNOWN7,
            UNKNOWN8,
            UNKNOWN9,
            UNKNOWN10,
            UNKNOWN11,
            UNKNOWN12,
            UNKNOWN13,
            UNKNOWN14,
            UNKNOWN15,
            UNKNOWN16,
            PREMAPJUMP2,
            TUTO
        }


        private enum Field_mods
        {
            INIT,
            DEBUGRENDER
        };

        internal static void Draw()
        {
            switch (mod)
            {
                case Field_mods.INIT:
                    break; //null
                case Field_mods.DEBUGRENDER:
                    DrawDebug();
                    break;
            }
        }

        public static void ResetField()
        {
            mod = Field_mods.INIT;
            if (ScriptSystem != null)
                ScriptSystem.Clear();
        }

        private static void DrawDebug()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchStartStencil();
            Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
            Rectangle dst = src;
            dst.Size = (dst.Size.ToVector2() * Memory.Scale(tex.Width, tex.Height)).ToPoint();
            //In game I think we'd keep the field from leaving the screen edge but would center on the Squall and the party when it can.
            //I setup scaling after noticing the field didn't size with the screen. I set it to center on screen.
            dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
            Memory.spriteBatch.Draw(tex,dst, src, Color.White);
            //new Microsoft.Xna.Framework.Rectangle(0, 0, 1280 + (width - 320), 720 + (height - 224)),
            //new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height)
            Memory.SpriteBatchEnd();
        }

        internal static void Update()
        {
#if DEBUG
            // lets you move through all the feilds just holding left or right. it will just loop when it runs out.
            if (Input.Button(Buttons.Left) )
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Module_main_menu_debug.FieldPointer--;
                ResetField();
            }
            if (Input.Button(Buttons.Right) )
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Module_main_menu_debug.FieldPointer++;
                ResetField();
            }
#endif
            switch (mod)
            {
                case Field_mods.INIT:
                    Init();
                    StartupScript();// DEBUG
                    break;
                case Field_mods.DEBUGRENDER:
                    break; //await events here
            }
        }
 
        private static void StartupScript()
        {
            if (ScriptSystem != null)
            {
                var initDefaultCollection = ScriptSystem.Where(x => x.localID == 0 || x.localID == 1).ToList();
                foreach (var scr in initDefaultCollection)
                    foreach (var opcode in scr.Scripts)
                        if (ScriptSystem.Count != 0)
                            ParseOpcode(opcode);
            }
        }

        private static void ParseOpcode(ScriptOpcode opcode)
        {
            int stack1 = 0;
            int stack2 = 0;
            //int stack3 = 0;
            switch (opcode.opcode)
            {
                case JSMopcodes.NOP:
                    return;
                case JSMopcodes.LBL:
                    return;
                case JSMopcodes.SETMODEL: //TODO
                    Console.WriteLine("TODO: ENTITY SUBSYSTEM");
                    return;
                case JSMopcodes.PSHN_L:
                    Stack.Add(opcode.parameter);
                    return;
                case JSMopcodes.BASEANIME: //TODO
                    if (Stack.Count > 0)
                        stack1 = POPstack();
                    if (Stack.Count > 0)
                        stack2 = POPstack();
                    Console.WriteLine("TODO: ENTITY SUBSYSTEM; ANIMATION");
                    return;
                case JSMopcodes.SETPC: //TODO
                    stack1 = POPstack();
                    Console.WriteLine("TODO: PARTY SUBSYSTEM");
                    return;
                case JSMopcodes.HIDE: //TODO
                    Console.WriteLine("TODO: ENTITY SUBSYSTEM");
                    return;
                case JSMopcodes.UCOFF: //TODO
                    Console.WriteLine("TODO: I/O SUBSYSTEM");
                    return;
                case JSMopcodes.CLEAR: //TODO
                    Console.WriteLine("TODO: CLEAR()");
                    return;
                case JSMopcodes.POPM_B:
                    if (Stack.Count > 0)
                        stack1 = POPstack();
                        if(stack1 < Memory.FieldHolder.FieldMemory.Length) // had one come out of range.
                            Memory.FieldHolder.FieldMemory[stack1] = (byte)opcode.parameter; //stack was count 0 something wrong here?
                    return;
                case JSMopcodes.POPM_W:
                    if (Stack.Count > 0)
                        stack1 = POPstack();
                    if (stack1 < Memory.FieldHolder.FieldMemory.Length)
                            Memory.FieldHolder.FieldMemory[stack1] = (ushort)opcode.parameter;
                    return;
                case JSMopcodes.BATTLEOFF: //TODO
                    return;
                case JSMopcodes.MAPFADEOFF: //TODO
                    return;
                case JSMopcodes.ADDMEMBER: //todo
                    return;
                case JSMopcodes.ADDPARTY: //todo
                    return;
                case JSMopcodes.SETBATTLEMUSIC:
                    Memory.SetBattleMusic = POPstack();
                    return;
                case JSMopcodes.CAL: //todo
                    if (Stack.Count > 0)
                        stack1 = POPstack();
                    if (Stack.Count > 0)
                        stack2 = POPstack(); 
                    return;
                case JSMopcodes.MOVIEREADY: //todo
                    if (Stack.Count > 0)
                        stack1 = POPstack();
                    if (Stack.Count > 0)
                        stack2 = POPstack(); 
                    return;
                case JSMopcodes.MOVIE: //todo
                    return;
                case JSMopcodes.MOVIESYNC: //todo
                    return;
                case JSMopcodes.MAPJUMPO: //todo
                    if (Stack.Count > 0)
                        stack2 = POPstack(); //walkmesh id

                    if (Stack.Count > 0)
                    {
                        stack1 = POPstack(); //field map id
                        Memory.FieldHolder.FieldID = (ushort)stack1;
                    }
                    ResetField();
                    //todo
                    return;
            }
        }

        private static int POPstack()
        {
            if (Stack.Count > 0)
            {
                int stack = Stack.Last();
                Stack.RemoveAt(Stack.Count() - 1);
                return stack;
            }
            return 0; // the stack is empty. maybe something is wrong?
        }

        private static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker($"{Memory.Archives.A_FIELD}.fs");
            string[] test = aw.GetListOfFiles();
            if (Memory.FieldHolder.FieldID >= Memory.FieldHolder.fields.Length ||
                Memory.FieldHolder.FieldID < 0)
                return;
            var CollectionEntry = test.Where(x => x.ToLower().Contains(Memory.FieldHolder.fields[Memory.FieldHolder.FieldID]));
            if (!CollectionEntry.Any()) return;
            string fieldArchive = CollectionEntry.First();
            int fieldLen = fieldArchive.Length - 2;
            fieldArchive = fieldArchive.Substring(0, fieldLen);
            byte[] fs = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{fieldArchive}fs");
            byte[] fi = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{fieldArchive}fi");
            byte[] fl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{fieldArchive}fl");
            if (fs == null || fi == null || fl == null) return;
            string[] test_ = ArchiveWorker.GetBinaryFileList(fl);
            string mim = null;
            string map = null;
            try
            {
                mim = test_.First(x => x.ToLower().Contains(".mim"));
            }
            catch{}
            try
            {
                 map = test_.First(x => x.ToLower().Contains(".map"));
            }
            catch{}

            if (mim != null && map != null)
            {
                byte[] mimb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, mim);
                byte[] mapb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, map);

                ParseBackground(mimb, mapb);
            }

#if DEBUG
            if (Memory.FieldHolder.FieldID == 180) goto safeDebugpoint; //delete me
#endif
            //let's start with scripts
            byte[] jsm = null;
            byte[] sy = null;
            string s_jsm = null;
            string s_sy = null;
            try
            {
                s_jsm = test_.First(x => x.ToLower().Contains(".jsm"));
            }
            catch { }
            try
            {
                s_sy = test_.First(x => x.ToLower().Contains(".sy"));
            }
            catch { }
            if (s_jsm != null && s_sy != null)
            {
                jsm = ArchiveWorker.FileInTwoArchives(fi, fs, fl, s_jsm);
                sy = ArchiveWorker.FileInTwoArchives(fi, fs, fl, s_sy);

                ParseScripts(jsm, sy);
            }
            Stack = new List<int>();
#if DEBUG
            OutputAllParsedScripts();

#endif

        //string mch = test_.Where(x => x.ToLower().Contains(".mch")).First();
        //string one = test_.Where(x => x.ToLower().Contains(".one")).First();
        //string msd = test_.Where(x => x.ToLower().Contains(".msd")).First();
        //string inf = test_.Where(x => x.ToLower().Contains(".inf")).First();
        //string id = test_.Where(x => x.ToLower().Contains(".id")).First();
        //string ca = test_.Where(x => x.ToLower().Contains(".ca")).First();
        //string tdw = test_.Where(x => x.ToLower().Contains(".tdw")).First();
        //string msk = test_.Where(x => x.ToLower().Contains(".msk")).First();
        //string rat = test_.Where(x => x.ToLower().Contains(".rat")).First();
        //string pmd = test_.Where(x => x.ToLower().Contains(".pmd")).First();
        //string sfx = test_.Where(x => x.ToLower().Contains(".sfx")).First();

        //byte[] mchb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, mch); //Field character models
        //byte[] oneb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, one); //Field character models container
        //byte[] msdb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, msd); //dialogs
        //byte[] infb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, inf); //gateways
        //byte[] idb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, id); //walkmesh
        //byte[] cab = ArchiveWorker.FileInTwoArchives(fi, fs, fl, ca); //camera
        //byte[] tdwb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, tdw); //extra font
        //byte[] mskb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, msk); //movie cam
        //byte[] ratb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, rat); //battle on field
        //byte[] pmdb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, pmd); //particle info
        //byte[] sfxb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, sfx); //sound effects



        safeDebugpoint:
            mod++;
            return;
        }

        private static void OutputAllParsedScripts()
        {
            foreach (var a in ScriptSystem)
            {
                Console.WriteLine($"Entity: {a.Entity}\tName: {a.ScriptName}");
                int lineNumber = 0;
                foreach (var b in a.Scripts)
                    Console.WriteLine($"{lineNumber++}:\t{b.opcodeASM} {b.parameter.ToString()}");
            }
        }

        private static void ParseScripts(byte[] jsmb, byte[] symb)
        {
            //string[] symbolNames = new string[symb.Length / 32];
            if(symb == null)
                    return;
            symbolNames = System.Text.Encoding.ASCII.GetString(symb).Replace(" ", "").Replace("\0", "").Split('\n');
            //for(int i = 0; i<symbolNames.Length; i++)
            //    symbolNames[i] = System.Text.Encoding.ASCII.GetString(symb, i * 32, 32).TrimEnd('\0', '\n', ' ');
            //File.WriteAllBytes("D:/symb.test", symb);
            jsm = new SJSM();
            //File.WriteAllBytes("D:\\test.jsm", jsmb);
            using (Stream str = new MemoryStream(jsmb))
            using (BinaryReader br = new BinaryReader(str))
            {
                jsm.cDoorEntity = br.ReadByte();
                jsm.cWalkmeshEntity = br.ReadByte();
                jsm.cBackgroundEntity = br.ReadByte();
                jsm.cOtherEntity = br.ReadByte();
                jsm.offsetSecOne = br.ReadUInt16();
                jsm.offsetScriptData = br.ReadUInt16();
                EntryPointEntity[] epe = new EntryPointEntity[jsm.cDoorEntity + jsm.cOtherEntity + jsm.cWalkmeshEntity + jsm.cBackgroundEntity];
                for (int i = 0; i < epe.Length; i++)
                {
                    ushort bb = br.ReadUInt16();
                    epe[i].scriptCount = (byte)((bb & 0x7F) + 1);
                    epe[i].label = (byte)(bb >> 7);
                    // was throwing exception
                    epe[i].labelASM = symbolNames != null && epe[i].label < symbolNames.Length ? symbolNames[epe[i].label] : "";
                }
                int SYMscriptNameStartingPoint = jsm.cDoorEntity + jsm.cOtherEntity + jsm.cWalkmeshEntity + jsm.cBackgroundEntity;
                jsm.EntityEntryPoints = epe;
                EntryPointScript[] eps = new EntryPointScript[(jsm.offsetScriptData - jsm.offsetSecOne) / 2 - 1];
                for (int i = 0; i < eps.Length; i++)
                {
                    ushort bb = br.ReadUInt16();
                    eps[i].position = (ushort)((bb & 0x7FFF) * 4);
                    eps[i].flag = (byte)(bb >> 15);
                }
                ushort eof = br.ReadUInt16();
                jsm.entryPointScripts = eps;

                //br.BaseStream.Seek(jsm.offsetScriptData, SeekOrigin.Begin);

                ScriptSystem = new List<ScriptEntry>();
                List<ScriptOpcode> scriptChunk = new List<ScriptOpcode>();
                int scriptLabelPointer = 0;
                while (br.BaseStream.Position != br.BaseStream.Length)
                {
                    if (br.BaseStream.Position == br.BaseStream.Length)
                        break; //??
                    uint binaryOpcode = br.ReadUInt32();
                    uint parameter = 0;
                    uint opcode = 0;
                    if ((binaryOpcode & 0xFFFFFF00) == 0) //when only function
                    {
                        opcode = binaryOpcode;
                        parameter = 0;
                    }
                    else
                    {
                        opcode = binaryOpcode >> 16;
                        opcode = (opcode >> 8) | opcode << 8 & 0xFF00;
                        parameter = binaryOpcode & 0xFFFF;
                    }
                    if (opcode == 5 && scriptChunk.Count != 0) //label
                    {
                        ushort entityNumber =0;
                        if (symbolNames != null && SYMscriptNameStartingPoint + scriptLabelPointer < symbolNames.Length)
                            entityNumber = (ushort)FindEntity(symbolNames[SYMscriptNameStartingPoint + scriptLabelPointer]);
                        int locId = ScriptSystem.Count(x => x.Entity == entityNumber);
                        ScriptSystem.Add(new ScriptEntry()
                        {
                            Entity = entityNumber,
                            ScriptName = (SYMscriptNameStartingPoint + scriptLabelPointer+1 < symbolNames.Length ? symbolNames[SYMscriptNameStartingPoint + scriptLabelPointer++] : ""),
                            ID = scriptChunk[0].parameter,
                            localID = (ushort)locId++,
                            Scripts = scriptChunk.ToArray()
                        });
                        scriptChunk.Clear();
                    }

                    scriptChunk.Add(new ScriptOpcode()
                    {
                        parameter = (ushort)parameter,
                        opcodeBinary = (ushort)opcode,
                        opcodeASM = Enum.GetName(typeof(JSMopcodes), opcode),
                        opcode = (JSMopcodes)opcode

                    });
                    if (br.BaseStream.Position == br.BaseStream.Length)
                    {
                        ushort entityNumber = 0;
                        if (symbolNames != null && SYMscriptNameStartingPoint + scriptLabelPointer < symbolNames.Length)
                            entityNumber = (ushort)FindEntity(symbolNames[SYMscriptNameStartingPoint + scriptLabelPointer]);
                        int locId = ScriptSystem.Count(x => x.Entity == entityNumber);
                        ScriptSystem.Add(new ScriptEntry()
                        {
                            Entity = entityNumber,
                            ScriptName = (SYMscriptNameStartingPoint + scriptLabelPointer + 1 < symbolNames.Length ? symbolNames[SYMscriptNameStartingPoint + scriptLabelPointer++]:""),
                            ID = scriptChunk[0].parameter,
                            localID = (ushort)locId++,
                            Scripts = scriptChunk.ToArray()
                        });
                        break;
                    }
                }

                /*
                 * okay, my notes on JSM:
                 * so the exec is always Lines first, they tend to always contain 8 IDs like touch touchoff etc
                 * it begins with setline
                 * 
                 * next are doors, yes?
                 * they are like open, close, on, off
                 * so you have to test the location of the player and see if he triggers any of this script
                 * 
                 * next are other things
                 * finally an character entity- director. He plays like if we should call some functions or not
                 * 
                 * it's like RET(8) makes it never use the code again in a loop of execution
                 * 
                 * so it all plays normally, because almost everytime it's RET of the function. All action is triggered by like PUSH or TALK
                 * 
                 * Other functions are not normally playing. See bgroom_4. Default code is actually playing the monitor.on functions
                 * 
                 * So for sure I'll need to read .SYM, then pair the names with script IDs, then sort by the exec priority and read only the 0 and 1 as default and init scripts
                 * everything else leave for triggering. Ugh, that's going to be painful
                 * 
                 */
            }
        }

        private static int FindEntity(string entityName)
        {
            if (entityName.Contains("::"))
                entityName = entityName.Substring(0, entityName.IndexOf(':'));
            return symbolNames.ToList().IndexOf(entityName);
        }

        private static void ParseBackground(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return;

            int type1Width = 1664;

            tiles = new List<Tile>();
            int palletes = 24;
            //128x256
            PseudoBufferedStream pbsmap = new PseudoBufferedStream(mapb);
            PseudoBufferedStream pbsmim = new PseudoBufferedStream(mimb);
            while (pbsmap.Tell() + 16 < pbsmap.Length)
            {
                Tile tile = new Tile { x = pbsmap.ReadShort() };
                if (tile.x == 0x7FFF)
                    break;
                tile.y = pbsmap.ReadShort();
                tile.z = pbsmap.ReadUShort();// (ushort)(4096 - pbsmap.ReadUShort());
                byte texIdBuffer = pbsmap.ReadByte();
                tile.texID = (byte)(texIdBuffer & 0xF);
                pbsmap.Seek(1, SeekOrigin.Current);
                //short testz = pbsmap.ReadShort();
                //testz = (short)(testz >> 6);
                //testz &= 0xF;
                tile.pallID = (byte)((pbsmap.ReadShort() >> 6) & 0xF);
                tile.srcx = pbsmap.ReadByte();
                tile.srcy = pbsmap.ReadByte();
                tile.layId = (byte)(pbsmap.ReadByte() & 0x7F);
                tile.blendType = pbsmap.ReadByte();
                tile.parameter = pbsmap.ReadByte();
                tile.state = pbsmap.ReadByte();
                tile.blend1 = (byte)((texIdBuffer >> 4) & 0x1);
                tile.blend2 = (byte)(texIdBuffer >> 5);
                tiles.Add(tile);
                //srcY = srcX == texID * 128 + srcX;
            }

            int lowestY = tiles.Min(x => x.y);
            int maximumY = tiles.Max(x => x.y);
            int lowestX = tiles.Min(x => x.x); //-160;
            int maximumX = tiles.Max(x => x.x);

            height = Math.Abs(lowestY) + maximumY + 16; //224
            width = Math.Abs(lowestX) + maximumX + 16; //320
            byte[] finalImage = new byte[height * width * 4]; //ARGB;
            byte[] finalOverlapImage = new byte[height * width * 4];
            tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            var MaximumLayer = tiles.Max(x => x.layId);
            var MinimumLayer = tiles.Min(x => x.layId);

            List<ushort> BufferDepth = tiles.GroupBy(x => x.z).Select(group => group.First()).Select(x => x.z).ToList();
            BufferDepth.Sort();

            for (int LayerId = 1; LayerId <= MaximumLayer + 1; LayerId++)
            {
                foreach (Tile tile in tiles)
                {
                    if (LayerId != MaximumLayer + 1)
                    {
                        if (tile.layId != LayerId)
                            continue;
                        //if (tile.z != BufferDepth[LayerId])
                        //    continue;
                    }
                    else
                        if (tile.layId != 0)
                        continue;

                    int palettePointer = 4096 + ((tile.pallID) * 512);
                    int sourceImagePointer = 512 * palletes;

                    int realX = Math.Abs(lowestX) + tile.x; //baseX
                    int realY = Math.Abs(lowestY) + tile.y; //*width
                    int realDestinationPixel = ((realY * width) + realX) * 4;
                    if (tile.blend2 >= 4)
                    {
                        int startPixel = sourceImagePointer + tile.srcx + 128 * tile.texID + (type1Width * tile.srcy);
                        for (int y = 0; y < 16; y++)
                            for (int x = 0; x < 16; x++)
                            {
                                byte pixel = mimb[startPixel + x + (y * 1664)];
                                ushort pixels = BitConverter.ToUInt16(mimb, 2 * pixel + palettePointer);
                                if (pixels == 00)
                                    continue;
                                byte red = (byte)((pixels) & 0x1F);
                                byte green = (byte)((pixels >> 5) & 0x1F);
                                byte blue = (byte)((pixels >> 10) & 0x1F);
                                red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                                green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                                blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                                if (tile.blendType < 4)
                                {
                                    if (true)//!bSaveToOverlapBuffer)
                                    {

                                        byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                                        byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                                        byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                                        Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                                    }
                                    else
                                    {
                                        byte baseColorR = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                                        byte baseColorG = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                                        byte baseColorB = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                                        Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalOverlapImage, realDestinationPixel, x, y);
                                    }
                                }
                                else
                                {
                                    if (true)//!bSaveToOverlapBuffer)
                                    {
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                                    }
                                    else
                                    {
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                                    }
                                }
                            }
                    }
                    else
                    {
                        //    int startPixel = sourceImagePointer + tile.srcx / 2 + 128 * tile.texID + (type1Width * tile.srcy);
                        //    for (int y = 0; y < 16; y++)
                        //        for (int x = 0; x < 16; x++)
                        //        {
                        //            byte index = mimb[startPixel + x + (y * 1664)];
                        //            ushort pixels = BitConverter.ToUInt16(mimb, 2 * (index & 0xF) + palettePointer);
                        //            byte red = (byte)((pixels) & 0x1F);
                        //            byte green = (byte)((pixels >> 5) & 0x1F);
                        //            byte blue = (byte)((pixels >> 10) & 0x1F);
                        //            red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                        //            green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                        //            blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                        //            if (pixels != 0)
                        //            {

                        //                if (tile.blendType < 4)
                        //                {
                        //                    byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                        //                    byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                        //                    byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                        //                    Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                        //                }
                        //            }
                        //            pixels = BitConverter.ToUInt16(mimb, 2 * (index >> 4) + palettePointer);
                        //            red = (byte)((pixels) & 0x1F);
                        //            green = (byte)((pixels >> 5) & 0x1F);
                        //            blue = (byte)((pixels >> 10) & 0x1F);
                        //            red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                        //            green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                        //            blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                        //            if (pixels != 0)
                        //            {

                        //                if (tile.blendType < 4)
                        //                {
                        //                    byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                        //                    byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                        //                    byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                        //                    Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                        //            }
                        //        }
                    }
                }
            }
            tex.SetData(finalImage);
            texOverlap.SetData(finalOverlapImage);
        }

        private static void DrawEntities()
        {
            throw new NotImplementedException();
        }

        private static void Blend(byte baseColorR, byte baseColorG, byte baseColorB, byte red, byte green, byte blue, Tile tile, byte[] finalImage, int realDestinationPixel, int x, int y)
        {
            switch (tile.blendType)
            {
                case 0:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)((baseColorR + red) / 2);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)((baseColorG + green) / 2);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)((baseColorB + blue) / 2);
                    break;
                case 1:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp(baseColorR + red, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp(baseColorG + green, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp(baseColorB + blue, 0, 255);
                    break;
                case 2:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp(baseColorR - red, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp(baseColorG - green, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp(baseColorB - blue, 0, 255);
                    break;
                case 3:
                    break;
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp((byte)(baseColorR + (0.25 * red)), 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp((byte)(baseColorG + (0.25 * green)), 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp((byte)(baseColorB + (0.25 * blue)), 0, 255);
                    break;
            }
        }
    }
}