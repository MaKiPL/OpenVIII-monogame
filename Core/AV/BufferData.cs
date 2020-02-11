namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;
    using System;
    using System.IO;

    /// <summary>
    /// Used only when reading ADPCM data from memory.
    /// </summary>
    public struct BufferData
    {
        public enum TargetFile
        {
            sound_dat,
            other_zzz
        }

        #region Fields

        public Int64 DataSeekLoc;
        public UInt32 DataSize;
        public UInt32 HeaderSize;
        private IntPtr Header;
        public TargetFile Target;

        #endregion Fields

        #region Properties

        public static string DataFileName { get; private set; }

        #endregion Properties

        #region Methods
        
        public unsafe int Read(byte* buf, int buf_size)
        {   
            int ret;
            if (HeaderSize >0 && (ret = ReadHeader(buf, buf_size)) != ffmpeg.AVERROR_EOF)
                return ret;
            else
                return ReadData(buf, buf_size);
        }

        public void SetHeader(IntPtr value) => Header = value;

        public unsafe void SetHeader(byte* value) => Header = (IntPtr)value;

        private unsafe int ReadData(byte* buf, int buf_size)
        {
            if (string.IsNullOrWhiteSpace(DataFileName))
                DataFileName = Path.Combine(Memory.FF8DIRdata, "Sound", "audio.dat");


            //Memory.Archive Sound = new Memory.Archive("audio.dat")
            buf_size = Math.Min(buf_size, (int)DataSize);

            if (buf_size == 0)
            {
                return ffmpeg.AVERROR_EOF;
            }
            Stream s = null;
            ArchiveZZZ other;
            switch (Target)
            {
                case TargetFile.sound_dat:
                    if (File.Exists(DataFileName))
                    {
                        s = new FileStream(DataFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    else
                    {
                        other = (ArchiveZZZ)ArchiveZZZ.Load(Memory.Archives.ZZZ_OTHER);
                        s = new MemoryStream(other.GetBinaryFile("audio.dat", true), false);
                    }
                    break;
                case TargetFile.other_zzz:
                    other = (ArchiveZZZ)ArchiveZZZ.Load(Memory.Archives.ZZZ_OTHER);
                    s = other.OpenStream();
                    break;

            }

            // binaryReader disposes of fs
            using (BinaryReader br = new BinaryReader(s))
            {
                s.Seek(DataSeekLoc, SeekOrigin.Begin);
                using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream(buf, buf_size, buf_size, FileAccess.Write))
                {
                    // copy public buffer data to buf
                    ums.Write(br.ReadBytes(buf_size), 0, buf_size);
                    DataSeekLoc += (uint)buf_size;
                    DataSize -= (uint)buf_size;

                    s = null;
                    return buf_size;
                }
            }
        }

        private unsafe int ReadHeader(byte* buf, int buf_size)
        {
            buf_size = Math.Min(buf_size, (int)HeaderSize);

            if (buf_size == 0)
            {
                return ffmpeg.AVERROR_EOF;
            }

            // copy public buffer data to buf
            Buffer.MemoryCopy((void*)Header, (void*)buf, buf_size, buf_size);
            Header += buf_size;
            HeaderSize -= (uint)buf_size;

            return buf_size;
        }

        #endregion Methods

        //can't do this because soon as fixed block ends the pointer is no good.
        //private void SetHeader(byte[] value)
        //{
        //    fixed (byte* tmp = &value[0])
        //    {
        //        Header = (IntPtr)tmp;
        //    }
        //}

        //< size left in the buffer
    };
}