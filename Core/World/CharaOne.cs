using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.World
{
    internal class CharaOne
    {
        private Debug_MCH[] mchInstances;
        private TextureHandler[] textures;
        private byte[] buffer;

        /// <summary>
        /// Reads chara.one buffer -&gt; chara one is a file packed with MCH files and TIM textures
        /// without pointers. File needs to be scanned
        /// </summary>
        /// <param name="buffer">Full chara.one file</param>
        public CharaOne(byte[] buffer)
        {
            this.buffer = buffer;
            List<Debug_MCH> mchs = new List<Debug_MCH>();
            List<TextureHandler> texturesList = new List<TextureHandler>();
            int i = 0;

            MemoryStream ms = null;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(this.buffer)))
            {
                uint eof = br.ReadUInt32();
                TIM2 tim;
                while (ms.CanRead)
                    if (ms.Position >= ms.Length)
                        break;
                    else if (BitConverter.ToUInt16(this.buffer, (int)ms.Position) == 0)
                        ms.Seek(2, SeekOrigin.Current);
                    else if (br.ReadUInt64() == 0x0000000800000010)
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        tim = new TIM2(this.buffer, (uint)ms.Position);
                        ms.Seek(tim.GetHeight * tim.GetWidth / 2 + 64, SeekOrigin.Current); //i.e. 64*20=1280/2=640 + 64= 704 + eof
                        texturesList.Add(TextureHandler.Create($"chara_tim{(i++).ToString("D2")}", tim, 0, null));
                    }
                    else //is geometry structure
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        Debug_MCH mch = new Debug_MCH(ms, br);
                        if (mch.bValid())
                            mchs.Add(mch);
                    }
                ms = null;
            }
            mchInstances = mchs.ToArray();
            textures = texturesList.ToArray();
        }

        public Debug_MCH GetMCH(int i) => i >= mchInstances.Length ? mchInstances[0] : mchInstances[i];

        public Texture2D GetCharaTexture(int i) => (Texture2D)textures[i];

        public void AssignTextureSizesForMchInstance(int mchInstanceIndex, int[] textureIndexes) =>
            mchInstances[mchInstanceIndex].AssignTextureSizes(textures, textureIndexes);
    }
}