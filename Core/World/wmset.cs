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
    class wmset
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
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                for (int i = 0; i < sectionPointers.Length; i++)
                    sectionPointers[i] = br.ReadInt32();
            }

            //if there's no section method either uncommented or commented out, then the section that is lacking is 4 byte NULL

            Section1(); //======FINISHED
            Section2(); //======FINISHED
            //Section3(); //=encounter
            Section4(); //======FINISHED
            //Section5(); //=encounter
            //Section6(); //=encounter
            //Section7(); //=something with roads colours, cluts. Can't really understand why it's needed, looks like some kind of helper for VRAM?
            Section8(); //wm2field WIP
            //Section9(); //=related to field2wm?
            //Section10();
            //Section11(); //??????
            //Section12();
            //Section13();
            Section14(); //======FINISHED
            Section16(); //======FINISHED
            Section17(); //=beach animations
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
            //Section41(); //=Water animation info
            Section42(); //======FINISHED

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
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
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
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[2 - 1], SeekOrigin.Begin);
                regionsBuffer = br.ReadBytes(768);
            }
        }

        public byte GetWorldRegionByBlock(int blockId) => regionsBuffer[blockId];
        public byte GetWorldRegionBySegmentPosition(int x, int y) => regionsBuffer[y * 32 + x];
        #endregion

        #region Section 4 - encounter pointer
        private const int SEC4_ENC_PER_CHUNK = 8; //there are 8 scene.out pointers per one block/entry
        private ushort[][] encounters;
        private void Section4()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
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

        #region Section 8 - World map to field

        public void Section8()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[8 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for(int i = 0; i<innerSec.Length; i++)
                {
                    //???
                }
            }
        }
        #endregion

        #region Section 14 - Side quest strings
        FF8String[] sideQuestDialogues;

        private void Section14()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
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
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
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

        #region Section 17 - World map texture animations for beach
        /*
         * Section 17 is responsible for beach texture animations. It's a double file- first it contains chunks. Every chunk contains 
         * animation frames information. It's constant as I can see 4 frames per animation. Despite the header there's nothing much to it.
         * oh, also the texture data is not casual TIM nor TEX. Probably raw, no palette inside. Needs testing!
         */

        public void Section17()
        {
            int[] innerPointers;
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[17 - 1], SeekOrigin.Begin);
                innerPointers = GetInnerPointers(br);

                for (int i = 0; i < innerPointers.Length; i++)
                    Section17_ParseBlock(sectionPointers[17 - 1] + innerPointers[i], ms, br);

            }
        }

        private void Section17_ParseBlock(int offset, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(offset, SeekOrigin.Begin);
            byte unk = br.ReadByte(); //not sure, something like starting delay or I don't know. Like some sort of extension to timeout?
            byte animationTimeout = br.ReadByte(); //controls how fast the animation frames pass-by. 1 is the fastest. 0 means invalid? here usually 0x20

            byte framesCount = br.ReadByte();   /* usually 4, but there are 8 pointers, so any value bigger than 8 may work, but not suitable with
                                                * current file. Actually in section17 there are always eight pointers, but 5th and bigger pointer
                                                * is always pointing to the same frame which is the 4th frame. Therefore engine reads this variable
                                                * and skips any additional pointers BUT! if you designed the file to have 12-16 pointers in anim 
                                                * frames, then it would totally work. The engine (at least in vanilla) would normally read the pointers
                                                * and play the animation. Cool, huh?  */

            byte bLoop = br.ReadByte(); //if 1 then loops backward. If 0 then sequential from zero i.e. 0>1>2>3> 0>1>2>3; if 1 then i.e. 0>1>2>3 X >2>1> 0>1>2>3...

            ushort VRAMpalX = br.ReadUInt16(); //example entry 0 has 832 (816+16)
            /* in assembly VRAMpalX is actually the byte count-> 4 * (v6>>2), therefore (VRAMpalX/4)*4*/
            ushort VRAMpalY = br.ReadUInt16(); //example entry 0 has 384



            uint preImagePosition = (uint)ms.Position;
            uint[] imagePointers = new uint[framesCount]; //looks like 8 is fixed? NOTE: it's not fixed, some bitshit is deciding whether read or not next pointer bleh...
            for (int i = 0; i < framesCount; i++) 
                imagePointers[i] = br.ReadUInt32() + preImagePosition;
        }

        #endregion

        #region Section 32 - World map location names
        FF8String[] locationsNames;
        private void Section32()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
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
        private List<Texture2D[]> sec38_textures;

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
            wmunk34,
            magicBarrier
        }

        private void Section38()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[38 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sec38_textures = new List<Texture2D[]>();
                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[38 - 1] + innerSec[i]));
                    sec38_textures.Add(new Texture2D[tim.GetClutCount]);
                    for (ushort k = 0; k < sec38_textures[i].Length; k++)
                        sec38_textures[i][k] = tim.GetTexture(k, true);
                }
            }
        }

        /// <summary>
        /// Gets textures from section 38
        /// </summary>
        /// <param name="index"></param>
        /// <param name="clut"></param>
        /// <returns></returns>
        public Texture2D GetWorldMapTexture(Section38_textures index, int clut)
            => sec38_textures[(int)index][clut];

        #endregion

        #region Section 39 - Textures of roads, train tracks and bridges

        const int SEC39_VRAM_STARTX = 832; //this is beginning of origX to map to one texture
        const int SEC39_VRAM_STARTY = 256; //used to map VRAM, but here it's used to create new atlas
        const int VRAM_TEXBLOCKWIDTH = 256; //wm faces ask VRAM, not texture, so the block is 256px in VRAM + additional chunks from other files that we deal normally as Tex2D[]
        const int VRAM_TEXBLOCKHEIGHT = 256; //see above
        private const int VRAM_BLOCKSIZE = 32; // =VRAM_BLOCKSTEP*4 - one origX of 16 is actually 16/2=8*32=finalXorig
        private const int VRAM_BLOCKSTEP = 8;

        private Texture2D sec39_texture;

        /// <summary>
        /// Section 39: Textures of roads, train tracks and bridge
        /// </summary>
        private void Section39()
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[39 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                sec39_texture = new Texture2D(Memory.graphics.GraphicsDevice, VRAM_TEXBLOCKWIDTH, VRAM_TEXBLOCKHEIGHT, false, SurfaceFormat.Color);
                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i]));
                    Texture2D atlasChunk = tim.GetTexture(0, true);
                    byte[] chunkBuffer = new byte[atlasChunk.Width * atlasChunk.Height * 4];
                    atlasChunk.GetData(chunkBuffer,0, chunkBuffer.Length);
                    int newX = tim.GetOrigX - SEC39_VRAM_STARTX;
                    int newY = tim.GetOrigY - SEC39_VRAM_STARTY;
                    newX= (newX / VRAM_BLOCKSTEP) * VRAM_BLOCKSIZE;
                    sec39_texture.SetData(0, new Microsoft.Xna.Framework.Rectangle(newX, newY, atlasChunk.Width, atlasChunk.Height), chunkBuffer, 0, chunkBuffer.Length);
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
        public Texture2D GetRoadsMiscTextures(Section39_Textures index, int clut) => sec39_texture;
        #endregion

        #region Section 42 - objects and vehicles textures
        const int SEC42_VRAM_STARTX = 832; //this is beginning of origX to map to one texture

        Texture2D[][] vehicleTextures;
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
            List<Texture2D[]> vehTextures = new List<Texture2D[]>();
            List<Vector2> timOriginHolderList = new List<Vector2>(); //VRAM atlas, holds X and Y origins for atlasing- here for calculating new UV
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(sectionPointers[42 - 1], SeekOrigin.Begin);
                var innerSec = GetInnerPointers(br);
                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[42 - 1] + innerSec[i]));
                    timOriginHolderList.Add(new Vector2((tim.GetOrigX - SEC42_VRAM_STARTX)*4, tim.GetOrigY));
                    vehTextures.Add(new Texture2D[tim.GetClutCount]);
                    for (ushort k = 0; k < vehTextures[i].Length; k++)
                        vehTextures[i][k] = tim.GetTexture(k, true);
                }
            }
            vehicleTextures = vehTextures.ToArray();
            timOrigHolder = timOriginHolderList.ToArray();
        }

        public Texture2D GetVehicleTexture(int index, int clut)
            => vehicleTextures[index][clut];

        public Texture2D GetVehicleTexture(VehicleTextureEnum index, int clut) => vehicleTextures[(int)index][clut];

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