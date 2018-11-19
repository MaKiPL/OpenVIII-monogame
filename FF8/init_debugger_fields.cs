using System;
using System.Linq;

namespace FF8
{
    internal class init_debugger_fields
    {
        internal static void DEBUG()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] lists = aw.GetListOfFiles();
            string maplist = lists.Where(x => x.ToLower().Contains("mapdata.fs")).First();
            maplist = maplist.Substring(0, maplist.Length - 2);
            byte[] fs = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}fs");
            byte[] fl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}fl");
            byte[] fi = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}fi");
            string map = System.Text.Encoding.UTF8.GetString(fl).TrimEnd();
            string[] maplistb = System.Text.Encoding.UTF8.GetString(
                ArchiveWorker.FileInTwoArchives(fi, fs, fl, map))
                .Replace("\r", "")
                .Split('\n');
            Memory.FieldHolder.fields = maplistb;
        }
    }
}