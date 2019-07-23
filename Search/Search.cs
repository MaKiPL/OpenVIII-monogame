using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII.Search
{
    public class Search
    {
        #region Fields

        public List<Tuple<string, string, long>> results = new List<Tuple<string, string, long>>();

        private static readonly Memory.Archive[] ArchiveList = new Memory.Archive[]
        {
            //Memory.Archives.A_MENU,
            //Memory.Archives.A_MAIN,
            //Memory.Archives.A_FIELD, // need support for file in two archives.
            //Memory.Archives.A_MAGIC,
            //Memory.Archives.A_WORLD,
            //Memory.Archives.A_BATTLE,
        };

        private static readonly string[] Files = new string[]
        {
            Path.Combine(Memory.FF8DIR,"FF8_EN.exe"),
            //Path.Combine(Memory.FF8DIR,"AF3DN.P"),
            //Path.Combine(Memory.FF8DIR,"AF4DN.P")
        };

        private byte[] s;

        private string[] skipext = new string[]
                {
            ".tim",
            ".tex",
            ".obj"
        };

        #endregion Fields

        #region Constructors

        public Search(FF8String searchstring)
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
                    Debug.WriteLine($"Searching {f}, in {a} for {searchstring}");
                    //byte[] bf = aw.GetBinaryFile(f);
                    //SearchBin(bf, a.ToString(), f);
                    using (BinaryReader br = new BinaryReader(new MemoryStream(aw.GetBinaryFile(f))))
                    {
                        SearchBin(br, a.ToString(), f);
                    }
                }
            }
            foreach (string a in Files)
            {
                Debug.WriteLine($"Searching {a}, for {searchstring}");
                using (BinaryReader br = new BinaryReader(File.OpenRead(a))) //new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.None, 65536)))//
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    //byte[] bf = br.ReadBytes((int)br.BaseStream.Length);

                    //SearchBin(bf, a, "");
                    SearchBin(br, a, "");
                }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Less Slow search.
        /// </summary>
        /// <param name="bf">buffer</param>
        /// <param name="a">file/archive it's searching</param>
        /// <param name="f">sub file</param>
        public void SearchBin(BinaryReader br, string a, string f)
        {
            long offset;
            while ((offset = br.BaseStream.Position) < br.BaseStream.Length)
            {
                int pos = 0;
                foreach (byte i in s)
                {
                    if (br.ReadByte() == i)
                    {
                        pos++;
                    }
                    else break;
                }
                if (pos == s.Length)
                {
                    Debug.WriteLine(string.Format("Found a match at: offset {0:X}", offset));
                    results.Add(new Tuple<string, string, long>(a, f, offset));
                }
            }
        }

        /// <summary>
        /// Slow AF search.
        /// </summary>
        /// <param name="bf">buffer</param>
        /// <param name="a">file/archive it's searching</param>
        /// <param name="f">sub file</param>
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
                        Debug.WriteLine(string.Format("Found a match at: offset {0:X}", i));
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

        #endregion Methods
    }
}