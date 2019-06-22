using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal class ArchiveSearch
    {
        public List<Tuple<string, string, long>> results = new List<Tuple<string, string, long>>();

        private static readonly Memory.Archive[] ArchiveList = new Memory.Archive[]
        {
            //Memory.Archives.A_MENU,
            //Memory.Archives.A_MAIN,
            //Memory.Archives.A_FIELD,
            Memory.Archives.A_MAGIC,
            //Memory.Archives.A_WORLD,
            Memory.Archives.A_BATTLE,
        };
        private static readonly string[] Files = new string[]
        {
            Path.Combine(Memory.FF8DIR,"FF8_EN.exe"),
            Path.Combine(Memory.FF8DIR,"AF3DN.P"),
            Path.Combine(Memory.FF8DIR,"AF4DN.P")
        };
        string[] skipext = new string[]
        {
            ".tim",
            ".tex"
        };
        byte[] s;
        public ArchiveSearch(FF8String searchstring)
        {
            s = searchstring;
            foreach (Memory.Archive a in ArchiveList)
            {
                ArchiveWorker aw = new ArchiveWorker(a);
                string[] lof = aw.GetListOfFiles();
                foreach (string f in lof)
                {
                    string ext = Path.GetExtension(f);
                    if (skipext.Where(x => x.IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0).Count() > 0) continue;
                    byte[] bf = aw.GetBinaryFile(f);
                    SearchBin(bf, a.ToString(), f);
                }
            }
            foreach (string a in Files)
            {
                using (var br = new BinaryReader(File.OpenRead(a)))
                {
                    byte[] bf = br.ReadBytes((int)br.BaseStream.Length);
                    SearchBin(bf, a, "");
                }
            }
        }
        public void SearchBin(byte[] bf, string a, string f)
        {
            int i = 0;
            do
            {
                i = Array.FindIndex(bf, i, bf.Length - i, x => x == s[0]);
                if (i >= 0 && bf != null)
                {
                    byte[] full = bf.Skip(i).Take(s.Length).ToArray();
                    if (full.SequenceEqual(s))
                    {
                        results.Add(new Tuple<string, string, long>(a, f, i));
                    }
                    i++;
                }
            }
            while (i > 0);
            //bf = bf.Reverse().ToArray();
            //i = 0;
            //do
            //{
            //    i = Array.FindIndex(bf, i, bf.Length - i, x => x == s[0]);
            //    if (i >= 0 && bf != null)
            //    {
            //        var full = bf.Skip(i).Take(s.Length).ToArray();
            //        if (full.SequenceEqual(s))
            //        {
            //            results.Add(new Tuple<string, string, long>(a, f, i));
            //        }
            //        i++;
            //    }
            //}
            //while (i > 0);
            //string decodedarchive = Memory.DirtyEncoding.GetString(aw.GetBinaryFile(f));
            //Regex r = new Regex(searchstring, RegexOptions.IgnoreCase);
            //Match m = r.Match(f);
            //if(m.Success)
            //{
            //    results.Add(new Tuple<string, string, long>(a, f, m.Index));
            //}
        }
    }
}