using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public struct Textures : IReadOnlyList<TextureHandler>
    {
        #region Fields

        /// <summary>
        /// EOF
        /// </summary>
        public readonly uint Eof;

        /// <summary>
        /// Texture 2D wrapped in TextureHandler for mod support
        /// </summary>
        private readonly IReadOnlyList<TextureHandler> _textures;

        #endregion Fields

        #region Constructors

        private Textures(byte[] buffer, long byteOffset, string fileName)
        {
            //#if DEBUG
            //            //Dump for debug
            //            _br.BaseStream.Seek(start, SeekOrigin.Begin);
            //            using (BinaryWriter fs = new BinaryWriter(File.Create(Path.Combine(Path.GetTempPath(), $"{start}.dump"), (int)(_br.BaseStream.Length - _br.BaseStream.Position), FileOptions.None)))
            //                fs.Write(_br.ReadBytes((int)(_br.BaseStream.Length - _br.BaseStream.Position)));
            //#endif
            IReadOnlyList<uint> pTim;
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
                //Begin create Textures struct
                //populate the tim Count;
                int cTim = br.ReadInt32();
                //create arrays per Count and Read pointers into array
                pTim = Enumerable.Range(0, cTim).Select(_ => (uint)(byteOffset + br.ReadUInt32())).ToList()
                    .AsReadOnly();
                //Read EOF
                Eof = br.ReadUInt32();
            }

            //Read TIM -> TextureHandler into array
            TextureHandler getTexture(uint offset, int i)
            {
                if (buffer[offset] == 0x10)
                {
                    TIM2 tm = new TIM2(buffer, offset); //broken
                    return TextureHandler.Create($"{fileName}_{i /*.ToString("D2")*/}", tm); // tm.GetTexture(0);
                }
                Memory.Log.WriteLine($"{nameof(Textures)}::{nameof(getTexture)}.{offset} :: Not a tim file!");
                return null;
            }
            _textures = pTim.Select(getTexture).ToList().AsReadOnly();
        }

        #endregion Constructors

        #region Properties

        public int Count => _textures.Count;

        #endregion Properties

        #region Indexers

        public TextureHandler this[int index] => _textures[index];

        #endregion Indexers

        #region Methods

        public Textures CreateInstance(byte[] buffer, long byteOffset, string fileName) => new Textures(buffer, byteOffset, fileName);

        public IEnumerator<TextureHandler> GetEnumerator() => _textures.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_textures).GetEnumerator();

        #endregion Methods
    }
}