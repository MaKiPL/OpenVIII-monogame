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
        private const bool enableExtractLowRes = false;

        /// <summary>
        /// Known valid Bink video formats
        /// </summary>
        private readonly byte[] bik1 = new byte[] { 0x61, 0x64, 0x66, 0x67, 0x68, 0x69 };

        /// <summary>
        /// Known valid Bink 2 video formats
        /// </summary>
        private readonly byte[] bik2 = new byte[] { 0x62, 0x64, 0x66, 0x67, 0x68, 0x69 };

        /// <summary>
        /// Each Movie has 1 cam and 2 versions of the video.
        /// </summary>
        private List<MovieClip> _movies;

        /// <summary>
        /// Remembers detected disc number.
        /// </summary>
        private int discCache = -1;

        /// <summary>
        /// Current working tmp movie clip
        /// </summary>
        private MovieClip movie;

        /// <summary>
        /// Depending on type you read it differently.
        /// </summary>
        private Dictionary<_Type, Func<BinaryReader, _Type, FileSection>> ReadFunctions;

        #endregion Fields

        #region Constructors

        public PAK(FileInfo info)
        {
            _movies = new List<MovieClip>(7);
            ReadFunctions = new Dictionary<_Type, Func<BinaryReader, _Type, FileSection>>()
            {
                {_Type.CAM, ReadCAM },
                {_Type.BIK, ReadBIK },
                {_Type.KB2, ReadKB2 },
            };
            Read(info);
        }

        #endregion Constructors

        #region Enums

        public enum _Type : uint
        {
            /// <summary>
            /// Cam file
            /// </summary>
            /// <remarks>F8P</remarks>
            CAM = 0x503846,

            /// <summary>
            /// Bink Video
            /// </summary>
            BIK = 0x4B4942,

            /// <summary>
            /// Bink Video Version 2.
            /// </summary>
            KB2 = 0X32424B,

            /// <summary>
            /// 3 Byte Mask to take uint and extract only the part we need.
            /// </summary>
            _3B_MASK = 0xFFFFFF,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Current path
        /// </summary>
        public FileInfo FilePath { get; private set; }

        /// <summary>
        /// Each Movie has 1 cam and 2 versions of the video.
        /// </summary>
        public IReadOnlyList<MovieClip> Movies => _movies;

        /// <summary>
        /// Total number of movies detected
        /// </summary>
        public int Count => _movies.Count;

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
            foreach (PAK.MovieClip item in this)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(FilePath.FullName)))
                {
                    Extract(br, item.CAM);
                    Extract(br, item.BINK_HIGH);
                    if (enableExtractLowRes)
                        Extract(br, item.BINK_LOW);
                }
            }
            void Extract(BinaryReader br, PAK.FileSection fs)
            {
                string outpath = Path.Combine(destPath, fs.FileName);
                bool overwrite = true;
                if (File.Exists(outpath))
                {
                    using (FileStream s = File.OpenRead(outpath))
                    {
                        overwrite = !(s.Length == fs.Size);
                    }

                    if (overwrite) File.Delete(outpath);
                    else
                    {
                        Console.WriteLine($"File Exists {fs.FileName}");
                        return;
                    }
                }
                using (BinaryWriter bw = new BinaryWriter(File.Create(outpath)))
                {
                    Console.WriteLine($"Extracting {fs.FileName}");
                    br.BaseStream.Seek(fs.Offset, SeekOrigin.Begin);
                    bw.Write(br.ReadBytes((int)fs.Size));
                }
            }
        }

        public IEnumerator GetEnumerator() => ((IEnumerable)_movies).GetEnumerator();

        private string GenerateFileName(string extension, string suffux = "") =>
            string.Format("disc{0:00}_{1:00}{2}.{3}", GetDiscNumber() - 1, Count, suffux, extension.Trim('.').ToLower());

        private int GetDiscNumber()
        {
            if (discCache == -1)
            {
                Regex re = new Regex(@"\d+");
                Match m = re.Match(Path.GetFileNameWithoutExtension(FilePath.FullName));

                if (m.Success)
                {
                    if (int.TryParse(m.Value, out discCache))
                    {
                        return discCache;
                    }
                    else
                        throw new Exception($"{this} :: Could not parse number from: {m.Value}");
                }
                else
                {
                    throw new Exception($"{this} :: No number in filename: {FilePath}");
                }
            }
            return discCache;
        }

        /// <summary>
        /// Read complete pak file for offsets and sizes of each section.
        /// </summary>
        /// <param name="info">File path info</param>
        private void Read(FileInfo info)
        {
            FilePath = info;
            using (BinaryReader br = new BinaryReader(File.OpenRead(info.FullName)))
            {
                movie = new MovieClip();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    _Type header = (_Type)(br.ReadUInt32() & (uint)_Type._3B_MASK);
                    if (ReadFunctions.ContainsKey(header))
                    {
                        ReadFunctions[header](br, header);
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
        private FileSection ReadBIK(BinaryReader br, _Type type)
        {
            br.BaseStream.Seek(-1, SeekOrigin.Current);
            byte version = br.ReadByte();
            if ((type == _Type.BIK && bik1.Contains(version)) || (type == _Type.KB2 && bik2.Contains(version)))
            {
            }
            else
                throw new Exception($"_Type {type}, version {version}, is invalid");
            FileSection fs = new FileSection()
            {
                _Type = type,
                Offset = br.BaseStream.Position - 4,
                Size = br.ReadUInt32() + 8,
                Frames = br.ReadUInt32()
            };
            br.BaseStream.Seek(fs.Offset + fs.Size, SeekOrigin.Begin);

            if (movie.BINK_HIGH == null)
            {
                movie.BINK_HIGH = fs;
                movie.BINK_HIGH.FileName = GenerateFileName(movie.BINK_HIGH._Type == _Type.BIK ? "bik" : "bk2", "h");
            }
            else
            {
                if (fs.Size > movie.BINK_HIGH.Size)
                {
                    movie.BINK_LOW = movie.BINK_HIGH;
                    movie.BINK_HIGH = fs;
                }
                else
                {
                    movie.BINK_LOW = fs;
                }

                movie.BINK_HIGH.FileName = GenerateFileName(movie.BINK_LOW._Type == _Type.BIK ? "bik" : "bk2", "h");
                movie.BINK_LOW.FileName = GenerateFileName(movie.BINK_LOW._Type == _Type.BIK ? "bik" : "bk2", "l");
                _movies.Add(movie);
                movie = new MovieClip();
            }
            return fs;
        }

        /// <summary>
        /// Read cam file offset and size
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="type">Header of file type</param>
        private FileSection ReadCAM(BinaryReader br, _Type type)
        {
            long offset = br.BaseStream.Position - 4; // start of section
            br.BaseStream.Seek(2, SeekOrigin.Current); // skip 2 bytes
            ushort frames = br.ReadUInt16(); // get approx number of frames
            br.BaseStream.Seek((frames) * CamSectionSize, SeekOrigin.Current);
            uint b = 0;
            // there seems to be 1 or more extra frames. Check for those.
            while ((b = br.ReadUInt32()) > 0 && !(((_Type)(b & (uint)_Type._3B_MASK)) == _Type.BIK || ((_Type)(b & (uint)_Type._3B_MASK)) == _Type.KB2))
            {
                br.BaseStream.Seek(CamSectionSize - sizeof(uint), SeekOrigin.Current);
                frames++;
            }
            // Found the end go back to it.
            br.BaseStream.Seek(-sizeof(uint), SeekOrigin.Current); 

            // There is only one cam per movie. Checking for possibility of only one video instead of the normal 2 per movie.
            if (movie.CAM != null)
            {
                if (!_movies.Contains(movie))
                    //add existing movie to movies list.
                    _movies.Add(movie);
                //start from scratch
                movie = new MovieClip();
            }

            FileSection fs = new FileSection() { _Type = type, Offset = offset, Size = br.BaseStream.Position - offset, Frames = frames, FileName = GenerateFileName("cam") };
            movie.CAM = fs;
            return fs;
        }

        private FileSection ReadKB2(BinaryReader br, _Type header) => ReadBIK(br, header);

        #endregion Methods

        #region Structs

        public struct MovieClip
        {

            #region Fields

            /// <summary>
            /// High res bink video
            /// </summary>
            public FileSection BINK_HIGH;

            /// <summary>
            /// Low res bink video
            /// </summary>
            public FileSection BINK_LOW;

            /// <summary>
            /// Cam file
            /// </summary>
            public FileSection CAM;

            #endregion Fields

        }

        #endregion Structs

        #region Classes

        public class FileSection
        {

            #region Fields

            /// <summary>
            /// Type of file in Section
            /// </summary>
            public _Type _Type;

            /// <summary>
            /// Output FileName;
            /// </summary>
            public string FileName;

            /// <summary>
            /// Frame count
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

            #endregion Fields
        }

        #endregion Classes
    }
}