using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII.PAK_Extractor
{
    /// <summary>
    /// PAK reading class, Gathers all the sections
    /// </summary>
    /// <remarks>Original class wrote by Maki for the ToolKit</remarks>
    /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/tree/master/SerahToolkit_SharpGL/FF8_Core"/>
    public class PAK : IEnumerable
    {
        #region Fields

        /// <summary>
        /// Each frame section of cam file is this many bytes
        /// </summary>
        private const byte CamSectionSize = 0x2C;

        /// <summary>
        /// If false don't extract low res videos!
        /// </summary>
        private const bool EnableExtractLowRes = false;

        /// <summary>
        /// Known valid Bink video formats
        /// </summary>
        private readonly byte[] _bik1 = { 0x61, 0x64, 0x66, 0x67, 0x68, 0x69 };

        /// <summary>
        /// Known valid Bink 2 video formats
        /// </summary>
        private readonly byte[] _bik2 = { 0x62, 0x64, 0x66, 0x67, 0x68, 0x69 };

        /// <summary>
        /// Each Movie has 1 cam and 2 versions of the video.
        /// </summary>
        private readonly List<MovieClip> _movies;

        /// <summary>
        /// Depending on type you read it differently.
        /// </summary>
        private readonly Dictionary<Type, Func<BinaryReader, Type, FileSection>> _readFunctions;

        /// <summary>
        /// Remembers detected disc number.
        /// </summary>
        private int _discCache = -1;

        /// <summary>
        /// Current working tmp movie clip
        /// </summary>
        private MovieClip _movie;

        #endregion Fields

        #region Constructors

        public PAK(FileInfo info)
        {
            _movies = new List<MovieClip>(7);
            _readFunctions = new Dictionary<Type, Func<BinaryReader, Type, FileSection>>()
            {
                {Type.Cam, ReadCam },
                {Type.Bik, ReadBik },
                {Type.Kb2, ReadKb2 },
            };
            Read(info);
        }

        #endregion Constructors

        #region Enums

        public enum Type : uint
        {
            /// <summary>
            /// Cam file
            /// </summary>
            /// <remarks>F8P</remarks>
            Cam = 0x503846,

            /// <summary>
            /// Bink Video
            /// </summary>
            Bik = 0x4B4942,

            /// <summary>
            /// Bink Video Version 2.
            /// </summary>
            Kb2 = 0X32424B,

            /// <summary>
            /// 3 Byte Mask to take uint and extract only the part we need.
            /// </summary>
            _3B_MASK = 0xFFFFFF,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Total number of movies detected
        /// </summary>
        public int Count => _movies.Count;

        /// <summary>
        /// Current path
        /// </summary>
        public FileInfo FilePath { get; private set; }

        /// <summary>
        /// Each Movie has 1 cam and 2 versions of the video.
        /// </summary>
        public IReadOnlyList<MovieClip> Movies => _movies;

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Gets Movie Clip at index
        /// </summary>
        /// <param name="i">Index</param>
        public MovieClip this[int i] => _movies[i];

        #endregion Indexers

        #region Methods

        public void Extract(string destPath)
        {
            Console.WriteLine($"Extracting {FilePath.FullName}");
            foreach (MovieClip item in this)
            {
                using (var br = new BinaryReader(File.OpenRead(FilePath.FullName)))
                {
                    Extract(br, item.Cam);
                    Extract(br, item.BinkHigh);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (EnableExtractLowRes)
                        // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
                        Extract(br, item.BinkLow);
#pragma warning restore 162
                }
            }
            void Extract(BinaryReader br, FileSection fs)
            {
                var outPath = Path.Combine(destPath, fs.FileName);
                if (File.Exists(outPath))
                {
                    bool overwrite;
                    using (var s = File.OpenRead(outPath))
                    {
                        overwrite = s.Length != fs.Size;
                    }

                    if (overwrite) File.Delete(outPath);
                    else
                    {
                        Console.WriteLine($"File Exists {fs.FileName}");
                        return;
                    }
                }
                using (var bw = new BinaryWriter(File.Create(outPath)))
                {
                    Console.WriteLine($"Extracting {fs.FileName}");
                    br.BaseStream.Seek(fs.Offset, SeekOrigin.Begin);
                    bw.Write(br.ReadBytes((int)fs.Size));
                }
            }
        }

        public IEnumerator GetEnumerator() => ((IEnumerable)_movies).GetEnumerator();

        private string GenerateFileName(string extension, string suffix = "") =>
            $"disc{GetDiscNumber() - 1:00}_{Count:00}{suffix}.{extension.Trim('.').ToLower()}";

        private int GetDiscNumber()
        {
            if (_discCache == -1)
            {
                var re = new Regex(@"\d+");
                var m = re.Match(Path.GetFileNameWithoutExtension(FilePath.FullName));

                if (m.Success)
                {
                    if (int.TryParse(m.Value, out _discCache))
                    {
                        return _discCache;
                    }
                    else
                        throw new Exception($"{this} :: Could not parse number from: {m.Value}");
                }
                else
                {
                    throw new Exception($"{this} :: No number in filename: {FilePath}");
                }
            }
            return _discCache;
        }

        /// <summary>
        /// Read complete pak file for offsets and sizes of each section.
        /// </summary>
        /// <param name="info">File path info</param>
        private void Read(FileInfo info)
        {
            FilePath = info;
            using (var br = new BinaryReader(File.OpenRead(info.FullName)))
            {
                _movie = new MovieClip();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var header = (Type)(br.ReadUInt32() & (uint)Type._3B_MASK);
                    if (_readFunctions.ContainsKey(header))
                    {
                        _readFunctions[header](br, header);
                    }
                    else throw new Exception($"{header} is invalid, reading from: {info}, offset {br.BaseStream.Position}");
                }
            }
        }

        /// <summary>
        /// Read Bink video offset and size
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="type">Header of file type</param>
        private FileSection ReadBik(BinaryReader br, Type type)
        {
            br.BaseStream.Seek(-1, SeekOrigin.Current);
            var version = br.ReadByte();
            if ((type == Type.Bik && _bik1.Contains(version)) || (type == Type.Kb2 && _bik2.Contains(version)))
            {
            }
            else
                throw new Exception($"_Type {type}, version {version}, is invalid");
            var fs = new FileSection()
            {
                Type = type,
                Offset = br.BaseStream.Position - 4,
                Size = br.ReadUInt32() + 8,
                Frames = br.ReadUInt32()
            };
            br.BaseStream.Seek(fs.Offset + fs.Size, SeekOrigin.Begin);

            if (_movie.BinkHigh == null)
            {
                _movie.BinkHigh = fs;
                _movie.BinkHigh.FileName = GenerateFileName(_movie.BinkHigh.Type == Type.Bik ? "bik" : "bk2", "h");
            }
            else
            {
                if (fs.Size > _movie.BinkHigh.Size)
                {
                    _movie.BinkLow = _movie.BinkHigh;
                    _movie.BinkHigh = fs;
                }
                else
                {
                    _movie.BinkLow = fs;
                }

                _movie.BinkHigh.FileName = GenerateFileName(_movie.BinkLow.Type == Type.Bik ? "bik" : "bk2", "h");
                _movie.BinkLow.FileName = GenerateFileName(_movie.BinkLow.Type == Type.Bik ? "bik" : "bk2", "l");
                _movies.Add(_movie);
                _movie = new MovieClip();
            }
            return fs;
        }

        /// <summary>
        /// Read cam file offset and size
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="type">Header of file type</param>
        private FileSection ReadCam(BinaryReader br, Type type)
        {
            var offset = br.BaseStream.Position - 4; // start of section
            br.BaseStream.Seek(2, SeekOrigin.Current); // skip 2 bytes
            var frames = br.ReadUInt16(); // get approx number of frames
            br.BaseStream.Seek((frames) * CamSectionSize, SeekOrigin.Current);
            uint b;
            // there seems to be 1 or more extra frames. Check for those.
            while ((b = br.ReadUInt32()) > 0 && !(((Type)(b & (uint)Type._3B_MASK)) == Type.Bik || ((Type)(b & (uint)Type._3B_MASK)) == Type.Kb2))
            {
                br.BaseStream.Seek(CamSectionSize - sizeof(uint), SeekOrigin.Current);
                frames++;
            }
            // Found the end go back to it.
            br.BaseStream.Seek(-sizeof(uint), SeekOrigin.Current);

            // There is only one cam per movie. Checking for possibility of only one video instead of the normal 2 per movie.
            if (_movie.Cam != null)
            {
                if (!_movies.Contains(_movie))
                    //add existing movie to movies list.
                    _movies.Add(_movie);
                //start from scratch
                _movie = new MovieClip();
            }

            var fs = new FileSection() { Type = type, Offset = offset, Size = br.BaseStream.Position - offset, Frames = frames, FileName = GenerateFileName("cam") };
            _movie.Cam = fs;
            return fs;
        }

        private FileSection ReadKb2(BinaryReader br, Type header) => ReadBik(br, header);

        #endregion Methods

        #region Structs

        public struct MovieClip
        {
            #region Fields

            /// <summary>
            /// High res bink video
            /// </summary>
            public FileSection BinkHigh;

            /// <summary>
            /// Low res bink video
            /// </summary>
            public FileSection BinkLow;

            /// <summary>
            /// Cam file
            /// </summary>
            public FileSection Cam;

            #endregion Fields
        }

        #endregion Structs

        #region Classes

        public class FileSection
        {
            #region Fields

            /// <summary>
            /// Output FileName;
            /// </summary>
            public string FileName;

            /// <summary>
            /// Frame Count
            /// </summary>
            public uint Frames;

            /// <summary>
            /// Location of Data
            /// </summary>
            public long Offset;

            /// <summary>
            /// Size of Data
            /// </summary>
            public long Size;

            /// <summary>
            /// Type of file in Section
            /// </summary>
            public Type Type;

            #endregion Fields
        }

        #endregion Classes
    }
}