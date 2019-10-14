using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.World
{
    class wmset : IDisposable
    {
        private const int WMSET_SECTION_COUNT = 48;
        private byte[] buffer;
        private int[] sectionPointers;


        /// <summary>
        /// wmset file is pseudo-archive containing 48 sections in which every 'chunk' has different data and meaning
        /// </summary>
        /// <param name="wmsetBuffer"></param>
        public wmset(byte[] wmsetBuffer)
        {
            buffer = wmsetBuffer;
            sectionPointers = new int[WMSET_SECTION_COUNT];
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                for (int i = 0; i < sectionPointers.Length; i++)
                    sectionPointers[i] = br.ReadInt32();
            }

            //if there's no section method either uncommented or commented out, then the section that is lacking is 4 byte NULL

            Section1(); //======FINISHED
            Section2(); //======FINISHED
            //Section3(); //=encounter - looks like it's some supply helping data or roll data? Not sure, it's way before getting encounters
            Section4(); //======FINISHED
            Section6(); //======FINISHED
            //Section7(); //=something with roads colours, cluts. Can't really understand why it's needed, looks like some kind of helper for VRAM?
            Section8(); //wm2field WIP
            //Section9(); //=related to field2wm?
            Section10(); //I still don't know what it is for- something with vehicles- maybe wm2field with vehicle? -> same structure as section8
            //Section11(); //??????
            //Section12();
            //Section13();
            Section14(); //======FINISHED
            Section16(); //======FINISHED
            //Section18(); //?????
            //Section19(); //=something with regions: wm_getRegionNumber(SquallPos) + wmsets19
            //Section31(); //?????
            Section32(); //======FINISHED
            //Section33(); //=SKY GRADIENT/REGION COLOURING
            //Section34(); //?????????
            //Section35(); //=draw points
            //Section36(); //?????????
            //Section37(); //ENCOUNTERS??????
            Section38(); //======FINISHED
            Section39(); //======FINISHED
            Section42(); //======FINISHED
            Section41(); //=Water animation info - 256x1 texture- how to use it?
            Section17(); //=======FINISHED=beach animations->let's read that at the end due to references to wmset38

            //AKAO BELOW
            //Section20();
            //Section21();
            //Section43();
            //Section44();
            //Section45();
            //Section46();
            //Section47();
            //Section48();

            //-not important in openviii implementation/ sections used for limited memory solutions/psx hardware limitations
            //Section29(); //water block for limited memory rendering- not important in today
            //Section40(); //can't understand this one
        }

        /// <summary>
        /// Every section can have inner-sections. Pointers to different textures or models
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        private int[] GetInnerPointers(BinaryReader br)
        {
            List<int> innerSections = new List<int>();
            int eof = -1;
            while ((eof = br.ReadInt32()) != 0)
                innerSections.Add(eof);
            return innerSections.ToArray();
        }



        #region Section 1 - Encounter setting
        /// <summary>
        /// Section1 helps providing the correct encounter based on groundId and regionId, then provides the pointer to struct of section4
        /// </summary>
        private const int SECTION1ENTRYCOUNT = 96;
        [StructLayout(LayoutKind.Sequential, Pack =1, Size =4)]
        private struct EncounterHelper
        {
            public byte regionId;
            public byte groundId;
            public ushort encounterPointer;
        }
        private EncounterHelper[] encounterHelpEntries;
        private void Section1()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[1- 1], SeekOrigin.Begin);
                ms.Seek(4, SeekOrigin.Current); //skip first DWORD- it's EOF of global file
                List<EncounterHelper> encounterHelperList = new List<EncounterHelper>();
                while(true)
                {
                    EncounterHelper entry = Extended.ByteArrayToStructure<EncounterHelper>(br.ReadBytes(4));
                    if (entry.groundId == 0 && entry.regionId == 0 && entry.encounterPointer == 0)
                        break;
                    encounterHelperList.Add(entry);
                }
                encounterHelpEntries = encounterHelperList.ToArray();
            }
        }

        public ushort GetEncounterHelperPointer(int regionId, int groundId)
        {
            var encList = encounterHelpEntries.Where(x => x.groundId == groundId).Where(n => n.regionId == regionId);
            if (encList.Count() == 0)
                return 0xFFFF;
            var enc = encList.First(); //always first, if there are two the same entries then it doesn't make sense- priority is over that one that is first
            return enc.encounterPointer;
        }
        #endregion

        #region Section 2 - world map regions
        private byte[] regionsBuffer;

        private void Section2()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[2 - 1], SeekOrigin.Begin);
                regionsBuffer = br.ReadBytes(768);
            }
        }

        public byte GetWorldRegionByBlock(int blockId) => regionsBuffer[blockId];
        public byte GetWorldRegionBySegmentPosition(int x, int y) => regionsBuffer[y * 32 + x]; // I had got an exception here. For out of range.
        #endregion

        #region Section 4 - Encounter pointer
        private const int SEC4_ENC_PER_CHUNK = 8; //there are 8 scene.out pointers per one block/entry
        private ushort[][] encounters;
        private void Section4()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[4 - 1], SeekOrigin.Begin);
                List<ushort[]> encounterList = new List<ushort[]>();
                while(true)
                {
                    uint dwordTester = br.ReadUInt32();
                    if (dwordTester == 0)
                        break;
                    ms.Seek(-4, SeekOrigin.Current);
                    ushort[] sceneOutPointers = new ushort[SEC4_ENC_PER_CHUNK];
                    for(int i = 0; i<SEC4_ENC_PER_CHUNK; i++)
                        sceneOutPointers[i] = br.ReadUInt16();
                    encounterList.Add(sceneOutPointers);
                }
                encounters = encounterList.ToArray();
            }
        }

        public ushort[] GetEncounters(int pointer) => encounters[pointer];
        #endregion

        #region Section 6 - Encounter pointer (Lunar Cry)
        private ushort[][] encountersLunar;
        private void Section6()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[6 - 1], SeekOrigin.Begin);
                List<ushort[]> encounterList = new List<ushort[]>();
                while (true)
                {
                    uint dwordTester = br.ReadUInt32();
                    if (dwordTester == 0)
                        break;
                    ms.Seek(-4, SeekOrigin.Current);
                    ushort[] sceneOutPointers = new ushort[SEC4_ENC_PER_CHUNK];
                    for (int i = 0; i < SEC4_ENC_PER_CHUNK; i++)
                        sceneOutPointers[i] = br.ReadUInt16();
                    encounterList.Add(sceneOutPointers);
                }
                encountersLunar = encounterList.ToArray();
            }
        }

        public ushort[] GetEncountersLunar(int pointer) => encountersLunar[pointer];
        #endregion

        #region Section 8 - World map to field

        /// <summary>
        /// I have to just understand the algorithm and then I'll be able to upgrade it. Right now
        /// it looks like it's reading 0xFF01 and setting the start modes (there are two)
        /// and it normally goes like checkCondition-> FAIL=get next entry; SUCCESS= parse next opcodes of this given entry
        /// so for sure the labeltest goto algorithm needs to be redone- there are two modes but we can still read the same entry if success
        /// I still don't know what happens when all the conditions will success- if they are only condiditions or actions?
        /// because if it succedes- then it just simply goes to next condition and next...nextt...next but I don't see anything like changing
        /// module when wm2field
        /// </summary>
        public void Section8()
        {
            /*
             * Test case available!
             * Stand before the entrance to Balamb and it would be the [848]/ so-> 12th entry in section8!
             * It passes first check of 0xFF06 for segment id = 273 and moves further
             */
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[8 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for(int i = 0; i<innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[8 - 1] + innerSec[i], SeekOrigin.Begin);
                    labeltest:
                    short Mode = 0;
                    uint controlVar = 0;
                    int v4 = (int)ms.Position;
                    while (true)
                    {
                        while (true)
                        {
                            controlVar = br.ReadUInt32();
                            if ((controlVar&0xFFFF) != 0xFF01)
                                break;
                            Mode = 1;
                        }
                        if ((controlVar & 0xFFFF) == 0xFF04)
                        {
                            Mode = 2;
                            v4 = 3;
                            continue;
                        }
                        if (Mode > 0)
                            break;
                    }

                    if(Mode == 1)
                    {
                        if (!bSec8_conditionals(controlVar))
                            continue; //parse next entry
                        else
                            goto labeltest;

                    }
                    else //Mode 2
                    {
                        if(controlVar == 0xFF16)
                        {
                            //;something?
                            continue;
                        }
                        switch(controlVar)
                        {
                            case 0xFF05:
                                if (v4 == 2)
                                {
                                    //here is goto label- miracle of assembly- make sure to rewrite this shit later
                                    goto labeltest;
                                }
                                if (v4 != 5)
                                    continue;
                                goto labeltest;
                            case 0xFF0A:
                                v4 = 1;
                                goto labeltest;
                            case 0xFF0B:
                                if(v4==1)
                                {
                                    v4 = 3;
                                    goto labeltest;
                                }
                                if (v4 != 4)
                                    goto labeltest;
                                v4 = 3;
                                goto labeltest;
                            case 0xFF0C:
                                if (v4 != 2 && v4 != 5)
                                    goto labeltest;
                                v4 = 4;
                                goto labeltest;
                            case 0xFF0D:
                                if (v4 != 2 && v4 != 5)
                                    goto labeltest;
                                v4 = 6;
                                goto labeltest;
                            default: //[sub_545D60:132]
                                //TODO
                                break;
                        }
                    }
                }
            }
        }

        enum sec8_conditional : ushort
        {
            /// <summary>
            /// Checks for segment number (as in wmx.obj- e.g. 273)
            /// See: ModuleWorld::GetRealSegmentId();
            /// </summary>
            SegmentId = 0xFF06,
            unk02 = 0xFF02,
            unk03 = 0xFF03,
            unk07 = 0xFF07,
            CheckForVehicleGroup = 0xFF09,
            unk0f = 0xFF0F,
            unk10 = 0xFF10,
            unk11 = 0xFF11,
            unk12 = 0xFF12,
            unk17 = 0xFF17,
            unk1a = 0xFF1A,
            unk1b = 0xFF1B,
            unk1c = 0xFF1C,
            unk1d = 0xFF1D,
            unk20 = 0xFF20,
            unk18 = 0xFF18,
            unk19 = 0xFF19,
            unk1e = 0xFF1E,
            unk21 = 0xFF21,
            unk22 = 0xFF22,
            unk25 = 0xFF25,
            unk27 = 0xFF27,
            unk29 = 0xFF29,
            unk2a = 0xFF2A,
            unk2c = 0xFF2C,
            unk2d = 0xFF2D,
            unk2f = 0xFF2F,
            unk30 = 0xFF30,
            unk31 = 0xFF31,
            unk32 = 0xFF32,
            unk33 = 0xFF33,
            unk34 = 0xFF34,
            unk35 = 0xFF35,
            unk38 = 0xFF38,
            unk39 = 0xFF39
        }

        /// <summary>
        /// Some wmset sections work on some conditional file structure- therefore during e.g. warping from wm2field it checks entries one-by-one and the one that successively happen to get along with all conditions is parsed/pushed through- if not read next entry
        /// </summary>
        /// <param name="controlVar"></param>
        /// <returns></returns>
        private bool bSec8_conditionals(uint controlVar)
        {
            ushort controlValue = (ushort)(controlVar >> 16);
            sec8_conditional condition = (sec8_conditional)controlVar;
            switch (condition)
            {
                case sec8_conditional.unk02:
                    break;
                case sec8_conditional.unk03:
                    break;
                case sec8_conditional.SegmentId: //[145F7F]
                    return Module_world_debug.GetRealSegmentId() == controlValue;
                case sec8_conditional.unk07:
                    break;
                case sec8_conditional.CheckForVehicleGroup:
                    break;
                case sec8_conditional.unk0f:
                    break;
                case sec8_conditional.unk10:
                    break;
                case sec8_conditional.unk11:
                    break;
                case sec8_conditional.unk12:
                    break;
                case sec8_conditional.unk17:
                    break;
                case sec8_conditional.unk18:
                    break;
                case sec8_conditional.unk19:
                    break;
                case sec8_conditional.unk1a:
                    break;
                case sec8_conditional.unk1b:
                    break;
                case sec8_conditional.unk1c:
                    break;
                case sec8_conditional.unk1d:
                    break;
                case sec8_conditional.unk1e:
                    break;
                case sec8_conditional.unk20:
                    break;
                case sec8_conditional.unk21:
                    break;
                case sec8_conditional.unk22:
                    break;
                case sec8_conditional.unk25:
                    break;
                case sec8_conditional.unk27:
                    break;
                case sec8_conditional.unk29:
                    break;
                case sec8_conditional.unk2a:
                    break;
                case sec8_conditional.unk2c:
                    break;
                case sec8_conditional.unk2d:
                    break;
                case sec8_conditional.unk2f:
                    break;
                case sec8_conditional.unk30:
                    break;
                case sec8_conditional.unk31:
                    break;
                case sec8_conditional.unk32:
                    break;
                case sec8_conditional.unk33:
                    break;
                case sec8_conditional.unk34:
                    break;
                case sec8_conditional.unk35:
                    break;
                case sec8_conditional.unk38:
                    break;
                case sec8_conditional.unk39:
                    break;
                default:
                    break;
            }
            return false;
        }
        #endregion

        #region Section 10 - [UNKNOWN]/ Something with vehicles, positions- probably wm2field in vehicle (?)
        /// <summary>
        /// This file follows some schema of 0xFF01->0xFF04
        /// </summary>
        private void Section10()
        {

        }
        #endregion

        #region Section 14 - Side quest strings
        FF8String[] sideQuestDialogues;

        private void Section14()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[14 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sideQuestDialogues = new FF8String[innerSec.Length];
                for(int i = 0; i<innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[14 - 1] + innerSec[i], SeekOrigin.Begin);
                    sideQuestDialogues[i] = Extended.GetBinaryString(br);
                }
            }
        }

        public FF8String GetSection14Text(int index) => sideQuestDialogues[index];
        #endregion

        #region Section 16 - World map objects and vehicles

        private struct s16Model
        {
            public ushort cTriangles;
            public ushort cQuads;
            public ushort texPage;
            public ushort cVerts;
            public s16Triangle[] triangles;
            public s16Quad[] quads;
            public Vector4[] vertices;
        }

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =12)]
        private struct s16Triangle
        {
            public byte A, B, C;
            public byte semitransp;
            public byte ua, va;
            public byte ub, vb;
            public byte uc, vc;
            public ushort clut;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
        private struct s16Quad
        {
            public byte A, B, C, D;
            public byte ua, va;
            public byte ub, vb;
            public byte uc, vc;
            public byte ud, vd;
            public ushort clut;
            public byte semitransp;
            public byte unk;
        }

        private s16Model[] s16Models;

        private const float s16Scale = 16f;

        private void Section16()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[16 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                s16Models = new s16Model[innerSec.Length];
                for (int i = 0; i < innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[16 - 1] + innerSec[i], SeekOrigin.Begin);
                    s16Models[i].cTriangles = br.ReadUInt16();
                    s16Models[i].cQuads = br.ReadUInt16();
                    s16Models[i].texPage = br.ReadUInt16();
                    s16Models[i].cVerts = br.ReadUInt16();
                    s16Models[i].triangles = new s16Triangle[s16Models[i].cTriangles];
                    s16Models[i].quads = new s16Quad[s16Models[i].cQuads];
                    s16Models[i].vertices = new Vector4[s16Models[i].cVerts];
                    for (int n = 0; n < s16Models[i].cTriangles; n++)
                        s16Models[i].triangles[n] = Extended.ByteArrayToStructure<s16Triangle>(br.ReadBytes(Marshal.SizeOf(typeof(s16Triangle))));
                    for (int n = 0; n < s16Models[i].cQuads; n++)
                        s16Models[i].quads[n] = Extended.ByteArrayToStructure<s16Quad>(br.ReadBytes(Marshal.SizeOf(typeof(s16Quad))));
                    for (int n = 0; n < s16Models[i].cVerts; n++)
                        s16Models[i].vertices[n] = new Vector4(
                            br.ReadInt16() / s16Scale,
                            br.ReadInt16() / s16Scale,
                            br.ReadInt16() / s16Scale,
                            br.ReadUInt16()
                            );

                }
            }
        }

        /// <summary>
        /// Gets simple geometry for vehicles. Takes the objectId, local Vector3 and quaternion for rotation. Returns tuple with data for
        /// renderer and byte[] with clut Ids
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="localTranslation"></param>
        /// <param name="rotation"></param>
        /// <param name="textureResolution">texture resolution for custom UV recalculation</param>
        /// <param name="textureOriginVector">texture vector for origin (x-832 based) and y zero based for custom UV recalculation</param>
        /// <returns></returns>
        public Tuple<VertexPositionTexture[], byte[]> GetVehicleGeometry(int objectId, Vector3 localTranslation, Quaternion rotation, Vector2? textureResolution = null, Vector2? textureOriginVector = null)
        {
            //This ones are simple- static meshes that are translated only by basic localTranslation and quaternion. Nothing much

            List<VertexPositionTexture> vptList = new List<VertexPositionTexture>();
            List<byte> vptTextureIndexList = new List<byte>();
            //step 1. grab triangles
            if (objectId > s16Models.Length)
                return new Tuple<VertexPositionTexture[], byte[]>(new VertexPositionTexture[0], new byte[0]); //error
            s16Model Model = s16Models[objectId];
            float texWidth = 256f;
            float texHeight = 256f;
            byte localXadd = 0;
            byte localYadd = 0;
            if(textureResolution != null)
            { texWidth = textureResolution.Value.X; texHeight = textureResolution.Value.Y; };
            if(textureOriginVector != null)
            { localXadd = (byte)textureOriginVector.Value.X; localYadd = (byte)textureOriginVector.Value.Y; }

            for(int i = 0; i<Model.cTriangles; i++)
            {
                Vector3 a = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].A], true);
                Vector3 b = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].B], true);
                Vector3 c = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].C], true);
                a= Vector3.Transform(a, Matrix.CreateFromQuaternion(rotation));
                b= Vector3.Transform(b, Matrix.CreateFromQuaternion(rotation));
                c= Vector3.Transform(c, Matrix.CreateFromQuaternion(rotation));
                a += localTranslation;
                b += localTranslation;
                c += localTranslation;

                vptList.Add(new VertexPositionTexture(
                    a, new Vector2(
                        (Model.triangles[i].ua-localXadd)/texWidth,
                        (Model.triangles[i].va-localYadd)/texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    b, new Vector2(
                        (Model.triangles[i].ub-localXadd)/texWidth,
                        (Model.triangles[i].vb-localYadd)/texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    c, new Vector2(
                        (Model.triangles[i].uc-localXadd)/texWidth,
                        (Model.triangles[i].vc-localYadd)/texHeight)
                    ));

                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
            }

            for (int i = 0; i < Model.cQuads; i++)
            {
                Vector3 a = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].A], true);
                Vector3 b = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].B], true);
                Vector3 c = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].C], true);
                Vector3 d = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].D], true);
                a = Vector3.Transform(a, Matrix.CreateFromQuaternion(rotation));
                b = Vector3.Transform(b, Matrix.CreateFromQuaternion(rotation));
                c = Vector3.Transform(c, Matrix.CreateFromQuaternion(rotation));
                d = Vector3.Transform(d, Matrix.CreateFromQuaternion(rotation));
                a += localTranslation;
                b += localTranslation;
                c += localTranslation;
                d += localTranslation;

                vptList.Add(new VertexPositionTexture(
                    a, new Vector2(
                        (Model.quads[i].ua-localXadd) / texWidth,
                        (Model.quads[i].va-localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    b, new Vector2(
                        (Model.quads[i].ub - localXadd) / texWidth,
                        (Model.quads[i].vb - localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    d, new Vector2(
                        (Model.quads[i].ud - localXadd) / texWidth,
                        (Model.quads[i].vd - localYadd) / texHeight)
                    ));

                vptList.Add(new VertexPositionTexture(
                    a, new Vector2(
                        (Model.quads[i].ua - localXadd) / texWidth,
                        (Model.quads[i].va - localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    c, new Vector2(
                        (Model.quads[i].uc - localXadd) / texWidth,
                        (Model.quads[i].vc - localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    d, new Vector2(
                        (Model.quads[i].ud - localXadd) / texWidth,
                        (Model.quads[i].vd - localYadd) / texHeight)
                    ));

                vptTextureIndexList.Add((byte)Model.quads[i].clut);
                vptTextureIndexList.Add((byte)Model.quads[i].clut);
                vptTextureIndexList.Add((byte)Model.quads[i].clut);
                vptTextureIndexList.Add((byte)Model.quads[i].clut);
                vptTextureIndexList.Add((byte)Model.quads[i].clut);
                vptTextureIndexList.Add((byte)Model.quads[i].clut);
            }

            return new Tuple<VertexPositionTexture[], byte[]>(vptList.ToArray(), vptTextureIndexList.ToArray());

        }
        public int GetVehicleModelsCount() => s16Models.Length;

        #endregion

        #region Section 17 + 41 - World map texture animations for beach[17] and water[41]
        /*
         * Section 17 is responsible for beach texture animations. It's a double file- first it contains chunks. Every chunk contains
         * animation frames information.
         * There's also new texture structure; I haven't seen such structures earlier. Palette is grabbed from section38
         */

         //MODDING NOTE: Thanks to OpenVIII you can now create bigger chunks than 64x64/64x32 without worrying about SSIGPU VRAM!

            [StructLayout(LayoutKind.Sequential, Pack =1, Size =8)]
        public struct textureAnimation
        {
            /// <summary>
            /// Unknown
            /// </summary>
            public byte unk;
            /// <summary>
            /// Animation speed/timeout between frames- usually 0x20. 1 is the fastest. 0 is invalid. 0xFF is long
            /// </summary>
            public byte animTimeout;
            /// <summary>
            /// Frames count- controls how many frames are valid. Usually 4
            /// </summary>
            public byte framesCount;
            /// <summary>
            /// If true then frames loop backward. For example 0-1-2-3-2-1-0. If false, then 0-1-2-3-0-1-2-3
            /// </summary>
            public byte bLooping;
            /// <summary>
            /// Unused in openviii. It is starting VRAM offset for this texture to be stored
            /// </summary>
            public ushort vramOrigX;
            /// <summary>
            /// Unused in openviii. It is starting VRAM offset for this texture to be stored
            /// </summary>
            public ushort vramOrigY;
            /// <summary>
            /// Contains texture data for given frame
            /// </summary>
            /// Did you know that there's no way to tell C# to not marshal one field?
            public Texture2D[] framesTextures;
            /// <summary>
            /// Contains palette data for given frame- because section41 is palette (version 17)
            /// </summary>
            public Color[][] framesPalette;

            /// <summary>
            /// OpenVIII helper value- if true then index is incrementing- if false then index is decrementing
            /// </summary>
            public bool bIncrementing;
            /// <summary>
            /// OpenVIII helper value- holds current frame index
            /// </summary>
            public int currentAnimationIndex;
            /// <summary>
            /// OpenVIII helper value- holds total deltaTime to be used with timeout calculation
            /// </summary>
            public float deltaTime;
        }

        private textureAnimation[] beachAnimations;
        private textureAnimation[] waterAnimations;
        public void Section17()
        {
            int[] innerPointers;
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[17 - 1], SeekOrigin.Begin);
                innerPointers = GetInnerPointers(br);
                BeachAnimations = new textureAnimation[innerPointers.Length];
                for (int i = 0; i < innerPointers.Length; i++)
                    BeachAnimations[i] = textureAnimation_ParseBlock(sectionPointers[17 - 1] + innerPointers[i], i ,ms, br);
                //for (int i = 0; i < beachAnimations.Length; i++)
                //    for (int n = 0; n < beachAnimations[i].framesCount; n++)
                //        Extended.DumpTexture(beachAnimations[i].framesTextures[n], $"D:\\b_{i}_{n}.png");
            }
        }

        public void Section41()
        {
            int[] innerPointers;
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[41 - 1], SeekOrigin.Begin);
                innerPointers = GetInnerPointers(br);
                waterAnimations = new textureAnimation[innerPointers.Length];
                for (int i = 0; i < innerPointers.Length; i++)
                    waterAnimations[i] = textureAnimation_ParseBlock(sectionPointers[41 - 1] + innerPointers[i], -1, ms, br);
            }
        }

        private const int sec17_imageHeaderSize = 12; //dword;dword; sizeDword

        /// <summary>
        /// Valid for section 41 and 17!
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="texturePointer">-1 means section41</param>
        /// <param name="ms"></param>
        /// <param name="br"></param>
        /// <returns></returns>
        private textureAnimation textureAnimation_ParseBlock(int offset, int texturePointer, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(offset, SeekOrigin.Begin);
            //beachAnimation animation = Extended.ByteArrayToStructure<beachAnimation>(br.ReadBytes(8));
            textureAnimation animation = new textureAnimation() //not using Extension because Marshalling doesn't go well with [][] even
                                                            //though it's not really even used!
            {
                unk = br.ReadByte(),
                animTimeout = br.ReadByte(),
                framesCount = br.ReadByte(),
                bLooping = br.ReadByte(),
                vramOrigX = br.ReadUInt16(),
                vramOrigY = br.ReadUInt16()
            };
            if (texturePointer == -1)
                ms.Seek(4, SeekOrigin.Current); //there are two WORD's that feel like they have again the palette X and Y but why??
            uint preImagePosition = (uint)ms.Position;
            uint[] imagePointers = new uint[animation.framesCount];
            Color[] palette = null;
            Color[][] customPalette = null;
            if(texturePointer!= -1)
                palette = GetWorldMapTexturePalette(texturePointer==0 ?
                Section38_textures.beach : Section38_textures.beachE, 0);
            for (int i = 0; i < animation.framesCount; i++)
                imagePointers[i] = br.ReadUInt32() + preImagePosition;
            Texture2D[] animationFrames = null;
            if (texturePointer != -1)
                animationFrames = new Texture2D[imagePointers.Length];
            else
            {
                customPalette = new Color[imagePointers.Length][];
                animationFrames = new Texture2D[imagePointers.Length];
            }
            for(int i = 0; i<animation.framesCount; i++)
            {
                ms.Seek(imagePointers[i], SeekOrigin.Begin);
                uint unknownHeader = br.ReadUInt32();
                uint unknownHeader_ = br.ReadUInt32();
                if (unknownHeader != 18 && unknownHeader != 17) //I don't know why 18 [those are some version or flags?]
                    throw new Exception("wmset::section17::texturePointerHeader != 17 or 18");
                if (unknownHeader_ != 1) //I don't know why 1
                    throw new Exception("wmset::section17::texturePointerHeader+4 != 1");
                _ = br.ReadUInt32() - sec17_imageHeaderSize; //imageSize, but doesn't really matter here
                _ = br.ReadUInt32(); //unknown
                ushort width = (ushort)(br.ReadUInt16()*2);
                ushort height = br.ReadUInt16();
                Texture2D texture;
                Color[] texBuffer;
                if (texturePointer != -1)
                {
                    texture = new Texture2D(Memory.graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                    texBuffer = new Color[width * height]; //32bpp because Color is ARGB byte : struct

                    if (palette.Length == 16)
                    {
                        for (int m = 0; m < texBuffer.Length; m += 2)
                        {
                            byte b = br.ReadByte();
                            texBuffer[m] = palette[b & 0xF];
                            texBuffer[m + 1] = palette[b >> 4];
                        }
                    }
                    else
                        for (int m = 0; m < texBuffer.Length; m++)
                        {
                            byte b = br.ReadByte();
                            texBuffer[m] = palette[b];
                        }
                    texture.SetData(texBuffer);
                    animationFrames[i] = texture;
                }
                else
                    {
                    width /= 2;
                    customPalette[i] = new Color[width]; //in sec41 width is the number of colours
                    for(int m=0; m<width; m++)
                    {
                        ushort color = br.ReadUInt16();
                        customPalette[i][m] = Texture_Base.ABGR1555toRGBA32bit(color);
                    }
                    }
            }
            animation.framesTextures = animationFrames;
            animation.framesPalette = customPalette;
            return animation;
        }

        public textureAnimation GetBeachAnimation(int animationId) => BeachAnimations[animationId];

        /// <summary>
        /// Gets chunk from beachAnim atlas (because they are structured 2x2)
        /// </summary>
        /// <param name="animationId">index of the animation wanted- there are two beach anim sets and one unknown</param>
        /// <param name="frameId">naturally the frame/keyframe of the animation</param>
        /// <returns></returns>
        public Texture2D GetBeachAnimationTextureFrame(int animationId, int frameId)
            => BeachAnimations[animationId].framesTextures[frameId];

        public Color[] GetWaterAnimationPalettes(int animId, int frameId) => waterAnimations[animId].framesPalette[frameId].ToArray();

        public Texture2D GetWaterAnimationTextureFrame(int animationId, int frameId) => WaterAnimations[animationId].framesTextures[frameId];

        internal textureAnimation[] BeachAnimations { get => beachAnimations; set => beachAnimations = value; }

        internal textureAnimation[] WaterAnimations { get => waterAnimations; set => waterAnimations = value; }

        #endregion

        #region Section 32 - World map location names
        FF8String[] locationsNames;
        private void Section32()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[32 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                locationsNames = new FF8String[innerSec.Length];
                for(int i = 0; i<locationsNames.Length; i++)
                {
                    ms.Seek(sectionPointers[32 - 1] + innerSec[i], SeekOrigin.Begin);
                    locationsNames[i] = Extended.GetBinaryString(br);
                }
            }
        }

        public FF8String GetLocationName(int index) => locationsNames[index];
        #endregion

        #region Section 38 - World map textures archive
        /// <summary>
        /// Section 38: World map textures archive
        /// </summary>
        ///
        private List<TextureHandler[]> sec38_textures;
        private List<Color[][]> sec38_pals; //because other sections rely on palettes of wmset.38

        TIM2 waterTim;
        TIM2 waterTim2;
        TIM2 waterTim3;
        TIM2 waterTim4;

        public enum Section38_textures
        {
            wmtex0,
            wmtex1,
            wmtex2,
            wmtex3,
            wmtex4,
            wmtex5,
            wmtex6,
            wmtex7,
            waterTex,
            moon,
            clouds,
            worldmapMinimap,
            wmunk12,
            wmunk13,
            wmfx14,
            wmfx_bush,
            waterTex2,
            waterTex3,
            waterTex4,
            waterfall,
            waterTex5,
            beach,
            beachE,
            waterTex6,
            minimapPointer,
            minimapFullScreenPointer,
            wmfx26,
            wmunk27,
            wmfx28,
            wmunk29,
            wmunk30,
            wmunk31,
            wmfx32,
            wmunk33,
            shadowBig,
            magicBarrier
        }

        private void Section38()
        {
            sec38_pals = new List<Color[][]>();
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[38 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sec38_textures = new List<TextureHandler[]>();
                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[38 - 1] + innerSec[i]));
                    if (i == (int)Section38_textures.waterTex2)
                        waterTim = tim;
                    if (i == (int)Section38_textures.waterTex3)
                        waterTim2 = tim;
                    if (i == (int)Section38_textures.waterTex4)
                        waterTim3 = tim;
                    if (i == (int)Section38_textures.waterfall)
                        waterTim4 = tim;
                    if (tim.GetBpp==4)
                        if(tim.GetClutSize != (tim.GetClutCount*tim.GetColorsCountPerPalette)) //broken header, force our own values
                        {
                            tim.ForceSetClutColors(16);
                            tim.ForceSetClutCount((ushort)(tim.GetClutSize / 16));
                        }
                    sec38_textures.Add(new TextureHandler[tim.GetClutCount]);
                    sec38_pals.Add(new Color[tim.GetClutCount][]);
                    for (ushort k = 0; k < sec38_textures[i].Length; k++)
                    {
                        Color[] table;
                        table = tim.GetPalette(k);
                        sec38_pals[i][k] = table;
                        sec38_textures[i][k] = TextureHandler.Create($"wmset_tim38_{(i + 1).ToString("D2")}.tim", tim, k, table);
                    }
                }
            }
            CreateWaterTextureAtlas();
        }

        private const int WATERBLOCKTEXW = 64; //should be probably dynamic to conform mods
        private const int WATERBLOCKTEXH = 64;

        Texture2D waterAtlas;

        /// <summary>
        /// Creates atlas with WaterBlockWidth*4 px based on 8BPP TIMs (for animated WM). Therefore user can replace 
        /// individual texture without breaking the atlas as it's created basing on TextureHandler
        /// but their size must conform 4x2 layout where each block has the SAME resolution
        /// </summary>
        private void CreateWaterTextureAtlas()
        {
            /*
             * for animated water materials there's ANOTHER water texture that is NOT watertex, but is rather built from various 
             * blocks in independent TIMs- they are NOT 4BPP but 8BPP- although they are not correctly ordered- once again
             * their VRAM x and y kick-in, but we got it covered- the blocks are 4x4 (64px per block). The blocks order is:
             *       0        64     128       192
             *  0   [24]     [17]    [22]      [23]
             *  64  [18]     [19]    [20]      [21]
             */
            waterAtlas = new Texture2D(Memory.graphics.GraphicsDevice, WATERBLOCKTEXW << 2, WATERBLOCKTEXH<<1, false, SurfaceFormat.Color); //64<<2 is 256

            WaterTextureAtlasPutChunk(Section38_textures.waterTex6,0,0); // 0 0 
            WaterTextureAtlasPutChunk(Section38_textures.waterTex2,WATERBLOCKTEXW,0); // 64 0 
            WaterTextureAtlasPutChunk(Section38_textures.beach,WATERBLOCKTEXW*2,0); // 128 0 
            WaterTextureAtlasPutChunk(Section38_textures.beachE,WATERBLOCKTEXW*3,0); // 192 0 
            WaterTextureAtlasPutChunk(Section38_textures.waterTex3,0,WATERBLOCKTEXH); // 0 64 
            WaterTextureAtlasPutChunk(Section38_textures.waterTex4,WATERBLOCKTEXW,WATERBLOCKTEXH); // 64 64 
            WaterTextureAtlasPutChunk(Section38_textures.waterfall,WATERBLOCKTEXW*2,WATERBLOCKTEXH); // 128 64
            WaterTextureAtlasPutChunk(Section38_textures.waterTex5,WATERBLOCKTEXW*3,WATERBLOCKTEXH); // 192 64
            Extended.DumpTexture(waterAtlas, "D:/test.png");
        }

        private void WaterTextureAtlasPutChunk(Section38_textures textureIndex, int x, int y)
        {
            Texture2D atlasChunk = (Texture2D)GetWorldMapTexture(textureIndex, 0); //there's only one clut
            byte[] chunkBuffer = new byte[WATERBLOCKTEXW * WATERBLOCKTEXH * 4];
            atlasChunk.GetData(level: 0, rect: new Rectangle(0, 0, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
            waterAtlas.SetData(0, new Rectangle(x, y, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
        }

        private void UpdateWaterTextureAtlasChunk(Texture2D animatedChunk, int x, int y)
        {
            byte[] chunkBuffer = new byte[WATERBLOCKTEXW * WATERBLOCKTEXH * 4];
            animatedChunk.GetData(level: 0, rect: new Rectangle(0, 0, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
            waterAtlas.SetData(0, new Rectangle(x, y, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
        }

        /// <summary>
        /// Gets textures from section 38
        /// </summary>
        /// <param name="index"></param>
        /// <param name="clut"></param>
        /// <returns></returns>
        public TextureHandler GetWorldMapTexture(Section38_textures index, int clut)
            => sec38_textures[(int)index][clut];

        /// <summary>
        /// Gets the pre-made atlas from TIMs- do not change to TextureHandler! This atlas
        /// is final product from moddable TextureHandlers- it shouldn't be directly changed
        /// </summary>
        /// <returns></returns>
        public Texture2D GetWorldMapWaterTexture() => waterAtlas;

        public Color[] GetWorldMapTexturePalette(Section38_textures index, int clut)
        => sec38_pals[(int)index][clut];

        public void UpdateWorldMapWaterTexturePaletteForAnimation(int index, Color[] palette)
        {
            if (waterTim == null)
                return;
            Texture2D animatedChunk = null;
            int x = 0, y = 0;
            switch(index)
            {
                case 0:
                    animatedChunk = waterTim2.GetTexture(palette);
                    x = 0;
                    y = WATERBLOCKTEXH;
                    break;
                case 3:
                    animatedChunk = waterTim.GetTexture(palette);
                    x = WATERBLOCKTEXW;
                    y = 0;
                    break;
                case 2:
                    animatedChunk = waterTim4.GetTexture(palette);
                    x = WATERBLOCKTEXW*2;
                    y = WATERBLOCKTEXH;
                    break;
                case 1:
                    animatedChunk = waterTim3.GetTexture(palette);
                    x = WATERBLOCKTEXW;
                    y = WATERBLOCKTEXH;
                    break;
            }
            UpdateWaterTextureAtlasChunk(animatedChunk, x, y);
        }

        #endregion

        #region Section 39 - Textures of roads, train tracks and bridges

        const int SEC39_VRAM_STARTX = 832; //this is beginning of origX to map to one texture
        const int SEC39_VRAM_STARTY = 256; //used to map VRAM, but here it's used to create new atlas
        const int VRAM_TEXBLOCKWIDTH = 256; //wm faces ask VRAM, not texture, so the block is 256px in VRAM + additional chunks from other files that we deal normally as Tex2D[]
        const int VRAM_TEXBLOCKHEIGHT = 256; //see above
        private const int VRAM_BLOCKSIZE = 32; // =VRAM_BLOCKSTEP*4 - one origX of 16 is actually 16/2=8*32=finalXorig
        private const int VRAM_BLOCKSTEP = 8;

        private TextureHandler sec39_texture;

        /// <summary>
        /// Section 39: Textures of roads, train tracks and bridge
        /// </summary>
        private void Section39()
        {
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[39 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                Texture2D sec39_texture = new Texture2D(Memory.graphics.GraphicsDevice, VRAM_TEXBLOCKWIDTH, VRAM_TEXBLOCKHEIGHT, false, SurfaceFormat.Color);

                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i]));
                    Texture2D atlasChunk = tim.GetTexture(0);
                    byte[] chunkBuffer = new byte[atlasChunk.Width * atlasChunk.Height * 4];
                    atlasChunk.GetData(chunkBuffer, 0, chunkBuffer.Length);
                    int newX = tim.GetOrigX - SEC39_VRAM_STARTX;
                    int newY = tim.GetOrigY - SEC39_VRAM_STARTY;
                    newX = (newX / VRAM_BLOCKSTEP) * VRAM_BLOCKSIZE;
                    sec39_texture.SetData(0, new Rectangle(newX, newY, atlasChunk.Width, atlasChunk.Height), chunkBuffer, 0, chunkBuffer.Length);
                }
                this.sec39_texture = TextureHandler.Create($"wmset_tim39.tim", new Texture2DWrapper(sec39_texture), 0, null);
                //sec39_texture = TextureHandler.Create($"wmset_tim{(i + 1).ToString("D2")}.tim", new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i])), k, null);
            }
        }

        public enum Section39_Textures
        {
            train,
            bridgeTrack,
            trainMetal,
            trainMetalCrossTrain,
            TrainCrossTrainMetal,
            asphalt,
            dirtWay,
            dirtWay2,
            dirtWay3,
            desertWay,
            desertWay2,
            desertWay3,
            unknownMetal
        }

        /// <summary>
        /// Gets textures from Section39
        /// </summary>
        /// <returns></returns>
        public Texture2D GetRoadsMiscTextures() => (Texture2D)sec39_texture;
        #endregion

        #region Section 42 - objects and vehicles textures
        const int SEC42_VRAM_STARTX = 832; //this is beginning of origX to map to one texture

        TextureHandler[][] vehicleTextures;
        Vector2[] timOrigHolder;

        public enum VehicleTextureEnum
        {
            BalambGarden,
            BalambHalo,
            TrainA_locomotive,
            TrainA_cab,
            TrainB_locomotive,
            TrainB_cab,
            TrainC_locomotive,
            TrainC_cab,
            TrainC_cab2,
            Car1,
            Car2,
            Car3,
            TrainRoadBlockade,
            unk2,
            Vessel,
            GalbadiaGarden,
            GalbadiaHalo,
            Car4,
            Car5,
            Car6,
            Car7,
            Car8,
            Car9,
            Car10,
            WhiteSeed,
            LunaticPandora,
            LunaticPandora2,
            LunaticPanodra_inside,
            UltimeciaGate,
            UltimeciaBarrier,
            Cactuar,
            RoadBlockade,
            UltimeciaBarrier2
        }

        private void Section42()
        {
            List<TextureHandler[]> vehTextures = new List<TextureHandler[]>();
            List<Vector2> timOriginHolderList = new List<Vector2>(); //VRAM atlas, holds X and Y origins for atlasing- here for calculating new UV
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[42 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[42 - 1] + innerSec[i]));
                    timOriginHolderList.Add(new Vector2((tim.GetOrigX - SEC42_VRAM_STARTX)*4, tim.GetOrigY));
                    vehTextures.Add(new TextureHandler[tim.GetClutCount]);
                    for (ushort k = 0; k < vehTextures[i].Length; k++)
                        vehTextures[i][k] = TextureHandler.Create($"wmset_tim42_{(i + 1).ToString("D2")}.tim", tim, k, null);
                }
            }
            vehicleTextures = vehTextures.ToArray();
            timOrigHolder = timOriginHolderList.ToArray();
        }

        public TextureHandler GetVehicleTexture(int index, int clut)
            => vehicleTextures[index][clut];

        public TextureHandler GetVehicleTexture(VehicleTextureEnum index, int clut) => vehicleTextures[(int)index][clut];

        /// <summary>
        /// Gets X and Y tim origin (psx VRAM) for recalculating UV
        /// </summary>
        /// <param name="index"></param>
        /// <param name="clut"></param>
        /// <returns></returns>
        public Vector2 GetVehicleTextureOriginVector(VehicleTextureEnum index, int clut) => timOrigHolder[(int)index];
        /// <summary>
        /// Gets X and Y tim origin (psx VRAM) for recalculating UV
        /// </summary>
        /// <param name="index"></param>
        /// <param name="clut"></param>
        /// <returns></returns>
        public Vector2 GetVehicleTextureOriginVector(int index, int clut) => timOrigHolder[index];

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                waterAtlas.Dispose();
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~wmset() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}

/*
 * Snippet for section w/ inner pointers
 *
 *
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[8 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
            }
*/
