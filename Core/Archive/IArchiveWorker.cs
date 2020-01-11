using System.IO;

namespace OpenVIII
{
    public interface IArchiveWorker
    {
        IArchiveWorker GetArchive(Memory.Archive archive);
        byte[] GetBinaryFile(string fileName, bool cache = false);
        Stream GetBinaryFileStream(string fileName, bool cache = false);
        string[] GetListOfFiles();
        Memory.Archive GetPath();
    }
}