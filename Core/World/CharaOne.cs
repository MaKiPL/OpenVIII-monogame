using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.World
{
    class CharaOne
    {
        Debug_MCH[] mchInstances;
        List<Texture2D[]> textures;
        private byte[] buffer;

        /// <summary>
        /// Reads chara.one buffer
        /// </summary>
        /// <param name="buffer">Full chara.one file</param>
        public CharaOne(byte[] buffer)
        {
            this.buffer = buffer;
            List<Debug_MCH> mchs = new List<Debug_MCH>();
            using (MemoryStream ms = new MemoryStream(this.buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                uint eof = br.ReadUInt32();
                TIM2 tim;
                while (ms.CanRead)
                    if (ms.Position > ms.Length)
                        break;
                    else if (BitConverter.ToUInt16(this.buffer, (int)ms.Position) == 0)
                        ms.Seek(2, SeekOrigin.Current);
                    else if (br.ReadUInt64() == 0x0000000800000010)
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        tim = new TIM2(this.buffer, (uint)ms.Position);
                        ms.Seek(tim.GetHeight * tim.GetWidth / 2 + 64, SeekOrigin.Current); //i.e. 64*20=1280/2=640 + 64= 704 + eof
                        if (textures == null)
                            textures = new List<Texture2D[]>();
                        Texture2D[] _2d = { tim.GetTexture(0, true) };
                        textures.Add(_2d);
                    }
                    else //is geometry structure
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        mchs.Add(new Debug_MCH(ms, br));
                    }
            }
            mchInstances = mchs.ToArray();
        }

    }
}
