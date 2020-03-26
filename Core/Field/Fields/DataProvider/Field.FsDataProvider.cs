using System;
using System.IO;

namespace OpenVIII.Fields
{
    public static partial class Field
    {
        public sealed class FsDataProvider : IDataProvider
        {
            private readonly String _fieldPath;
            private readonly String _fieldDirectory;

            public FsDataProvider(String fieldPath)
            {
                if (!File.Exists(fieldPath))
                    throw new FileNotFoundException(fieldPath);

                _fieldPath = Path.ChangeExtension(fieldPath, extension: null);
                _fieldDirectory = Path.GetDirectoryName(fieldPath);
            }

            public Byte[] FindPart(Part part)
            {
                var filePath = GetFilePath(part);
                if (File.Exists(filePath))
                    return File.ReadAllBytes(filePath);

                return null;
            }

            private String GetFilePath(Part part)
            {
                switch (part)
                {
                    case Part.One: return Path.Combine(_fieldDirectory, "chara.one");
                    case Part.Ca: return _fieldPath + ".ca";
                    case Part.Id: return _fieldPath + ".ID";
                    case Part.Inf: return _fieldPath + ".inf";
                    case Part.Jsm: return _fieldPath + ".jsm";
                    case Part.Map: return _fieldPath + ".map";
                    case Part.Mim: return _fieldPath + ".mim";
                    case Part.Mrt: return _fieldPath + ".mrt";
                    case Part.Msd: return _fieldPath + ".msd";
                    case Part.Pcb: return _fieldPath + ".pcb";
                    case Part.Pmd: return _fieldPath + ".pmd";
                    case Part.Pmp: return _fieldPath + ".pmp";
                    case Part.Pvp: return _fieldPath + ".pvp";
                    case Part.Rat: return _fieldPath + ".rat";
                    case Part.Sfx: return _fieldPath + ".sfx";
                    case Part.Sym: return _fieldPath + ".sym";
                    case Part.Tdw: return _fieldPath + ".tdw";
                    default: throw new NotSupportedException(part.ToString());
                }
            }
        }
    }
}