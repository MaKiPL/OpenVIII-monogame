using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8
{
    /// <summary>
    /// class to add function to dictionary
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/22595655/how-to-do-a-dictionary-reverse-lookup"/>
    public static class DictionaryEx
    {
        #region Methods

        /// <summary>
        /// Reverses Key and Value of dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            Dictionary<TValue, TKey> dictionary = new Dictionary<TValue, TKey>();
            foreach (KeyValuePair<TKey, TValue> entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }

        #endregion Methods
    }

    /// <summary>
    /// This holds the encoded bytes and provides implict casts to string and byte[]
    /// </summary>
    public class FF8String : IEnumerator, IEnumerable
    {
        #region Fields

        private byte[] value;
        private int position = 0;

        #endregion Fields

        #region Constructors

        public FF8String()
        {
        }

        public FF8String(byte[] @value) => Value = @value;

        public FF8String(string @value) => Value = Memory.DirtyEncoding.GetBytes(@value);

        #endregion Constructors

        #region Properties

        public byte[] Value { get => value; set => this.value = value; }
        public string Value_str => ToString();
        public int Length => value.Length;
        public object Current { get => Value[position-1] ; }

        #endregion Properties

        #region Indexers

        public byte this[int index] => value[index];

        #endregion Indexers

        #region Methods

        public static implicit operator byte[] (FF8String input) => input != null ? input.Value : null;

        public static implicit operator FF8String(string input) => new FF8String(input);

        public static implicit operator FF8String(byte[] input) => new FF8String(input);

        public static implicit operator string(FF8String input) => input != null ? input.ToString() : null;

        public override string ToString() => Memory.DirtyEncoding.GetString(Value);

        public bool MoveNext() => ++position <= Length;
        public void Reset() => position = 0;
        public IEnumerator GetEnumerator() => this;

        #endregion Methods
    }

    /// <summary>
    /// Goal is to improve conversion from FF8 byte[] to string and back. possible 0 to 4 ratio on
    /// special chracters.
    /// </summary>
    internal class DirtyEncoding : Encoding
    {
        #region Fields

        public static readonly Dictionary<byte, char> BytetoChar = new Dictionary<byte, char>
        {
            //Commented out special bytes so they are passed through and I can see them in the dump file. Then I can figure out what to do with them.
            {0x00, '\0'},// pos:-32, col:0, row:-1 -- is end of a string. MSG files have more than one string sepperated by \0
            //{0x01, ''},// pos:-31, col:0, row:0 -- //some strings start with 0x01. might mark correct answer on SeeD exams.
            {0x02, '\n'},// pos:-30, col:0, row:0 -- new line
            //{0x03, ''},// pos:-29, col:0, row:0 -- special character. {0x03,0x40 = [Angelo's Name]}
            //{0x04, ''},// pos:-28, col:0, row:0 --
            //{0x05, ''},// pos:-27, col:0, row:0 --
            //{0x06, ''},// pos:-26, col:0, row:0 --
            //{0x07, ''},// pos:-25, col:0, row:0 --
            //{0x08, ''},// pos:-24, col:0, row:0 --
            //{0x09, ''},// pos:-23, col:0, row:0 --
            //{0x0A, ''},// pos:-22, col:0, row:0 --
            //{0x0B, ''},// pos:-21, col:0, row:0 --
            //{0x0C, ''},// pos:-20, col:0, row:0 -- <MAGBYTE>
            //{0x0D, ''},// pos:-19, col:0, row:0 --
            //{0x0E, ''},// pos:-18, col:0, row:0 -- <$>
            //{0x0F, ''},// pos:-17, col:0, row:0 --
            //{0x10, ''},// pos:-16, col:0, row:0 --
            //{0x11, ''},// pos:-15, col:0, row:0 --
            //{0x12, ''},// pos:-14, col:0, row:0 --
            //{0x13, ''},// pos:-13, col:0, row:0 --
            //{0x14, ''},// pos:-12, col:0, row:0 --
            //{0x15, ''},// pos:-11, col:0, row:0 --
            //{0x16, ''},// pos:-10, col:0, row:1 --
            //{0x17, ''},// pos:-9, col:0, row:0 --
            //{0x18, ''},// pos:-8, col:0, row:0 --
            //{0x19, ''},// pos:-7, col:0, row:0 --
            //{0x1A, ''},// pos:-6, col:0, row:0 --
            //{0x1B, ''},// pos:-5, col:0, row:0 --
            //{0x1C, ''},// pos:-4, col:0, row:0 --
            //{0x1D, ''},// pos:-3, col:0, row:0 --
            //{0x1E, ''},// pos:-2, col:0, row:0 --
            //{0x1F, ''},// pos:-1, col:0, row:0 --
            {0x20, ' '},// pos:0, col:1, row:1 -- Start of font texture
            {0x21, '0'},// pos:1, col:2, row:1 --
            {0x22, '1'},// pos:2, col:3, row:1 --
            {0x23, '2'},// pos:3, col:4, row:1 --
            {0x24, '3'},// pos:4, col:5, row:1 --
            {0x25, '4'},// pos:5, col:6, row:1 --
            {0x26, '5'},// pos:6, col:7, row:1 --
            {0x27, '6'},// pos:7, col:8, row:1 --
            {0x28, '7'},// pos:8, col:9, row:1 --
            {0x29, '8'},// pos:9, col:10, row:1 --
            {0x2A, '9'},// pos:10, col:11, row:1 --
            {0x2B, '%'},// pos:11, col:12, row:2 --
            {0x2C, '/'},// pos:12, col:13, row:2 --
            {0x2D, ':'},// pos:13, col:14, row:2 --
            {0x2E, '!'},// pos:14, col:15, row:2 --
            {0x2F, '?'},// pos:15, col:16, row:2 --
            {0x30, '…'},// pos:16, col:17, row:2 --
            {0x31, '+'},// pos:17, col:18, row:2 --
            {0x32, '-'},// pos:18, col:19, row:2 --
            {0x33, '='},// pos:19, col:20, row:2 --
            {0x34, '*'},// pos:20, col:21, row:2 --
            {0x35, '&'},//&amp; pos:21, col:1, row:2 -- temporarly set to this so i could have formatting on pastebin
            {0x36, '「'},// pos:22, col:2, row:2 --
            {0x37, '」'},// pos:23, col:3, row:2 --
            {0x38, '('},// pos:24, col:4, row:2 --
            {0x39, ')'},// pos:25, col:5, row:2 --
            {0x3A, '·'},// pos:26, col:6, row:2 --
            {0x3B, '.'},// pos:27, col:7, row:2 --
            {0x3C, ','},// pos:28, col:8, row:2 --
            {0x3D, '~'},// pos:29, col:9, row:2 --
            {0x3E, '“'},// pos:30, col:10, row:2 --
            {0x3F, '”'},// pos:31, col:11, row:2 --
            {0x40, '\''},// pos:32, col:12, row:3 --
            {0x41, '#'},// pos:33, col:13, row:3 --
            {0x42, '$'},// pos:34, col:14, row:3 --
            {0x43, '`'},// pos:35, col:15, row:3 --
            {0x44, '_'},// pos:36, col:16, row:3 --
            {0x45, 'A'},// pos:37, col:17, row:3 --
            {0x46, 'B'},// pos:38, col:18, row:3 --
            {0x47, 'C'},// pos:39, col:19, row:3 --
            {0x48, 'D'},// pos:40, col:20, row:3 --
            {0x49, 'E'},// pos:41, col:21, row:3 --
            {0x4A, 'F'},// pos:42, col:1, row:3 --
            {0x4B, 'G'},// pos:43, col:2, row:3 --
            {0x4C, 'H'},// pos:44, col:3, row:3 --
            {0x4D, 'I'},// pos:45, col:4, row:3 --
            {0x4E, 'J'},// pos:46, col:5, row:3 --
            {0x4F, 'K'},// pos:47, col:6, row:3 --
            {0x50, 'L'},// pos:48, col:7, row:3 --
            {0x51, 'M'},// pos:49, col:8, row:3 --
            {0x52, 'N'},// pos:50, col:9, row:3 --
            {0x53, 'O'},// pos:51, col:10, row:3 --
            {0x54, 'P'},// pos:52, col:11, row:3 --
            {0x55, 'Q'},// pos:53, col:12, row:4 --
            {0x56, 'R'},// pos:54, col:13, row:4 --
            {0x57, 'S'},// pos:55, col:14, row:4 --
            {0x58, 'T'},// pos:56, col:15, row:4 --
            {0x59, 'U'},// pos:57, col:16, row:4 --
            {0x5A, 'V'},// pos:58, col:17, row:4 --
            {0x5B, 'W'},// pos:59, col:18, row:4 --
            {0x5C, 'X'},// pos:60, col:19, row:4 --
            {0x5D, 'Y'},// pos:61, col:20, row:4 --
            {0x5E, 'Z'},// pos:62, col:21, row:4 --
            {0x5F, 'a'},// pos:63, col:1, row:4 --
            {0x60, 'b'},// pos:64, col:2, row:4 --
            {0x61, 'c'},// pos:65, col:3, row:4 --
            {0x62, 'd'},// pos:66, col:4, row:4 --
            {0x63, 'e'},// pos:67, col:5, row:4 --
            {0x64, 'f'},// pos:68, col:6, row:4 --
            {0x65, 'g'},// pos:69, col:7, row:4 --
            {0x66, 'h'},// pos:70, col:8, row:4 --
            {0x67, 'i'},// pos:71, col:9, row:4 --
            {0x68, 'j'},// pos:72, col:10, row:4 --
            {0x69, 'k'},// pos:73, col:11, row:4 --
            {0x6A, 'l'},// pos:74, col:12, row:5 --
            {0x6B, 'm'},// pos:75, col:13, row:5 --
            {0x6C, 'n'},// pos:76, col:14, row:5 --
            {0x6D, 'o'},// pos:77, col:15, row:5 --
            {0x6E, 'p'},// pos:78, col:16, row:5 --
            {0x6F, 'q'},// pos:79, col:17, row:5 --
            {0x70, 'r'},// pos:80, col:18, row:5 --
            {0x71, 's'},// pos:81, col:19, row:5 --
            {0x72, 't'},// pos:82, col:20, row:5 --
            {0x73, 'u'},// pos:83, col:21, row:5 --
            {0x74, 'v'},// pos:84, col:1, row:5 --
            {0x75, 'w'},// pos:85, col:2, row:5 --
            {0x76, 'x'},// pos:86, col:3, row:5 --
            {0x77, 'y'},// pos:87, col:4, row:5 --
            {0x78, 'z'},// pos:88, col:5, row:5 --
            {0x79, 'À'},// pos:89, col:6, row:5 --
            {0x7A, 'Á'},// pos:90, col:7, row:5 --
            {0x7B, 'Â'},// pos:91, col:8, row:5 --
            {0x7C, 'Ä'},// pos:92, col:9, row:5 --
            {0x7D, 'Ç'},// pos:93, col:10, row:5 --
            {0x7E, 'È'},// pos:94, col:11, row:5 --
            {0x7F, 'É'},// pos:95, col:12, row:6 --
            {0x80, 'Ê'},// pos:96, col:13, row:6 --
            {0x81, 'Ë'},// pos:97, col:14, row:6 --
            {0x82, 'Ì'},// pos:98, col:15, row:6 --
            {0x83, 'Í'},// pos:99, col:16, row:6 --
            {0x84, 'Î'},// pos:100, col:17, row:6 --
            {0x85, 'Ï'},// pos:101, col:18, row:6 --
            {0x86, 'Ñ'},// pos:102, col:19, row:6 --
            {0x87, 'Ò'},// pos:103, col:20, row:6 --
            {0x88, 'Ó'},// pos:104, col:21, row:6 --
            {0x89, 'Ô'},// pos:105, col:1, row:6 --
            {0x8A, 'Ö'},// pos:106, col:2, row:6 --
            {0x8B, 'Ù'},// pos:107, col:3, row:6 --
            {0x8C, 'Ú'},// pos:108, col:4, row:6 --
            {0x8D, 'Û'},// pos:109, col:5, row:6 --
            {0x8E, 'Ü'},// pos:110, col:6, row:6 --
            {0x8F, 'Œ'},// pos:111, col:7, row:6 --
            {0x90, 'Ʀ'},// pos:112, col:8, row:6 --
            {0x91, 'à'},// pos:113, col:9, row:6 --
            {0x92, 'á'},// pos:114, col:10, row:6 --
            {0x93, 'â'},// pos:115, col:11, row:6 --
            {0x94, 'ä'},// pos:116, col:12, row:7 --
            {0x95, 'ç'},// pos:117, col:13, row:7 --
            {0x96, 'è'},// pos:118, col:14, row:7 --
            {0x97, 'é'},// pos:119, col:15, row:7 --
            {0x98, 'ê'},// pos:120, col:16, row:7 --
            {0x99, 'ë'},// pos:121, col:17, row:7 --
            {0x9A, 'ì'},// pos:122, col:18, row:7 --
            {0x9B, 'í'},// pos:123, col:19, row:7 --
            {0x9C, 'î'},// pos:124, col:20, row:7 --
            {0x9D, 'ï'},// pos:125, col:21, row:7 --
            {0x9E, 'ñ'},// pos:126, col:1, row:7 --
            {0x9F, 'ò'},// pos:127, col:2, row:7 --
            {0xA0, 'ó'},// pos:128, col:3, row:7 --
            {0xA1, 'ô'},// pos:129, col:4, row:7 --
            {0xA2, 'ö'},// pos:130, col:5, row:7 --
            {0xA3, 'ù'},// pos:131, col:6, row:7 --
            {0xA4, 'ú'},// pos:132, col:7, row:7 --
            {0xA5, 'û'},// pos:133, col:8, row:7 --
            {0xA6, 'ü'},// pos:134, col:9, row:7 --
            {0xA7, 'œ'},// pos:135, col:10, row:7 --
            {0xA8, 'Ⅷ'},// pos:136, col:11, row:7 --
            {0xA9, '['},// pos:137, col:12, row:8 --
            {0xAA, ']'},// pos:138, col:13, row:8 --
            {0xAB, '⬛'},// pos:139, col:14, row:8 --
            {0xAC, '⦾'},// pos:140, col:15, row:8 --
            {0xAD, '◆'},// pos:141, col:16, row:8 --
            {0xAE, '【'},// pos:142, col:17, row:8 --
            {0xAF, '】'},// pos:143, col:18, row:8 --
            {0xB0, '⬜'},// pos:144, col:19, row:8 --
            {0xB1, '★'},// pos:145, col:20, row:8 --
            {0xB2, '『'},// pos:146, col:21, row:8 --
            {0xB3, '』'},// pos:147, col:1, row:8 --
            {0xB4, '▽'},// pos:148, col:2, row:8 --
            {0xB5, ';'},// pos:149, col:3, row:8 --
            {0xB6, '▼'},// pos:150, col:4, row:8 --
            {0xB7, '‾'},// pos:151, col:5, row:8 --
            {0xB8, '×'},// pos:152, col:6, row:8 --
            {0xB9, '☆'},// pos:153, col:7, row:8 --
            {0xBA, '’'},// pos:154, col:8, row:8 --
            {0xBB, '↓'},// pos:155, col:9, row:8 --
            {0xBC, '°'},// pos:156, col:10, row:8 --
            {0xBD, '¡'},// pos:157, col:11, row:8 --
            {0xBE, '¿'},// pos:158, col:12, row:9 -- // pc version sysfld00 and 01 has "Slo" here so there isn't a upside down questionmark on highres.
            {0xBF, '—'},// pos:159, col:13, row:9 --
            {0xC0, '«'},// pos:160, col:14, row:9 --
            {0xC1, '»'},// pos:161, col:15, row:9 --
            {0xC2, '±'},// pos:162, col:16, row:9 --
            {0xC3, '♫'},// pos:163, col:17, row:9 --
            //{0xC4, ''},// pos:164, col:18, row:9 -- seems to be used as an alignment or a place holder many strings have 3 of these in a row.
            {0xC5, '↑'},// pos:165, col:19, row:9 --
            {0xC8, '¡'},// pos:168, col:1, row:9 --
            {0xC9, '™'},// pos:169, col:2, row:9 --
            {0xCA, '<'},// pos:170, col:3, row:9 --
            {0xCB, '>'},// pos:171, col:4, row:9 --
            {0xE2, '®'},// pos:194, col:6, row:10 -- End of font texture
        };

        /// <summary>
        /// multi characters bytes and double character bytes
        /// </summary>
        public static readonly Dictionary<byte, string> BytetoStr = new Dictionary<byte, string>
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
            //{0xE3, ""},// pos:195, col:0, row:0 --
            //{0xE4, ""},// pos:196, col:0, row:0 --
            //{0xE5, ""},// pos:197, col:0, row:0 --
            //{0xE6, ""},// pos:198, col:0, row:0 --
            //{0xE7, ""},// pos:199, col:0, row:0 --
            {0xE8, "in"},// pos:200, col:0, row:0 --
            {0xE9, "e "},// pos:201, col:0, row:0 --
            {0xEA, "ne"},// pos:202, col:0, row:0 --
            {0xEB, "to"},// pos:203, col:0, row:0 --
            {0xEC, "to"},// pos:204, col:0, row:0 --
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

        public static readonly Dictionary<char, byte> ChartoByte = BytetoChar.Reverse();
        public static readonly Dictionary<string, byte> StrtoByte = BytetoStr.Reverse();

        #endregion Fields

        #region Constructors

        public DirtyEncoding()
        {
            //reverse key and value pairs for dictionarys for the reverse lookup
            SpecialCharacters = new byte[256 - BytetoChar.Count() - BytetoStr.Count];
            int i = 0;
            for (byte b = byte.MinValue; b <= byte.MaxValue && i < SpecialCharacters.Length; b++)
            {
                if (!BytetoChar.ContainsKey(b) && !BytetoStr.ContainsKey(b))
                    SpecialCharacters[i++] = b;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Lists all bytes directly passed through unalterd. Functions are expected to handle or
        /// skip these.
        /// </summary>
        public byte[] SpecialCharacters { get; private set; }

        #endregion Properties

        //NOTE: There are some abstract members requiring you to implement or declare in this derived class.

        #region Methods

        public override int GetByteCount(char[] chars, int index, int count) => count + index <= chars.Length ? count - index : chars.Length - index;

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) => throw new System.NotImplementedException();

        public override byte[] GetBytes(string s)
        {
            using (MemoryStream ms = new MemoryStream(GetMaxCharCount(s.Length)))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                //Only way I could see to do MCB or DCB is to do a string.replace for each strtobyte.
                //So why not just only do SCB
                foreach (char c in s)
                {
                    byte b = ChartoByte.ContainsKey(c) ? ChartoByte[c] : byte.Parse(c.ToString());
                    bw.Write(b);
                }
                return ms.ToArray();
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count) => GetString(bytes.Skip(index).Take(count).ToArray()).Length;

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) => throw new System.NotImplementedException();

        public override int GetMaxByteCount(int charCount) => charCount;

        public override int GetMaxCharCount(int byteCount) => byteCount * 4; //largest multichar byte is 4;

        public override string GetString(byte[] bytes)
        {
            //using (MemoryStream ms = new MemoryStream(GetMaxByteCount(bytes.Length)))
            //using (BinaryWriter bw = new BinaryWriter(ms))
            //using (StreamReader sr = new StreamReader(ms))
            //{
            string @out = "";
            foreach (byte c in bytes)
            {
                string b = BytetoChar.ContainsKey(c) ? BytetoChar[c].ToString() :
                    BytetoStr.ContainsKey(c) ? BytetoStr[c] :
                    ((char)c).ToString();
                //bw.Write(b);
                @out += b;
            }
            return @out;
            //ms.Seek(0, SeekOrigin.Begin);
            //return sr.ReadToEnd();
            //    }
        }

        #endregion Methods

        //And many other virtual (overridable) methods which you can override to implement your custom Encoding fully
    }
}