using System.IO;

namespace OpenVIII
{
    public abstract class ArchiveBase
    {
        public abstract ArchiveBase GetArchive(Memory.Archive archive);
        public abstract byte[] GetBinaryFile(string fileName, bool cache = false);
        //public abstract Stream GetBinaryFileStream(string fileName, bool cache = false);
        public abstract string[] GetListOfFiles();
        public abstract Memory.Archive GetPath();
    }
}