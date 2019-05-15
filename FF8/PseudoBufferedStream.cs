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

        internal PseudoBufferedStream(byte[] buffer) : base(buffer)
        {
            br = new BinaryReader(this);
        }

        internal void DisposeAll()
        {
            br.Close();
            br.Dispose();
            this.Close();
            Dispose();
        }

        internal ushort ReadUShort() => br.ReadUInt16();

        internal byte[] ReadBytes(uint count) => br.ReadBytes((int)count);

#pragma warning disable 0114
        internal byte ReadByte()  => br.ReadByte(); //ms readbyte returns int
#pragma warning restore 0114

        internal short ReadShort() => br.ReadInt16();

        internal uint ReadUInt() => br.ReadUInt32();

        internal int ReadInt() => br.ReadInt32();

        internal long Tell() => Position;
    }
}
