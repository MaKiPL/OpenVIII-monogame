using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS0649

namespace OpenVIII
{
    public static partial class Memory
    {
        /// <summary>
        /// Archive class handles the filename formatting and extensions for archive files.
        /// </summary>
        public class Archive : IReadOnlyList<string>
        {
            public List<Archive> Parent { get; set; }
            public bool IsDir { get; private set; }
            public bool IsFile { get; private set; }

            //public string _Root { get; set; }
            private string _filename;

            public Archive(string path, params Archive[] parent) : this(path, false) => Parent = parent.ToList();

            public Archive(string path, bool keepext, params Archive[] parent) : this(path, keepext) => Parent = parent.ToList();
            public Archive(string path) : this(path, false)
            {}
            
            public Archive(string path, bool keepext)
            {
                Parent = null;
                SetFilename(path, keepext);
            }

            /// <summary>
            /// SetFileName can be used to update path.
            /// </summary>
            /// <param name="path">Path to search parent for.</param>
            /// <param name="keepext">Extensions for FIFLFS are all different. Where ZZZ would be just one extension</param>
            public void SetFilename(string path, bool keepext = false)
            {
                if (Directory.Exists(path))
                {
                    IsDir = true;
                    _filename = path;
                }
                else if (File.Exists(path))
                {
                    IsFile = true;
                    _filename = path;
                }
                else if(!keepext)
                    _filename = Path.GetFileNameWithoutExtension(path);
                else
                    _filename = Path.GetFileName(path);
            }
            public static implicit operator string(Archive val) => val._filename;

            public static implicit operator Archive(string path) => new Archive(path);

            private readonly string[] ext = new string[] { B_FileList, B_FileIndex, B_FileArchive, B_ZZZ };

            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileList = ".fl";

            /// <summary>
            /// File Index Extension
            /// </summary>
            public const string B_FileIndex = ".fi";

            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileArchive = ".fs";

            public const string B_ZZZ = ".zzz";

            public bool IsZZZ => _filename.EndsWith(B_ZZZ, StringComparison.OrdinalIgnoreCase);
            public bool IsFileList => _filename.EndsWith(B_FileList, StringComparison.OrdinalIgnoreCase);
            public bool IsFileIndex => _filename.EndsWith(B_FileIndex, StringComparison.OrdinalIgnoreCase);
            public bool IsFileArchive => _filename.EndsWith(B_FileArchive, StringComparison.OrdinalIgnoreCase);

            private string NoExtension
            {
                get
                {
                    if(!string.IsNullOrWhiteSpace(_filename))
                    return Path.Combine(Path.GetDirectoryName(_filename),Path.GetFileNameWithoutExtension(_filename));
                    return "";
                    //int startIndex = _filename.LastIndexOf('.');
                    //if (startIndex != -1)
                    //    return _filename.Remove(startIndex, _filename.Length - startIndex);
                    //return _filename;
                }
            }

            /// <summary>
            /// File Index
            /// </summary>
            public string FI => $"{NoExtension}{B_FileIndex}";

            /// <summary>
            /// File List
            /// </summary>
            public string FL => $"{NoExtension}{B_FileList}";

            /// <summary>
            /// File Archive
            /// </summary>
            public string FS => $"{NoExtension}{B_FileArchive}";

            /// <summary>
            /// ZZZ File
            /// </summary>
            public string ZZZ => $"{NoExtension}{B_ZZZ}";

            public int Count => ((IReadOnlyList<string>)ext).Count;

            public string this[int index] => ((IReadOnlyList<string>)ext)[index];

            //public bool FileExistsInFolder { get; private set; }

            ///// <summary>
            ///// Test if input file path exists
            ///// </summary>
            ///// <param name="input">file path</param>
            ///// <returns></returns>
            //private string Test(string input)
            //{
            //    //using this for archives in archives would always throw exceptions
            //    if (!File.Exists(input)) FileExistsInFolder = false; //throw new FileNotFoundException($"There is no {input} file!\nExiting...");
            //    else FileExistsInFolder = true;
            //    return input;
            //}

            public override string ToString() => this;

            public IEnumerator<string> GetEnumerator() => ((IReadOnlyList<string>)ext).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<string>)ext).GetEnumerator();
        }

       
    }
}