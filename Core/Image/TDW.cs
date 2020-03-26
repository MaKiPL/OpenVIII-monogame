using System;
using System.IO;

namespace OpenVIII
{
    public class TDW : TIM2
    {
        public TDW(BinaryReader br, uint offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            var widthPointer = br.ReadUInt32();
            var dataPointer = br.ReadUInt32();
            br.BaseStream.Seek(widthPointer+offset, SeekOrigin.Begin);
            GetWidths(br.ReadBytes((int)(dataPointer - widthPointer)));
            _Init(br, dataPointer+offset);
        }

        public TDW(byte[] buffer, uint offset = 0)
        {
            var widthPointer = BitConverter.ToUInt32(buffer, (int)(0+offset));
            var dataPointer = BitConverter.ToUInt32(buffer, (int)(4+offset));
            GetWidths(buffer, widthPointer + offset, dataPointer - widthPointer);
            _Init(buffer, dataPointer + offset);
        }

        private void GetWidths(byte[] buffer) => GetWidths(buffer, 0, (uint)buffer.Length);

        private void GetWidths(byte[] Tdw, uint offset, uint length)
        {
            if (length > int.MaxValue) return;
            var ms = new MemoryStream(Tdw);
            var os = new MemoryStream(checked((int)length * 2));
            //BinaryWriter and BinaryReader dispose of streams
            using (var bw = new BinaryWriter(os)) 
            using (var br = new BinaryReader(ms))
            {
                //bw.Write((byte)10);//width of space
                ms.Seek(offset, SeekOrigin.Begin);
                while (ms.Position < offset + length)
                {
                    var b = br.ReadByte();
                    var low = (byte)(b & 0x0F);
                    var high = (byte)((b & 0xF0) >> 4);
                    bw.Write(low);
                    bw.Write(high);
                }
                CharWidths = os.ToArray();
            }
        }

        public byte[] CharWidths { get; private set; }
    }
}