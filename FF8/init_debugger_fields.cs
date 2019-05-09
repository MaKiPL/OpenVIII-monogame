using System;
using System.Linq;

namespace FF8
{
    internal static class Init_debugger_fields
    {
        internal static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] lists = aw.GetListOfFiles();
            string maplist = lists.First(x => x.ToLower().Contains("mapdata.fs"));
            maplist = maplist.Substring(0,maplist.Length - 3);
            byte[] fs = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileArchive}");
            byte[] fl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileList}");
            byte[] fi = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileIndex}");
            string map = System.Text.Encoding.UTF8.GetString(fl).TrimEnd();
            string[] maplistb = System.Text.Encoding.UTF8.GetString(
                ArchiveWorker.FileInTwoArchives(fi, fs, fl, map))
                .Replace("\r", "")
                .Split('\n');
            Memory.FieldHolder.fields = maplistb;
        }
    }
}