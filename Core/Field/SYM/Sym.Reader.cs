using System;
using System.IO;
using System.Text;

namespace OpenVIII.Fields
{
    public static partial class Sym
    {
        public static class Reader
        {
            public static GameObjects FromBytes(Byte[] buffer)
            {
                using (var ms = new MemoryStream(buffer))
                    return FromStream(ms);
            }

            public static GameObjects FromStream(Stream input)
            {
                using (var reader = new StreamReader(input, System.Text.Encoding.ASCII, false, bufferSize: 4096, leaveOpen: true))
                    return FromReader(reader);
            }

            public static GameObjects FromReader(StreamReader reader)
            {
                GameObjects result = new GameObjects();

                while (!reader.EndOfStream)
                {
                    String str = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;

                    Parse(str, result);
                }

                return result;
            }

            private static void Parse(String str, GameObjects result)
            {
                String[] pair = str.Split(new[] {"::"}, StringSplitOptions.None);
                if (pair.Length == 1)
                    result.AddObject(pair[0].Trim());
                else if (pair.Length == 2)
                    result.AddScript(pair[0].Trim(), pair[1].Trim());
                else
                    throw new NotSupportedException($"Cannot parse the invalid symbol: {str}");
            }
        }
    }
}
