using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class PseudoBufferedStream
    {
        //same as stream naturally but working with pre-loaded byte[] buffer
        //normally made to have consinstency with original and rising pointer
        //due to the reason of C# and C+assembler based approach

        public const int SEEK_BEGIN = 0;
        public const int SEEK_CURRENT = 1;
        public const int SEEK_END = 2;
        byte[] buffer;
        int pointer = 0;
        public PseudoBufferedStream(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Seek(long offset, int mode)
        {
            switch(mode)
            {
                case SEEK_BEGIN:
                    pointer = (int)offset;
                    break;
                case SEEK_CURRENT:
                    pointer += (int)offset;
                    break;
                case SEEK_END:
                    pointer = buffer.Length - 1 - (int)offset;
                    break;
                default:
                    break;
            }
        }

        public byte ReadByte() => buffer[pointer++];

        public ushort ReadUShort()
        {
            ushort r = BitConverter.ToUInt16(buffer, pointer);
            pointer += 2; //sizeof ushort
            return r;
        }

        public byte[] ReadBytes(uint count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(this.buffer,pointer, buffer,0, count);
            pointer += (int)count;
            return buffer;
        }

        public short ReadShort()
        {
            short r = BitConverter.ToInt16(buffer, pointer);
            pointer += 2; //sizeof ushort
            return r;
        }

        public char ReadChar() => (char)ReadByte(); //naturally
        
        public uint ReadUInt()
        {
            uint r = BitConverter.ToUInt32(buffer, pointer);
            pointer += 4;
            return r;
        }

        public int ReadInt()
        {
            int r = BitConverter.ToInt32(buffer, pointer);
            pointer += 4;
            return r;
        }

        public int Length => buffer.Length;

        public int Tell() => pointer;
    }
}
