using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace FF8
{
    #region Structs

    public class Entry //Rectangle + File
    {
        #region Fields

        private Rectangle src;
        private Point offset;
        private Loc loc;

        #endregion Fields

        #region Properties
        public ushort CurrentPos { get; set; }
        public byte Part { get; set; } = 1;
        public byte File { get; set; }
        public byte Height { get => (byte)src.Height; set => src.Height = value; }
        public byte Width { get => (byte)src.Width; set => src.Width = value; }
        public byte X { get => (byte)src.X; set => src.X = value; }
        public byte Y { get => (byte)src.Y; set => src.Y = value; }
        /// <summary>
        /// when you draw shift by X
        /// </summary>
        public sbyte Offset_X {get => (sbyte) offset.X; set => offset.X = value; }
        /// <summary>
        /// when you draw shift by Y
        /// </summary>
        public sbyte Offset_Y {get => (sbyte)offset.Y; set => offset.Y = value; }
    

        public Loc GetLoc() => loc; public void SetLoc(Loc value) => loc = value; 
        public byte[] UNK;

        public Entry()
        {
            UNK = new byte[2];
            File = 0;
            Part = 1;
        }

        #endregion Properties

        #region Methods

        public Rectangle GetRectangle() => src;

        public byte LoadfromStreamSP2(BinaryReader br, UInt16 loc = 0, byte prevY = 0, byte fid = 0)
        {
            if(loc>0)
                br.BaseStream.Seek(loc + 4, SeekOrigin.Begin);
            CurrentPos = loc;
            X = br.ReadByte();
            Y = br.ReadByte();
            UNK[0] = br.ReadByte();
            UNK[1] = br.ReadByte();
            //br.BaseStream.Seek(2, SeekOrigin.Current);
            Width = br.ReadByte();
            Offset_X = br.ReadSByte();
            //br.BaseStream.Seek(1, SeekOrigin.Current);
            Height = br.ReadByte();
            Offset_Y = br.ReadSByte();
            //br.BaseStream.Seek(1, SeekOrigin.Current);
            if (prevY > 0 && Y < prevY)
                fid++;
            File = fid;
            return fid;
        }

        #endregion Methods
    }

    #endregion Structs
}