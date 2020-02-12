using K4os.Compression.LZ4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class ArchiveMap : IReadOnlyDictionary<string, FI>
    {
        #region Fields

        private Dictionary<string, FI> entries;

        #endregion Fields

        #region Constructors

        public ArchiveMap(byte[] fi, byte[] fl)
        {
            FL list = new FL(fl);
            using (BinaryReader br = new BinaryReader(new MemoryStream(fi, false)))
                entries = list.ToDictionary(x => x, x => Extended.ByteArrayToClass<FI>(br.ReadBytes(12)));
        }

        public ArchiveMap(StreamWithRangeValues fI, StreamWithRangeValues fL)
        {
            using (StreamReader sr = new StreamReader(fL, System.Text.Encoding.UTF8))
            using (BinaryReader br = new BinaryReader(fI))
            {
                fL.Seek(fL.Offset, SeekOrigin.Begin);
                fI.Seek(fI.Offset, SeekOrigin.Begin);
                entries = new Dictionary<string, FI>();
                long Count = fI.Size / 12;
                while (Count-->0)
                    entries.Add(sr.ReadLine().TrimEnd(), Extended.ByteArrayToClass<FI>(br.ReadBytes(12)));
            }
        }

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyDictionary<string, FI>)entries).Count;
        public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, FI>)entries).Keys;
        public IReadOnlyList<KeyValuePair<string, FI>> OrderedByName => entries.Where(x => !string.IsNullOrWhiteSpace(x.Key)).OrderBy(x => x.Key).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToList();
        public IReadOnlyList<KeyValuePair<string, FI>> OrderedByOffset => entries.Where(x => !string.IsNullOrWhiteSpace(x.Key)).OrderBy(x => x.Value.Offset).ThenBy(x => x.Key).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToList();
        public IEnumerable<FI> Values => ((IReadOnlyDictionary<string, FI>)entries).Values;

        #endregion Properties

        #region Indexers

        public FI this[string key] => ((IReadOnlyDictionary<string, FI>)entries)[key];

        #endregion Indexers

        #region Methods

        public static byte[] LZ4Uncompress(byte[] input, int fsUncompressedSize, int offset = 12)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveMap)}::{nameof(LZ4Uncompress)} :: decompressing data");
            //ReadOnlySpan<byte> input = new ReadOnlySpan<byte>(file);
            byte[] output = new byte[fsUncompressedSize];
            //Span<byte> output = new Span<byte>(r);
            int count = 0;
            while (input.Length - offset > 0 && (count = LZ4Codec.Decode(input, offset, input.Length - offset, output, 0, output.Length)) <= -1)
            {
                offset++;
            }
            if (offset > -1)
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(LZ4Uncompress)}::{nameof(offset)}: {offset}");
                return output;//.ToArray();
            }
            else
                throw new Exception("failed to decompress");
        }

        public bool ContainsKey(string key) => ((IReadOnlyDictionary<string, FI>)entries).ContainsKey(key);

        public FI FindString(ref string input, out int size)
        {
            string _input = input;
            KeyValuePair<string, FI> result = OrderedByName.FirstOrDefault(x => x.Key.IndexOf(_input, StringComparison.OrdinalIgnoreCase) > -1);
            if (string.IsNullOrWhiteSpace(result.Key))
            {
                size = 0;
                return null;
            }
            KeyValuePair<string, FI> result2 = OrderedByOffset.FirstOrDefault(x => x.Value.Offset > result.Value.Offset);
            if (result2.Value == default && result2.Value == default)
                size = 0;
            else size = result2.Value.Offset - result.Value.Offset;
            input = result.Key;
            return result.Value;
        }

        public byte[] GetBinaryFile(string input, Stream data)
        {
            long Max = data.Length;
            long Offset = 0;
            if(data.GetType() == typeof(StreamWithRangeValues))
            {
                StreamWithRangeValues s = (StreamWithRangeValues)data;
                Offset = s.Offset;
                Max = s.Max;
            }
            FI fi = FindString(ref input, out int size);
            if (fi == null)
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)}:: failed to load {input}");
                return null;
            }
            if (size == 0)
                size = checked((int)(Max - (fi.Offset+Offset)));
            byte[] buffer;
            using (BinaryReader br = new BinaryReader(data))
            {
                br.BaseStream.Seek(fi.Offset+Offset, SeekOrigin.Begin);
                if (fi.CompressionType == 1)
                {
                    size = br.ReadInt32();
                    buffer = br.ReadBytes(size);
                }
                else
                    buffer = br.ReadBytes(size);
            }
            switch (fi.CompressionType)
            {
                case 0:
                    return buffer;

                case 1:
                    return LZSS.DecompressAllNew(buffer);

                case 2:
                    return LZ4Uncompress(buffer, fi.UncompressedSize);

                default:
                    throw new InvalidDataException($"{nameof(fi.CompressionType)}: {fi.CompressionType} is invalid...");
            }
        }

        public IEnumerator<KeyValuePair<string, FI>> GetEnumerator() => ((IReadOnlyDictionary<string, FI>)entries).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyDictionary<string, FI>)entries).GetEnumerator();

        public bool TryGetValue(string key, out FI value) => ((IReadOnlyDictionary<string, FI>)entries).TryGetValue(key, out value);

        #endregion Methods
    }
}