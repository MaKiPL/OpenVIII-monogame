using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// This class stores the reference to where the string is. Can be read with Read();
    /// </summary>
    /// <remarks>
    /// I thought his would be a good idea but it doesn't look easy to implement. The strings class
    /// is kinda messy. I think I need to recode it to be more like the kernel class where file has
    /// it's own class.
    /// </remarks>
    public class FF8StringReference : FF8String
    {
        #region Fields

        private readonly Memory.Archive Archive;

        private readonly string Filename;

        private readonly long Offset;

        private readonly ushort ReadLength;

        private readonly Settings StringSettings;

        /// <summary>
        /// Check if had read already encause the actual length is 0.
        /// </summary>
        private bool HadRead = false;

        private object lockvar = new object();

        #endregion Fields

        #region Constructors

        public FF8StringReference(Memory.Archive archive, string filename, long offset, ushort length = 0, Settings settings = Settings.None)
        {
            Archive = archive;
            Filename = filename;
            Offset = offset;
            ReadLength = length;
            StringSettings = settings;
        }

        #endregion Constructors

        #region Enums

        [Flags]
        public enum Settings : byte
        {
            None = 0x0,

            //May contain multibyte characters
            MultiCharByte = 0x1,

            //May require replacement of tags from Namedic
            Namedic = 0x2,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// multi characters bytes and double character bytes
        /// </summary>
        /// TODO replace me.
        public static Dictionary<byte, FF8String> ByteToString { get; private set; }

        public override int Length
        {
            get
            {
                if (base.Length == 0) Read();
                return base.Length;
            }
        }

        public override byte[] Value
        {
            get
            {
                if (base.Length == 0) Read();
                return base.Value;
            }
            set => base.Value = value;
        }

        #endregion Properties

        #region Methods

        public static void Init()
        {
            if (ByteToString == null)
            {
                ByteToString = new Dictionary<byte, FF8String>
                {
                    //{0x01, "" },
                    {0xC6, "VI"},// pos:166, col:20, row:9 --
                    {0xC7, "II"},// pos:167, col:21, row:9 --
                    //pc version sysfld00 and 01
                    {0xCC, "GA"},// pos:172, col:5, row:9 --
                    {0xCD, "ME"},// pos:173, col:6, row:9 --
                    {0xCE, "FO"},// pos:174, col:7, row:9 --
                    {0xCF, "LD"},// pos:175, col:8, row:9 --
                    {0xD0, "ER"},// pos:176, col:9, row:9 --
                    ////original texture - sysfont
                    //{0xCC, "ME"},// pos:172, col:5, row:9 --
                    //{0xCD, "MO"},// pos:173, col:6, row:9 --
                    //{0xCE, "RY"},// pos:174, col:7, row:9 --
                    //{0xCF, "CA"},// pos:175, col:8, row:9 --
                    //{0xD0, "RD"},// pos:176, col:9, row:9 --

                    {0xD1, "Sl"},// pos:177, col:10, row:9 --
                    {0xD2, "ot"},// pos:178, col:11, row:9 --
                    {0xD3, "ing"},// pos:179, col:12, row:10 --
                    {0xD4, "St"},// pos:180, col:13, row:10 --
                    {0xD5, "ec"},// pos:181, col:14, row:10 --
                    {0xD6, "kp"},// pos:182, col:15, row:10 --
                    {0xD7, "la"},// pos:183, col:16, row:10 --
                    {0xD8, ":z"},// pos:184, col:17, row:10 --
                    {0xD9, "Fr"},// pos:185, col:18, row:10 --
                    {0xDA, "nt"},// pos:186, col:19, row:10 --
                    {0xDB, "elng"},// pos:187, col:20, row:10 --
                    {0xDC, "re"},// pos:188, col:21, row:10 --
                    {0xDD, "S:"},// pos:189, col:1, row:10 --
                    {0xDE, "so"},// pos:190, col:2, row:10 --
                    {0xDF, "Ra"},// pos:191, col:3, row:10 --
                    {0xE0, "nu"},// pos:192, col:4, row:10 --
                    {0xE1, "ra"},// pos:193, col:5, row:10 --
                    // all above is renderable meaning there is an image in the texture atlas for it.
                    // all below needs expanded into single byte characters.
                    //{0xE3, ""},// pos:195, col:0, row:0 --
                    //{0xE4, ""},// pos:196, col:0, row:0 --
                    //{0xE5, ""},// pos:197, col:0, row:0 --
                    //{0xE6, ""},// pos:198, col:0, row:0 --
                    //{0xE7, ""},// pos:199, col:0, row:0 --
                    {0xE8, "in"},// pos:200, col:0, row:0 --
                    {0xE9, "e "},// pos:201, col:0, row:0 --
                    {0xEA, "ne"},// pos:202, col:0, row:0 --
                    {0xEB, "to"},// pos:203, col:0, row:0 --
                    {0xEC, "re"},// pos:204, col:0, row:0 --
                    {0xED, "HP"},// pos:205, col:0, row:0 --
                    {0xEE, "l "},// pos:206, col:0, row:0 --
                    {0xEF, "ll"},// pos:207, col:0, row:0 --
                    {0xF0, "GF"},// pos:208, col:0, row:0 --
                    {0xF1, "nt"},// pos:209, col:0, row:0 --
                    {0xF2, "il"},// pos:210, col:0, row:0 --
                    {0xF3, "o "},// pos:211, col:0, row:0 --
                    {0xF4, "ef"},// pos:212, col:0, row:0 --
                    {0xF5, "on"},// pos:213, col:0, row:0 --
                    {0xF6, " w"},// pos:214, col:0, row:0 --
                    {0xF7, " r"},// pos:215, col:0, row:0 --
                    {0xF8, "wi"},// pos:216, col:0, row:0 --
                    {0xF9, "fi"},// pos:217, col:0, row:0 --
                    //{0xFA, ""},// pos:218, col:0, row:0 --
                    {0xFB, "s "},// pos:219, col:0, row:0 --
                    {0xFC, "ar"},// pos:220, col:0, row:0 --
                    //{0xFD, ""},// pos:221, col:0, row:0 --
                    {0xFE, " S"},// pos:222, col:0, row:0 --
                    {0xFF, "ag"},// pos:223, col:0, row:0 --
                };
            }
        }

        private void InsertNamedic()
        {
            if (Length > 0)
            {
                int i = 0;
                do
                {
                    i = Array.FindIndex(base.Value, i, Length - i, x => x == 0x0E);
                    if (i >= 0)
                    {
                        byte id = (byte)(value[i + 1] - 0x20);
                        byte[] newdata = Memory.Strings.Read(Strings.FileID.NAMEDIC, 0, id);
                        byte[] end = value.Skip(2 + i).ToArray();

                        Array.Resize(ref value, Length + newdata.Length - 2);
                        Array.Copy(newdata, 0, value, i, newdata.Length);
                        Array.Copy(end, 0, value, i + newdata.Length, end.Length);
                        i += newdata.Length;
                    }
                }
                while (i >= 0 && i < Length);
            }
        }

        private void Read()
        {
            lock (lockvar)
                if (base.Length == 0 && !HadRead)
                {
                    HadRead = true;
                    ArchiveWorker aw = new ArchiveWorker(Archive, true);
                    using (BinaryReader br = new BinaryReader(new MemoryStream(aw.GetBinaryFile(Filename, true))))
                    {
                        br.BaseStream.Seek(Offset, SeekOrigin.Begin);
                        if (ReadLength > 0 && (StringSettings & Settings.MultiCharByte) == 0) // ReadLength set, read that. unless contains multicharbytes
                            Value = br.ReadBytes(ReadLength);
                        else // Length unknown read to null
                        {
                            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
                            {
                                for (int i = 0; i < br.BaseStream.Length; i++)
                                {
                                    if (ReadLength > 0 && i > ReadLength) break;
                                    byte b = br.ReadByte();
                                    if (i == 0 || b != 0)
                                    {
                                        if (b > 0xE1 && (StringSettings & Settings.MultiCharByte) != 0 && ByteToString.ContainsKey(b))
                                            bw.Write(ByteToString[b].Value);
                                        else
                                            bw.Write(b);
                                    }
                                    else break;
                                }
                                Value = ((MemoryStream)bw.BaseStream).ToArray();
                            }
                        }
                    }
                    if ((StringSettings & Settings.Namedic) != 0)
                    {
                        InsertNamedic();
                    }
                }
        }

        #endregion Methods

        public static FF8String operator +(FF8StringReference a, FF8String b) => (FF8String)a + b;

        public static FF8String operator +(FF8String a, FF8StringReference b) => a + (FF8String)b;

        public static FF8String operator +(FF8StringReference a, FF8StringReference b) => (FF8String)a + (FF8String)b;

        public static FF8String operator +(FF8StringReference a, string b) => (FF8String)a + b;

        public static FF8String operator +(string a, FF8StringReference b) => a + (FF8String)b;

        /// old read method encase i missed something.
        //public FF8String Read(BinaryReader br, FileID fid, uint pos)
        //{
        //    if (pos == 0)
        //        return new FF8String("");
        //    if (pos < br.BaseStream.Length)
        //        using (MemoryStream os = new MemoryStream(50))
        //        {p
        //            br.BaseStream.Seek(pos, SeekOrigin.Begin);
        //            int c = 0;
        //            byte b = 0;
        //            do
        //            {
        //                if (br.BaseStream.Position > br.BaseStream.Length) break;
        //                //sometimes strings start with 00 or 01. But there is another 00 at the end.
        //                //I think it's for SeeD test like 1 is right and 0 is wrong. for now i skip them.
        //                b = br.ReadByte();
        //                if (b != 0 && b != 1)
        //                {
        //                    os.WriteByte(b);
        //                }
        //                c++;
        //            }
        //            while (b != 0 || c == 0);
        //            if (os.Length > 0)
        //                return os.ToArray();
        //        }
        //    return null;
        //}
    }
}