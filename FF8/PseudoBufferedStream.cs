using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //useless class, to remove sooner or later- currently it's ms+br class, but there's no way to control the 
    class PseudoBufferedStream : MemoryStream
    {
        BinaryReader br;

        public PseudoBufferedStream(byte[] buffer) : base(buffer)
        {
            br = new BinaryReader(this);
        }

        public ushort ReadUShort() => br.ReadUInt16();

        public byte[] ReadBytes(uint count) => br.ReadBytes((int)count);

        public byte ReadByte()  => br.ReadByte(); //ms readbyte returns int

        public short ReadShort() => br.ReadInt16();

        public uint ReadUInt() => br.ReadUInt32();

        public int ReadInt() => br.ReadInt32();

        public long Tell() => Position;
    }
}
