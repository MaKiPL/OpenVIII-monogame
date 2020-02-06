using System.IO;
using System.Threading;

namespace OpenVIII
{
    public abstract class ArchiveBase
    {
        public abstract ArchiveBase GetArchive(string fileName);

        public void GetArchive(Memory.Archive archive, out byte[] FI, out byte[] FS, out byte[] FL)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            FI = GetBinaryFile(archive.FI);
            FS = GetBinaryFile(archive.FS);
            FL = GetBinaryFile(archive.FL);
        }
        public abstract ArchiveBase GetArchive(Memory.Archive archive);

        public abstract byte[] GetBinaryFile(string fileName, bool cache = false);

        //public abstract Stream GetBinaryFileStream(string fileName, bool cache = false);
        public abstract string[] GetListOfFiles();

        public abstract Memory.Archive GetPath();

        public static ArchiveBase Open(Memory.Archive archive)
        {
            if (archive.IsDir)
            {
                return new ArchiveWorker(archive);
            }
            else if (archive.IsFile)
            {
                if (archive.IsFileArchive || archive.IsFileIndex || archive.IsFileList)
                {
                    Memory.Archive a = new Memory.Archive(Path.GetFileNameWithoutExtension(archive), Path.GetDirectoryName(archive));
                    return new ArchiveWorker(a);
                }
                else if (archive.IsZZZ)
                {
                    return new ArchiveZZZ(archive);
                }
                else
                    return new ArchiveWorker(archive);
            }
            else
                return null;
        }
    }
}