using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FF8
{
    class ArchiveSearch
    {
        internal List<Tuple<string, string, long>> results = new List<Tuple<string, string, long>>();
        static readonly string[] ArchiveList = new string[]
        {
            Memory.Archives.A_MENU,
            Memory.Archives.A_MAIN,
            //Memory.Archives.A_FIELD,
            Memory.Archives.A_MAGIC,
            Memory.Archives.A_WORLD,
            //Memory.Archives.A_BATTLE,
        };
        internal ArchiveSearch(string searchstring)
        {
            byte[] s = Memory.DirtyEncoding.GetBytes(searchstring);
            foreach (string a in ArchiveList)
            {
                ArchiveWorker aw = new ArchiveWorker(a);
                string[] lof = aw.GetListOfFiles();
                foreach(string f in lof)
                {
                    byte[] bf = aw.GetBinaryFile(f);
                    int i = 0;
                    do
                    {
                        i = Array.FindIndex(bf, i, bf.Length - i, x => x == s[0]);
                        if (i >= 0 && bf != null)
                        {
                            var full = bf.Skip(i).Take(s.Length).ToArray();
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
    }
}
