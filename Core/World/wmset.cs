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

        private List<Texture2D[]> sec38_textures;
        private Texture2D sec39_texture;


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

            //Section1();
            Section2(); //Finished
            //Section3();
            //Section4();
            //Section5();
            //Section6();
            //Section7();
            //Section8();
            //Section9();
            //Section10();
            //Section11();
            //Section12();
            //Section13();
            //Section14();
            Section16();
            //Section17();
            //Section18();
            //Section19();
            //Section20();
            //Section21();
            //Section29();
            //Section30();
            //Section31();
            //Section32();
            //Section33();
            //Section34();
            //Section35();
            //Section36();
            //Section37();
            Section38(); //Finished
            Section39(); //Finished
            //Section40();
            //Section41();
            //Section42();
            //Section43();
            //Section44();
            //Section45();
            //Section46();
            //Section47();
            //Section48();
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

        #region Sections parsing

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

        public Tuple<VertexPositionTexture[], byte[]> GetVehicleGeometry(int objectId, Vector3 localTranslation, Quaternion rotation)
        {
            //This ones are simple- static meshes that are translated only by basic localTranslation and quaternion. Nothing much

            List<VertexPositionTexture> vptList = new List<VertexPositionTexture>();
            List<byte> vptTextureIndexList = new List<byte>();
            //step 1. grab triangles
            if (objectId > s16Models.Length)
                return new Tuple<VertexPositionTexture[], byte[]>(new VertexPositionTexture[0], new byte[0]); //error
            s16Model Model = s16Models[objectId];
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
                    a, new Vector2(Model.triangles[i].ua/256f, Model.triangles[i].va/256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    b, new Vector2(Model.triangles[i].ub/256f, Model.triangles[i].vb/256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    c, new Vector2(Model.triangles[i].uc/256f, Model.triangles[i].vc/256f)
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
                    a, new Vector2(Model.quads[i].ua / 256f, Model.quads[i].va / 256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    b, new Vector2(Model.quads[i].ub / 256f, Model.quads[i].vb / 256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    d, new Vector2(Model.quads[i].ud / 256f, Model.quads[i].vd / 256f)
                    ));

                vptList.Add(new VertexPositionTexture(
                    a, new Vector2(Model.quads[i].ua / 256f, Model.quads[i].va / 256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    c, new Vector2(Model.quads[i].uc / 256f, Model.quads[i].vc / 256f)
                    ));
                vptList.Add(new VertexPositionTexture(
                    d, new Vector2(Model.quads[i].ud / 256f, Model.quads[i].vd / 256f)
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

        #endregion

        #region Section 38 - World map textures archive
        /// <summary>
        /// Section 38: World map textures archive
        /// </summary>

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


        #endregion
    }
}
