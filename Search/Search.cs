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

        public List<Tuple<string, string, long>> Results = new List<Tuple<string, string, long>>();

        private static readonly Memory.Archive[] ArchiveList = {
            Memory.Archives.A_MENU,
            Memory.Archives.A_MAIN,
            Memory.Archives.A_FIELD, // need support for file in two archives.
            Memory.Archives.A_MAGIC,
            Memory.Archives.A_WORLD,
            Memory.Archives.A_BATTLE,
        };

        private static readonly string[] Files = new string[]
        {
            Path.Combine(Memory.FF8DIR,"FF8_EN.exe"),
            //Path.Combine(Memory.FF8DIR,"AF3DN.P"),
            //Path.Combine(Memory.FF8DIR,"AF4DN.P")
        };

        private readonly byte[] _s;

        private readonly string[] _skipExtension = {
            ".tim",
            ".tex",
            ".obj"
        };

        #endregion Fields

        #region Constructors

        public Search(FF8String searchstring)
        {
            _s = searchstring;
            foreach (var a in ArchiveList)
            {
                var aw = ArchiveWorker.Load(a);
                var lof = aw.GetListOfFiles();
                foreach (var f in lof)
                {
                    var ext = Path.GetExtension(f);
                    if (_skipExtension.Any(x => ext != null && x.IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                    Debug.WriteLine($"Searching {f}, in {a} for {searchstring}");
                    //byte[] bf = aw.GetBinaryFile(f);
                    //SearchBin(bf, a.ToString(), f);
                    using (var br = new BinaryReader(new MemoryStream(aw.GetBinaryFile(f))))
                    {
                        SearchBin(br, a.ToString(), f);
                    }
                }
            }
            foreach (var a in Files)
            {
                Debug.WriteLine($"Searching {a}, for {searchstring}");
                using (var br = new BinaryReader(File.OpenRead(a))) //new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.None, 65536)))//
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
                var pos = 0;
                foreach (var i in _s)
                {
                    if (br.ReadByte() == i)
                    {
                        pos++;
                    }
                    else break;
                }
                if (pos == _s.Length)
                {
                    Debug.WriteLine($"Found a match at: offset {offset:X}");
                    Results.Add(new Tuple<string, string, long>(a, f, offset));
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
            var i = 0;
            do
            {
                i = Array.FindIndex(bf, i, bf.Length - i, x => x == _s[0]);
                if (i < 0) continue;
                var full = bf.Skip(i).Take(_s.Length).ToArray();
                if (full.SequenceEqual(_s))
                {
                    Debug.WriteLine($"Found a match at: offset {i:X}");
                    Results.Add(new Tuple<string, string, long>(a, f, i));
                }
                i++;
            }
            while (i > 0);
        }
        #endregion Methods
    }
}