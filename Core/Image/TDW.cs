using System;
using System.IO;

namespace OpenVIII
{
    public class TDW : TIM2
    {
        public TDW(BinaryReader br, uint offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            uint widthPointer = br.ReadUInt32();
            uint dataPointer = br.ReadUInt32();
            br.BaseStream.Seek(widthPointer+offset, SeekOrigin.Begin);
            GetWidths(br.ReadBytes((int)(dataPointer - widthPointer)));
            _Init(br, dataPointer+offset);
        }

        public TDW(byte[] buffer, uint offset = 0)
        {
            uint widthPointer = BitConverter.ToUInt32(buffer, (int)(0+offset));
            uint dataPointer = BitConverter.ToUInt32(buffer, (int)(4+offset));
            GetWidths(buffer, widthPointer + offset, dataPointer - widthPointer);
            _Init(buffer, dataPointer + offset);
        }

        private void GetWidths(byte[] buffer) => GetWidths(buffer, 0, (uint)buffer.Length);

        private void GetWidths(byte[] Tdw, uint offset, uint length)
        {
            using (MemoryStream os = new MemoryStream((int)length * 2))
            using (BinaryWriter bw = new BinaryWriter(os))
            using (MemoryStream ms = new MemoryStream(Tdw))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //bw.Write((byte)10);//width of space
                ms.Seek(offset, SeekOrigin.Begin);
                while (ms.Position < offset + length)
                {
                    byte b = br.ReadByte();
                    byte low = (byte)(b & 0x0F);
                    byte high = (byte)((b & 0xF0) >> 4);
                    bw.Write(low);
                    bw.Write(high);
                }
                CharWidths = os.ToArray();
            }
        }

        public byte[] CharWidths { get; private set; }
    }
}