using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// This class stores the reference to where the string is. Can be read with Read();
    /// </summary>
    public class FF8StringReference : FF8String
    {
        private readonly Memory.Archive Archive;
        private readonly string Filename;
        private readonly long Offset;
        private readonly ushort ReadLength;
        private object lockvar = new object();

        public FF8StringReference(Memory.Archive archive, string filename, long offset, ushort length = 0)
        {
            Archive = archive;
            Filename = filename;
            Offset = offset;
            ReadLength = length;
        }

        public override byte[] Value
        {
            get
            {
                if (Length == 0) Read();
                return base.Value;
            }
            set => base.Value = value;
        }

        private void Read()
        {
            lock (lockvar)
                if (Length == 0)
                {
                    ArchiveWorker aw = new ArchiveWorker(Archive, true);
                    using (BinaryReader br = new BinaryReader(new MemoryStream(aw.GetBinaryFile(Filename, true))))
                    {
                        br.BaseStream.Seek(Offset, SeekOrigin.Begin);
                        if (ReadLength > 0) // ReadLength set, read that.
                            Value = br.ReadBytes(ReadLength);
                        else // Length unknown read to null
                        {
                            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
                            {
                                for (int i = 0; i < br.BaseStream.Length; i++)
                                {
                                    byte b = br.ReadByte();
                                    if (i == 0 || b != 0)
                                        bw.Write(b);
                                    else break;
                                }
                                Value = ((MemoryStream)bw.BaseStream).ToArray();
                            }
                        }
                    }
                }
        }
    }
}