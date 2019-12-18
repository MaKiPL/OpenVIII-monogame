using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenVIII.Battle
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=15056.msg211220"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=16283.0"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=15906.0"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_magfiles"/>
    public class Mag
    {
        #region Fields

        public uint
            pBones,
            pTextureSize,
            pGeometry,
            pSCOT,
            pTexture;

        #endregion Fields

        #region Constructors

        public Mag(string filename, BinaryReader br)
        {
            FileName = filename;
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            if (TryReadTIM(br) == null)
            {
                isTIM = false;
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                //Offset Description
                //0x00    Probably always null
                uint pPadding = br.ReadUInt32();
                if (pPadding != 0)
                    return;
                //0x04    Probably bones/ animation data, might be 0x00
                pBones = br.ReadUInt32();
                //0x08    Unknown(used to determinate texture size *), might be 0x64
                pTextureSize = br.ReadUInt32();
                //0x0C    Geometry pointer, might be 0xAC
                pGeometry = br.ReadUInt32();
                //0x10    SCOT pointer, might be 0x00
                pSCOT = br.ReadUInt32();
                //0x14    Texture pointer, might be 0x30
                pTexture = br.ReadUInt32();
                //0x18 == 0x98
                //0x1C == 0xAC
                if(pBones > br.BaseStream.Length||
                    pTextureSize > br.BaseStream.Length ||
                    pGeometry > br.BaseStream.Length ||
                    pSCOT > br.BaseStream.Length ||
                    pTexture > br.BaseStream.Length)
                {
                    return;
                }
                isPackedMag = true;
                ReadGeometry(br);
                ReadTextures(br);
            }
        }

        private void ReadGeometry(BinaryReader br)
        {
            br.BaseStream.Seek(pGeometry, SeekOrigin.Begin);
            int count = br.ReadInt32();
            List<uint> positions = new List<uint>();
            while(count-- >0)
            {
                uint pos = br.ReadUInt32();
                if (pos > 0)
                    positions.Add(pos+pGeometry);
            }
        }

        #endregion Constructors

        #region Properties

        public byte DataType => getValue(2);

        public string FileName { get; private set; }

        public byte IDnumber => getValue(1);

        public bool isTIM { get; private set; } = false;

        public byte SequenceNumber => getValue(3);

        public TIM2[] TIM { get; private set; }
        public bool isPackedMag { get; private set; } = false;

        private bool bFileNameTest => FileName != null && Path.GetExtension(FileName).Trim('.').Length == 3;

        #endregion Properties

        #region Methods

        public TIM2[] TryReadTIM(BinaryReader br)
        {
            try
            {
                TIM2 tim = new TIM2(br, noExec: true);
                if (tim.NOT_TIM)
                    return TIM = null;
                isTIM = true;
                return TIM = new TIM2[] { tim };
            }
            catch (InvalidDataException)
            {
                return TIM = null;
            }
        }

        private byte getValue(int index) => bFileNameTest ? byte.Parse(FileName.Substring(FileName.Length - index, 1),
                                                System.Globalization.NumberStyles.HexNumber) : (byte)0xff;

        private TIM2[] ReadTextures(BinaryReader br)
        { //this doesn't sound like a tim file per documentation. 
            br.BaseStream.Seek(pTexture, SeekOrigin.Begin);
            List<uint> positions = new List<uint>();
            //Debug.Assert(pTextureSize > 0 && pTextureSize < br.BaseStream.Length);
            while (pTextureSize > 0 && br.BaseStream.Position < pTextureSize && br.BaseStream.Position + 4 < br.BaseStream.Length)
            {
                uint pos = br.ReadUInt32();
                if (pos != 0)
                    positions.Add(pos + pTexture);
            }
            if (positions.Count > 0)
            {
                //textures here?
                return null;
            }
            else
                return null;
        }

        #endregion Methods
    }
}