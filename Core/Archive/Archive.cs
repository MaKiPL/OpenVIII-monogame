using System.IO;

#pragma warning disable CS0649

namespace OpenVIII
{

    public static partial class Memory
    {
        /// <summary>
        /// Archive class handles the filename formatting and extensions for archive files.
        /// </summary>
        public class Archive
        {
            public Archive Parent;

            public string _Root { get; set; }
            public string _Filename { get; private set; }

            public Archive(Archive parent, string path)
            {
                Parent = parent;
                _Root = "";
                if (Path.HasExtension(path))
                {
                    string ext = Path.GetExtension(path);
                    if (ext == B_FileArchive || ext == B_FileIndex || ext == B_FileList)
                    {
                        int index = path.LastIndexOf('.');
                        path = index == -1 ? path : path.Substring(0, index);
                    }
                }
                _Filename = path;
            }

            public Archive(string path) : this(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))
            { }
            public static implicit operator string(Archive val) => Extended.GetUnixFullPath($"{Path.Combine(val._Root, val._Filename)}");

            public static implicit operator Archive(string path) => new Archive(path);
            public Archive(string root, string filename)
            {
                _Root = root;
                _Filename = filename;
            }

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

            /// <summary>
            /// File Index
            /// </summary>
            public string FI => Test(Extended.GetUnixFullPath($"{Path.Combine(_Root, _Filename)}{B_FileIndex}"));

            /// <summary>
            /// File List
            /// </summary>
            public string FL => Test(Extended.GetUnixFullPath($"{Path.Combine(_Root, _Filename)}{B_FileList}"));

            /// <summary>
            /// File Archive
            /// </summary>
            public string FS => Test(Extended.GetUnixFullPath($"{Path.Combine(_Root, _Filename)}{B_FileArchive}"));

            public bool FileExistsInFolder { get; private set; }

            /// <summary>
            /// Test if input file path exists
            /// </summary>
            /// <param name="input">file path</param>
            /// <returns></returns>
            private string Test(string input)
            {
                //using this for archives in archives would always throw exceptions
                if (!File.Exists(input)) FileExistsInFolder = false; //throw new FileNotFoundException($"There is no {input} file!\nExiting...");
                else FileExistsInFolder = true;
                return input;
            }

            public override string ToString() => this;
        }
    }
}