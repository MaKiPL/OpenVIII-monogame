using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private List<Texture2D[]> sec39_textures;


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

            Section38();
            Section39();
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

        #region Section38
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

        #region Section39
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
                sec39_textures = new List<Texture2D[]>();

                for (int i = 0; i < innerSec.Length; i++)
                {
                    TIM2 tim = new TIM2(buffer, (uint)(sectionPointers[39 - 1] + innerSec[i]));
                    sec39_textures.Add(new Texture2D[tim.GetClutCount]);
                    for (ushort k = 0; k < sec39_textures[i].Length; k++)
                        sec39_textures[i][k] = tim.GetTexture(k, true);
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
        public Texture2D GetRoadsMiscTextures(Section39_Textures index, int clut) => sec38_textures[(int)index][clut];
        #endregion


        #endregion
    }
}
