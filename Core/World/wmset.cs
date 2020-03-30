using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.World
{
    public class Wmset : IDisposable
    {
        private const int WMSET_SECTION_COUNT = 48;
        private byte[] buffer;
        private int[] sectionPointers;

        /// <summary>
        /// wmset file is pseudo-archive containing 48 sections in which every 'chunk' has different data and meaning
        /// </summary>
        /// <param name="wmsetBuffer"></param>
        public Wmset(byte[] wmsetBuffer)
        {
            buffer = wmsetBuffer;
            sectionPointers = new int[WMSET_SECTION_COUNT];
            using (var br = new BinaryReader(new MemoryStream(buffer)))
            {
                for (var i = 0; i < sectionPointers.Length; i++)
                    sectionPointers[i] = br.ReadInt32();
            }

            //if there's no section method either uncommented or commented out, then the section that is lacking is 4 byte NULL

            Section1(); //======FINISHED [Encounter setting]
            Section2(); //======FINISHED [World map regions]
            //Section3(); //=encounter - looks like it's some supply helping data or roll data? Not sure, it's way before getting encounters
            Section4(); //======FINISHED [Encounter pointer]
            Section6(); //======FINISHED [Encounter pointer (Lunar cry)]
            Section8(); //wm2field WIP - DEBUG data only, almost completed
            Section9(); //======FINISHED [Field to world map coordinates]
            Section11(); //??????
            //Section12(); //[UNKNOWN] Scripts- maybe some additional warp zones?
            //Section13(); // 12b per entry- SUB_548940
            Section14(); //======FINISHED [Side quest strings]
            Section16(); //======FINISHED [objects and vehicles]
            //Section18(); //?????
            //Section19(); //=something with regions: it's that island with many drawpoints- regions setting
            //Section31(); //????? - referenced by FullScreen map - prob. 12 bytes per entry, where 3rd byte is locationPointer
            Section32(); //======FINISHED [location names]
            Section33(); //=SKY GRADIENT/REGION COLOURING
            //Section34(); //related to 14- something with side quests
            //Section35(); //=draw points
            //Section36(); //?????????
            //Section37(); //Ufo, Pupu and Thrusta encounters
            Section38(); //======FINISHED [textures archive]
            Section39(); //======FINISHED [textures of roads, train tracks and bridges]
            Section42(); //======FINISHED [object and vehicle textures]
            Section41(); //======FINISHED [texture animation for water] *DO NOT MOVE UP- sec38 needs to be parsed before this
            Section17(); //======FINISHED [texture animation for beach] *DO NOT MOVE UP- sec38 needs to be parsed before this

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
            //Section7(); //helpers for roads textures- as we have it working and no issues this will be useless to reverse/investigate
            //Section10(); //Looks like it's a helper for model and texture on world map objects. By altering some variables
                            //you are able to use e.g. Selphie texture on Ragnarok- 0xFF13 - arg=texIndex
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
            var innerSections = new List<int>();
            var eof = -1;
            while ((eof = br.ReadInt32()) != 0)
                innerSections.Add(eof);
            return innerSections.ToArray();
        }

        #region Section 1 - Encounter setting

        /// <summary>
        /// Section1 helps providing the correct encounter based on groundId and regionId, then provides the pointer to struct of section4
        /// </summary>
        private const int SECTION1ENTRYCOUNT = 96;

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
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
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[1 - 1], SeekOrigin.Begin);
                ms.Seek(4, SeekOrigin.Current); //skip first DWORD- it's EOF of global file
                var encounterHelperList = new List<EncounterHelper>();
                while (true)
                {
                    var entry = Extended.ByteArrayToStructure<EncounterHelper>(br.ReadBytes(4));
                    if (entry.groundId == 0 && entry.regionId == 0 && entry.encounterPointer == 0)
                        break;
                    encounterHelperList.Add(entry);
                }
                encounterHelpEntries = encounterHelperList.ToArray();
            }
        }

        public ushort GetEncounterHelperPointer(int regionId, int groundId)
        {
            var encList = encounterHelpEntries.Where(x => x.groundId == groundId && x.regionId == regionId);
            if (!encList.Any())
                return 0xFFFF;
            return encList.First().encounterPointer; //always first, if there are two the same entries then it doesn't make sense- priority is over that one that is first
            
        }

        #endregion Section 1 - Encounter setting

        #region Section 2 - world map regions

        private byte[] regionsBuffer;

        private void Section2()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[2 - 1], SeekOrigin.Begin);
                regionsBuffer = br.ReadBytes(768);
            }
        }

        public byte GetWorldRegionByBlock(int blockId) => regionsBuffer[blockId];

        public byte GetWorldRegionBySegmentPosition(int x, int y)
        {
            var regionIndex = y * 32 + x;
            if (regionIndex >= regionsBuffer.Length || regionIndex < 0)
                return 255;
            else return regionsBuffer[y * 32 + x]; // I had got an exception here. For out of range. //Ok- fixed
        }

        #endregion Section 2 - world map regions

        #region Section 4 - Encounter pointer

        private const int SEC4_ENC_PER_CHUNK = 8; //there are 8 scene.out pointers per one block/entry
        public ushort[][] Encounters { get; private set; }

        private void Section4()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[4 - 1], SeekOrigin.Begin);
                var encounterList = new List<ushort[]>();
                while (true)
                {
                    var dwordTester = br.ReadUInt32();
                    if (dwordTester == 0)
                        break;
                    ms.Seek(-4, SeekOrigin.Current);
                    var sceneOutPointers = new ushort[SEC4_ENC_PER_CHUNK];
                    for (var i = 0; i < SEC4_ENC_PER_CHUNK; i++)
                        sceneOutPointers[i] = br.ReadUInt16();
                    encounterList.Add(sceneOutPointers);
                }
                Encounters = encounterList.ToArray();
            }
        }

        public ushort[] GetEncounters(int pointer) => Encounters[pointer];

        #endregion Section 4 - Encounter pointer

        #region Section 6 - Encounter pointer (Lunar Cry)

        public ushort[][] EncountersLunar { get; private set; }

        private void Section6()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[6 - 1], SeekOrigin.Begin);
                var encounterList = new List<ushort[]>();
                while (true)
                {
                    var dwordTester = br.ReadUInt32();
                    if (dwordTester == 0)
                        break;
                    ms.Seek(-4, SeekOrigin.Current);
                    var sceneOutPointers = new ushort[SEC4_ENC_PER_CHUNK];
                    for (var i = 0; i < SEC4_ENC_PER_CHUNK; i++)
                        sceneOutPointers[i] = br.ReadUInt16();
                    encounterList.Add(sceneOutPointers);
                }
                EncountersLunar = encounterList.ToArray();
            }
        }

        public ushort[] GetEncountersLunar(int pointer) => EncountersLunar[pointer];

        #endregion Section 6 - Encounter pointer (Lunar Cry)

        #region Section 8 - World map to field

        public enum worldScriptOpcodes : ushort
        {
            Mode1 = 0xFF01,
            flagBelow = 0xFF02, //word 2036BDE, on disc3 it's 0xECE
            flagAbove = 0xFF03, //see above
            Mode2 = 0xFF04, //probably so-called null animation
            EndZone = 0xFF05, //this splits the conditions- always at the end or in the middle of two zones
            SetRegionId = 0xFF06, //for example 283 (If I remember correctly) for Balamb
            playerPositionUnk = 0xFF07, //probably X and Y for worldmap
            wm2fieldEntryId = 0xFF08,
            CheckForVehicle = 0xFF09,
            Set1 = 0xFF0A,
            Set3If1OrNot4 = 0xFF0B,
            Set3IfNot2or5 = 0xFF0C,
            Set6IfNot2Or5 = 0xFF0D,
            squallXlocAbove = 0xFF0F,
            squallZLocAbove = 0xFF10,
            squallXlocBelow = 0xFF11,
            squallZlocBelow = 0xFF12,
            SetTexture = 0xFF13,
            ScriptEnd = 0xFF16,
            setUnk2 = 0xFF17,
            unkBelov2 = 0xFF1A,
            ReturnMinusOne = 0xFF1E, //return -1
            checkVehicleArgument = 0xFF25, //checks argument for vehicle Id
        }

        public struct worldMapScript
        {
            public worldScriptOpcodes opcode;
            public int argument;
        }

        public struct section8WarpZone
        {
            public int field;
            public bool bAlreadySetField; //only for openviii and testing purpouses- based on conditions one thing can

                                          //point to different fields. This is to make sure only first fieldId is set
            public int segmentId;

            public worldMapScript[] conditions;
        }

        public section8WarpZone[] section8WarpZones;

        /// <summary>
        /// Section is responsible for world map to field transition and works as a factor of conditions
        /// You have to read the header of 0xFF01 and read conditions up to 0xFF16
        /// The game engine checks given condition and returns TRUE or FALSE. If something fails, then it
        /// moves forward with checking. Currently we know wm2field pointer, vehicle conditions and
        /// that's more or less all we know
        /// </summary>
        public void Section8()
        {
            var localZones = new List<section8WarpZone>();
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[8 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for (var i = 0; i < innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[8 - 1] + innerSec[i], SeekOrigin.Begin);
                    short Mode = 0;
                    Mode = (short)br.ReadUInt32();
                    var localZone = new section8WarpZone
                    {
                        field = -1
                    };
                    var conditions = new List<worldMapScript>();
                    while (true)
                    {
                        var opcode = (worldScriptOpcodes)br.ReadUInt16();
                        var argument = br.ReadUInt16();
                        if (opcode == worldScriptOpcodes.ScriptEnd) //?? maybe
                        {
                            localZone.conditions = conditions.ToArray();
                            localZones.Add(localZone);
                            break;
                        }
                        switch (opcode)
                        {
                            case worldScriptOpcodes.SetRegionId:
                                conditions.Add(new worldMapScript() { argument = argument, opcode = opcode });
                                localZone.segmentId = argument;
                                break;

                            case worldScriptOpcodes.wm2fieldEntryId:
                                conditions.Add(new worldMapScript() { argument = argument, opcode = opcode });
                                localZone.field = argument;
                                break;

                            default:
                                conditions.Add(new worldMapScript() { argument = argument, opcode = opcode });
                                break;
                        }
                    }
                }
            }
            section8WarpZones = localZones.ToArray();
        }

        #endregion Section 8 - World map to field

        #region Section 9 - Field to World map

        public Vector3[] fieldToWorldMapLocations;

        private void Section9()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[9 - 1], SeekOrigin.Begin);
                var sectionSize = sectionPointers[10 - 1] - sectionPointers[9 - 1] - 4;
                var entriesCount = sectionSize / 12;
                fieldToWorldMapLocations = new Vector3[entriesCount];
                for (var i = 0; i < entriesCount; i++)
                {
                    var x = br.ReadInt32();
                    var z = br.ReadInt32();
                    int y = br.ReadInt16();
                    fieldToWorldMapLocations[i].X = Extended.ConvertVanillaWorldXAxisToOpenVIII(x);
                    fieldToWorldMapLocations[i].Y = Extended.ConvertVanillaWorldYAxisToOpenVIII(y);
                    fieldToWorldMapLocations[i].Z = Extended.ConvertVanillaWorldZAxisToOpenVIII(z);
                    ms.Seek(2, SeekOrigin.Current);
                }
            }
        }

        #endregion Section 9 - Field to World map

        //#region Section 10 - [UNKNOWN]/ Scripts

        //public static worldMapScript[][] section10Scripts;

        ///// <summary>
        ///// This file follows some schema of 0xFF01->0xFF04
        ///// </summary>
        //private void Section10()
        //{
        //    List<List<worldMapScript>> scripts = new List<List<worldMapScript>>();
        //    MemoryStream ms;
        //    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
        //    {
        //        ms.Seek(sectionPointers[8 - 1], SeekOrigin.Begin);
        //        int[] innerSec = GetInnerPointers(br);
        //        for (int i = 0; i < innerSec.Length; i++)
        //        {
        //            ms.Seek(sectionPointers[8 - 1] + innerSec[i], SeekOrigin.Begin);
        //            short Mode = 0;
        //            Mode = (short)br.ReadUInt32();
        //            List<worldMapScript> localScript = new List<worldMapScript>();
        //            while (true)
        //            {
        //                worldScriptOpcodes opcode = (worldScriptOpcodes)br.ReadUInt16();
        //                ushort argument = br.ReadUInt16();
        //                if (opcode == worldScriptOpcodes.EndZone) //?? maybe
        //                {
        //                    scripts.Add(localScript);
        //                    break;
        //                }
        //                localScript.Add(new worldMapScript() { argument = argument, opcode = opcode });
        //            }
        //        }
        //    }
        //    section10Scripts = scripts.Select(x => x.ToArray()).ToArray();
        //}

        //#endregion Section 10 - [UNKNOWN]/ Scripts

        #region Section 11 - [UNKNOWN]

        public Vector3[] sec11Locations;
        private void Section11()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[11 - 1], SeekOrigin.Begin);
                var entriesCount = sectionPointers[12 - 1] - sectionPointers[11 - 1] - 4;
                entriesCount /= 16;
                entriesCount--; //first entry is null
                ms.Seek(16, SeekOrigin.Current); //pass first entry, it's null
                sec11Locations = new Vector3[entriesCount];
                for(var i = 0; i<entriesCount; i++)
                {
                    var x = br.ReadInt32();
                    var z = br.ReadInt32();
                    var y = br.ReadInt32();
                    var x_ = Extended.ConvertVanillaWorldXAxisToOpenVIII(x);
                    var y_ = Extended.ConvertVanillaWorldYAxisToOpenVIII(y);
                    var z_ = Extended.ConvertVanillaWorldZAxisToOpenVIII(z);
                    int unk = br.ReadInt16();
                    int unk2 = br.ReadInt16();
                    sec11Locations[i] = new Vector3(x_, y_, z_);
                }
            }
                //16 per entry- it's related to sec10
                /*
              v26 = wmsetS11 + 16 * v17;
              v83 = *(_DWORD *)v26;
              v84 = *(_DWORD *)(v26 + 4);
              v85 = *(_DWORD *)(v26 + 8);
              v86 = *(_DWORD *)(v26 + 12);
              v88 = *(_WORD *)(v26 + 12);
              v89 = *(_DWORD *)(v26 + 12) >> 16;
              v87 = 0;
              sub_5450D0(v2, v13, (unsigned __int8)v90, (_BYTE)v90 != 80 ? 0 : 4, (int)&v87, (int)&v83);
              */
            }

        #endregion Section 11 - [UNKNOWN]

        #region Section 14 - Side quest strings

        private FF8String[] sideQuestDialogues;

        private void Section14()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[14 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sideQuestDialogues = new FF8String[innerSec.Length];
                for (var i = 0; i < innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[14 - 1] + innerSec[i], SeekOrigin.Begin);
                    sideQuestDialogues[i] = Extended.GetBinaryString(br);
                }
            }
        }

        public FF8String GetSection14Text(int index) => sideQuestDialogues[index];

        #endregion Section 14 - Side quest strings

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

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
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
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[16 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                s16Models = new s16Model[innerSec.Length];
                for (var i = 0; i < innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[16 - 1] + innerSec[i], SeekOrigin.Begin);
                    s16Models[i].cTriangles = br.ReadUInt16();
                    s16Models[i].cQuads = br.ReadUInt16();
                    s16Models[i].texPage = br.ReadUInt16();
                    s16Models[i].cVerts = br.ReadUInt16();
                    s16Models[i].triangles = new s16Triangle[s16Models[i].cTriangles];
                    s16Models[i].quads = new s16Quad[s16Models[i].cQuads];
                    s16Models[i].vertices = new Vector4[s16Models[i].cVerts];
                    for (var n = 0; n < s16Models[i].cTriangles; n++)
                        s16Models[i].triangles[n] = Extended.ByteArrayToStructure<s16Triangle>(br.ReadBytes(Marshal.SizeOf(typeof(s16Triangle))));
                    for (var n = 0; n < s16Models[i].cQuads; n++)
                        s16Models[i].quads[n] = Extended.ByteArrayToStructure<s16Quad>(br.ReadBytes(Marshal.SizeOf(typeof(s16Quad))));
                    for (var n = 0; n < s16Models[i].cVerts; n++)
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

            var vptList = new List<VertexPositionTexture>();
            var vptTextureIndexList = new List<byte>();
            //step 1. grab triangles
            if (objectId > s16Models.Length)
                return new Tuple<VertexPositionTexture[], byte[]>(new VertexPositionTexture[0], new byte[0]); //error
            var Model = s16Models[objectId];
            var texWidth = 256f;
            var texHeight = 256f;
            byte localXadd = 0;
            byte localYadd = 0;
            if (textureResolution != null)
            { texWidth = textureResolution.Value.X; texHeight = textureResolution.Value.Y; };
            if (textureOriginVector != null)
            { localXadd = (byte)textureOriginVector.Value.X; localYadd = (byte)textureOriginVector.Value.Y; }

            for (var i = 0; i < Model.cTriangles; i++)
            {
                var a = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].A], true);
                var b = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].B], true);
                var c = Extended.ShrinkVector4ToVector3(Model.vertices[Model.triangles[i].C], true);
                a = Vector3.Transform(a, Matrix.CreateFromQuaternion(rotation));
                b = Vector3.Transform(b, Matrix.CreateFromQuaternion(rotation));
                c = Vector3.Transform(c, Matrix.CreateFromQuaternion(rotation));
                a += localTranslation;
                b += localTranslation;
                c += localTranslation;

                vptList.Add(new VertexPositionTexture(
                    a, new Vector2(
                        (Model.triangles[i].ua - localXadd) / texWidth,
                        (Model.triangles[i].va - localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    b, new Vector2(
                        (Model.triangles[i].ub - localXadd) / texWidth,
                        (Model.triangles[i].vb - localYadd) / texHeight)
                    ));
                vptList.Add(new VertexPositionTexture(
                    c, new Vector2(
                        (Model.triangles[i].uc - localXadd) / texWidth,
                        (Model.triangles[i].vc - localYadd) / texHeight)
                    ));

                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
                vptTextureIndexList.Add((byte)Model.triangles[i].clut);
            }

            for (var i = 0; i < Model.cQuads; i++)
            {
                var a = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].A], true);
                var b = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].B], true);
                var c = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].C], true);
                var d = Extended.ShrinkVector4ToVector3(Model.vertices[Model.quads[i].D], true);
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
                        (Model.quads[i].ua - localXadd) / texWidth,
                        (Model.quads[i].va - localYadd) / texHeight)
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

        #endregion Section 16 - World map objects and vehicles

        #region Section 17 + 41 - World map texture animations for beach[17] and water[41]

        /*
         * Section 17 is responsible for beach texture animations. It's a double file- first it contains chunks. Every chunk contains
         * animation frames information.
         * There's also new texture structure; I haven't seen such structures earlier. Palette is grabbed from section38
         */

        //MODDING NOTE: Thanks to OpenVIII you can now create bigger chunks than 64x64/64x32 without worrying about SSIGPU VRAM!

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
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
            /// Frames Count- controls how many frames are valid. Usually 4
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
            public TimeSpan deltaTime;
        }

        private textureAnimation[] beachAnimations;
        private textureAnimation[] waterAnimations;

        public void Section17()
        {
            if (Memory.Graphics?.GraphicsDevice != null)
            {
                int[] innerPointers;
                MemoryStream ms;
                using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
                {
                    ms.Seek(sectionPointers[17 - 1], SeekOrigin.Begin);
                    innerPointers = GetInnerPointers(br);
                    BeachAnimations = new textureAnimation[innerPointers.Length];
                    for (var i = 0; i < innerPointers.Length; i++)
                        BeachAnimations[i] = textureAnimation_ParseBlock(sectionPointers[17 - 1] + innerPointers[i], i, ms, br);
                    //for (int i = 0; i < beachAnimations.Length; i++)
                    //    for (int n = 0; n < beachAnimations[i].framesCount; n++)
                    //        Extended.DumpTexture(beachAnimations[i].framesTextures[n], $"D:\\b_{i}_{n}.png");
                }
            }
        }

        public void Section41()
        {
            int[] innerPointers;
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[41 - 1], SeekOrigin.Begin);
                innerPointers = GetInnerPointers(br);
                waterAnimations = new textureAnimation[innerPointers.Length];
                for (var i = 0; i < innerPointers.Length; i++)
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
            var animation = new textureAnimation() //not using Extension because Marshalling doesn't go well with [][] even
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
            var preImagePosition = (uint)ms.Position;
            var imagePointers = new uint[animation.framesCount];
            Color[] palette = null;
            Color[][] customPalette = null;
            if (texturePointer != -1)
                palette = GetWorldMapTexturePalette(texturePointer == 0 ?
                Section38_textures.beach : Section38_textures.beachE, 0);
            for (var i = 0; i < animation.framesCount; i++)
                imagePointers[i] = br.ReadUInt32() + preImagePosition;
            Texture2D[] animationFrames = null;
            if (texturePointer != -1)
                animationFrames = new Texture2D[imagePointers.Length];
            else
            {
                customPalette = new Color[imagePointers.Length][];
                animationFrames = new Texture2D[imagePointers.Length];
            }
            for (var i = 0; i < animation.framesCount; i++)
            {
                ms.Seek(imagePointers[i], SeekOrigin.Begin);
                var unknownHeader = br.ReadUInt32();
                var unknownHeader_ = br.ReadUInt32();
                if (unknownHeader != 18 && unknownHeader != 17) //I don't know why 18 [those are some version or flags?]
                    throw new Exception("wmset::section17::texturePointerHeader != 17 or 18");
                if (unknownHeader_ != 1) //I don't know why 1
                    throw new Exception("wmset::section17::texturePointerHeader+4 != 1");
                _ = br.ReadUInt32() - sec17_imageHeaderSize; //imageSize, but doesn't really matter here
                _ = br.ReadUInt32(); //unknown
                var width = (ushort)(br.ReadUInt16() * 2);
                var height = br.ReadUInt16();
                Texture2D texture;
                Color[] texBuffer;
                if (texturePointer != -1)
                {
                    texture = new Texture2D(Memory.Graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                    texBuffer = new Color[width * height]; //32bpp because Color is ARGB byte : struct

                    if (palette.Length == 16)
                    {
                        for (var m = 0; m < texBuffer.Length; m += 2)
                        {
                            var b = br.ReadByte();
                            texBuffer[m] = palette[b & 0xF];
                            texBuffer[m + 1] = palette[b >> 4];
                        }
                    }
                    else
                        for (var m = 0; m < texBuffer.Length; m++)
                        {
                            var b = br.ReadByte();
                            texBuffer[m] = palette[b];
                        }
                    texture.SetData(texBuffer);
                    animationFrames[i] = texture;
                }
                else
                {
                    width /= 2;
                    customPalette[i] = new Color[width]; //in sec41 width is the number of colours
                    for (var m = 0; m < width; m++)
                    {
                        var color = br.ReadUInt16();
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

        #endregion Section 17 + 41 - World map texture animations for beach[17] and water[41]

        #region Section 32 - World map location names

        private FF8String[] locationsNames;

        private void Section32()
        {
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[32 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                locationsNames = new FF8String[innerSec.Length];
                for (var i = 0; i < locationsNames.Length; i++)
                {
                    ms.Seek(sectionPointers[32 - 1] + innerSec[i], SeekOrigin.Begin);
                    locationsNames[i] = Extended.GetBinaryString(br);
                }
            }
        }

        public FF8String GetLocationName(int index) => locationsNames[index];

        public int GetlocationNamesLength => locationsNames.Length;

        #endregion Section 32 - World map location names

        #region Section 33 - Sky and ambient colour
        [StructLayout(LayoutKind.Sequential,Pack =1, Size =52)]
        public struct sec33SkyEntry
        {
            public int positionX;
            public int positionY;
            public int positionZ;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] shadows;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] vehicles;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] skyGradientTop;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] skyGradientCenter;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] skyGradientBottom;
            
            public byte unk1_1; 
            public byte unk1_2; //unk1_2 - R value [0-127]
            public byte unk1_3;
            public byte unk1_4; //unk1_4 = R value [0-127]

            public byte unk2_1;
            public byte unk2_2; //unk2_2 - G value [0-127]
            public byte unk2_3;
            public byte unk2_4; //unk2_4 = G value [0-127]

            public byte unk3_1;
            public byte unk3_2; //unk3_2 - B value [0-127]
            public byte unk3_3;
            public byte unk3_4; //unk3_4 = B value [0-127]

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] unk4; //wrong place- one of the byte was switch for full colorization of wm
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] unk5; //wrong place- one of the byte was switch for full colorization of wm

            public Color GetShadowsColor() =>
                new Color(shadows[0], shadows[1], shadows[2], (byte)255);
            public Color GetVehiclesColor() =>
                new Color(vehicles[0], vehicles[1], vehicles[2], (byte)255);
            public Color GetTopBGColor() =>
                new Color(skyGradientTop[0], skyGradientTop[1], skyGradientTop[2], (byte)255);
            public Color GetCenterBGColor() =>
                new Color(skyGradientCenter[0], skyGradientCenter[1], skyGradientCenter[2], (byte)255);
            public Color GetBottomBGColor() =>
                new Color(skyGradientBottom[0], skyGradientBottom[1], skyGradientBottom[2], (byte)255);

            public Vector3 GetLocation() =>
                new Vector3()
                {
                    X = Extended.ConvertVanillaWorldXAxisToOpenVIII(positionX),
                    Y = Extended.ConvertVanillaWorldYAxisToOpenVIII(positionZ),
                    Z = Extended.ConvertVanillaWorldZAxisToOpenVIII(positionY)
                };
        }

        public sec33SkyEntry[] skyColors;
        private void Section33()
        {
            using (var ms = new MemoryStream(buffer)) //didn't know you can skip braces with using
            using (var br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[33 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                var skyEntries = new List<sec33SkyEntry>();
                for (var i = 0; i < innerSec.Length; i++)
                {
                    ms.Seek(sectionPointers[33 - 1] + innerSec[i], SeekOrigin.Begin);
                    skyEntries.Add(Extended.ByteArrayToStructure<sec33SkyEntry>(br.ReadBytes(52)));
                }

                skyColors = skyEntries.ToArray();
            }
        }
        #endregion

        #region Section 38 - World map textures archive

        /// <summary>
        /// Section 38: World map textures archive
        /// </summary>
        ///
        private List<TextureHandler[]> sec38_textures;

        private List<Color[][]> sec38_pals; //because other sections rely on palettes of wmset.38

        private TIM2 waterTim;
        private TIM2 waterTim2;
        private TIM2 waterTim3;
        private TIM2 waterTim4;

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
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[38 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sec38_textures = new List<TextureHandler[]>();
                for (var i = 0; i < innerSec.Length; i++)
                {
                    var tim = new TIM2(buffer, (uint)(sectionPointers[38 - 1] + innerSec[i]));
                    if (i == (int)Section38_textures.waterTex2)
                        waterTim = tim;
                    if (i == (int)Section38_textures.waterTex3)
                        waterTim2 = tim;
                    if (i == (int)Section38_textures.waterTex4)
                        waterTim3 = tim;
                    if (i == (int)Section38_textures.waterfall)
                        waterTim4 = tim;
                    if (tim.GetBytesPerPixel == 4)
                        if (tim.GetClutSize != (tim.GetClutCount * tim.GetColorsCountPerPalette)) //broken header, force our own values
                        {
                            tim.ForceSetClutColors(16);
                            tim.ForceSetClutCount((ushort)(tim.GetClutSize / 16));
                        }
                    sec38_textures.Add(new TextureHandler[tim.GetClutCount]);
                    sec38_pals.Add(new Color[tim.GetClutCount][]);
                    for (ushort k = 0; k < sec38_textures[i].Length; k++)
                    {
                        var table = tim.GetPalette(k);
                        sec38_pals[i][k] = table;
                        sec38_textures[i][k] = TextureHandler.Create($"wmset_tim38_{(i + 1).ToString("D2")}.tim", tim, k, table);
                    }
                }
            }
            CreateWaterTextureAtlas();
        }

        private const int WATERBLOCKTEXW = 64; //should be probably dynamic to conform mods
        private const int WATERBLOCKTEXH = 64;
        private Texture2D waterAtlas;

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
            if (Memory.Graphics?.GraphicsDevice != null)
            {
                waterAtlas = new Texture2D(Memory.Graphics.GraphicsDevice, WATERBLOCKTEXW << 2, WATERBLOCKTEXH << 1, false, SurfaceFormat.Color); //64<<2 is 256

                WaterTextureAtlasPutChunk(Section38_textures.waterTex6, 0, 0); // 0 0
                WaterTextureAtlasPutChunk(Section38_textures.waterTex2, WATERBLOCKTEXW, 0); // 64 0
                WaterTextureAtlasPutChunk(Section38_textures.beach, WATERBLOCKTEXW * 2, 0); // 128 0
                WaterTextureAtlasPutChunk(Section38_textures.beachE, WATERBLOCKTEXW * 3, 0); // 192 0
                WaterTextureAtlasPutChunk(Section38_textures.waterTex3, 0, WATERBLOCKTEXH); // 0 64
                WaterTextureAtlasPutChunk(Section38_textures.waterTex4, WATERBLOCKTEXW, WATERBLOCKTEXH); // 64 64
                WaterTextureAtlasPutChunk(Section38_textures.waterfall, WATERBLOCKTEXW * 2, WATERBLOCKTEXH); // 128 64
                WaterTextureAtlasPutChunk(Section38_textures.waterTex5, WATERBLOCKTEXW * 3, WATERBLOCKTEXH); // 192 64
                Extended.DumpTexture(waterAtlas, "D:/test.png");
            }
        }

        private void WaterTextureAtlasPutChunk(Section38_textures textureIndex, int x, int y)
        {
            var atlasChunk = (Texture2D)GetWorldMapTexture(textureIndex, 0); //there's only one clut
            var chunkBuffer = new byte[WATERBLOCKTEXW * WATERBLOCKTEXH * 4];
            atlasChunk.GetData(level: 0, rect: new Rectangle(0, 0, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
            waterAtlas.SetData(0, new Rectangle(x, y, WATERBLOCKTEXW, WATERBLOCKTEXH), chunkBuffer, 0, chunkBuffer.Length);
        }

        private void UpdateWaterTextureAtlasChunk(Texture2D animatedChunk, int x, int y)
        {
            var chunkBuffer = new byte[WATERBLOCKTEXW * WATERBLOCKTEXH * 4];
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
            switch (index)
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
                    x = WATERBLOCKTEXW * 2;
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

        #endregion Section 38 - World map textures archive

        #region Section 39 - Textures of roads, train tracks and bridges

        private const int SEC39_VRAM_STARTX = 832; //this is beginning of origX to map to one texture
        private const int SEC39_VRAM_STARTY = 256; //used to map VRAM, but here it's used to create new atlas
        private const int VRAM_TEXBLOCKWIDTH = 256; //wm faces ask VRAM, not texture, so the block is 256px in VRAM + additional chunks from other files that we deal normally as Tex2D[]
        private const int VRAM_TEXBLOCKHEIGHT = 256; //see above
        private const int VRAM_BLOCKSIZE = 32; // =VRAM_BLOCKSTEP*4 - one origX of 16 is actually 16/2=8*32=finalXorig
        private const int VRAM_BLOCKSTEP = 8;

        private TextureHandler sec39_texture;

        /// <summary>
        /// Section 39: Textures of roads, train tracks and bridge
        /// </summary>
        private void Section39()
        {
            if (Memory.Graphics?.GraphicsDevice != null)
            {
                MemoryStream ms;
                using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
                {
                    ms.Seek(sectionPointers[39 - 1], SeekOrigin.Begin);
                    var innerSec = GetInnerPointers(br);
                    var sec39_texture = new Texture2D(Memory.Graphics.GraphicsDevice, VRAM_TEXBLOCKWIDTH, VRAM_TEXBLOCKHEIGHT, false, SurfaceFormat.Color);

                    for (var i = 0; i < innerSec.Length; i++)
                    {
                        var tim = new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i]));
                        var atlasChunk = tim.GetTexture(0);
                        var chunkBuffer = new byte[atlasChunk.Width * atlasChunk.Height * 4];
                        atlasChunk.GetData(chunkBuffer, 0, chunkBuffer.Length);
                        var newX = tim.GetOrigX - SEC39_VRAM_STARTX;
                        var newY = tim.GetOrigY - SEC39_VRAM_STARTY;
                        newX = (newX / VRAM_BLOCKSTEP) * VRAM_BLOCKSIZE;
                        sec39_texture.SetData(0, new Rectangle(newX, newY, atlasChunk.Width, atlasChunk.Height), chunkBuffer, 0, chunkBuffer.Length);
                    }
                    this.sec39_texture = TextureHandler.Create($"wmset_tim39.tim", new Texture2DWrapper(sec39_texture), 0, null);
                    //sec39_texture = TextureHandler.Create($"wmset_tim{(i + 1).ToString("D2")}.tim", new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i])), k, null);
                }
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

        #endregion Section 39 - Textures of roads, train tracks and bridges

        #region Section 42 - objects and vehicles textures

        private const int SEC42_VRAM_STARTX = 832; //this is beginning of origX to map to one texture

        private TextureHandler[][] vehicleTextures;
        private Vector2[] timOrigHolder;

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
            var vehTextures = new List<TextureHandler[]>();
            var timOriginHolderList = new List<Vector2>(); //VRAM atlas, holds X and Y origins for atlasing- here for calculating new UV
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(sectionPointers[42 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for (var i = 0; i < innerSec.Length; i++)
                {
                    var tim = new TIM2(buffer, (uint)(sectionPointers[42 - 1] + innerSec[i]));
                    timOriginHolderList.Add(new Vector2((tim.GetOrigX - SEC42_VRAM_STARTX) * 4, tim.GetOrigY));
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

                waterAtlas?.Dispose();
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~wmset() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        #endregion IDisposable Support

        #endregion Section 42 - objects and vehicles textures
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